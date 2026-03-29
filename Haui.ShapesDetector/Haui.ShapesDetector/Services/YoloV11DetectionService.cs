using Haui.ShapesDetector.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLaborsImage = SixLabors.ImageSharp.Image;
using SixLaborsSize = SixLabors.ImageSharp.Size;

namespace Haui.ShapesDetector.Services;

/// <summary>
/// Optimized YOLOv11 Detection Service with:
/// - Letterbox padding (maintain aspect ratio - CRITICAL for accuracy!)
/// - GPU acceleration support (for FP16 half=True model)
/// - Bicubic resampling for better quality
/// - Accurate coordinate transformation
/// </summary>
public class YoloV11DetectionService : IDetectionService, IDisposable
{
    private InferenceSession? _session;
    private string[]? _classes;
    private float _confidenceThreshold = 0.75f; // Keep 75% threshold
    private float _iouThreshold = 0.45f;
    private const int ModelInputSize = 640;
    private bool _useGpu = false;

    public bool IsInitialized => _session != null && _classes != null;

    public async Task InitializeAsync(string modelPath, string classesPath)
    {
        try
        {
            // Load ONNX model with optimizations
            var sessionOptions = new SessionOptions();
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;

            // Try to use CUDA GPU (for FP16 half=True model)
            try
            {
                sessionOptions.AppendExecutionProvider_CUDA(0);
                _useGpu = true;
            }
            catch
            {
                // Fallback to CPU if CUDA not available
                _useGpu = false;
            }

            _session = new InferenceSession(modelPath, sessionOptions);

            // Load classes
            _classes = await File.ReadAllLinesAsync(classesPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to initialize YOLO model: {ex.Message}", ex);
        }
    }

    public void SetConfidenceThreshold(float threshold)
    {
        _confidenceThreshold = threshold;
    }

    public void SetIouThreshold(float threshold)
    {
        _iouThreshold = threshold;
    }

    public async Task<List<DetectionResult>> DetectAsync(byte[] imageData, int originalWidth, int originalHeight)
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Detection service not initialized");

        return await Task.Run(() =>
        {
            // Load image
            using var image = SixLaborsImage.Load<Rgb24>(imageData);

            // Preprocess with letterbox (CRITICAL: maintain aspect ratio like training)
            var (input, ratio, padX, padY) = PreprocessImageWithLetterbox(image);

            // Run inference
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", input)
            };

            using var results = _session!.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();

            // Post-process results (adjust for letterbox padding)
            var detections = PostProcess(output, originalWidth, originalHeight, ratio, padX, padY);
            return detections;
        });
    }

    /// <summary>
    /// Letterbox preprocessing - maintain aspect ratio with gray padding (like YOLO training)
    /// This prevents image distortion and significantly improves accuracy!
    /// </summary>
    private (DenseTensor<float> tensor, float ratio, int padX, int padY) PreprocessImageWithLetterbox(
        SixLabors.ImageSharp.Image<Rgb24> image)
    {
        int originalWidth = image.Width;
        int originalHeight = image.Height;

        // Calculate resize ratio (maintain aspect ratio)
        float ratio = Math.Min(
            ModelInputSize / (float)originalWidth,
            ModelInputSize / (float)originalHeight
        );

        int newWidth = (int)(originalWidth * ratio);
        int newHeight = (int)(originalHeight * ratio);

        // Calculate padding to center the image
        int padX = (ModelInputSize - newWidth) / 2;
        int padY = (ModelInputSize - newHeight) / 2;

        // Resize image (maintain aspect ratio, use Bicubic for better quality)
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new SixLaborsSize(newWidth, newHeight),
            Mode = ResizeMode.Max, // Maintain aspect ratio
            Sampler = KnownResamplers.Bicubic // Better quality than default
        }));

        // Create tensor with padding [1, 3, 640, 640]
        var tensor = new DenseTensor<float>(new[] { 1, 3, ModelInputSize, ModelInputSize });

        // Fill with gray padding (114/255 = 0.447 - standard YOLO padding color)
        for (int c = 0; c < 3; c++)
        {
            for (int y = 0; y < ModelInputSize; y++)
            {
                for (int x = 0; x < ModelInputSize; x++)
                {
                    tensor[0, c, y, x] = 0.447f;
                }
            }
        }

        // Copy resized image to center with padding
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < newHeight; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < newWidth; x++)
                {
                    int tensorY = y + padY;
                    int tensorX = x + padX;

                    tensor[0, 0, tensorY, tensorX] = pixelRow[x].R / 255f; // R
                    tensor[0, 1, tensorY, tensorX] = pixelRow[x].G / 255f; // G
                    tensor[0, 2, tensorY, tensorX] = pixelRow[x].B / 255f; // B
                }
            }
        });

        return (tensor, ratio, padX, padY);
    }

    /// <summary>
    /// Post-process with letterbox coordinate transformation
    /// Adjusts bounding boxes coordinates to account for padding
    /// </summary>
    private List<DetectionResult> PostProcess(
        float[] output, 
        int originalWidth, 
        int originalHeight,
        float ratio,
        int padX,
        int padY)
    {
        var results = new List<DetectionResult>();

        // YOLOv11 output format: [1, 4 + num_classes, num_predictions]
        int numClasses = _classes!.Length;
        int numPredictions = output.Length / (4 + numClasses);

        var detections = new List<(BoundingBox box, int classId, float confidence)>();

        for (int i = 0; i < numPredictions; i++)
        {
            // Extract box coordinates (cx, cy, w, h) in 640x640 space
            float cx = output[i];
            float cy = output[numPredictions + i];
            float w = output[2 * numPredictions + i];
            float h = output[3 * numPredictions + i];

            // Find class with max confidence
            int bestClassId = 0;
            float maxConfidence = 0f;

            for (int c = 0; c < numClasses; c++)
            {
                float confidence = output[(4 + c) * numPredictions + i];
                if (confidence > maxConfidence)
                {
                    maxConfidence = confidence;
                    bestClassId = c;
                }
            }

            if (maxConfidence > _confidenceThreshold)
            {
                // Remove letterbox padding and scale back to original coordinates
                float x1 = (cx - w / 2 - padX) / ratio;
                float y1 = (cy - h / 2 - padY) / ratio;
                float x2 = (cx + w / 2 - padX) / ratio;
                float y2 = (cy + h / 2 - padY) / ratio;

                // Clamp to original image bounds
                x1 = Math.Max(0, Math.Min(x1, originalWidth));
                y1 = Math.Max(0, Math.Min(y1, originalHeight));
                x2 = Math.Max(0, Math.Min(x2, originalWidth));
                y2 = Math.Max(0, Math.Min(y2, originalHeight));

                float width = x2 - x1;
                float height = y2 - y1;

                // Filter out invalid boxes
                if (width > 0 && height > 0)
                {
                    detections.Add((new BoundingBox
                    {
                        X = x1,
                        Y = y1,
                        Width = width,
                        Height = height
                    }, bestClassId, maxConfidence));
                }
            }
        }

        // Apply Non-Maximum Suppression
        var nmsResults = ApplyNMS(detections);

        foreach (var (box, classId, confidence) in nmsResults)
        {
            results.Add(new DetectionResult
            {
                ClassName = _classes[classId],
                Confidence = confidence,
                BoundingBox = box,
                DetectedAt = DateTime.Now
            });
        }

        return results;
    }

    private List<(BoundingBox box, int classId, float confidence)> ApplyNMS(
        List<(BoundingBox box, int classId, float confidence)> detections)
    {
        var result = new List<(BoundingBox, int, float)>();
        var sorted = detections.OrderByDescending(d => d.confidence).ToList();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            result.Add(best);
            sorted.RemoveAt(0);

            sorted = sorted.Where(d =>
            {
                if (d.classId != best.classId)
                    return true;

                float iou = CalculateIoU(best.box, d.box);
                return iou <= _iouThreshold;
            }).ToList();
        }

        return result;
    }

    private float CalculateIoU(BoundingBox box1, BoundingBox box2)
    {
        float x1 = Math.Max(box1.X, box2.X);
        float y1 = Math.Max(box1.Y, box2.Y);
        float x2 = Math.Min(box1.X + box1.Width, box2.X + box2.Width);
        float y2 = Math.Min(box1.Y + box1.Height, box2.Y + box2.Height);

        float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        float area1 = box1.Width * box1.Height;
        float area2 = box2.Width * box2.Height;
        float union = area1 + area2 - intersection;

        return union > 0 ? intersection / union : 0;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}

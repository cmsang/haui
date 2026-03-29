using Haui.ShapesDetector.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLaborsImage = SixLabors.ImageSharp.Image;
using SixLaborsSize = SixLabors.ImageSharp.Size;

namespace Haui.ShapesDetector.Services;

public class YoloV11DetectionService : IDetectionService, IDisposable
{
    private InferenceSession? _session;
    private string[]? _classes;
    private float _confidenceThreshold = 0.5f;
    private float _iouThreshold = 0.45f;
    private const int ModelInputSize = 640;

    public bool IsInitialized => _session != null && _classes != null;

    public async Task InitializeAsync(string modelPath, string classesPath)
    {
        try
        {
            // Load ONNX model
            var sessionOptions = new SessionOptions();
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
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
            // Load and preprocess image
            using var image = SixLaborsImage.Load<Rgb24>(imageData);
            var input = PreprocessImage(image);

            // Run inference
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", input)
            };

            using var results = _session!.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();

            // Post-process results
            var detections = PostProcess(output, originalWidth, originalHeight);
            return detections;
        });
    }

    private DenseTensor<float> PreprocessImage(SixLabors.ImageSharp.Image<Rgb24> image)
    {
        // Resize to 640x640
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new SixLaborsSize(ModelInputSize, ModelInputSize),
            Mode = ResizeMode.Stretch
        }));

        // Create tensor [1, 3, 640, 640]
        var tensor = new DenseTensor<float>(new[] { 1, 3, ModelInputSize, ModelInputSize });

        // Convert to RGB normalized [0, 1]
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < ModelInputSize; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < ModelInputSize; x++)
                {
                    tensor[0, 0, y, x] = pixelRow[x].R / 255f; // R
                    tensor[0, 1, y, x] = pixelRow[x].G / 255f; // G
                    tensor[0, 2, y, x] = pixelRow[x].B / 255f; // B
                }
            }
        });

        return tensor;
    }

    private List<DetectionResult> PostProcess(float[] output, int originalWidth, int originalHeight)
    {
        var results = new List<DetectionResult>();

        // YOLOv11 output format: [1, 84, 8400] for 80 classes
        // For custom model: [1, 4 + num_classes, num_predictions]
        int numClasses = _classes!.Length;
        int numPredictions = output.Length / (4 + numClasses);

        var detections = new List<(BoundingBox box, int classId, float confidence)>();

        for (int i = 0; i < numPredictions; i++)
        {
            // Extract box coordinates (cx, cy, w, h)
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
                // Convert from center format to corner format
                float x = (cx - w / 2) * originalWidth / ModelInputSize;
                float y = (cy - h / 2) * originalHeight / ModelInputSize;
                float width = w * originalWidth / ModelInputSize;
                float height = h * originalHeight / ModelInputSize;

                detections.Add((new BoundingBox
                {
                    X = Math.Max(0, x),
                    Y = Math.Max(0, y),
                    Width = Math.Min(width, originalWidth - x),
                    Height = Math.Min(height, originalHeight - y)
                }, bestClassId, maxConfidence));
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

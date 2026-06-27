using Haui.PCB.Models.Configuration;
using Haui.PCB.Processing.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>
/// YOLO ONNX detector with letterbox preprocessing and Ultralytics-style output decode.
/// </summary>
public sealed class OnnxYoloDetector : IOnnxYoloDetector, IDisposable
{
    private readonly object _sessionLock = new();
    private InferenceSession? _session;
    private string? _loadedModelPath;
    private string _inputName = "images";
    private int _inputWidth = ComponentDetectionSettings.DefaultInputWidth;
    private int _inputHeight = ComponentDetectionSettings.DefaultInputHeight;
    private float _confThreshold = (float)ComponentDetectionSettings.DefaultConfThreshold;
    private float _iouThreshold = (float)ComponentDetectionSettings.DefaultIouThreshold;
    private string[] _classNames = ComponentDetectionSettings.DefaultClassNames;

    public IReadOnlyList<YoloDetection> Detect(Mat board)
    {
        if (board.Empty())
            return [];

        EnsureSession();
        RefreshThresholdsFromConfig();

        var input = PrepareInput(board, out var letterbox);

        var tensor = new DenseTensor<float>(input, [1, 3, _inputHeight, _inputWidth]);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, tensor)
        };

        using var results = _session!.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();
        var outputShape = results.First().AsTensor<float>().Dimensions.ToArray();

        return Decode(output, outputShape, letterbox);
    }

    private void EnsureSession()
    {
        var settings = AppSettingsStore.LoadComponentDetection();
        var modelPath = ResolveModelPath(settings.ModelPath);

        lock (_sessionLock)
        {
            if (_session is not null
                && string.Equals(_loadedModelPath, modelPath, StringComparison.OrdinalIgnoreCase))
            {
                _inputWidth = settings.InputWidth > 0
                    ? settings.InputWidth
                    : ComponentDetectionSettings.DefaultInputWidth;
                _inputHeight = settings.InputHeight > 0
                    ? settings.InputHeight
                    : ComponentDetectionSettings.DefaultInputHeight;
                _classNames = NormalizeClassNames(settings.ClassNames);
                return;
            }

            _session?.Dispose();
            _session = new InferenceSession(modelPath);
            _loadedModelPath = modelPath;

            var inputMeta = _session.InputMetadata.First();
            _inputName = inputMeta.Key;

            _inputWidth = settings.InputWidth > 0
                ? settings.InputWidth
                : ComponentDetectionSettings.DefaultInputWidth;
            _inputHeight = settings.InputHeight > 0
                ? settings.InputHeight
                : ComponentDetectionSettings.DefaultInputHeight;
            _classNames = NormalizeClassNames(settings.ClassNames);
        }
    }

    private void RefreshThresholdsFromConfig()
    {
        var settings = AppSettingsStore.LoadComponentDetection();
        _confThreshold = (float)Math.Clamp(settings.ConfThreshold, 0.01, 0.99);
        _iouThreshold = (float)Math.Clamp(settings.IouThreshold, 0.01, 0.99);
        _classNames = NormalizeClassNames(settings.ClassNames);
    }

    private static string ResolveModelPath(string configuredPath)
    {
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? ComponentDetectionSettings.DefaultModelPath
            : configuredPath.Trim();

        if (Path.IsPathRooted(path) && File.Exists(path))
            return path;

        var fromCwd = Path.GetFullPath(path);
        if (File.Exists(fromCwd))
            return fromCwd;

        var fromBase = Path.Combine(AppContext.BaseDirectory, path);
        if (File.Exists(fromBase))
            return Path.GetFullPath(fromBase);

        throw new FileNotFoundException($"Không tìm thấy model ONNX: {path}", path);
    }

    private static string[] NormalizeClassNames(IReadOnlyList<string> names)
    {
        if (names is null || names.Count == 0)
            return ComponentDetectionSettings.DefaultClassNames;

        return names.Select(n => n.Trim()).Where(n => n.Length > 0).ToArray();
    }

    private float[] PrepareInput(Mat board, out LetterboxParams letterbox)
    {
        using var bgr = EnsureBgr(board);
        letterbox = ComputeLetterbox(bgr.Width, bgr.Height, _inputWidth, _inputHeight);

        using var resized = new Mat();
        Cv2.Resize(bgr, resized, new Size(letterbox.ResizedWidth, letterbox.ResizedHeight),
            interpolation: InterpolationFlags.Linear);

        using var padded = new Mat(new Size(_inputWidth, _inputHeight), MatType.CV_8UC3, new Scalar(114, 114, 114));
        resized.CopyTo(padded[new Rect(letterbox.PadLeft, letterbox.PadTop,
            letterbox.ResizedWidth, letterbox.ResizedHeight)]);

        // Ultralytics YOLO is trained on RGB; OpenCV Mat is BGR -> convert before tensor.
        using var rgb = new Mat();
        Cv2.CvtColor(padded, rgb, ColorConversionCodes.BGR2RGB);

        var plane = _inputWidth * _inputHeight;
        var hwc = new byte[plane * 3];
        if (rgb.IsContinuous())
        {
            System.Runtime.InteropServices.Marshal.Copy(rgb.Data, hwc, 0, hwc.Length);
        }
        else
        {
            using var clone = rgb.Clone();
            System.Runtime.InteropServices.Marshal.Copy(clone.Data, hwc, 0, hwc.Length);
        }

        // HWC (interleaved RGB) -> CHW float normalized to 0..1.
        var data = new float[plane * 3];
        for (var p = 0; p < plane; p++)
        {
            var src = p * 3;
            data[p] = hwc[src] / 255f;
            data[plane + p] = hwc[src + 1] / 255f;
            data[2 * plane + p] = hwc[src + 2] / 255f;
        }

        return data;
    }

    private List<YoloDetection> Decode(float[] output, int[] shape, LetterboxParams letterbox)
    {
        if (shape.Length < 2)
            return [];

        int dim1 = shape[^2];
        int dim2 = shape[^1];
        int numClasses = _classNames.Length;

        // End-to-end (NMS-free) export, e.g. YOLO26/YOLOv10: [1, N, 6]
        // each row = [x1, y1, x2, y2, confidence, classId], already decoded + NMS.
        if (dim2 == 6 && dim1 != 4 + numClasses)
            return DecodeEndToEnd(output, dim1, letterbox);

        return DecodeRaw(output, shape, letterbox);
    }

    private List<YoloDetection> DecodeEndToEnd(float[] output, int numDet, LetterboxParams letterbox)
    {
        var results = new List<YoloDetection>();
        for (var i = 0; i < numDet; i++)
        {
            var o = i * 6;
            var confidence = output[o + 4];
            if (confidence < _confThreshold)
                continue;

            var classId = (int)MathF.Round(output[o + 5]);
            var box = MapToSource(output[o], output[o + 1], output[o + 2], output[o + 3], letterbox);
            if (box.Width <= 0 || box.Height <= 0)
                continue;

            var label = classId >= 0 && classId < _classNames.Length
                ? _classNames[classId]
                : $"class_{classId}";
            results.Add(new YoloDetection(classId, label, confidence, box));
        }

        return results;
    }

    private List<YoloDetection> DecodeRaw(float[] output, int[] shape, LetterboxParams letterbox)
    {
        // Ultralytics raw export: [1, 4+nc, N] or [1, N, 4+nc]
        int dim1 = shape[^2];
        int dim2 = shape[^1];
        bool channelsFirst = dim1 == 4 + _classNames.Length || dim1 < dim2;

        int numClasses = _classNames.Length;
        int numBoxes = channelsFirst ? dim2 : dim1;
        int channels = channelsFirst ? dim1 : dim2;

        if (channels < 4 + numClasses)
            numClasses = channels - 4;

        var candidates = new List<(Rect Box, int ClassId, float Score)>();

        for (var i = 0; i < numBoxes; i++)
        {
            float cx, cy, w, h;
            if (channelsFirst)
            {
                cx = output[0 * numBoxes + i];
                cy = output[1 * numBoxes + i];
                w = output[2 * numBoxes + i];
                h = output[3 * numBoxes + i];
            }
            else
            {
                var offset = i * channels;
                cx = output[offset + 0];
                cy = output[offset + 1];
                w = output[offset + 2];
                h = output[offset + 3];
            }

            var bestClass = -1;
            var bestScore = 0f;
            for (var c = 0; c < numClasses; c++)
            {
                float score = channelsFirst
                    ? output[(4 + c) * numBoxes + i]
                    : output[i * channels + 4 + c];

                if (score > bestScore)
                {
                    bestScore = score;
                    bestClass = c;
                }
            }

            if (bestScore < _confThreshold || bestClass < 0)
                continue;

            var x1 = cx - w / 2f;
            var y1 = cy - h / 2f;
            var x2 = cx + w / 2f;
            var y2 = cy + h / 2f;

            var mapped = MapToSource(x1, y1, x2, y2, letterbox);
            if (mapped.Width <= 0 || mapped.Height <= 0)
                continue;

            candidates.Add((mapped, bestClass, bestScore));
        }

        return ApplyNms(candidates);
    }

    private List<YoloDetection> ApplyNms(List<(Rect Box, int ClassId, float Score)> candidates)
    {
        if (candidates.Count == 0)
            return [];

        var indices = NonMaxSuppression(candidates, _iouThreshold);
        var results = new List<YoloDetection>(indices.Count);
        foreach (var idx in indices)
        {
            var item = candidates[idx];
            var label = item.ClassId < _classNames.Length
                ? _classNames[item.ClassId]
                : $"class_{item.ClassId}";
            results.Add(new YoloDetection(item.ClassId, label, item.Score, item.Box));
        }

        return results;
    }

    private static List<int> NonMaxSuppression(
        List<(Rect Box, int ClassId, float Score)> candidates,
        float iouThreshold)
    {
        var ordered = candidates
            .Select((c, i) => (Index: i, c.Score))
            .OrderByDescending(x => x.Score)
            .Select(x => x.Index)
            .ToList();

        var kept = new List<int>();
        var suppressed = new bool[candidates.Count];

        foreach (var idx in ordered)
        {
            if (suppressed[idx]) continue;
            kept.Add(idx);

            var boxA = candidates[idx].Box;
            for (var j = 0; j < candidates.Count; j++)
            {
                if (suppressed[j] || j == idx) continue;
                if (ComputeIoU(boxA, candidates[j].Box) > iouThreshold)
                    suppressed[j] = true;
            }
        }

        return kept;
    }

    private static float ComputeIoU(Rect a, Rect b)
    {
        var x1 = Math.Max(a.X, b.X);
        var y1 = Math.Max(a.Y, b.Y);
        var x2 = Math.Min(a.Right, b.Right);
        var y2 = Math.Min(a.Bottom, b.Bottom);

        var interW = Math.Max(0, x2 - x1);
        var interH = Math.Max(0, y2 - y1);
        var inter = interW * interH;
        if (inter <= 0) return 0;

        var union = a.Width * a.Height + b.Width * b.Height - inter;
        return union <= 0 ? 0 : inter / (float)union;
    }

    private static Rect MapToSource(float x1, float y1, float x2, float y2, LetterboxParams lb)
    {
        var left = (x1 - lb.PadLeft) / lb.Scale;
        var top = (y1 - lb.PadTop) / lb.Scale;
        var right = (x2 - lb.PadLeft) / lb.Scale;
        var bottom = (y2 - lb.PadTop) / lb.Scale;

        var ix1 = (int)Math.Floor(left);
        var iy1 = (int)Math.Floor(top);
        var ix2 = (int)Math.Ceiling(right);
        var iy2 = (int)Math.Ceiling(bottom);

        ix1 = Math.Clamp(ix1, 0, lb.SourceWidth - 1);
        iy1 = Math.Clamp(iy1, 0, lb.SourceHeight - 1);
        ix2 = Math.Clamp(ix2, ix1 + 1, lb.SourceWidth);
        iy2 = Math.Clamp(iy2, iy1 + 1, lb.SourceHeight);

        return new Rect(ix1, iy1, ix2 - ix1, iy2 - iy1);
    }

    private static LetterboxParams ComputeLetterbox(int srcW, int srcH, int dstW, int dstH)
    {
        var scale = Math.Min(dstW / (double)srcW, dstH / (double)srcH);
        var resizedW = (int)Math.Round(srcW * scale);
        var resizedH = (int)Math.Round(srcH * scale);
        var padLeft = (dstW - resizedW) / 2;
        var padTop = (dstH - resizedH) / 2;

        return new LetterboxParams(srcW, srcH, scale, resizedW, resizedH, padLeft, padTop);
    }

    private static Mat EnsureBgr(Mat source)
    {
        if (source.Channels() == 3)
            return source.Clone();

        var bgr = new Mat();
        Cv2.CvtColor(source, bgr, ColorConversionCodes.GRAY2BGR);
        return bgr;
    }

    public void Dispose()
    {
        lock (_sessionLock)
        {
            _session?.Dispose();
            _session = null;
            _loadedModelPath = null;
        }
    }

    private readonly record struct LetterboxParams(
        int SourceWidth,
        int SourceHeight,
        double Scale,
        int ResizedWidth,
        int ResizedHeight,
        int PadLeft,
        int PadTop);
}

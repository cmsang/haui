using System.IO;
using Haui.GTObjectDetector.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Tùy chọn tiền xử lý ảnh trước khi đưa vào YOLO.
/// </summary>
public sealed class PreprocessOptions
{
    /// <summary>Bật CLAHE để tăng độ tương phản cục bộ (hữu ích với camera thiếu sáng).</summary>
    public bool EnableClahe { get; set; } = true;

    /// <summary>Giới hạn clip của CLAHE (càng cao càng tăng sáng mạnh, mặc định 2.0).</summary>
    public double ClaheClipLimit { get; set; } = 2.0;

    /// <summary>Bật Gaussian blur nhẹ để khử nhiễu trước khi detect.</summary>
    public bool EnableDenoise { get; set; } = true;

    /// <summary>Kích thước kernel Gaussian (phải lẻ, mặc định 3).</summary>
    public int DenoiseKernelSize { get; set; } = 3;

    /// <summary>
    /// Chuyển ảnh sang grayscale trước khi đưa vào model.
    /// Hữu ích khi môi trường thiếu sáng hoặc màu sắc không mang thông tin phân biệt.
    /// Ảnh sẽ được nhân 3 kênh giống nhau để giữ đúng định dạng [1,3,H,W] của YOLO.
    /// </summary>
    public bool EnableGrayscale { get; set; } = false;
}

/// <summary>
/// Thông tin letterbox dùng để map tọa độ output → ảnh gốc.
/// </summary>
internal readonly struct LetterboxInfo
{
    /// <summary>Tỉ lệ scale đồng nhất áp dụng cho cả chiều.</summary>
    public float Scale { get; init; }

    /// <summary>Số pixel padding bên trái (theo chiều ngang).</summary>
    public float PadLeft { get; init; }

    /// <summary>Số pixel padding bên trên (theo chiều dọc).</summary>
    public float PadTop { get; init; }
}

/// <summary>
/// Dịch vụ nhận diện đối tượng sử dụng model YOLOv8/v11 định dạng ONNX.
/// Hỗ trợ đầu vào [1, 3, H, W] và đầu ra [1, 4+numClasses, numAnchors].
/// </summary>
public sealed class YoloDetectionService : IObjectDetectionService
{
    private readonly InferenceSession _session;
    private readonly string[] _classNames;
    private readonly int _inputWidth;
    private readonly int _inputHeight;

    // Màu fill letterbox — giống giá trị chuẩn Ultralytics (114, 114, 114)
    private static readonly Scalar LetterboxFill = new(114, 114, 114);

    // Màu sắc cho từng class (BGR)
    private static readonly Scalar[] ClassColors =
    [
        new Scalar(0,   128, 255),  // car        — cam
        new Scalar(255, 0,   128),  // van        — hồng
        new Scalar(0,   255, 128),  // truck      — xanh lá nhạt
        new Scalar(128, 0,   255),  // pedestrian — tím
        new Scalar(0,   255, 255),  // person     — vàng
        new Scalar(255, 128, 0),    // cyclist    — xanh dương nhạt
        new Scalar(128, 255, 0),    // tram       — xanh lá đậm
        new Scalar(255, 255, 0),    // misc       — cyan
    ];

    private bool _disposed;

    /// <param name="modelPath">Đường dẫn tới file best.onnx.</param>
    /// <param name="classFilePath">Đường dẫn tới file object_class.txt.</param>
    /// <param name="inputWidth">Chiều rộng đầu vào model (mặc định 640).</param>
    /// <param name="inputHeight">Chiều cao đầu vào model (mặc định 640).</param>
    public YoloDetectionService(string modelPath, string classFilePath, int inputWidth = 640, int inputHeight = 640)
    {
        _inputWidth  = inputWidth;
        _inputHeight = inputHeight;
        _classNames  = File.ReadAllLines(classFilePath)
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToArray();

        var options = new SessionOptions();
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        _session = new InferenceSession(modelPath, options);
    }

    /// <inheritdoc/>
    public IReadOnlyList<DetectionResult> Detect(
        Mat frame,
        float confidenceThreshold = 0.45f,
        float nmsThreshold        = 0.45f,
        PreprocessOptions?        preprocessOptions = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        preprocessOptions ??= new PreprocessOptions();
        int origW = frame.Width;
        int origH = frame.Height;

        // ── 1. Tiền xử lý ─────────────────────────────────────────────────
        using var preprocessed = Preprocess(frame, preprocessOptions);

        // ── 2. Letterbox resize (giữ tỉ lệ + padding màu xám) ─────────────
        // Letterbox là bước BẮT BUỘC của pipeline YOLO chuẩn:
        // resize ảnh về kích thước model nhưng giữ nguyên aspect ratio,
        // phần thừa được lấp bằng màu (114,114,114) thay vì kéo méo ảnh.
        using var letterboxed = new Mat();
        var lbInfo = Letterbox(preprocessed, letterboxed, _inputWidth, _inputHeight);

        // ── 3. Normalize + chuyển sang tensor [1,3,H,W] ───────────────────
        var inputTensor = MatToTensor(letterboxed, _inputWidth, _inputHeight);

        // ── 4. Chạy inference ──────────────────────────────────────────────
        string inputName = _session.InputMetadata.Keys.First();
        using var inputs = new DisposableNamedOnnxValueList(
        [
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        ]);

        using var outputs = _session.Run(inputs);
        var outputTensor = outputs.First().AsTensor<float>();

        // ── 5. Phân tích đầu ra YOLO: shape [1, 4+numClasses, numAnchors] ─
        var shape      = outputTensor.Dimensions;
        int numAnchors = shape[2];      // tổng số anchor boxes
        int numClasses = shape[1] - 4;  // loại bỏ 4 giá trị tọa độ

        // Chỉ xét đúng số class đã định nghĩa trong file,
        // tránh nhận diện class "lạ" mà model export thừa ra
        int validClassCount = Math.Min(numClasses, _classNames.Length);

        var boxes    = new List<Rect2d>();
        var scores   = new List<float>();
        var classIds = new List<int>();

        for (int a = 0; a < numAnchors; a++)
        {
            // Tìm class có score cao nhất trong anchor này,
            // chỉ xét trong phạm vi validClassCount
            float maxScore  = 0f;
            int   bestClass = 0;
            for (int c = 0; c < validClassCount; c++)
            {
                float s = outputTensor[0, 4 + c, a];
                if (s > maxScore) { maxScore = s; bestClass = c; }
            }

            if (maxScore < confidenceThreshold) continue;

            float cx = outputTensor[0, 0, a];
            float cy = outputTensor[0, 1, a];
            float bw = outputTensor[0, 2, a];
            float bh = outputTensor[0, 3, a];

            // Map tọa độ từ input-space về ảnh gốc qua letterbox info:
            // x_orig = (x_input - padLeft) / scale
            double x = ((cx - bw / 2f) - lbInfo.PadLeft) / lbInfo.Scale;
            double y = ((cy - bh / 2f) - lbInfo.PadTop)  / lbInfo.Scale;
            double w = bw / lbInfo.Scale;
            double h = bh / lbInfo.Scale;

            boxes.Add(new Rect2d(x, y, w, h));
            scores.Add(maxScore);
            classIds.Add(bestClass);
        }

        if (boxes.Count == 0) return [];

        // ── 6. Non-Maximum Suppression ─────────────────────────────────────
        CvDnn.NMSBoxes(boxes, scores, confidenceThreshold, nmsThreshold, out int[] indices);

        var results = new List<DetectionResult>(indices.Length);
        foreach (int idx in indices)
        {
            var b = boxes[idx];

            // Clamp về kích thước ảnh gốc
            int rx = Math.Max(0, (int)Math.Round(b.X));
            int ry = Math.Max(0, (int)Math.Round(b.Y));
            int rw = Math.Min((int)Math.Round(b.Width),  origW - rx);
            int rh = Math.Min((int)Math.Round(b.Height), origH - ry);

            if (rw <= 0 || rh <= 0) continue;

            int    cid   = classIds[idx];

            // Bỏ qua nếu class index vượt ngoài danh sách đã định nghĩa
            if (cid >= _classNames.Length) continue;

            string label = _classNames[cid];

            results.Add(new DetectionResult
            {
                BoundingBox = new Rect(rx, ry, rw, rh),
                Label       = label,
                Confidence  = scores[idx],
                ClassId     = cid,
            });
        }

        return results;
    }

    /// <summary>
    /// Vẽ kết quả nhận diện lên frame (bounding box + nhãn).
    /// </summary>
    public static void DrawDetections(Mat frame, IReadOnlyList<DetectionResult> detections)
    {
        foreach (var det in detections)
        {
            var color = det.ClassId < ClassColors.Length
                ? ClassColors[det.ClassId]
                : new Scalar(0, 255, 0);

            // Vẽ bounding box dày 2px
            Cv2.Rectangle(frame, det.BoundingBox, color, 2);

            // Nhãn: "car 0.87"
            string text = $"{det.Label} {det.Confidence:F2}";

            int baseLine = 0;
            var textSize = Cv2.GetTextSize(text, HersheyFonts.HersheySimplex, 0.55, 1, out baseLine);

            // Vị trí nền nhãn (trên cạnh trên bounding box)
            var labelRect = new Rect(
                det.BoundingBox.X,
                det.BoundingBox.Y - textSize.Height - baseLine - 4,
                textSize.Width + 6,
                textSize.Height + baseLine + 4);

            // Đảm bảo nền nhãn không vượt ra ngoài ảnh
            labelRect.X = Math.Max(0, labelRect.X);
            labelRect.Y = Math.Max(0, labelRect.Y);

            Cv2.Rectangle(frame, labelRect, color, -1);
            Cv2.PutText(
                frame,
                text,
                new Point(labelRect.X + 3, labelRect.Y + textSize.Height + 2),
                HersheyFonts.HersheySimplex,
                0.55,
                new Scalar(255, 255, 255),
                1,
                LineTypes.AntiAlias);
        }
    }

    // ── Tiền xử lý ảnh ───────────────────────────────────────────────────────

    /// <summary>
    /// Bước tiền xử lý trước letterbox:
    /// 1) CLAHE — tăng độ tương phản cục bộ (giúp nhận diện tốt hơn khi thiếu sáng)
    /// 2) Gaussian denoising — khử nhiễu nhẹ để giảm false positive
    /// </summary>
    private static Mat Preprocess(Mat src, PreprocessOptions opts)
    {
        var result = src.Clone();

        // ── Bước 1: CLAHE trên kênh Luminance (không làm lệch màu sắc) ────
        if (opts.EnableClahe)
        {
            // Chuyển sang LAB để chỉ xử lý kênh L (độ sáng)
            using var lab = new Mat();
            Cv2.CvtColor(result, lab, ColorConversionCodes.BGR2Lab);

            Cv2.Split(lab, out Mat[] labChannels);
            using var l = labChannels[0];
            using var a = labChannels[1];
            using var b = labChannels[2];

            // Áp dụng CLAHE lên kênh L
            using var clahe   = Cv2.CreateCLAHE(opts.ClaheClipLimit, new Size(8, 8));
            using var lClahe  = new Mat();
            clahe.Apply(l, lClahe);

            // Ghép lại và chuyển về BGR
            using var labMerged = new Mat();
            Cv2.Merge([lClahe, a, b], labMerged);
            Cv2.CvtColor(labMerged, result, ColorConversionCodes.Lab2BGR);

            foreach (var ch in labChannels) ch.Dispose();
        }

        // ── Bước 2: Gaussian blur nhẹ để khử nhiễu muối tiêu ───────────────
        if (opts.EnableDenoise)
        {
            // Đảm bảo kernel lẻ tối thiểu 3
            int k = Math.Max(3, opts.DenoiseKernelSize | 1);
            Cv2.GaussianBlur(result, result, new Size(k, k), sigmaX: 0);
        }

        // ── Bước 3: Chuyển sang Grayscale rồi nhân lại thành BGR 3 kênh ────
        // Thực hiện SAU CLAHE/Denoise vì CLAHE cần ảnh màu (kênh LAB).
        // Nhân kênh gray → BGR để tensor đầu vào vẫn đúng shape [1,3,H,W].
        if (opts.EnableGrayscale)
        {
            using var gray = new Mat();
            Cv2.CvtColor(result, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(gray, result, ColorConversionCodes.GRAY2BGR);
        }

        return result;
    }

    /// <summary>
    /// Letterbox resize: thu/phóng ảnh về <paramref name="targetW"/>×<paramref name="targetH"/>
    /// giữ nguyên tỉ lệ khung hình, lấp vùng thừa bằng màu xám (114,114,114).
    /// Trả về <see cref="LetterboxInfo"/> để map tọa độ ngược lại.
    /// </summary>
    private static LetterboxInfo Letterbox(Mat src, Mat dst, int targetW, int targetH)
    {
        float scale = Math.Min((float)targetW / src.Width, (float)targetH / src.Height);

        int scaledW = (int)Math.Round(src.Width  * scale);
        int scaledH = (int)Math.Round(src.Height * scale);

        // Padding đều hai phía (letterbox chuẩn Ultralytics)
        float padLeft = (targetW - scaledW) / 2f;
        float padTop  = (targetH - scaledH) / 2f;

        int padL = (int)Math.Floor(padLeft);
        int padT = (int)Math.Floor(padTop);
        int padR = targetW - scaledW - padL;
        int padB = targetH - scaledH - padT;

        // Resize về kích thước đã scale
        using var scaled = new Mat();
        Cv2.Resize(src, scaled, new Size(scaledW, scaledH), interpolation: InterpolationFlags.Linear);

        // Thêm padding bốn phía
        Cv2.CopyMakeBorder(scaled, dst, padT, padB, padL, padR,
            BorderTypes.Constant, LetterboxFill);

        return new LetterboxInfo
        {
            Scale   = scale,
            PadLeft = padLeft,
            PadTop  = padTop,
        };
    }

    // ── Chuyển Mat (BGR) sang DenseTensor [1,3,H,W] ──────────────────────────

    /// <summary>
    /// Normalize pixel về [0,1] và sắp xếp sang tensor NCHW theo thứ tự kênh RGB.
    /// </summary>
    private static DenseTensor<float> MatToTensor(Mat mat, int w, int h)
    {
        var tensor = new DenseTensor<float>([1, 3, h, w]);

        using var floatMat = new Mat();
        mat.ConvertTo(floatMat, MatType.CV_32FC3, 1.0 / 255.0);

        // Split BGR → [B, G, R]; YOLO cần thứ tự RGB nên đảo kênh
        Cv2.Split(floatMat, out Mat[] mats);
        for (int c = 0; c < 3; c++)
        {
            // c=0 → R (mats[2]), c=1 → G (mats[1]), c=2 → B (mats[0])
            using var ch = mats[2 - c];
            ch.GetArray(out float[] raw);
            for (int i = 0; i < h * w; i++)
                tensor[0, c, i / w, i % w] = raw[i];
        }

        foreach (var m in mats) m.Dispose();
        return tensor;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _session.Dispose();
        _disposed = true;
    }
}

/// <summary>Helper để dispose danh sách NamedOnnxValue.</summary>
file sealed class DisposableNamedOnnxValueList : List<NamedOnnxValue>, IDisposable
{
    public DisposableNamedOnnxValueList(IEnumerable<NamedOnnxValue> items) : base(items) { }
    public void Dispose() { /* NamedOnnxValue không IDisposable */ }
}

using Haui.GTObjectDetector.Models;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Giao diện dịch vụ nhận diện đối tượng bằng model YOLO ONNX.
/// </summary>
public interface IObjectDetectionService : IDisposable
{
    /// <summary>
    /// Nhận diện đối tượng trong frame, trả về danh sách kết quả.
    /// </summary>
    /// <param name="frame">Frame đầu vào (BGR).</param>
    /// <param name="confidenceThreshold">Ngưỡng tin cậy tối thiểu.</param>
    /// <param name="nmsThreshold">Ngưỡng NMS (Non-Maximum Suppression).</param>
    /// <param name="preprocessOptions">Tùy chọn tiền xử lý ảnh (null = dùng mặc định).</param>
    IReadOnlyList<DetectionResult> Detect(
        Mat frame,
        float confidenceThreshold  = 0.45f,
        float nmsThreshold         = 0.45f,
        PreprocessOptions? preprocessOptions = null);
}

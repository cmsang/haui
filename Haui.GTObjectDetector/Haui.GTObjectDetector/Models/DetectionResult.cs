using OpenCvSharp;

namespace Haui.GTObjectDetector.Models;

/// <summary>
/// Kết quả nhận diện một đối tượng từ model YOLO.
/// </summary>
public sealed record DetectionResult
{
    /// <summary>Bounding box (tọa độ pixel trên ảnh gốc).</summary>
    public Rect BoundingBox { get; init; }

    /// <summary>Nhãn class (ví dụ: "car", "person", ...).</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Độ tin cậy từ 0.0 đến 1.0.</summary>
    public float Confidence { get; init; }

    /// <summary>Index class trong danh sách.</summary>
    public int ClassId { get; init; }
}

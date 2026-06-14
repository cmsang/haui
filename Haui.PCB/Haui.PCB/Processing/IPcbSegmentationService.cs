using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện trừu tượng cho dịch vụ phân vùng bo mạch PCB.
/// Tuân theo Dependency Inversion Principle.
/// </summary>
public interface IPcbSegmentationService
{
    /// <summary>
    /// Phân vùng và trả về ảnh bo mạch đã cắt từ ảnh gốc.
    /// Trả về null nếu không tìm thấy bo mạch.
    /// </summary>
    Mat? Segment(Mat source);
}

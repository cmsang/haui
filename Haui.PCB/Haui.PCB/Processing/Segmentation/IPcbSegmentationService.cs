using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Giao diện trừu tượng cho dịch vụ phân vùng bo mạch PCB.
/// Tuân theo Dependency Inversion Principle.
/// </summary>
public interface IPcbSegmentationService
{
    /// <summary>
    /// Chạy toàn bộ pipeline và trả về ảnh trung gian từng bước. Caller phải Dispose kết quả.
    /// </summary>
    SegmentationPipelineResult RunPipeline(Mat source);

    /// <summary>
    /// Phân vùng theo khung hộp đỡ và trả về ảnh bo mạch đã cắt từ ảnh gốc.
    /// Trả về null nếu không tìm thấy khung hộp đỡ hợp lệ.
    /// </summary>
    Mat? Segment(Mat source);
}

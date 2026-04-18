using Haui.GarlicDetector.Models;
using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Phân vùng ảnh đã tiền xử lý để xác định vị trí các vùng tỏi.
/// </summary>
public interface IGarlicSegmentor
{
    /// <summary>
    /// Phân tích frame đã tiền xử lý (e.g. HSV) và trả về danh sách
    /// kết quả phân vùng (bounding rect + diện tích contour + độ tròn).
    /// </summary>
    /// <param name="preprocessedFrame">Frame đã qua IImagePreprocessor — không bị thay đổi.</param>
    List<GarlicSegmentResult> Segment(Mat preprocessedFrame);
}

using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Phân vùng ảnh đã tiền xử lý để xác định vị trí các vùng tỏi.
/// </summary>
public interface IGarlicSegmentor
{
    /// <summary>
    /// Phân tích frame đã tiền xử lý (e.g. HSV) và trả về danh sách bounding box của các vùng tỏi.
    /// </summary>
    /// <param name="preprocessedFrame">Frame đã qua IImagePreprocessor — không bị thay đổi.</param>
    List<Rect> Segment(Mat preprocessedFrame);
}

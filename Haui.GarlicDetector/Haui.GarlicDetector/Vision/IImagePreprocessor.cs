using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Tiền xử lý ảnh BGR đầu vào trước khi đưa vào thuật toán phân vùng.
/// Caller chịu trách nhiệm Dispose() Mat trả về.
/// </summary>
public interface IImagePreprocessor
{
    /// <summary>Nhận ảnh BGR từ camera và trả về ảnh đã được tiền xử lý (e.g. HSV, Lab).</summary>
    /// <param name="bgrFrame">Frame BGR gốc — không bị thay đổi.</param>
    /// <returns>Mat mới đã tiền xử lý; caller phải Dispose() sau khi dùng xong.</returns>
    Mat Preprocess(Mat bgrFrame);
}

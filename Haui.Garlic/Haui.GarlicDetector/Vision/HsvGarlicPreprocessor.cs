using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Tiền xử lý ảnh BGR: làm mịn Gaussian (tùy chọn) → chuyển không gian màu BGR → HSV.
/// </summary>
public sealed class HsvGarlicPreprocessor : IImagePreprocessor
{
    /// <summary>
    /// Kích thước kernel Gaussian Blur (phải là số lẻ ≥ 1).
    /// Đặt thành 0 hoặc 1 để bỏ qua bước làm mịn.
    /// </summary>
    public int BlurKernelSize { get; set; } = 5;

    /// <inheritdoc />
    public Mat Preprocess(Mat bgrFrame)
    {
        var hsvResult = new Mat();

        // Làm mịn trước khi chuyển màu để giảm nhiễu salt-and-pepper
        if (BlurKernelSize > 1)
        {
            using var blurred = new Mat();
            var ksize = new OpenCvSharp.Size(BlurKernelSize, BlurKernelSize);
            Cv2.GaussianBlur(bgrFrame, blurred, ksize, sigmaX: 0);
            Cv2.CvtColor(blurred, hsvResult, ColorConversionCodes.BGR2HSV);
        }
        else
        {
            Cv2.CvtColor(bgrFrame, hsvResult, ColorConversionCodes.BGR2HSV);
        }

        return hsvResult;
    }
}

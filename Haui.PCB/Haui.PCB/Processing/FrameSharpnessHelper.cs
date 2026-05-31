using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Đo độ sắc nét frame bằng phương sai Laplacian.
/// </summary>
internal static class FrameSharpnessHelper
{
    public static double ComputeLaplacianVariance(Mat src)
    {
        using var gray = new Mat();
        using var lap = new Mat();

        if (src.Channels() > 1)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        else
            src.CopyTo(gray);

        Cv2.Laplacian(gray, lap, MatType.CV_64F);
        Cv2.MeanStdDev(lap, out _, out var stdDev);
        double sigma = stdDev.Val0;
        return sigma * sigma;
    }
}

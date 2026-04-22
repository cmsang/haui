using OpenCvSharp;

namespace Haui.GarlicDetector.ML;

/// <summary>
/// Trích xuất vector đặc trưng từ một vùng ảnh tỏi (Mat BGR).
/// </summary>
public interface IFeatureExtractor
{
    /// <summary>
    /// Trích xuất đặc trưng từ <paramref name="roi"/> và trả về vector float.
    /// </summary>
    /// <param name="roi">Ảnh BGR của vùng tỏi (sẽ được resize nội bộ).</param>
    float[] Extract(Mat roi);

    /// <summary>Số chiều của vector đặc trưng đầu ra.</summary>
    int FeatureDimension { get; }
}

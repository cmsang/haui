using OpenCvSharp;
using OcvSize = OpenCvSharp.Size;

namespace Haui.GarlicDetector.ML;

/// <summary>
/// Trích xuất đặc trưng tỏi gồm: HOG (texture), 7 Hu Moments (shape),
/// mean/std HSV (color health). Tổng số chiều = <see cref="FeatureDimension"/>.
/// </summary>
public sealed class GarlicFeatureExtractor : IFeatureExtractor
{
    // ─── Thông số HOG ────────────────────────────────────────────────────────
    // winSize = imageSize x imageSize; blockSize=16x16; blockStride=8x8;
    // cellSize=8x8; nbins=9

    private readonly int _imageSize;

    // HOG descriptor size = ((W-bW)/bS+1)^2 * (bW/cW)^2 * nbins
    //                     = 7*7 * 4 * 9  = 1764  (khi imageSize=64)
    private readonly int _hogDim;

    // 7 Hu Moments + 6 color stats (mean/std H,S,V)
    private const int ExtraDim = 7 + 6;

    /// <inheritdoc/>
    public int FeatureDimension => _hogDim + ExtraDim;

    public GarlicFeatureExtractor(int imageSize = 64)
    {
        _imageSize = imageSize;

        int blockW   = 16, blockStride = 8, cellSize = 8, nbins = 9;
        int numBlock = (_imageSize - blockW) / blockStride + 1;
        _hogDim      = numBlock * numBlock * (blockW / cellSize) * (blockW / cellSize) * nbins;
    }

    /// <inheritdoc/>
    public float[] Extract(Mat roi)
    {
        using var resized = new Mat();
        Cv2.Resize(roi, resized, new OcvSize(_imageSize, _imageSize));

        var features = new float[FeatureDimension];
        int offset   = 0;

        // ── HOG ─────────────────────────────────────────────────────────────
        using var gray = new Mat();
        Cv2.CvtColor(resized, gray, ColorConversionCodes.BGR2GRAY);

        var hog    = new HOGDescriptor(
            winSize:     new OcvSize(_imageSize, _imageSize),
            blockSize:   new OcvSize(16, 16),
            blockStride: new OcvSize(8, 8),
            cellSize:    new OcvSize(8, 8),
            nbins:       9);

        float[] hogDesc = hog.Compute(gray);
        hogDesc.CopyTo(features, offset);
        offset += hogDesc.Length;

        // ── Hu Moments ───────────────────────────────────────────────────────
        // Moments là struct, không phải IDisposable — không dùng using
        var moments = Cv2.Moments(gray);
        double[] hu = ComputeHuMoments(moments);

        for (int i = 0; i < 7; i++)
        {
            // Log-transform để cân bằng magnitude
            double v = hu[i] == 0 ? 0 : -Math.Sign(hu[i]) * Math.Log10(Math.Abs(hu[i]));
            features[offset + i] = (float)v;
        }
        offset += 7;

        // ── Color stats trong HSV (mean/std của H, S, V) ─────────────────────
        using var hsv = new Mat();
        Cv2.CvtColor(resized, hsv, ColorConversionCodes.BGR2HSV);

        Cv2.MeanStdDev(hsv, out Scalar mean, out Scalar std);
        // mean và std có 3 kênh H, S, V
        features[offset + 0] = (float)mean.Val0;   // mean H
        features[offset + 1] = (float)mean.Val1;   // mean S
        features[offset + 2] = (float)mean.Val2;   // mean V
        features[offset + 3] = (float)std.Val0;    // std H
        features[offset + 4] = (float)std.Val1;    // std S
        features[offset + 5] = (float)std.Val2;    // std V

        return features;
    }

    // ─── Tính 7 Hu Moments thủ công từ central moments ───────────────────────
    // (Cv2.HuMoments không available trong một số phiên bản OpenCvSharp4)
    private static double[] ComputeHuMoments(Moments m)
    {
        double n20 = m.M20 / Math.Pow(m.M00, 2.0);
        double n02 = m.M02 / Math.Pow(m.M00, 2.0);
        double n11 = m.M11 / Math.Pow(m.M00, 2.0);
        double n30 = m.M30 / Math.Pow(m.M00, 2.5);
        double n03 = m.M03 / Math.Pow(m.M00, 2.5);
        double n21 = m.M21 / Math.Pow(m.M00, 2.5);
        double n12 = m.M12 / Math.Pow(m.M00, 2.5);

        double[] hu = new double[7];
        hu[0] = n20 + n02;
        hu[1] = (n20 - n02) * (n20 - n02) + 4 * n11 * n11;
        hu[2] = (n30 - 3 * n12) * (n30 - 3 * n12) + (3 * n21 - n03) * (3 * n21 - n03);
        hu[3] = (n30 + n12) * (n30 + n12) + (n21 + n03) * (n21 + n03);
        hu[4] = (n30 - 3 * n12) * (n30 + n12) * ((n30 + n12) * (n30 + n12) - 3 * (n21 + n03) * (n21 + n03))
              + (3 * n21 - n03) * (n21 + n03) * (3 * (n30 + n12) * (n30 + n12) - (n21 + n03) * (n21 + n03));
        hu[5] = (n20 - n02) * ((n30 + n12) * (n30 + n12) - (n21 + n03) * (n21 + n03))
              + 4 * n11 * (n30 + n12) * (n21 + n03);
        hu[6] = (3 * n21 - n03) * (n30 + n12) * ((n30 + n12) * (n30 + n12) - 3 * (n21 + n03) * (n21 + n03))
              - (n30 - 3 * n12) * (n21 + n03) * (3 * (n30 + n12) * (n30 + n12) - (n21 + n03) * (n21 + n03));
        return hu;
    }
}

using OpenCvSharp;
using OcvSize = OpenCvSharp.Size;

namespace Haui.GarlicDetector.ML;

/// <summary>
/// Trích xuất đặc trưng tỏi gồm 4 nhóm:
/// (1) Color ~20, (2) LBP ~59, (3) Shape ~6, (4) Statistical ~12.
/// Tổng ~97 chiều, min-max normalized về [0, 1].
/// </summary>
public sealed class GarlicFeatureExtractor : IFeatureExtractor
{
    private readonly int _imageSize;

    // ~20 + ~59 + ~6 + ~12 = ~97 features
    private const int ColorDim = 20;
    private const int LbpDim   = 59;
    private const int ShapeDim = 6;
    private const int StatDim  = 12;

    /// <inheritdoc/>
    public int FeatureDimension => ColorDim + LbpDim + ShapeDim + StatDim;

    public GarlicFeatureExtractor(int imageSize = 64)
    {
        _imageSize = imageSize;
    }

    /// <inheritdoc/>
    public float[] Extract(Mat roi)
    {
        using var resized = new Mat();
        Cv2.Resize(roi, resized, new OcvSize(_imageSize, _imageSize));

        var raw = new List<double>(FeatureDimension);
        raw.AddRange(ExtractColorFeatures(resized));
        raw.AddRange(ExtractLbpFeatures(resized));
        raw.AddRange(ExtractShapeFeatures(resized));
        raw.AddRange(ExtractStatFeatures(resized));

        return MinMaxNormalize(raw);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. ĐẶC TRƯNG MÀU SẮC (~20 features)
    // ═══════════════════════════════════════════════════════════════════════
    private double[] ExtractColorFeatures(Mat img)
    {
        var f = new List<double>();

        // ── HSV mean/std ─────────────────────────────────────────────────
        using var hsv = new Mat();
        Cv2.CvtColor(img, hsv, ColorConversionCodes.BGR2HSV);
        Cv2.MeanStdDev(hsv, out Scalar hsvMean, out Scalar hsvStd);
        f.Add(hsvMean.Val0 / 180.0); // H mean
        f.Add(hsvMean.Val1 / 255.0); // S mean
        f.Add(hsvMean.Val2 / 255.0); // V mean
        f.Add(hsvStd.Val0  / 180.0); // H std
        f.Add(hsvStd.Val1  / 255.0); // S std
        f.Add(hsvStd.Val2  / 255.0); // V std

        // ── Tỉ lệ pixel màu bất thường ──────────────────────────────────
        // Màu xanh lá (thối xanh): H = 35–85
        using var greenMask = new Mat();
        Cv2.InRange(hsv, new Scalar(35, 40, 40), new Scalar(85, 255, 255), greenMask);
        f.Add(GetPixelRatio(greenMask));

        // Màu tím/xanh dương: H = 100–160
        using var purpleMask = new Mat();
        Cv2.InRange(hsv, new Scalar(100, 40, 40), new Scalar(160, 255, 255), purpleMask);
        f.Add(GetPixelRatio(purpleMask));

        // Màu nâu/đen (thối): V < 60
        using var darkMask = new Mat();
        Cv2.InRange(hsv, new Scalar(0, 0, 0), new Scalar(180, 255, 60), darkMask);
        f.Add(GetPixelRatio(darkMask));

        // ── LAB mean/std ─────────────────────────────────────────────────
        using var lab = new Mat();
        Cv2.CvtColor(img, lab, ColorConversionCodes.BGR2Lab);
        Cv2.MeanStdDev(lab, out Scalar labMean, out Scalar labStd);
        f.Add(labMean.Val0 / 255.0); // L
        f.Add(labMean.Val1 / 255.0); // A
        f.Add(labMean.Val2 / 255.0); // B
        f.Add(labStd.Val0  / 255.0);
        f.Add(labStd.Val1  / 255.0);
        f.Add(labStd.Val2  / 255.0);

        // ── HSV-H histogram (8 bins) ─────────────────────────────────────
        using var hChannel = new Mat();
        Cv2.ExtractChannel(hsv, hChannel, 0);
        float[] hHist = ComputeHistogram(hChannel, 8, 0f, 180f);
        foreach (var v in hHist) f.Add(v);

        return [.. f];
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. ĐẶC TRƯNG KẾT CẤU — Uniform LBP (~59 features)
    // ═══════════════════════════════════════════════════════════════════════
    private static double[] ExtractLbpFeatures(Mat img)
    {
        using var gray = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);

        int rows = gray.Rows, cols = gray.Cols;
        var lbpHist = new int[LbpDim];

        for (int y = 1; y < rows - 1; y++)
        {
            for (int x = 1; x < cols - 1; x++)
            {
                byte center = gray.At<byte>(y, x);
                int code = 0;

                // 8 neighbors theo chiều kim đồng hồ
                byte[] neighbors =
                [
                    gray.At<byte>(y-1, x-1), gray.At<byte>(y-1, x),
                    gray.At<byte>(y-1, x+1), gray.At<byte>(y,   x+1),
                    gray.At<byte>(y+1, x+1), gray.At<byte>(y+1, x),
                    gray.At<byte>(y+1, x-1), gray.At<byte>(y,   x-1)
                ];

                for (int i = 0; i < 8; i++)
                    if (neighbors[i] >= center)
                        code |= (1 << i);

                lbpHist[GetUniformLbpIndex(code)]++;
            }
        }

        double total = lbpHist.Sum();
        return lbpHist.Select(v => total > 0 ? v / total : 0.0).ToArray();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. ĐẶC TRƯNG HÌNH DẠNG (~6 features)
    // ═══════════════════════════════════════════════════════════════════════
    private double[] ExtractShapeFeatures(Mat img)
    {
        using var gray   = new Mat();
        using var binary = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

        Cv2.FindContours(binary, out OpenCvSharp.Point[][] contours, out _,
            RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        if (contours.Length == 0)
            return new double[ShapeDim];

        var c         = contours.OrderByDescending(ct => Cv2.ContourArea(InputArray.Create(ct))).First();
        double area   = Cv2.ContourArea(c);
        double perim  = Cv2.ArcLength(c, true);
        double circ   = perim > 0 ? 4 * Math.PI * area / (perim * perim) : 0;

        var hull      = Cv2.ConvexHull(c);
        double hArea  = Cv2.ContourArea(hull);
        double convex = hArea > 0 ? area / hArea : 0;

        Rect  bbox   = Cv2.BoundingRect(c);
        double aspect = (double)bbox.Width / Math.Max(bbox.Height, 1);

        return
        [
            area   / (_imageSize * _imageSize),
            circ,
            convex,
            aspect,
            perim  / (4.0 * _imageSize),
            (double)c.Length / (4.0 * _imageSize)
        ];
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 4. ĐẶC TRƯNG THỐNG KÊ (~12 features)
    // ═══════════════════════════════════════════════════════════════════════
    private static double[] ExtractStatFeatures(Mat img)
    {
        var f        = new List<double>();
        var channels = Cv2.Split(img); // B, G, R

        foreach (var ch in channels)
        {
            using var _ = ch;
            Cv2.MeanStdDev(ch, out Scalar mean, out Scalar std);
            f.Add(mean.Val0 / 255.0);
            f.Add(std.Val0  / 255.0);

            // Entropy từ histogram 16 bins
            float[] hist    = ComputeHistogram(ch, 16, 0f, 256f);
            double entropy  = -hist.Where(v => v > 0)
                                   .Sum(v => v * Math.Log(v + 1e-10));
            f.Add(entropy / Math.Log(16));
        }

        // Gradient magnitude (Sobel) — đo độ sắc nét / vết hỏng
        using var gray  = new Mat();
        using var gradX = new Mat();
        using var gradY = new Mat();
        using var grad  = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Sobel(gray, gradX, MatType.CV_64F, 1, 0);
        Cv2.Sobel(gray, gradY, MatType.CV_64F, 0, 1);
        Cv2.Magnitude(gradX, gradY, grad);

        Cv2.MeanStdDev(grad, out Scalar gMean, out Scalar gStd);
        f.Add(gMean.Val0 / 255.0);
        f.Add(gStd.Val0  / 255.0);
        f.Add(gMean.Val0 / (gStd.Val0 + 1e-10)); // SNR

        return [.. f];
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════════

    private static double GetPixelRatio(Mat mask) =>
        (double)Cv2.CountNonZero(mask) / (mask.Rows * mask.Cols);

    private static float[] ComputeHistogram(Mat channel, int bins, float min, float max)
    {
        using var hist = new Mat();
        Mat[] src    = [channel];
        int[] chIdx  = [0];
        int[] hSizes = [bins];
        Rangef[] ranges = [new Rangef(min, max)];
        Cv2.CalcHist(src, chIdx, null, hist, 1, hSizes, ranges);
        Cv2.Normalize(hist, hist, 0, 1, NormTypes.MinMax);

        var result = new float[bins];
        for (int i = 0; i < bins; i++)
            result[i] = hist.At<float>(i);
        return result;
    }

    /// <summary>Trả về bin index của Uniform LBP (0–8 cho uniform, 58 cho non-uniform).</summary>
    private static int GetUniformLbpIndex(int code)
    {
        int transitions = 0;
        for (int i = 0; i < 8; i++)
        {
            int curr = (code >> i) & 1;
            int next = (code >> ((i + 1) % 8)) & 1;
            if (curr != next) transitions++;
        }

        if (transitions <= 2)
        {
            int ones = 0;
            for (int i = 0; i < 8; i++)
                if (((code >> i) & 1) == 1) ones++;
            return ones; // 0–8
        }
        return 58; // non-uniform → bin cuối
    }

    /// <summary>Min-max normalize toàn bộ vector về [0, 1].</summary>
    private static float[] MinMaxNormalize(List<double> features)
    {
        double min   = features.Min();
        double max   = features.Max();
        double range = max - min;

        return features
            .Select(v => range < 1e-10 ? 0f : (float)((v - min) / range))
            .ToArray();
    }
}

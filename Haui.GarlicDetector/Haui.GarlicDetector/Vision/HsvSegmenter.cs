using Haui.GarlicDetector.Models;
using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Thuật toán phân vùng màu sắc theo không gian màu HSV để phát hiện tỏi.
/// Quy trình: lọc ngưỡng HSV → hình thái học (mở + đóng) → tìm contour.
/// Nhận frame đã được tiền xử lý (HSV) từ <see cref="IImagePreprocessor"/>.
/// </summary>
public sealed class HsvSegmenter : IGarlicSegmentor
{
    // ─── Ngưỡng HSV ──────────────────────────────────────────────────────────

    /// <summary>Ngưỡng Hue tối thiểu (0–179).</summary>
    public int HMin { get; set; } = 0;

    /// <summary>Ngưỡng Hue tối đa (0–179).</summary>
    public int HMax { get; set; } = 35;

    /// <summary>Ngưỡng Saturation tối thiểu (0–255).</summary>
    public int SMin { get; set; } = 0;

    /// <summary>Ngưỡng Saturation tối đa (0–255).</summary>
    public int SMax { get; set; } = 80;

    /// <summary>Ngưỡng Value tối thiểu (0–255).</summary>
    public int VMin { get; set; } = 150;

    /// <summary>Ngưỡng Value tối đa (0–255).</summary>
    public int VMax { get; set; } = 255;

    /// <summary>Diện tích tối thiểu (pixel²) để vùng được coi là tỏi hợp lệ.</summary>
    public double MinArea { get; set; } = 1000;

    // ─── Phân vùng ───────────────────────────────────────────────────────────

    /// <inheritdoc />
    /// <param name="preprocessedFrame">Frame HSV đã được tiền xử lý bởi <see cref="IImagePreprocessor"/>.</param>
    public List<GarlicSegmentResult> Segment(Mat preprocessedFrame)
    {
        // Bước 1: Lọc các pixel nằm trong ngưỡng HSV chỉ định
        using var mask = new Mat();
        Cv2.InRange(
            preprocessedFrame,
            new Scalar(HMin, SMin, VMin),
            new Scalar(HMax, SMax, VMax),
            mask);

        // Bước 2: Hình thái học
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(7, 7));
        using var opened = new Mat();
        Cv2.MorphologyEx(mask,   opened, MorphTypes.Open,  kernel, iterations: 2);
        using var closed = new Mat();
        Cv2.MorphologyEx(opened, closed, MorphTypes.Close, kernel, iterations: 3);

        // Bước 3: Tìm contour
        Cv2.FindContours(
            closed,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // Bước 4: Lọc theo diện tích và tính circularity = 4π·A / P²
        var results = new List<GarlicSegmentResult>();
        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area < MinArea) continue;

            double perimeter   = Cv2.ArcLength(contour, closed: true);
            double circularity = perimeter > 0
                ? Math.Clamp(4.0 * Math.PI * area / (perimeter * perimeter), 0.0, 1.0)
                : 0.0;

            results.Add(new GarlicSegmentResult(
                BoundingRect: Cv2.BoundingRect(contour),
                Area:         area,
                Circularity:  circularity));
        }

        return results;
    }
}

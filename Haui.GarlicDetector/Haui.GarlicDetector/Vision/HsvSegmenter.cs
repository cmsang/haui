using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Thuật toán phân vùng màu sắc theo không gian màu HSV để phát hiện tỏi.
/// Quy trình: BGR → HSV → lọc ngưỡng → hình thái học (mở + đóng) → tìm contour.
/// </summary>
public sealed class HsvSegmenter
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

    /// <summary>
    /// Phân tích frame BGR và trả về danh sách bounding box của các vùng tỏi.
    /// </summary>
    /// <param name="bgrFrame">Frame ảnh BGR nhận từ camera.</param>
    public List<Rect> Segment(Mat bgrFrame)
    {
        // Bước 1: Chuyển không gian màu BGR → HSV
        using var hsvMat = new Mat();
        Cv2.CvtColor(bgrFrame, hsvMat, ColorConversionCodes.BGR2HSV);

        // Bước 2: Lọc các pixel nằm trong ngưỡng HSV chỉ định
        using var mask = new Mat();
        Cv2.InRange(
            hsvMat,
            new Scalar(HMin, SMin, VMin),
            new Scalar(HMax, SMax, VMax),
            mask);

        // Bước 3: Hình thái học — Mở để loại nhiễu nhỏ, Đóng để lấp lỗ hổng bên trong vùng
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(7, 7));
        using var opened = new Mat();
        Cv2.MorphologyEx(mask,   opened, MorphTypes.Open,  kernel, iterations: 2);
        using var closed = new Mat();
        Cv2.MorphologyEx(opened, closed, MorphTypes.Close, kernel, iterations: 3);

        // Bước 4: Tìm contour bên ngoài (không lồng nhau)
        Cv2.FindContours(
            closed,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // Bước 5: Lọc theo diện tích tối thiểu và trả về bounding box
        var results = new List<Rect>();
        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area >= MinArea)
                results.Add(Cv2.BoundingRect(contour));
        }

        return results;
    }
}

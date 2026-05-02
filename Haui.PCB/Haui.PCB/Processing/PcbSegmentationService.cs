using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Dịch vụ phân vùng và cắt bo mạch PCB từ ảnh nền bằng thuật toán phát hiện đường biên.
/// Implements <see cref="IPcbSegmentationService"/>.
/// </summary>
public class PcbSegmentationService : IPcbSegmentationService
{
    // Ngưỡng Canny để phát hiện cạnh
    private const double CannyThreshold1 = 50;
    private const double CannyThreshold2 = 150;

    // Kích thước kernel morphology để đóng lỗ hổng biên
    private const int MorphKernelSize = 5;

    // Tỉ lệ diện tích tối thiểu của contour so với ảnh để được coi là bo mạch
    private const double MinAreaRatio = 0.01;

    /// <summary>
    /// Phân vùng và trả về ảnh bo mạch đã cắt từ ảnh gốc.
    /// Trả về null nếu không tìm thấy bo mạch.
    /// </summary>
    public Mat? Segment(Mat source)
    {
        using var gray = new Mat();
        using var blurred = new Mat();
        using var edges = new Mat();
        using var closed = new Mat();
        using var kernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(MorphKernelSize, MorphKernelSize));

        // Chuyển sang ảnh xám
        Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);

        // Làm mờ để giảm nhiễu
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

        // Phát hiện cạnh bằng Canny
        Cv2.Canny(blurred, edges, CannyThreshold1, CannyThreshold2);

        // Đóng kín các khoảng hở trên biên bằng morphology Close
        Cv2.MorphologyEx(edges, closed, MorphTypes.Close, kernel, iterations: 3);

        // Tìm contour
        Cv2.FindContours(
            closed,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        if (contours.Length == 0)
            return null;

        double imageArea = source.Rows * source.Cols;
        double minArea = imageArea * MinAreaRatio;

        // Tìm contour lớn nhất thỏa mãn diện tích tối thiểu
        Point[]? bestContour = null;
        double bestArea = 0;

        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area > minArea && area > bestArea)
            {
                bestArea = area;
                bestContour = contour;
            }
        }

        if (bestContour is null)
            return null;

        // Xấp xỉ đa giác để lấy hình chữ nhật bao quanh
        double epsilon = 0.02 * Cv2.ArcLength(bestContour, true);
        var approx = Cv2.ApproxPolyDP(bestContour, epsilon, true);

        // Nếu xấp xỉ là tứ giác → dùng perspective transform để cắt thẳng
        if (approx.Length == 4)
        {
            return WarpPerspective(source, approx);
        }

        // Fallback: dùng bounding rect nếu không phải tứ giác
        var boundingRect = Cv2.BoundingRect(bestContour);

        // Đảm bảo rect nằm trong ảnh
        boundingRect = ClampRect(boundingRect, source.Size());

        if (boundingRect.Width <= 0 || boundingRect.Height <= 0)
            return null;

        return new Mat(source, boundingRect);
    }

    /// <summary>
    /// Thực hiện perspective transform để cắt bo mạch về dạng chữ nhật thẳng.
    /// </summary>
    private static Mat WarpPerspective(Mat source, Point[] quad)
    {
        // Sắp xếp 4 điểm theo thứ tự: top-left, top-right, bottom-right, bottom-left
        var ordered = OrderPoints(quad.Select(p => new Point2f(p.X, p.Y)).ToArray());

        float width = Math.Max(
            Distance(ordered[0], ordered[1]),
            Distance(ordered[3], ordered[2]));

        float height = Math.Max(
            Distance(ordered[0], ordered[3]),
            Distance(ordered[1], ordered[2]));

        var dst = new Point2f[]
        {
            new(0, 0),
            new(width - 1, 0),
            new(width - 1, height - 1),
            new(0, height - 1)
        };

        using var M = Cv2.GetPerspectiveTransform(ordered, dst);
        var result = new Mat();
        Cv2.WarpPerspective(source, result, M, new Size((int)width, (int)height));
        return result;
    }

    /// <summary>
    /// Sắp xếp 4 điểm theo thứ tự: top-left, top-right, bottom-right, bottom-left.
    /// </summary>
    private static Point2f[] OrderPoints(Point2f[] pts)
    {
        // top-left: tổng nhỏ nhất; bottom-right: tổng lớn nhất
        // top-right: hiệu nhỏ nhất; bottom-left: hiệu lớn nhất
        var sums = pts.Select(p => p.X + p.Y).ToArray();
        var diffs = pts.Select(p => p.Y - p.X).ToArray();

        return
        [
            pts[Array.IndexOf(sums, sums.Min())],
            pts[Array.IndexOf(diffs, diffs.Min())],
            pts[Array.IndexOf(sums, sums.Max())],
            pts[Array.IndexOf(diffs, diffs.Max())]
        ];
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private static Rect ClampRect(Rect r, Size imageSize)
    {
        int x = Math.Max(0, r.X);
        int y = Math.Max(0, r.Y);
        int w = Math.Min(r.Width, imageSize.Width - x);
        int h = Math.Min(r.Height, imageSize.Height - y);
        return new Rect(x, y, w, h);
    }
}

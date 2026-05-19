using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Dịch vụ phân vùng và cắt bo mạch PCB từ ảnh nền bằng thuật toán phát hiện đường biên.
/// Hỗ trợ bo mạch hình chữ nhật / vuông xoay 360° — sử dụng MinAreaRect để
/// lấy hình chữ nhật nhỏ nhất bao quanh PCB, sau đó warp perspective căn thẳng.
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

    // Padding (pixel) thêm vào 4 cạnh để không bị cắt sát biên bo mạch
    private const int EdgePadding = 2;

    /// <summary>
    /// Phân vùng và trả về ảnh bo mạch đã cắt + căn thẳng từ ảnh gốc.
    /// Bo mạch có thể xoay bất kỳ góc nào — pipeline dùng MinAreaRect để xác định
    /// hình chữ nhật nhỏ nhất bao khít PCB rồi warp perspective về ảnh thẳng.
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

        // Tìm contour ngoài cùng
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

        // Thử xấp xỉ tứ giác trước (contour rõ ràng 4 góc)
        double epsilon = 0.02 * Cv2.ArcLength(bestContour, true);
        var approx = Cv2.ApproxPolyDP(bestContour, epsilon, true);

        if (approx.Length == 4)
        {
            // Perspective transform từ 4 góc tứ giác phát hiện được
            return WarpPerspective(source, approx.Select(p => new Point2f(p.X, p.Y)).ToArray());
        }

        // Fallback: dùng MinAreaRect — hình chữ nhật xoay nhỏ nhất bao khít contour.
        // Phù hợp khi PCB xoay bất kỳ góc nào trong khung hình.
        var rotatedRect = Cv2.MinAreaRect(bestContour);
        var boxPoints = Cv2.BoxPoints(rotatedRect);  // 4 góc của rotated rect (Point2f[])

        return WarpPerspective(source, boxPoints);
    }

    /// <summary>
    /// Thực hiện perspective transform, căn thẳng bo mạch về ảnh chữ nhật axis-aligned.
    /// Sau warp, nếu chiều cao lớn hơn chiều rộng thì xoay 90° để bo mạch nằm ngang (landscape).
    /// </summary>
    private static Mat WarpPerspective(Mat source, Point2f[] quad)
    {
        // Sắp xếp 4 điểm theo thứ tự: top-left, top-right, bottom-right, bottom-left
        var ordered = OrderPoints(quad);

        float width = Math.Max(
            Distance(ordered[0], ordered[1]),
            Distance(ordered[3], ordered[2]));

        float height = Math.Max(
            Distance(ordered[0], ordered[3]),
            Distance(ordered[1], ordered[2]));

        // Thêm padding nhỏ để không cắt sát biên vật lý của bo mạch
        float paddedWidth  = width  + EdgePadding * 2;
        float paddedHeight = height + EdgePadding * 2;

        var dst = new Point2f[]
        {
            new(EdgePadding, EdgePadding),
            new(paddedWidth - 1 - EdgePadding, EdgePadding),
            new(paddedWidth - 1 - EdgePadding, paddedHeight - 1 - EdgePadding),
            new(EdgePadding, paddedHeight - 1 - EdgePadding)
        };

        using var M = Cv2.GetPerspectiveTransform(ordered, dst);
        var warped = new Mat();
        Cv2.WarpPerspective(source, warped, M, new Size((int)paddedWidth, (int)paddedHeight));

        // Xoay 90° theo chiều kim đồng hồ nếu bo mạch đang bị dọc (portrait)
        // để đầu ra luôn ở dạng ngang (landscape) — giảm chiều cao thừa
        if (warped.Height > warped.Width)
        {
            var rotated = new Mat();
            Cv2.Transpose(warped, rotated);
            Cv2.Flip(rotated, rotated, FlipMode.Y);
            warped.Dispose();
            return rotated;
        }

        return warped;
    }

    /// <summary>
    /// Sắp xếp 4 điểm theo thứ tự: top-left, top-right, bottom-right, bottom-left.
    /// </summary>
    private static Point2f[] OrderPoints(Point2f[] pts)
    {
        // top-left: tổng (x+y) nhỏ nhất; bottom-right: tổng lớn nhất
        // top-right: hiệu (y-x) nhỏ nhất; bottom-left: hiệu lớn nhất
        var sums  = pts.Select(p => p.X + p.Y).ToArray();
        var diffs = pts.Select(p => p.Y - p.X).ToArray();

        return
        [
            pts[Array.IndexOf(sums,  sums.Min())],
            pts[Array.IndexOf(diffs, diffs.Min())],
            pts[Array.IndexOf(sums,  sums.Max())],
            pts[Array.IndexOf(diffs, diffs.Max())]
        ];
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

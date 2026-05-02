using Haui.PCB.Models;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.Processing;

/// <summary>
/// Chạy pipeline phân vùng PCB theo từng bước và thu thập ảnh trung gian.
/// Mỗi bước tạo ra một <see cref="PipelineStep"/> với ảnh đã Freeze để hiển thị cross-thread.
/// </summary>
public class PipelineDebugService : IPipelineDebugService
{
    private const double CannyThreshold1 = 50;
    private const double CannyThreshold2 = 150;
    private const int MorphKernelSize = 5;
    private const double MinAreaRatio = 0.01;
    private const int EdgePadding = 2;

    public IReadOnlyList<PipelineStep> RunSteps(Mat source)
    {
        var steps = new List<PipelineStep>();

        // Bước 0: Ảnh gốc
        steps.Add(MakeStep("Ảnh gốc", source.Clone(), "Ảnh đầu vào từ camera / vùng đã chọn"));

        using var gray = new Mat();
        using var blurred = new Mat();
        using var edges = new Mat();
        using var closed = new Mat();
        using var kernel = Cv2.GetStructuringElement(
            MorphShapes.Rect, new Size(MorphKernelSize, MorphKernelSize));

        // Bước 1: Chuyển grayscale
        Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
        steps.Add(MakeStep("Grayscale", gray.Clone(), "Chuyển ảnh màu sang ảnh xám (BGR → GRAY)"));

        // Bước 2: Gaussian Blur
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);
        steps.Add(MakeStep("Gaussian Blur", blurred.Clone(), "Làm mờ để giảm nhiễu (kernel 5×5)"));

        // Bước 3: Canny Edge
        Cv2.Canny(blurred, edges, CannyThreshold1, CannyThreshold2);
        steps.Add(MakeStep("Canny Edges", edges.Clone(),
            $"Phát hiện cạnh Canny (threshold1={CannyThreshold1}, threshold2={CannyThreshold2})"));

        // Bước 4: Morphology Close
        Cv2.MorphologyEx(edges, closed, MorphTypes.Close, kernel, iterations: 3);
        steps.Add(MakeStep("Morphology Close", closed.Clone(),
            "Đóng kín khoảng hở trên biên (Close, 3 lần lặp)"));

        // Bước 5: Contour lớn nhất + hộp bao
        Cv2.FindContours(closed, out var contours, out _, RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        using var contourVis = source.Clone();
        Point[]? bestContour = null;
        double bestArea = 0;
        double imageArea = source.Rows * source.Cols;
        double minArea = imageArea * MinAreaRatio;

        foreach (var c in contours)
        {
            double area = Cv2.ContourArea(c);
            if (area > minArea && area > bestArea)
            {
                bestArea = area;
                bestContour = c;
            }
        }

        if (bestContour != null)
        {
            // Vẽ tất cả contour (xanh lá) và contour lớn nhất (đỏ)
            Cv2.DrawContours(contourVis, contours, -1, new Scalar(0, 200, 0), 1);
            Cv2.DrawContours(contourVis, [bestContour], -1, new Scalar(0, 0, 255), 3);
        }

        steps.Add(MakeStep("Contour Detection", contourVis,
            $"Tìm contour — xanh: tất cả, đỏ: lớn nhất ({(bestContour is null ? "không tìm thấy" : $"area≈{bestArea:F0}px²")})"));

        // Bước 6: Hộp bao / perspective
        if (bestContour != null)
        {
            using var boxVis = source.Clone();

            double epsilon = 0.02 * Cv2.ArcLength(bestContour, true);
            var approx = Cv2.ApproxPolyDP(bestContour, epsilon, true);

            Point2f[] quad;
            string boxDesc;

            if (approx.Length == 4)
            {
                quad = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();
                boxDesc = "Xấp xỉ tứ giác (4 góc phát hiện rõ)";
                DrawQuad(boxVis, quad, new Scalar(255, 128, 0));
            }
            else
            {
                var rotRect = Cv2.MinAreaRect(bestContour);
                quad = Cv2.BoxPoints(rotRect);
                boxDesc = $"MinAreaRect (góc xoay ≈ {rotRect.Angle:F1}°)";
                DrawQuad(boxVis, quad, new Scalar(255, 128, 0));
            }

            steps.Add(MakeStep("Bounding Quad", boxVis, boxDesc));

            // Bước 7: Kết quả warp perspective
            using var warped = WarpPerspective(source, quad);
            steps.Add(MakeStep("Warp Perspective", warped.Clone(),
                "Bo mạch sau khi căn thẳng bằng perspective transform"));
        }
        else
        {
            steps.Add(MakeStep("Kết quả", source.Clone(), "Không phát hiện được bo mạch"));
        }

        return steps;
    }

    // ──── Helpers ─────────────────────────────────────────────────────────────

    private static PipelineStep MakeStep(string name, Mat mat, string description)
    {
        var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
        bitmap.Freeze();
        mat.Dispose();
        return new PipelineStep { StepName = name, Image = bitmap, Description = description };
    }

    private static void DrawQuad(Mat img, Point2f[] pts, Scalar color)
    {
        for (int i = 0; i < 4; i++)
        {
            var p1 = new Point((int)pts[i].X, (int)pts[i].Y);
            var p2 = new Point((int)pts[(i + 1) % 4].X, (int)pts[(i + 1) % 4].Y);
            Cv2.Line(img, p1, p2, color, 2);
        }
    }

    private static Mat WarpPerspective(Mat source, Point2f[] quad)
    {
        var ordered = OrderPoints(quad);

        float width = Math.Max(Distance(ordered[0], ordered[1]), Distance(ordered[3], ordered[2]));
        float height = Math.Max(Distance(ordered[0], ordered[3]), Distance(ordered[1], ordered[2]));

        float pw = width + EdgePadding * 2;
        float ph = height + EdgePadding * 2;

        var dst = new Point2f[]
        {
            new(EdgePadding, EdgePadding),
            new(pw - 1 - EdgePadding, EdgePadding),
            new(pw - 1 - EdgePadding, ph - 1 - EdgePadding),
            new(EdgePadding, ph - 1 - EdgePadding)
        };

        using var m = Cv2.GetPerspectiveTransform(ordered, dst);
        var result = new Mat();
        Cv2.WarpPerspective(source, result, m, new Size((int)pw, (int)ph));

        // Xoay về landscape nếu cần
        if (result.Rows > result.Cols)
        {
            var rotated = new Mat();
            Cv2.Rotate(result, rotated, RotateFlags.Rotate90Clockwise);
            result.Dispose();
            return rotated;
        }

        return result;
    }

    private static Point2f[] OrderPoints(Point2f[] pts)
    {
        var sorted = pts.OrderBy(p => p.X + p.Y).ToArray();
        var tl = sorted[0];
        var br = sorted[3];
        var remaining = new[] { sorted[1], sorted[2] };
        var tr = remaining.OrderBy(p => p.Y).First();
        var bl = remaining.OrderByDescending(p => p.Y).First();
        return [tl, tr, br, bl];
    }

    private static float Distance(Point2f a, Point2f b)
        => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
}

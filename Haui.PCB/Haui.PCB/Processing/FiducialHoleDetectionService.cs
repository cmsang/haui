using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Template matching tìm 4 lỗ tròn giống nhau — dùng toàn bộ thư viện mẫu, không phân biệt vị trí lỗ.
/// </summary>
public class FiducialHoleDetectionService : IFiducialHoleDetectionService
{
    private const int RequiredHoleCount = FiducialHoleTemplateService.RequiredDetectionCount;
    private const int PeaksPerTemplate = 6;

    public FiducialDetectionResult Detect(Mat searchImage, IReadOnlyList<Mat> templates, double minMatchScore)
    {
        if (searchImage.Empty())
            return Fail("Ảnh Morphology Close rỗng.");

        if (templates.Count == 0)
            return Fail("Chưa có mẫu lỗ tròn trong thư mục.");

        using var gray = EnsureGray(searchImage);
        var allCandidates = new List<(Point2f Center, double Score)>();

        foreach (var template in templates)
        {
            if (template.Empty())
                continue;

            if (template.Width > gray.Width || template.Height > gray.Height)
                continue;

            allCandidates.AddRange(FindTopMatches(gray, template, minMatchScore, PeaksPerTemplate));
        }

        if (allCandidates.Count == 0)
            return Fail($"Không khớp mẫu nào (ngưỡng {minMatchScore:P0}).");

        var selected = SelectDistinctCandidates(allCandidates, gray.Width, gray.Height, RequiredHoleCount);
        if (selected.Count < RequiredHoleCount)
            return Fail($"Chỉ tìm thấy {selected.Count}/{RequiredHoleCount} lỗ (ngưỡng {minMatchScore:P0}).");

        var ordered = OrderPoints(selected.Select(c => c.Center).ToArray());
        return new FiducialDetectionResult
        {
            Success = true,
            Centers = ordered,
            MatchScores = selected.Select(c => c.Score).ToArray(),
            Message = $"Đã tìm thấy {RequiredHoleCount} lỗ ({templates.Count} mẫu, Morphology Close)."
        };
    }

    private static List<(Point2f Center, double Score)> SelectDistinctCandidates(
        List<(Point2f Center, double Score)> candidates,
        int imageWidth,
        int imageHeight,
        int count)
    {
        double minDistance = Math.Min(imageWidth, imageHeight) * 0.08;
        var sorted = candidates.OrderByDescending(c => c.Score).ToList();
        var selected = new List<(Point2f Center, double Score)>();

        foreach (var candidate in sorted)
        {
            if (selected.Any(s => Distance(s.Center, candidate.Center) < minDistance))
                continue;

            selected.Add(candidate);
            if (selected.Count == count)
                break;
        }

        return selected;
    }

    private static List<(Point2f Center, double Score)> FindTopMatches(
        Mat gray,
        Mat template,
        double minMatchScore,
        int maxPeaks)
    {
        using var result = new Mat();
        Cv2.MatchTemplate(gray, template, result, TemplateMatchModes.CCoeffNormed);

        double minDistance = Math.Min(gray.Width, gray.Height) * 0.08;
        var selected = new List<(Point2f Center, double Score)>();
        using var scratch = result.Clone();

        for (int i = 0; i < maxPeaks; i++)
        {
            Cv2.MinMaxLoc(scratch, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
            if (maxVal < minMatchScore)
                break;

            var center = new Point2f(maxLoc.X + template.Width / 2f, maxLoc.Y + template.Height / 2f);
            selected.Add((center, maxVal));
            SuppressNeighborhood(scratch, maxLoc, template.Width, template.Height, minDistance);
        }

        return selected;
    }

    private static void SuppressNeighborhood(Mat matchMap, OpenCvSharp.Point peak, int templateW, int templateH, double minDistance)
    {
        int radius = (int)Math.Max(minDistance, Math.Max(templateW, templateH) * 0.75);
        int x1 = Math.Max(0, peak.X - radius);
        int y1 = Math.Max(0, peak.Y - radius);
        int x2 = Math.Min(matchMap.Width - 1, peak.X + radius);
        int y2 = Math.Min(matchMap.Height - 1, peak.Y + radius);
        Cv2.Rectangle(matchMap, new OpenCvSharp.Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1), Scalar.All(0), -1);
    }

    private static Mat EnsureGray(Mat source)
    {
        if (source.Channels() == 1)
            return source.Clone();

        var gray = new Mat();
        Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private static Point2f[] OrderPoints(Point2f[] pts)
    {
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
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static FiducialDetectionResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

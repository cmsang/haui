using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Template matching tìm 4 lỗ tròn — thử mẫu theo điểm nhận diện giảm dần, dừng sớm khi đủ 4 lỗ.
/// </summary>
public class FiducialHoleDetectionService : IFiducialHoleDetectionService
{
    private const int RequiredHoleCount = FiducialHoleTemplateService.RequiredDetectionCount;
    private const int PeaksPerTemplate = 6;
    private const int MinTemplateSize = 8;

    public FiducialDetectionResult Detect(
        Mat searchImage,
        IReadOnlyList<FiducialTemplateEntry> templates,
        double minMatchScore,
        int maxMatchDimension)
    {
        if (searchImage.Empty())
            return Fail("Ảnh Morphology Close rỗng.");

        if (templates.Count == 0)
            return Fail("Chưa có mẫu lỗ tròn trong thư mục.");

        Mat? ownedGray = null;
        try
        {
            Mat gray = searchImage.Channels() == 1
                ? searchImage
                : ownedGray = new Mat();

            if (ownedGray is not null)
                Cv2.CvtColor(searchImage, ownedGray, ColorConversionCodes.BGR2GRAY);

            double scale = ComputeMatchScale(gray.Width, gray.Height, maxMatchDimension);
            using var scaledSearch = scale < 1.0 - 1e-6
                ? ResizeMat(gray, scale)
                : null;

            Mat searchForMatch = scaledSearch ?? gray;
            var allCandidates = new List<(Point2f Center, double Score, string FileName)>(
                templates.Count * PeaksPerTemplate);
            var triedTemplates = new List<string>();

            foreach (var entry in templates)
            {
                var template = entry.Template;
                if (template.Empty())
                    continue;

                using var scaledTemplate = scale < 1.0 - 1e-6
                    ? ResizeMat(template, scale)
                    : null;

                Mat templateForMatch = scaledTemplate ?? template;
                if (templateForMatch.Width > searchForMatch.Width
                    || templateForMatch.Height > searchForMatch.Height
                    || templateForMatch.Width < MinTemplateSize
                    || templateForMatch.Height < MinTemplateSize)
                    continue;

                triedTemplates.Add(entry.FileName);

                var peaks = FindTopMatches(searchForMatch, templateForMatch, minMatchScore, PeaksPerTemplate);
                if (scale < 1.0 - 1e-6)
                {
                    double invScale = 1.0 / scale;
                    foreach (var peak in peaks)
                    {
                        allCandidates.Add((
                            new Point2f((float)(peak.Center.X * invScale), (float)(peak.Center.Y * invScale)),
                            peak.Score,
                            entry.FileName));
                    }
                }
                else
                {
                    foreach (var peak in peaks)
                        allCandidates.Add((peak.Center, peak.Score, entry.FileName));
                }

                var selectedSoFar = SelectDistinctCandidates(
                    allCandidates,
                    gray.Width,
                    gray.Height,
                    RequiredHoleCount);

                if (selectedSoFar.Count >= RequiredHoleCount)
                    break;
            }

            if (allCandidates.Count == 0)
            {
                return Fail(
                    $"Không khớp mẫu nào (ngưỡng {minMatchScore:P0}).",
                    BuildOutcomes(triedTemplates, []));
            }

            var selected = SelectDistinctCandidates(
                allCandidates,
                gray.Width,
                gray.Height,
                RequiredHoleCount);

            if (selected.Count < RequiredHoleCount)
            {
                var partialOrdered = OrderPoints(selected.Select(c => c.Center).ToArray());
                return new FiducialDetectionResult
                {
                    Success = false,
                    Centers = partialOrdered,
                    MatchScores = selected.Select(c => c.Score).ToArray(),
                    Message = $"Chỉ tìm thấy {selected.Count}/{RequiredHoleCount} lỗ (ngưỡng {minMatchScore:P0}).",
                    TemplateOutcomes = BuildOutcomes(triedTemplates, [])
                };
            }

            var ordered = OrderPoints(selected.Select(c => c.Center).ToArray());
            var contributing = selected.Select(c => c.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            return new FiducialDetectionResult
            {
                Success = true,
                Centers = ordered,
                MatchScores = selected.Select(c => c.Score).ToArray(),
                Message = $"Đã tìm thấy {RequiredHoleCount} lỗ ({triedTemplates.Count}/{templates.Count} mẫu, scale={scale:F2}).",
                TemplateOutcomes = BuildOutcomes(triedTemplates, contributing)
            };
        }
        finally
        {
            ownedGray?.Dispose();
        }
    }

    private static IReadOnlyList<FiducialTemplateRecognitionOutcome> BuildOutcomes(
        IReadOnlyList<string> triedTemplates,
        HashSet<string> contributingFileNames)
    {
        if (triedTemplates.Count == 0)
            return [];

        return triedTemplates
            .Select(fileName => new FiducialTemplateRecognitionOutcome
            {
                FileName = fileName,
                ContributedToFinalHoles = contributingFileNames.Contains(fileName)
            })
            .ToList();
    }

    private static double ComputeMatchScale(int width, int height, int maxMatchDimension)
    {
        if (maxMatchDimension <= 0) return 1.0;
        int maxDim = Math.Max(width, height);
        if (maxDim <= maxMatchDimension) return 1.0;
        return maxMatchDimension / (double)maxDim;
    }

    private static Mat ResizeMat(Mat source, double scale)
    {
        var resized = new Mat();
        Cv2.Resize(source, resized, new Size(), scale, scale, InterpolationFlags.Area);
        return resized;
    }

    private static List<(Point2f Center, double Score, string FileName)> SelectDistinctCandidates(
        List<(Point2f Center, double Score, string FileName)> candidates,
        int imageWidth,
        int imageHeight,
        int count)
    {
        double minDistance = Math.Min(imageWidth, imageHeight) * 0.08;
        var sorted = candidates.OrderByDescending(c => c.Score).ToList();
        var selected = new List<(Point2f Center, double Score, string FileName)>(count);

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
        var selected = new List<(Point2f Center, double Score)>(maxPeaks);

        for (int i = 0; i < maxPeaks; i++)
        {
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
            if (maxVal < minMatchScore)
                break;

            var center = new Point2f(maxLoc.X + template.Width / 2f, maxLoc.Y + template.Height / 2f);
            selected.Add((center, maxVal));
            SuppressNeighborhood(result, maxLoc, template.Width, template.Height, minDistance);
        }

        return selected;
    }

    private static void SuppressNeighborhood(
        Mat matchMap,
        OpenCvSharp.Point peak,
        int templateW,
        int templateH,
        double minDistance)
    {
        int radius = (int)Math.Max(minDistance, Math.Max(templateW, templateH) * 0.75);
        int x1 = Math.Max(0, peak.X - radius);
        int y1 = Math.Max(0, peak.Y - radius);
        int x2 = Math.Min(matchMap.Width - 1, peak.X + radius);
        int y2 = Math.Min(matchMap.Height - 1, peak.Y + radius);
        Cv2.Rectangle(matchMap, new OpenCvSharp.Rect(x1, y1, x2 - x1 + 1, y2 - y1 + 1), Scalar.All(0), -1);
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

    private static FiducialDetectionResult Fail(
        string message,
        IReadOnlyList<FiducialTemplateRecognitionOutcome>? outcomes = null) => new()
    {
        Success = false,
        Message = message,
        TemplateOutcomes = outcomes
    };
}

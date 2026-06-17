using Haui.PCB.Models.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Template matching finds fiducial holes; geometry + board aspect ratio selects the correct outer quad.
/// </summary>
public class FiducialHoleDetectionService : IFiducialHoleDetectionService
{
    private const int RequiredHoleCount = FiducialHoleTemplateService.RequiredDetectionCount;
    private const int PeaksPerTemplate = 6;
    private const int MinTemplateSize = 8;

    public FiducialDetectionResult Detect(
        Mat searchImage,
        IReadOnlyList<FiducialTemplateEntry> templates,
        FiducialHoleSettings fiducialSettings,
        PcbBoardSettings boardSettings)
    {
        if (searchImage.Empty())
            return Fail("Ảnh Morphology Close rỗng.");

        if (templates.Count == 0)
            return Fail("Chưa có mẫu lỗ tròn trong thư mục.");

        double minMatchScore = fiducialSettings.MinMatchScore;
        int maxMatchDimension = fiducialSettings.MaxMatchDimension;

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
            int maxPoolSize = fiducialSettings.MaxQuadSearchCandidates;

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

                var poolSoFar = FiducialQuadSelector.BuildCandidatePool(
                    allCandidates,
                    (float)(Math.Min(gray.Width, gray.Height) * 0.08),
                    maxPoolSize);

                if (poolSoFar.Count >= maxPoolSize)
                    break;
            }

            if (allCandidates.Count == 0)
                return Fail($"Không khớp mẫu nào (ngưỡng {minMatchScore:P0}).");

            var selection = FiducialQuadSelector.SelectBestQuad(
                allCandidates,
                gray.Width,
                gray.Height,
                boardSettings,
                fiducialSettings);

            float minCornerDistance = (float)(Math.Min(gray.Width, gray.Height) * 0.08);

            if (selection is null)
            {
                var partialPool = FiducialQuadSelector.BuildCandidatePool(
                    allCandidates,
                    minCornerDistance,
                    maxPoolSize);

                if (partialPool.Count == 0)
                    return Fail(BuildNoQuadMessage(boardSettings, fiducialSettings, minMatchScore));

                int partialCount = Math.Min(partialPool.Count, RequiredHoleCount);
                var partialCenters = partialPool.Take(partialCount).Select(c => c.Center).ToArray();
                var partialScores = partialPool.Take(partialCount).Select(c => c.Score).ToArray();

                return new FiducialDetectionResult
                {
                    Success = false,
                    Centers = partialCenters,
                    MatchScores = partialScores,
                    Message = BuildNoQuadMessage(boardSettings, fiducialSettings, minMatchScore)
                };
            }

            var ordered = selection.Value.OrderedCorners;
            if (!FiducialQuadOrdering.AreDistinctCorners(ordered, minCornerDistance))
            {
                int distinctCount = FiducialQuadOrdering.CountDistinctCorners(ordered, minCornerDistance);
                return new FiducialDetectionResult
                {
                    Success = false,
                    Centers = ordered,
                    MatchScores = selection.Value.MatchScores,
                    Message = $"Chỉ tìm thấy {distinctCount}/{RequiredHoleCount} lỗ phân biệt (ngưỡng {minMatchScore:P0})."
                };
            }

            string modeLabel = boardSettings.HasAspectConstraint
                ? $"tỷ lệ bo mạch {boardSettings.WidthMm:0.#}×{boardSettings.HeightMm:0.#} mm"
                : "diện tích lớn nhất";

            return new FiducialDetectionResult
            {
                Success = true,
                Centers = ordered,
                MatchScores = selection.Value.MatchScores,
                Message =
                    $"Đã tìm thấy {RequiredHoleCount} lỗ ({triedTemplates.Count}/{templates.Count} mẫu, {modeLabel}, scale={scale:F2}).",
                TemplateOutcomes = BuildOutcomesFromMembers(selection.Value.Members)
            };
        }
        finally
        {
            ownedGray?.Dispose();
        }
    }

    private static string BuildNoQuadMessage(
        PcbBoardSettings board,
        FiducialHoleSettings options,
        double minMatchScore)
    {
        if (board.HasAspectConstraint)
        {
            return $"Không tìm được 4 lỗ hợp lệ (tỷ lệ {board.WidthMm:0.#}×{board.HeightMm:0.#} mm, "
                   + $"sai số ≤{options.AspectRatioTolerance:P0}, ngưỡng khớp {minMatchScore:P0}).";
        }

        return $"Không tìm được 4 lỗ hợp lệ (chế độ diện tích, ngưỡng khớp {minMatchScore:P0}).";
    }

    private static IReadOnlyList<FiducialTemplateRecognitionOutcome> BuildOutcomesFromMembers(
        IReadOnlyList<(Point2f Center, double Score, string FileName)> members)
        => members
            .GroupBy(m => m.FileName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new FiducialTemplateRecognitionOutcome
            {
                FileName = g.Key,
                RecognizedHoleCount = g.Count()
            })
            .ToList();

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

    private static FiducialDetectionResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

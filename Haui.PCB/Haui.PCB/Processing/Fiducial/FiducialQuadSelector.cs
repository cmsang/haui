using Haui.PCB.Models.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>Selects the best four-hole quad from template-match candidates using geometry and board aspect ratio.</summary>
internal static class FiducialQuadSelector
{
    public readonly record struct QuadSelection(
        Point2f[] OrderedCorners,
        double[] MatchScores,
        List<(Point2f Center, double Score, string FileName)> Members);

    public static QuadSelection? SelectBestQuad(
        IReadOnlyList<(Point2f Center, double Score, string FileName)> candidates,
        int imageWidth,
        int imageHeight,
        PcbBoardSettings board,
        FiducialHoleSettings options)
    {
        if (candidates.Count < FiducialHoleTemplateService.RequiredDetectionCount)
            return null;

        float minDistance = (float)(Math.Min(imageWidth, imageHeight) * 0.08);
        float minEdgeLength = minDistance * 0.5f;
        var pool = BuildCandidatePool(candidates, minDistance, options.MaxQuadSearchCandidates);
        if (pool.Count < FiducialHoleTemplateService.RequiredDetectionCount)
            return null;

        bool useAspect = board.HasAspectConstraint;
        double aspectTolerance = options.AspectRatioTolerance;
        double minRect = options.MinQuadRectangularity;

        QuadSelection? best = null;
        double bestScore = double.NegativeInfinity;

        int count = pool.Count;
        for (int i0 = 0; i0 < count - 3; i0++)
        for (int i1 = i0 + 1; i1 < count - 2; i1++)
        for (int i2 = i1 + 1; i2 < count - 1; i2++)
        for (int i3 = i2 + 1; i3 < count; i3++)
        {
            var group = new[]
            {
                pool[i0],
                pool[i1],
                pool[i2],
                pool[i3]
            };

            if (!ArePairwiseFarEnough(group, minDistance))
                continue;

            var corners = group.Select(g => g.Center).ToArray();
            var ordered = FiducialQuadOrdering.OrderCorners(corners);

            if (!FiducialQuadGeometry.IsValidQuad(ordered, minEdgeLength, (float)minRect, out var metrics))
                continue;

            if (useAspect)
            {
                double aspectErr = FiducialQuadGeometry.AspectRatioError(
                    metrics.Width,
                    metrics.Height,
                    board.WidthMm,
                    board.HeightMm);
                if (aspectErr > aspectTolerance)
                    continue;
            }

            double matchSum = group.Sum(g => g.Score);
            double areaNorm = metrics.Area / Math.Max(imageWidth * imageHeight, 1);
            double composite = useAspect
                ? matchSum + areaNorm * 0.25
                : matchSum + areaNorm * 2.0;

            if (composite > bestScore)
            {
                bestScore = composite;
                best = new QuadSelection(
                    ordered,
                    OrderScoresByCorners(ordered, group),
                    group.ToList());
            }
        }

        return best;
    }

    public static List<(Point2f Center, double Score, string FileName)> BuildCandidatePool(
        IReadOnlyList<(Point2f Center, double Score, string FileName)> candidates,
        float minDistance,
        int maxCount)
    {
        var sorted = candidates.OrderByDescending(c => c.Score).ToList();
        var pool = new List<(Point2f Center, double Score, string FileName)>(maxCount);

        foreach (var candidate in sorted)
        {
            if (pool.Any(p => Distance(p.Center, candidate.Center) < minDistance))
                continue;

            pool.Add(candidate);
            if (pool.Count >= maxCount)
                break;
        }

        return pool;
    }

    private static bool ArePairwiseFarEnough(
        (Point2f Center, double Score, string FileName)[] group,
        float minDistance)
    {
        for (int i = 0; i < group.Length; i++)
        {
            for (int j = i + 1; j < group.Length; j++)
            {
                if (Distance(group[i].Center, group[j].Center) < minDistance)
                    return false;
            }
        }

        return true;
    }

    private static double[] OrderScoresByCorners(
        Point2f[] orderedCorners,
        (Point2f Center, double Score, string FileName)[] group)
    {
        return orderedCorners
            .Select(corner => group
                .OrderBy(s => Distance(s.Center, corner))
                .First()
                .Score)
            .ToArray();
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>Orders four corners as TL, TR, BR, BL without reusing the same point.</summary>
internal static class QuadOrdering
{
    public static Point2f[] OrderCorners(Point2f[] pts)
    {
        if (pts.Length != 4)
            throw new ArgumentException("Expected exactly 4 points.", nameof(pts));

        var remaining = new List<Point2f>(pts);

        return
        [
            TakeExtremum(remaining, p => p.X + p.Y, takeMin: true),
            TakeExtremum(remaining, p => p.Y - p.X, takeMin: true),
            TakeExtremum(remaining, p => p.X + p.Y, takeMin: false),
            remaining[0]
        ];
    }

    public static bool AreDistinctCorners(Point2f[] pts, float minDistance)
    {
        if (pts.Length < 4)
            return false;

        for (int i = 0; i < pts.Length; i++)
        {
            for (int j = i + 1; j < pts.Length; j++)
            {
                if (Distance(pts[i], pts[j]) < minDistance)
                    return false;
            }
        }

        return true;
    }

    private static Point2f TakeExtremum(
        List<Point2f> remaining,
        Func<Point2f, float> metric,
        bool takeMin)
    {
        int index = 0;
        float best = metric(remaining[0]);
        for (int i = 1; i < remaining.Count; i++)
        {
            float value = metric(remaining[i]);
            if (takeMin ? value < best : value > best)
            {
                best = value;
                index = i;
            }
        }

        var point = remaining[index];
        remaining.RemoveAt(index);
        return point;
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

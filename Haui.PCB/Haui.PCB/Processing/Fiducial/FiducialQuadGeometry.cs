using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>Measures and validates a four-corner fiducial quad (TL, TR, BR, BL).</summary>
internal static class FiducialQuadGeometry
{
    public readonly record struct QuadMetrics(float Width, float Height, float Area, float Rectangularity);

    public static bool TryMeasureQuad(Point2f[] ordered, out QuadMetrics metrics)
    {
        metrics = default;
        if (ordered.Length != 4)
            return false;

        float topWidth = Distance(ordered[0], ordered[1]);
        float bottomWidth = Distance(ordered[3], ordered[2]);
        float leftHeight = Distance(ordered[0], ordered[3]);
        float rightHeight = Distance(ordered[1], ordered[2]);

        if (topWidth < 1e-3f || bottomWidth < 1e-3f || leftHeight < 1e-3f || rightHeight < 1e-3f)
            return false;

        float width = (topWidth + bottomWidth) * 0.5f;
        float height = (leftHeight + rightHeight) * 0.5f;
        float area = ComputeArea(ordered);
        if (area < 1f)
            return false;

        float widthRatio = MathF.Min(topWidth, bottomWidth) / MathF.Max(topWidth, bottomWidth);
        float heightRatio = MathF.Min(leftHeight, rightHeight) / MathF.Max(leftHeight, rightHeight);
        float rectangularity = (widthRatio + heightRatio) * 0.5f;

        metrics = new QuadMetrics(width, height, area, rectangularity);
        return true;
    }

    public static bool IsValidQuad(
        Point2f[] ordered,
        float minEdgeLength,
        float minRectangularity,
        out QuadMetrics metrics)
    {
        if (!TryMeasureQuad(ordered, out metrics))
            return false;

        if (!IsConvex(ordered))
            return false;

        if (metrics.Rectangularity < minRectangularity)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (EdgeLength(ordered, i) < minEdgeLength)
                return false;
        }

        return true;
    }

    /// <summary>Relative aspect-ratio error; considers 90° board rotation.</summary>
    public static double AspectRatioError(float quadWidth, float quadHeight, double boardWidthMm, double boardHeightMm)
    {
        if (quadWidth <= 0 || quadHeight <= 0 || boardWidthMm <= 0 || boardHeightMm <= 0)
            return double.MaxValue;

        double detected = quadWidth / quadHeight;
        double expectedA = boardWidthMm / boardHeightMm;
        double expectedB = boardHeightMm / boardWidthMm;

        double errDirect = Math.Min(RelativeError(detected, expectedA), RelativeError(detected, expectedB));
        double invDetected = 1.0 / detected;
        double errInverted = Math.Min(RelativeError(invDetected, expectedA), RelativeError(invDetected, expectedB));

        return Math.Min(errDirect, errInverted);
    }

    private static double RelativeError(double value, double expected)
        => Math.Abs(value - expected) / Math.Max(expected, 1e-6);

    private static float EdgeLength(Point2f[] pts, int index)
        => Distance(pts[index], pts[(index + 1) % 4]);

    private static bool IsConvex(Point2f[] pts)
    {
        bool? sign = null;
        for (int i = 0; i < 4; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % 4];
            var c = pts[(i + 2) % 4];
            float cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            if (MathF.Abs(cross) < 1e-4f)
                continue;

            bool current = cross > 0;
            sign ??= current;
            if (sign != current)
                return false;
        }

        return sign.HasValue;
    }

    private static float ComputeArea(Point2f[] pts)
    {
        float sum = 0;
        for (int i = 0; i < 4; i++)
        {
            var a = pts[i];
            var b = pts[(i + 1) % 4];
            sum += a.X * b.Y - b.X * a.Y;
        }

        return MathF.Abs(sum) * 0.5f;
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

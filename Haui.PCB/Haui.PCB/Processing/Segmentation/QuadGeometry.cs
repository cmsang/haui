using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>Measures and validates a four-corner quad (TL, TR, BR, BL).</summary>
internal static class QuadGeometry
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

    public static bool MeetsRectTolerance(
        Point2f[] ordered,
        double tolerancePercent,
        out QuadMetrics metrics)
    {
        metrics = default;
        if (!TryMeasureQuad(ordered, out metrics))
            return false;

        if (!IsConvex(ordered))
            return false;

        float minRect = (float)(1.0 - Math.Clamp(tolerancePercent, 0, 50) / 100.0);
        return metrics.Rectangularity >= minRect;
    }

    public static bool MeetsAngleTolerance(
        Point2f[] ordered,
        double maxDeviationDegrees,
        out float maxDeviation)
    {
        maxDeviation = 0;
        if (ordered.Length != 4)
            return false;

        float limit = (float)Math.Clamp(maxDeviationDegrees, 0, 90);
        for (int i = 0; i < 4; i++)
        {
            float angle = CornerAngleDegrees(
                ordered[(i + 3) % 4],
                ordered[i],
                ordered[(i + 1) % 4]);
            float deviation = MathF.Abs(angle - 90f);
            if (deviation > maxDeviation)
                maxDeviation = deviation;
        }

        return maxDeviation <= limit;
    }

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

    private static float CornerAngleDegrees(Point2f prev, Point2f vertex, Point2f next)
    {
        float toPrevX = prev.X - vertex.X;
        float toPrevY = prev.Y - vertex.Y;
        float toNextX = next.X - vertex.X;
        float toNextY = next.Y - vertex.Y;

        float dot = toPrevX * toNextX + toPrevY * toNextY;
        float len = MathF.Sqrt((toPrevX * toPrevX + toPrevY * toPrevY) * (toNextX * toNextX + toNextY * toNextY));
        if (len < 1e-6f)
            return 0f;

        float cos = Math.Clamp(dot / len, -1f, 1f);
        return MathF.Acos(cos) * (180f / MathF.PI);
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

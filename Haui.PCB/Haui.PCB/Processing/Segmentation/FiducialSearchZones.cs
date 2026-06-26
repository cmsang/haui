using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Builds disjoint search zones in corner-first, outside-in order for fiducial template matching.
/// Never emits a single zone covering the full image when a layered layout is possible.
/// </summary>
internal static class FiducialSearchZones
{
    private const double CornerFraction = 0.20;
    private const double MinCornerFraction = 0.12;
    private const double MaxCornerFraction = 0.28;
    private const int MinCornerPixels = 32;

    public readonly record struct SearchZone(Rect Rect, bool IsCorner);

    public static IReadOnlyList<SearchZone> BuildCornerFirstOutsideIn(
        int width,
        int height,
        int minTemplateSize)
    {
        var zones = new List<SearchZone>();
        if (width < minTemplateSize || height < minTemplateSize)
            return zones;

        int band = Math.Max(minTemplateSize, (int)(Math.Min(width, height) * 0.08));
        int margin = 0;

        while (true)
        {
            int innerW = width - 2 * margin;
            int innerH = height - 2 * margin;
            if (innerW < minTemplateSize * 2 || innerH < minTemplateSize * 2)
                break;

            int cornerSize = ComputeCornerSize(innerW, innerH, minTemplateSize);
            if (cornerSize > innerW || cornerSize > innerH)
                break;

            int ox = margin;
            int oy = margin;

            TryAdd(zones, ox, oy, cornerSize, cornerSize, isCorner: true, minTemplateSize);
            TryAdd(zones, ox + innerW - cornerSize, oy, cornerSize, cornerSize, isCorner: true, minTemplateSize);
            TryAdd(zones, ox + innerW - cornerSize, oy + innerH - cornerSize, cornerSize, cornerSize, isCorner: true, minTemplateSize);
            TryAdd(zones, ox, oy + innerH - cornerSize, cornerSize, cornerSize, isCorner: true, minTemplateSize);

            int edgeW = innerW - 2 * cornerSize;
            int edgeH = innerH - 2 * cornerSize;
            int edgeBand = Math.Min(band, cornerSize);

            if (edgeW >= minTemplateSize)
            {
                TryAdd(zones, ox + cornerSize, oy, edgeW, edgeBand, isCorner: false, minTemplateSize);
                TryAdd(zones, ox + cornerSize, oy + innerH - edgeBand, edgeW, edgeBand, isCorner: false, minTemplateSize);
            }

            if (edgeH >= minTemplateSize)
            {
                TryAdd(zones, ox, oy + cornerSize, edgeBand, edgeH, isCorner: false, minTemplateSize);
                TryAdd(zones, ox + innerW - edgeBand, oy + cornerSize, edgeBand, edgeH, isCorner: false, minTemplateSize);
            }

            margin += band;
            if (margin * 2 >= width || margin * 2 >= height)
                break;
        }

        return zones;
    }

    public static bool TryGetOutermostZoneIndex(
        IReadOnlyList<SearchZone> zones,
        Point2f point,
        out int zoneIndex)
    {
        int px = (int)MathF.Round(point.X);
        int py = (int)MathF.Round(point.Y);

        for (int i = 0; i < zones.Count; i++)
        {
            if (Contains(zones[i].Rect, px, py))
            {
                zoneIndex = i;
                return true;
            }
        }

        zoneIndex = -1;
        return false;
    }

    private static bool Contains(Rect rect, int x, int y)
        => x >= rect.X && x < rect.X + rect.Width
           && y >= rect.Y && y < rect.Y + rect.Height;

    private static int ComputeCornerSize(int width, int height, int minTemplateSize)
    {
        int minDim = Math.Min(width, height);
        int size = (int)(minDim * CornerFraction);
        size = Math.Clamp(size, (int)(minDim * MinCornerFraction), (int)(minDim * MaxCornerFraction));
        size = Math.Max(size, MinCornerPixels);
        size = Math.Max(size, minTemplateSize * 2);
        return Math.Min(size, minDim);
    }

    private static void TryAdd(
        List<SearchZone> zones,
        int x,
        int y,
        int w,
        int h,
        bool isCorner,
        int minTemplateSize)
    {
        if (w < minTemplateSize || h < minTemplateSize)
            return;

        zones.Add(new SearchZone(new Rect(x, y, w, h), isCorner));
    }
}

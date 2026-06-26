using System.IO;
using Haui.PCB.Models.Templates;
using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// Caches grayscale orientation-marker crops per library folder to avoid repeated full PNG decodes.
/// </summary>
public static class OrientationMarkerCache
{
    private static readonly object Sync = new();

    private static string? _cachedFolder;
    private static string? _cachedOrientationName;
    private static long _cachedRevision;
    private static List<CachedOrientationCandidate>? _candidates;

    public static void Invalidate()
    {
        lock (Sync)
        {
            ClearCandidates();
        }
    }

    public static IReadOnlyList<CachedOrientationCandidate> GetCandidates(
        ITemplateLibraryService libraryService,
        string orientationName)
    {
        var folder = Path.GetFullPath(libraryService.GetLibraryFolder());
        var revision = ComputeLibraryRevision(folder);

        lock (Sync)
        {
            if (_candidates is not null
                && string.Equals(_cachedFolder, folder, StringComparison.OrdinalIgnoreCase)
                && string.Equals(_cachedOrientationName, orientationName, StringComparison.Ordinal)
                && _cachedRevision == revision)
            {
                return _candidates;
            }

            ClearCandidates();
            _candidates = BuildCandidates(libraryService, orientationName);
            _cachedFolder = folder;
            _cachedOrientationName = orientationName;
            _cachedRevision = revision;
            return _candidates;
        }
    }

    private static List<CachedOrientationCandidate> BuildCandidates(
        ITemplateLibraryService libraryService,
        string orientationName)
    {
        var list = new List<CachedOrientationCandidate>();

        foreach (var entry in libraryService.LoadAll())
        {
            var region = entry.Regions.FirstOrDefault(r =>
                r.IsOrientationMarker
                && string.Equals(r.Name.Trim(), orientationName, StringComparison.Ordinal));

            if (region is null)
                continue;

            using var board = LoadBoardGrayscale(entry.BoardImagePath);
            if (board is null || board.Empty())
                continue;

            using var crop = OrientationRegionCropper.Crop(board, region);
            if (crop is null || crop.Empty())
                continue;

            var ownedCrop = crop.Clone();
            var componentCount = entry.Regions.Count(r => !r.IsOrientationMarker);
            list.Add(new CachedOrientationCandidate(entry, region, ownedCrop, componentCount));
        }

        return list;
    }

    private static Mat? LoadBoardGrayscale(string boardImagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(boardImagePath) || !File.Exists(boardImagePath))
                return null;

            var mat = Cv2.ImRead(boardImagePath, ImreadModes.Grayscale);
            return mat.Empty() ? null : mat;
        }
        catch
        {
            return null;
        }
    }

    private static long ComputeLibraryRevision(string folder)
    {
        if (!Directory.Exists(folder))
            return 0;

        long revision = 0;

        try
        {
            foreach (var path in Directory.EnumerateFiles(folder))
            {
                var ext = Path.GetExtension(path);
                if (!ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                    && !ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                revision ^= File.GetLastWriteTimeUtc(path).Ticks;
            }
        }
        catch
        {
            return DateTime.UtcNow.Ticks;
        }

        return revision;
    }

    private static void ClearCandidates()
    {
        if (_candidates is null)
            return;

        foreach (var candidate in _candidates)
            candidate.Dispose();

        _candidates = null;
        _cachedFolder = null;
        _cachedOrientationName = null;
        _cachedRevision = 0;
    }
}

/// <summary>Orientation marker grayscale crop and metadata for one template entry.</summary>
public sealed class CachedOrientationCandidate : IDisposable
{
    public CachedOrientationCandidate(
        TemplateEntry entry,
        TemplateRegion region,
        Mat grayscaleCrop,
        int componentRegionCount)
    {
        Entry = entry;
        Region = region;
        GrayscaleCrop = grayscaleCrop;
        ComponentRegionCount = componentRegionCount;
    }

    public TemplateEntry Entry { get; }

    public TemplateRegion Region { get; }

    public Mat GrayscaleCrop { get; }

    public int ComponentRegionCount { get; }

    public void Dispose()
    {
        GrayscaleCrop.Dispose();
    }
}

/// <summary>Shared relative crop logic for orientation marker regions.</summary>
internal static class OrientationRegionCropper
{
    public static Mat? Crop(Mat board, TemplateRegion region)
    {
        int x = (int)(region.RelX * board.Width);
        int y = (int)(region.RelY * board.Height);
        int w = (int)(region.RelWidth * board.Width);
        int h = (int)(region.RelHeight * board.Height);

        x = Math.Clamp(x, 0, board.Width - 1);
        y = Math.Clamp(y, 0, board.Height - 1);
        w = Math.Clamp(w, 1, board.Width - x);
        h = Math.Clamp(h, 1, board.Height - y);

        if (w < 1 || h < 1)
            return null;

        return board[new Rect(x, y, w, h)].Clone();
    }
}

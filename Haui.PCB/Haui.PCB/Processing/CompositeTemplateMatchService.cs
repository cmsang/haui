using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Đọc toàn bộ thư viện mẫu, phân nhóm vùng theo tên, rồi với mỗi tên trong
/// <see cref="ComponentTemplateSettings.AllowedRegionNames"/> lấy ứng viên đầu tiên đạt
/// <see cref="ComponentTemplateSettings.MinMatchSimilarityPercent"/>.
/// </summary>
public sealed class CompositeTemplateMatchService : ICompositeTemplateMatchService
{
    private readonly ITemplateLibraryService _libraryService;
    private readonly IRegionComparisonService _comparisonService;

    public CompositeTemplateMatchService(
        ITemplateLibraryService libraryService,
        IRegionComparisonService comparisonService)
    {
        _libraryService = libraryService;
        _comparisonService = comparisonService;
    }

    public CompositeTemplateMatchResult? Match(Mat newBoard)
    {
        var entries = _libraryService.LoadAll();
        if (entries.Count == 0)
            return null;

        var allowedNames = ComponentTemplateRegionNames.LoadAllowedNames();
        var groups = BuildRegionGroups(entries, allowedNames);
        if (groups.Count == 0)
            return null;

        var allowedOrder = ComponentTemplateRegionNames.LoadAllowedNamesInOrder();
        var matchThreshold = AppSettingsStore.LoadMatchThresholdPercent();
        var boardCache = new Dictionary<string, Mat>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var results = new List<RegionComparisonResult>();
            int stt = 1;

            foreach (var name in allowedOrder)
            {
                if (!groups.TryGetValue(name, out var candidates) || candidates.Count == 0)
                    continue;

                RegionComparisonResult? picked = null;

                foreach (var candidate in candidates)
                {
                    if (!TryGetTemplateBoard(boardCache, candidate.BoardImagePath, out var templateBoard))
                        continue;

                    var compared = _comparisonService.Compare(
                        templateBoard,
                        newBoard,
                        [candidate.Region]);

                    if (compared.Count == 0)
                        continue;

                    var current = compared[0];
                    picked ??= current;

                    if (current.IsMatch)
                    {
                        picked = current;
                        break;
                    }
                }

                if (picked is null)
                    continue;

                results.Add(new RegionComparisonResult
                {
                    Stt = stt++,
                    Name = name,
                    Similarity = picked.Similarity,
                    BoardRect = picked.BoardRect,
                    MatchThresholdPercent = picked.MatchThresholdPercent
                });
            }

            if (results.Count == 0)
                return null;

            return new CompositeTemplateMatchResult
            {
                RegionResults = results,
                MatchThresholdPercent = matchThreshold
            };
        }
        finally
        {
            foreach (var board in boardCache.Values)
                board.Dispose();
        }
    }

    private static Dictionary<string, List<TemplateRegionCandidate>> BuildRegionGroups(
        IReadOnlyList<TemplateEntry> entries,
        HashSet<string> allowedNames)
    {
        var groups = new Dictionary<string, List<TemplateRegionCandidate>>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (entry.Regions.Count == 0)
                continue;

            foreach (var region in entry.Regions)
            {
                var name = region.Name.Trim();
                if (string.IsNullOrEmpty(name) || !allowedNames.Contains(name))
                    continue;

                if (!groups.TryGetValue(name, out var list))
                {
                    list = [];
                    groups[name] = list;
                }

                list.Add(new TemplateRegionCandidate
                {
                    Region = region,
                    TemplateName = entry.Name,
                    BoardImagePath = entry.BoardImagePath
                });
            }
        }

        return groups;
    }

    private bool TryGetTemplateBoard(
        Dictionary<string, Mat> cache,
        string boardImagePath,
        out Mat templateBoard)
    {
        if (cache.TryGetValue(boardImagePath, out templateBoard!))
            return true;

        var loaded = _libraryService.LoadBoardImage(boardImagePath);
        if (loaded is null)
        {
            templateBoard = null!;
            return false;
        }

        cache[boardImagePath] = loaded;
        templateBoard = loaded;
        return true;
    }

    private sealed class TemplateRegionCandidate
    {
        public required TemplateRegion Region { get; init; }
        public required string TemplateName { get; init; }
        public required string BoardImagePath { get; init; }
    }
}

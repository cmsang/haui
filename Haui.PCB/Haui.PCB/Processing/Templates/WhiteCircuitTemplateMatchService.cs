using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// Legacy full-board white-circuit matcher (replaced by per-region match with inverted presence).
/// </summary>
public sealed class WhiteCircuitTemplateMatchService
{
    private static readonly TemplateRegion FullBoardRegion = new()
    {
        Name = "Mạch trắng",
        RelX = 0,
        RelY = 0,
        RelWidth = 1,
        RelHeight = 1
    };

    private readonly ITemplateLibraryService _libraryService;
    private readonly IRegionComparisonService _comparisonService;

    public WhiteCircuitTemplateMatchService(
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

        var matchThreshold = AppSettingsStore.LoadMatchThresholdPercent();
        double peakSimilarity = 0;
        string? peakTemplateName = null;
        var boardCache = new Dictionary<string, Mat>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var entry in entries)
            {
                if (!TryGetTemplateBoard(boardCache, entry.BoardImagePath, out var templateBoard))
                    continue;

                var compared = _comparisonService.Compare(
                    templateBoard,
                    newBoard,
                    [FullBoardRegion]);

                if (compared.Count == 0)
                    continue;

                var similarity = compared[0].Similarity;
                if (similarity > peakSimilarity)
                {
                    peakSimilarity = similarity;
                    peakTemplateName = entry.Name;
                }
            }

            if (peakTemplateName is null)
                return null;

            var passed = peakSimilarity < matchThreshold;
            var boardRect = new Rect(0, 0, newBoard.Width, newBoard.Height);

            return new CompositeTemplateMatchResult
            {
                UseInvertedPassLogic = true,
                PeakSimilarity = peakSimilarity,
                PeakTemplateName = peakTemplateName,
                MatchThresholdPercent = matchThreshold,
                RegionResults =
                [
                    new RegionComparisonResult
                    {
                        Stt = 1,
                        Name = FullBoardRegion.Name,
                        Similarity = peakSimilarity,
                        BoardRect = boardRect,
                        MatchThresholdPercent = matchThreshold,
                        Outcome = passed
                            ? RegionMatchOutcome.BelowThreshold
                            : RegionMatchOutcome.Matched
                    }
                ]
            };
        }
        finally
        {
            foreach (var board in boardCache.Values)
                board.Dispose();
        }
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
}

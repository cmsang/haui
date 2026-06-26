using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// Routes template matching to the active library folder. White-circuit mode uses the same
/// per-region matcher as component mode; <see cref="CompositeTemplateMatchResult.UseInvertedPassLogic"/>
/// inverts match → component presence.
/// </summary>
public sealed class ModeAwareTemplateMatchService : ICompositeTemplateMatchService
{
    private readonly CompositeTemplateMatchService _componentMatch;

    public ModeAwareTemplateMatchService(
        ITemplateLibraryService libraryService,
        IRegionComparisonService comparisonService)
    {
        _componentMatch = new CompositeTemplateMatchService(libraryService, comparisonService);
    }

    public CompositeTemplateMatchResult? Match(Mat newBoard, TemplateEntry? restrictToTemplate = null)
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        var result = _componentMatch.Match(newBoard, restrictToTemplate);
        if (result is null)
            return null;

        if (!TemplateLibraryPaths.IsWhiteCircuitMode(settings))
            return result;

        return new CompositeTemplateMatchResult
        {
            RegionResults = result.RegionResults,
            MatchThresholdPercent = result.MatchThresholdPercent,
            UseInvertedPassLogic = true
        };
    }
}

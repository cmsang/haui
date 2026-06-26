namespace Haui.PCB.ViewModels;

/// <summary>One row in the developer inspection result list.</summary>
public sealed class InspectionResultRowItem
{
    public int Stt { get; init; }

    public string Name { get; init; } = string.Empty;

    public string SimilarityDisplayText { get; init; } = string.Empty;

    public string ResultText { get; init; } = string.Empty;

    public string StatusNote { get; init; } = string.Empty;

    public static InspectionResultRowItem From(RegionComparisonResult result, bool invertedPassLogic)
    {
        var resultText = result.Outcome is RegionMatchOutcome.NoTemplateInLibrary
            or RegionMatchOutcome.NotComparable
            ? result.StatusNote
            : result.HasComponent(invertedPassLogic) ? "Có linh kiện" : "Thiếu";

        return new InspectionResultRowItem
        {
            Stt = result.Stt,
            Name = result.Name,
            SimilarityDisplayText = result.SimilarityDisplayText,
            ResultText = resultText,
            StatusNote = result.StatusNote
        };
    }
}

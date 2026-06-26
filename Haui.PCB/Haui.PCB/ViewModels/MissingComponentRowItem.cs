namespace Haui.PCB.ViewModels;

/// <summary>One row in the Dashboard missing-components grid.</summary>
public sealed class MissingComponentRowItem
{
    public int Stt { get; init; }

    public string Name { get; init; } = string.Empty;

    public string SimilarityDisplayText { get; init; } = string.Empty;

    public static MissingComponentRowItem From(RegionComparisonResult result, bool whiteCircuitMode)
        => new()
        {
            Stt = result.Stt,
            Name = result.Name,
            SimilarityDisplayText = result.FormatSimilarityDisplayText(whiteCircuitMode)
        };
}

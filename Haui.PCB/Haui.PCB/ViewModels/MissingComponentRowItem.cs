using Haui.PCB.Models.Detection;

namespace Haui.PCB.ViewModels;

/// <summary>One row in the Dashboard missing-components grid.</summary>
public sealed class MissingComponentRowItem
{
    public int Stt { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ConfidenceDisplayText { get; init; } = string.Empty;

    public static MissingComponentRowItem From(MissingComponent missing, int stt)
        => new()
        {
            Stt = stt,
            Name = missing.Label,
            ConfidenceDisplayText = $"{missing.Confidence * 100.0:F1}%"
        };
}

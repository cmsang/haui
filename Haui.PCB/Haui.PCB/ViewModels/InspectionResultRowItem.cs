using Haui.PCB.Models.Detection;

namespace Haui.PCB.ViewModels;

/// <summary>One row in the developer inspection result list.</summary>
public sealed class InspectionResultRowItem
{
    public int Stt { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ConfidenceDisplayText { get; init; } = string.Empty;

    public string ResultText { get; init; } = "Thiếu";

    public static InspectionResultRowItem From(MissingComponent missing, int stt)
        => new()
        {
            Stt = stt,
            Name = missing.Label,
            ConfidenceDisplayText = $"{missing.Confidence * 100.0:F1}%",
            ResultText = "Thiếu"
        };
}

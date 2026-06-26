namespace Haui.PCB.Models.Segmentation;

/// <summary>Result of matching the orientation marker region against template library entries.</summary>
public sealed class BoardOrientationDetectionResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public Templates.TemplateEntry? MatchedTemplate { get; init; }

    /// <summary>Best CCoeffNormed score (0..1) for the winning template.</summary>
    public double MatchScore { get; init; }

    /// <summary>True when the board must be rotated 180° before component matching.</summary>
    public bool RequiresRotation180 { get; init; }

    public static BoardOrientationDetectionResult Skipped()
        => new() { Success = true };

    public static BoardOrientationDetectionResult Fail(string message)
        => new() { Success = false, ErrorMessage = message };

    public static BoardOrientationDetectionResult Ok(
        Templates.TemplateEntry template,
        double score,
        bool requiresRotation180)
        => new()
        {
            Success = true,
            MatchedTemplate = template,
            MatchScore = score,
            RequiresRotation180 = requiresRotation180
        };
}

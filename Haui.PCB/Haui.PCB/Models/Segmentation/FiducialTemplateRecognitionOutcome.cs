namespace Haui.PCB.Models.Segmentation;

/// <summary>
/// Per-template outcome of one successful fiducial detection run (+1 per recognized hole).
/// </summary>
public sealed class FiducialTemplateRecognitionOutcome
{
    public required string FileName { get; init; }
    public int RecognizedHoleCount { get; init; }
}

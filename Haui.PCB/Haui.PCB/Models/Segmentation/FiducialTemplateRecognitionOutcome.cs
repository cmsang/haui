namespace Haui.PCB.Models.Segmentation;

/// <summary>
/// Per-template outcome of one fiducial detection run for stats update (+1 / -1).
/// </summary>
public sealed class FiducialTemplateRecognitionOutcome
{
    public required string FileName { get; init; }
    public bool ContributedToFinalHoles { get; init; }
}

using OpenCvSharp;

namespace Haui.PCB.Models.Segmentation;

/// <summary>
/// One fiducial hole template with its cumulative recognition score for ordered matching.
/// </summary>
public sealed class FiducialTemplateEntry
{
    public required string FileName { get; init; }
    public int RecognitionCount { get; init; }
    public required Mat Template { get; init; }
}

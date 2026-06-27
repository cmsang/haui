using OpenCvSharp;

namespace Haui.PCB.Models.Segmentation;

/// <summary>
/// Per-step PCB segmentation pipeline output — caller must Dispose when done.
/// </summary>
public sealed class SegmentationPipelineResult : IDisposable
{
    public required Mat Gray { get; init; }
    public required Mat Blurred { get; init; }
    public required Mat Edges { get; init; }
    public required Mat Closed { get; init; }

    public required double CannyThreshold1 { get; init; }
    public required double CannyThreshold2 { get; init; }

    /// <summary>Four holder-frame corners used for perspective warp; null when detection failed.</summary>
    public Point2f[]? WarpQuadCorners { get; init; }

    public string? ContourDescription { get; init; }

    /// <summary>Axis-aligned ROI of edge pixels used for holder search; null when no edges found.</summary>
    public Rect? EdgeSearchRoi { get; init; }

    /// <summary>Detection-space image size when Canny/Close/holder ran on a downscaled copy; null when full-res.</summary>
    public Size? DetectionSize { get; init; }

    /// <summary>Warped board image; null when detection failed.</summary>
    public Mat? Warped { get; init; }

    /// <summary>Per-step elapsed time; keys from <see cref="SegmentationPipelineSteps"/>.</summary>
    public IReadOnlyDictionary<string, TimeSpan> StepTimings { get; init; }
        = new Dictionary<string, TimeSpan>();

    public void Dispose()
    {
        Gray.Dispose();
        Blurred.Dispose();
        Edges.Dispose();
        Closed.Dispose();
        Warped?.Dispose();
    }
}

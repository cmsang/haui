namespace Haui.PCB.Models.Segmentation;

/// <summary>Internal keys for per-step timing in <see cref="SegmentationPipelineResult.StepTimings"/>.</summary>
public static class SegmentationPipelineSteps
{
    public const string Grayscale = "Grayscale";
    public const string GaussianBlur = "GaussianBlur";
    public const string Canny = "Canny";
    public const string MorphologyClose = "MorphologyClose";
    public const string Fiducial = "Fiducial";
    public const string Warp = "Warp";
}

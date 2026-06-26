namespace Haui.PCB.Models.Segmentation;

/// <summary>Internal keys for per-step timing in <see cref="SegmentationPipelineResult.StepTimings"/>.</summary>
public static class SegmentationPipelineSteps
{
    public const string GaussianBlur = "GaussianBlur";
    public const string Canny = "Canny";
    public const string MorphologyClose = "MorphologyClose";
    public const string HolderContour = "HolderContour";
    public const string Warp = "Warp";
}

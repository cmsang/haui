namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Downscale bounds for holder-frame detection (Canny / Close / hull) — section
/// <c>HolderDetectionDownscale</c> in <c>setting.json</c>.
/// Warp and YOLO still run on full-resolution source.
/// </summary>
public sealed class HolderDetectionDownscaleSettings
{
    public const int DefaultWidth = 1920;
    public const int DefaultHeight = 1080;

    /// <summary>When true, Canny/Close/holder search run on a downscaled copy of the frame.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Maximum width (px) when downscaling for detection.</summary>
    public int Width { get; set; } = DefaultWidth;

    /// <summary>Maximum height (px) when downscaling for detection.</summary>
    public int Height { get; set; } = DefaultHeight;

    public HolderDetectionDownscaleSettings Clone() => new()
    {
        Enabled = Enabled,
        Width = Width,
        Height = Height
    };
}

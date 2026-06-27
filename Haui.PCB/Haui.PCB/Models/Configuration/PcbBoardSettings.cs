using System.Text.Json.Serialization;

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Holder support frame dimensions and quad validation — section <c>PcbBoard</c> in <c>setting.json</c>.
/// </summary>
public sealed class PcbBoardSettings
{
    public const double DefaultWidthMm = 460;
    public const double DefaultHeightMm = 590;
    public const double DefaultQuadRectTolerancePercent = 15;
    public const double DefaultAspectRatioTolerancePercent = 15;
    public const double DefaultQuadAngleToleranceDegrees = 15;

    [JsonPropertyName("widthMm")]
    public double WidthMm { get; set; } = DefaultWidthMm;

    [JsonPropertyName("heightMm")]
    public double HeightMm { get; set; } = DefaultHeightMm;

    [JsonPropertyName("quadRectTolerancePercent")]
    public double QuadRectTolerancePercent { get; set; } = DefaultQuadRectTolerancePercent;

    [JsonPropertyName("aspectRatioTolerancePercent")]
    public double AspectRatioTolerancePercent { get; set; } = DefaultAspectRatioTolerancePercent;

    [JsonPropertyName("quadAngleToleranceDegrees")]
    public double QuadAngleToleranceDegrees { get; set; } = DefaultQuadAngleToleranceDegrees;

    [JsonIgnore]
    public bool HasAspectConstraint => WidthMm > 0 && HeightMm > 0;

    [JsonIgnore]
    public double AspectRatioTolerance =>
        Math.Clamp(AspectRatioTolerancePercent, 1, 100) / 100.0;
}

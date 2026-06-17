using System.Text.Json.Serialization;

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Physical PCB dimensions — section <c>PcbBoard</c> in <c>setting.json</c>.
/// Used for fiducial quad aspect-ratio matching (mm; not pixel calibration).
/// </summary>
public sealed class PcbBoardSettings
{
    public const double DefaultWidthMm = 400;
    public const double DefaultHeightMm = 550;

    /// <summary>Board width in mm. Set to 0 with <see cref="HeightMm"/> to disable aspect constraint.</summary>
    [JsonPropertyName("widthMm")]
    public double WidthMm { get; set; } = DefaultWidthMm;

    /// <summary>Board height in mm. Set to 0 with <see cref="WidthMm"/> to disable aspect constraint.</summary>
    [JsonPropertyName("heightMm")]
    public double HeightMm { get; set; } = DefaultHeightMm;

    /// <summary>True when both dimensions are positive — aspect-ratio filter is active.</summary>
    public bool HasAspectConstraint => WidthMm > 0 && HeightMm > 0;
}

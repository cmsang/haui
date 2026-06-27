using System.Text.Json.Serialization;

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// PCB segmentation pipeline — section <c>Segmentation</c> in <c>setting.json</c>.
/// </summary>
public sealed class SegmentationPipelineSettings
{
    public const double DefaultCannyThreshold1 = 30;
    public const double DefaultCannyThreshold2 = 100;

    [JsonPropertyName("cannyThreshold1")]
    public double CannyThreshold1 { get; set; } = DefaultCannyThreshold1;

    [JsonPropertyName("cannyThreshold2")]
    public double CannyThreshold2 { get; set; } = DefaultCannyThreshold2;
}

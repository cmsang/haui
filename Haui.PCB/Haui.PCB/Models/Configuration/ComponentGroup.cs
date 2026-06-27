namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Parent YOLO class mapped to child component labels for result listing and box splitting.
/// </summary>
public sealed class ComponentGroup
{
    public const string SplitHorizontal = "Horizontal";

    public const string SplitVertical = "Vertical";

    /// <summary>YOLO parent class name (e.g. L23).</summary>
    public string Parent { get; set; } = string.Empty;

    /// <summary>Child component labels in display order (e.g. L2, L3).</summary>
    public List<string> Children { get; set; } = [];

    /// <summary>
    /// Split direction for marking boxes: <see cref="SplitHorizontal"/> (columns) or
    /// <see cref="SplitVertical"/> (rows). Empty uses <see cref="ComponentDetectionSettings.DefaultGroupSplit"/>.
    /// </summary>
    public string? Split { get; set; }
}

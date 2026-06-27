namespace Haui.PCB.Models.Configuration;

/// <summary>
/// YOLO missing-component detection — section <c>ComponentDetection</c> in <c>setting.json</c>.
/// Each detection box marks a missing component location on the warped board image.
/// </summary>
public sealed class ComponentDetectionSettings
{
    public const string DefaultModelPath = "Models/yolo26m_960x1280.onnx";

    public const int DefaultInputWidth = 960;

    public const int DefaultInputHeight = 1280;

    public const double DefaultConfThreshold = 0.25;

    public const double DefaultIouThreshold = 0.45;

    public static readonly string[] DefaultClassNames =
    [
        "KF", "C2", "AMS1", "L23", "R12", "C365", "D12", "C1", "LM1", "L1", "C4"
    ];

    public const string DefaultGroupSplitDirection = ComponentGroup.SplitHorizontal;

    public static readonly List<ComponentGroup> DefaultComponentGroups =
    [
        new() { Parent = "L23", Children = ["L2", "L3"] },
        new() { Parent = "R12", Children = ["R1", "R2"] },
        new() { Parent = "C365", Children = ["C3", "C6", "C5"] },
        new() { Parent = "D12", Children = ["D1", "D2"], Split = ComponentGroup.SplitVertical },
        new() { Parent = "KF", Children = ["KF1", "KF2", "KF3"] }
    ];

    /// <summary>Path to ONNX model (relative to app working directory or absolute).</summary>
    public string ModelPath { get; set; } = DefaultModelPath;

    public int InputWidth { get; set; } = DefaultInputWidth;

    public int InputHeight { get; set; } = DefaultInputHeight;

    /// <summary>Minimum detection confidence (0..1).</summary>
    public double ConfThreshold { get; set; } = DefaultConfThreshold;

    /// <summary>NMS IoU threshold (0..1).</summary>
    public double IouThreshold { get; set; } = DefaultIouThreshold;

    /// <summary>Class index to label mapping (order must match training <c>data.yaml</c>).</summary>
    public List<string> ClassNames { get; set; } = [.. DefaultClassNames];

    /// <summary>
    /// Default split direction when a <see cref="ComponentGroup"/> has no <c>split</c> value.
    /// Horizontal = columns side-by-side; Vertical = rows top-to-bottom.
    /// </summary>
    public string DefaultGroupSplit { get; set; } = DefaultGroupSplitDirection;

    /// <summary>Parent class to child labels for expanded listing and split marking boxes.</summary>
    public List<ComponentGroup> ComponentGroups { get; set; } =
        DefaultComponentGroups.Select(g => new ComponentGroup
        {
            Parent = g.Parent,
            Children = [.. g.Children],
            Split = g.Split
        }).ToList();
}

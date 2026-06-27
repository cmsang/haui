namespace Haui.PCB.Models.Detection;

/// <summary>Result of YOLO missing-component inspection on a warped board image.</summary>
public sealed class ComponentInspectionResult
{
    public IReadOnlyList<MissingComponent> Missing { get; init; } = [];

    /// <summary>True when no missing components were detected (PASS).</summary>
    public bool IsComplete => Missing.Count == 0;

    public int MissingCount => Missing.Count;

    public double ConfThreshold { get; init; }
}

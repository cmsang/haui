using OpenCvSharp;

namespace Haui.PCB.Models.Segmentation;

/// <summary>Result of holder-frame contour detection on morphology-close edges.</summary>
public sealed class HolderContourResult
{
    public bool Success { get; init; }
    public Point2f[]? Corners { get; init; }
    public string? Message { get; init; }

    public static HolderContourResult Ok(Point2f[] corners, string message)
        => new() { Success = true, Corners = corners, Message = message };

    public static HolderContourResult Fail(string message)
        => new() { Success = false, Message = message };
}

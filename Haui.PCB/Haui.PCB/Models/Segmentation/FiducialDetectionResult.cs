using OpenCvSharp;

namespace Haui.PCB.Models.Segmentation;

/// <summary>
/// Kết quả template matching 4 lỗ tròn định vị — tâm lỗ đã sắp xếp TL, TR, BR, BL.
/// </summary>
public sealed class FiducialDetectionResult
{
    public bool Success { get; init; }
    public Point2f[]? Centers { get; init; }
    public double[]? MatchScores { get; init; }
    public string? Message { get; init; }
}

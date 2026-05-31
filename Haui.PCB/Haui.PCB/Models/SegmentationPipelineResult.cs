using OpenCvSharp;

namespace Haui.PCB.Models;

/// <summary>
/// Kết quả từng bước pipeline phân vùng PCB — caller phải Dispose khi xong.
/// </summary>
public sealed class SegmentationPipelineResult : IDisposable
{
    public required Mat Gray { get; init; }
    public required Mat Blurred { get; init; }
    public required Mat Edges { get; init; }
    public required Mat Closed { get; init; }

    public required double CannyThreshold1 { get; init; }
    public required double CannyThreshold2 { get; init; }

    public Point[][] Contours { get; init; } = [];
    public Point[]? BestContour { get; init; }
    public double BestArea { get; init; }

    public Point2f[]? Quad { get; init; }
    public string? BoundingBoxDescription { get; init; }

    public bool UsedFiducialDetection { get; init; }
    public Point2f[]? FiducialCenters { get; init; }
    public string? FiducialDescription { get; init; }

    /// <summary>Ảnh bo mạch đã warp; null nếu không phát hiện được.</summary>
    public Mat? Warped { get; init; }

    public void Dispose()
    {
        Gray.Dispose();
        Blurred.Dispose();
        Edges.Dispose();
        Closed.Dispose();
        Warped?.Dispose();
    }
}

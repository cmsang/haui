namespace Haui.GarlicDetector.Models;

/// <summary>Thông tin một vùng tỏi được phát hiện trong khung hình.</summary>
public sealed class GarlicRegion
{
    /// <summary>Hình chữ nhật bao quanh vùng tỏi (tính bằng pixel).</summary>
    public Rectangle BoundingBox { get; init; }

    /// <summary>Diện tích vùng phát hiện (số pixel²).</summary>
    public double Area { get; init; }

    /// <summary>Thời điểm phát hiện vùng này.</summary>
    public DateTime DetectedAt { get; init; } = DateTime.Now;
}

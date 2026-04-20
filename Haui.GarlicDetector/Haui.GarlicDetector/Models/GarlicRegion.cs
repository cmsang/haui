namespace Haui.GarlicDetector.Models;

/// <summary>Thông tin một vùng tỏi được phát hiện trong khung hình.</summary>
public sealed class GarlicRegion
{
    /// <summary>Hình chữ nhật bao quanh vùng tỏi (tính bằng pixel).</summary>
    public Rectangle BoundingBox { get; init; }

    /// <summary>Diện tích contour thực tế (số pixel²).</summary>
    public double Area { get; init; }

    /// <summary>
    /// Độ tròn của vùng tỏi — giá trị trong [0, 1], càng gần 1 càng tròn.
    /// Công thức: 4π·A / P²
    /// </summary>
    public double Circularity { get; init; }

    /// <summary>Thời điểm phát hiện vùng này.</summary>
    public DateTime DetectedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Nhãn phân loại cuối cùng sau 2 giai đoạn SVM + phân kích thước.
    /// <c>null</c> khi model SVM chưa được nạp hoặc chưa phân loại.
    /// </summary>
    public GarlicLabel? FinalLabel { get; set; }
}

using OpenCvSharp;

namespace Haui.PCB.Models;

/// <summary>
/// Kết quả so sánh một vùng giữa ảnh mẫu và ảnh bo mạch mới.
/// </summary>
public class RegionComparisonResult
{
    /// <summary>Tên vùng.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Số thứ tự.</summary>
    public int Stt { get; init; }

    /// <summary>Độ tương đồng (0..100).</summary>
    public double Similarity { get; init; }

    /// <summary>Hiển thị phần trăm.</summary>
    public string SimilarityText => $"{Similarity:F1}%";

    /// <summary>Ngưỡng % từ <c>setting.json</c> khi tạo kết quả.</summary>
    public double MatchThresholdPercent { get; init; } = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;

    /// <summary>True nếu độ tương đồng đạt ngưỡng cấu hình.</summary>
    public bool IsMatch => Similarity >= MatchThresholdPercent;

    /// <summary>Tọa độ tuyệt đối (pixel) của vùng trên ảnh bo mạch mới.</summary>
    public Rect BoardRect { get; init; }
}

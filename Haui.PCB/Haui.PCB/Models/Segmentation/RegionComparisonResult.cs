using OpenCvSharp;

namespace Haui.PCB.Models.Segmentation;

/// <summary>How a configured component name resolved during template match.</summary>
public enum RegionMatchOutcome
{
    Matched,
    BelowThreshold,
    NoTemplateInLibrary,
    NotComparable
}

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

    public RegionMatchOutcome Outcome { get; init; } = RegionMatchOutcome.BelowThreshold;

    /// <summary>True when the component is recognized at or above the configured threshold.</summary>
    public bool IsMatch => Outcome == RegionMatchOutcome.Matched;

    public string SimilarityDisplayText =>
        Outcome is RegionMatchOutcome.NoTemplateInLibrary or RegionMatchOutcome.NotComparable
            ? "—"
            : SimilarityText;

    public string StatusNote => Outcome switch
    {
        RegionMatchOutcome.BelowThreshold => "Không đạt ngưỡng",
        RegionMatchOutcome.NoTemplateInLibrary => "Không có mẫu",
        RegionMatchOutcome.NotComparable => "Không so được",
        _ => string.Empty
    };

    /// <summary>Tọa độ tuyệt đối (pixel) của vùng trên ảnh bo mạch mới.</summary>
    public Rect BoardRect { get; init; }

    /// <summary>True when a rectangle can be drawn on the annotated board image.</summary>
    public bool HasBoardRect => BoardRect.Width > 0 && BoardRect.Height > 0;
}

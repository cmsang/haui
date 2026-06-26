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

    /// <summary>True when similarity to the active template library meets the configured threshold.</summary>
    public bool IsMatch => Outcome == RegionMatchOutcome.Matched;

    /// <summary>
    /// Resolves component presence from template match.
    /// Component library: match means present; white-circuit library: match means absent.
    /// </summary>
    public bool HasComponent(bool matchMeansAbsent)
    {
        if (Outcome is RegionMatchOutcome.NoTemplateInLibrary or RegionMatchOutcome.NotComparable)
            return false;

        return matchMeansAbsent ? !IsMatch : IsMatch;
    }

    public string FormatSimilarityDisplayText(bool invertWhiteCircuitMatch)
    {
        if (Outcome is RegionMatchOutcome.NoTemplateInLibrary or RegionMatchOutcome.NotComparable)
            return "—";

        if (!invertWhiteCircuitMatch)
            return SimilarityText;

        return $"{100.0 - Similarity:F1}%";
    }

    public string SimilarityDisplayText =>
        FormatSimilarityDisplayText(invertWhiteCircuitMatch: false);

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

using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// So khớp bo mạch test: đọc thư viện, nhóm vùng theo tên, với mỗi tên trong
/// <c>AllowedRegionNames</c> lấy ứng viên đầu tiên đạt <c>MinMatchSimilarityPercent</c>.
/// </summary>
public interface ICompositeTemplateMatchService
{
    /// <summary>
    /// So sánh <paramref name="newBoard"/> với thư viện mẫu theo từng tên vùng cấu hình.
    /// Trả về null nếu thư viện trống hoặc không có vùng hợp lệ để so.
    /// </summary>
    CompositeTemplateMatchResult? Match(Mat newBoard, TemplateEntry? restrictToTemplate = null);
}

/// <summary>Kết quả so khớp tổng hợp (mỗi tên = ứng viên đầu tiên đạt ngưỡng cấu hình trong thư viện).</summary>
public sealed class CompositeTemplateMatchResult
{
    public double MatchThresholdPercent { get; init; } = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;

    public IReadOnlyList<RegionComparisonResult> RegionResults { get; init; } = [];

    /// <summary>True for white-circuit mode (PASS when no template is similar enough).</summary>
    public bool UseInvertedPassLogic { get; init; }

    /// <summary>Peak similarity across all white-circuit templates (0..100).</summary>
    public double PeakSimilarity { get; init; }

    /// <summary>Name of the template with highest similarity in white-circuit mode.</summary>
    public string? PeakTemplateName { get; init; }

    public int MatchedCount => RegionResults.Count(r => r.IsMatch);

    public int DifferentCount => RegionResults.Count(r => !r.IsMatch);

    public int TotalCount => RegionResults.Count;

    /// <summary>Regions where a component is considered present.</summary>
    public int PresentComponentCount => RegionResults.Count(r => r.HasComponent(UseInvertedPassLogic));

    /// <summary>Regions where a component is considered missing.</summary>
    public int AbsentComponentCount => TotalCount - PresentComponentCount;

    public bool IsFullMatch => TotalCount > 0 && PresentComponentCount == TotalCount;

    public double AverageSimilarity => TotalCount > 0
        ? RegionResults.Average(r => r.Similarity)
        : 0;
}

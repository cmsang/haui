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
    CompositeTemplateMatchResult? Match(Mat newBoard);
}

/// <summary>Kết quả so khớp tổng hợp (mỗi tên = ứng viên đầu tiên đạt ngưỡng cấu hình trong thư viện).</summary>
public sealed class CompositeTemplateMatchResult
{
    public double MatchThresholdPercent { get; init; } = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;

    public IReadOnlyList<RegionComparisonResult> RegionResults { get; init; } = [];

    public int MatchedCount => RegionResults.Count(r => r.IsMatch);

    public int DifferentCount => RegionResults.Count(r => !r.IsMatch);

    public int TotalCount => RegionResults.Count;

    public bool IsFullMatch => TotalCount > 0 && DifferentCount == 0;

    public double AverageSimilarity => TotalCount > 0
        ? RegionResults.Average(r => r.Similarity)
        : 0;
}

using Haui.GTObjectDetector.Models;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Giao diện so sánh từng vùng giữa ảnh mẫu và ảnh bo mạch mới.
/// </summary>
public interface IRegionComparisonService
{
    /// <summary>
    /// So sánh các vùng được chỉ định giữa ảnh mẫu và ảnh bo mạch mới.
    /// </summary>
    /// <param name="templateBoard">Ảnh bo mạch mẫu đã cắt.</param>
    /// <param name="newBoard">Ảnh bo mạch mới đã cắt.</param>
    /// <param name="regions">Danh sách vùng cần so sánh.</param>
    /// <returns>Danh sách kết quả so sánh.</returns>
    IReadOnlyList<RegionComparisonResult> Compare(
        Mat templateBoard,
        Mat newBoard,
        IReadOnlyList<TemplateRegion> regions);
}

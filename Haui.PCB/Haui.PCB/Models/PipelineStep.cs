using System.Windows.Media.Imaging;

namespace Haui.PCB.Models;

/// <summary>
/// Đại diện cho một bước xử lý trong pipeline phân vùng PCB.
/// Giữ tên bước và ảnh kết quả tương ứng.
/// </summary>
public sealed class PipelineStep
{
    public string StepName { get; init; } = string.Empty;
    public BitmapSource? Image { get; init; }
    public string Description { get; init; } = string.Empty;
}

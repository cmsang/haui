using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Tham số pipeline phân vùng dùng chung — cập nhật từ MainWindow, đọc bởi các service OpenCV.
/// </summary>
public static class SegmentationSettings
{
    public static SegmentationParameters Current { get; } = new();
}

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Tham số pipeline phân vùng PCB (Canny và các bước liên quan).
/// Một instance dùng chung cho toàn app — chỉnh từ MainWindow, đọc khi chạy Segment/RunSteps.
/// </summary>
public sealed class SegmentationParameters
{
    public const double DefaultCannyThreshold1 = 50;
    public const double DefaultCannyThreshold2 = 150;

    public double CannyThreshold1 { get; set; } = DefaultCannyThreshold1;
    public double CannyThreshold2 { get; set; } = DefaultCannyThreshold2;

    public void ResetToDefaults()
    {
        CannyThreshold1 = DefaultCannyThreshold1;
        CannyThreshold2 = DefaultCannyThreshold2;
    }
}

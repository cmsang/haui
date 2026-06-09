namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình phần cứng đọc từ setting.json.
/// </summary>
public class AppSetting
{
    /// <summary>Cổng COM robot (vd: COM3).</summary>
    public string Com { get; set; } = "COM3";

    public int BaudRate { get; set; } = 115200;

    /// <summary>Số bước motor trên 1 độ — STEPS_PER_DEG.</summary>
    public int StepsPerDeg { get; set; } = 100;
}

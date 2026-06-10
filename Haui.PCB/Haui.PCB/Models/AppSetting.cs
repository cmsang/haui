using System.Text.Json.Serialization;

namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình phần cứng đọc từ setting.json.
/// </summary>
public class AppSetting
{
    /// <summary>Cổng COM robot (vd: COM3).</summary>
    public string Com { get; set; } = "COM3";

    /// <summary>Cổng COM warehouse — nhận lệnh C1/C2 khi buffer đầy.</summary>
    [JsonPropertyName("warehouseCom")]
    public string WarehouseCom { get; set; } = string.Empty;

    public int BaudRate { get; set; } = 115200;

    /// <summary>Số bước motor trên 1 độ — STEPS_PER_DEG.</summary>
    public int StepsPerDeg { get; set; } = 100;

    /// <summary>Bước jog mỗi lần nhấn +/- (độ).</summary>
    public double JogStepDegrees { get; set; } = 10;

    /// <summary>Tốc độ di chuyển khi Go To (0–100%).</summary>
    public int SpeedPercent { get; set; } = 50;

    /// <summary>Chuỗi kết nối SQL Server — AGVControlSystem.</summary>
    [JsonPropertyName("DatabaseConnection")]
    public string DatabaseConnection { get; set; } =
        @"Data Source=.\SQLExpress;Initial Catalog=AGVControlSystem;Integrated Security=True;TrustServerCertificate=True";
}

using System.Text.Json.Serialization;

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Cấu hình ứng dụng — đọc/ghi từ <c>Config/setting.json</c>.
/// </summary>
public class AppSetting
{
    /// <summary>Enables developer UI (template creation, virtual serial settings).</summary>
    [JsonPropertyName("developerMode")]
    public bool DeveloperMode { get; set; }

    /// <summary>Show <c>InspectionResultWindow</c> after Dashboard inspection completes (before robot sort).</summary>
    [JsonPropertyName("showInspectionResultAfterRecognition")]
    public bool ShowInspectionResultAfterRecognition { get; set; }

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

    /// <summary>Offset J2/J3/J4 khi tính Wait PickUp / Wait OK / Wait NG (độ).</summary>
    [JsonPropertyName("waitPointJoint234OffsetDegrees")]
    public double WaitPointJoint234OffsetDegrees { get; set; } = -20;

    /// <summary>Chuỗi kết nối SQL Server.</summary>
    [JsonPropertyName("DatabaseConnection")]
    public string DatabaseConnection { get; set; } =
        @"Data Source=.\SQLExpress;Initial Catalog=AGVControlSystem;Integrated Security=True;TrustServerCertificate=True";

    [JsonPropertyName("ComponentDetection")]
    public ComponentDetectionSettings ComponentDetection { get; set; } = new();

    [JsonPropertyName("PcbBoard")]
    public PcbBoardSettings PcbBoard { get; set; } = new();

    [JsonPropertyName("CameraBasler")]
    public CameraParameters CameraBasler { get; set; } = new();

    [JsonPropertyName("CameraCapture")]
    public CameraCaptureSettings CameraCapture { get; set; } = new();

    [JsonPropertyName("CameraDownscale")]
    public ImageDownscaleSettings CameraDownscale { get; set; } = new();

    [JsonPropertyName("HolderDetectionDownscale")]
    public HolderDetectionDownscaleSettings HolderDetectionDownscale { get; set; } = new();
}

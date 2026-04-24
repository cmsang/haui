using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haui.GarlicDetector.Common;

/// <summary>DTO đại diện cho một hình chữ nhật, tương thích với System.Text.Json.</summary>
public sealed record RegionDto(int X, int Y, int Width, int Height)
{
    public Rectangle ToRectangle() => new(X, Y, Width, Height);
    public static RegionDto From(Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
}

/// <summary>DTO lưu ngưỡng HSV cho bộ phân vùng tỏi.</summary>
public sealed record HsvDto(int HMin, int HMax, int SMin, int SMax, int VMin, int VMax)
{
    /// <summary>Giá trị mặc định cho tỏi trắng/kem (bình thường).</summary>
    public static HsvDto Default => new(0, 179, 0, 60, 170, 255);

    /// <summary>
    /// Giá trị mặc định cho tỏi hỏng (nâu/tối).
    /// H=5–25: vàng→nâu nhạt; S=40–255: đủ bão hòa; V=50–175: tối hơn tỏi lành.
    /// </summary>
    public static HsvDto DefaultDamaged => new(5, 25, 40, 255, 50, 175);
}

/// <summary>
/// Cài đặt ứng dụng — lưu/tải từ file <c>settings.json</c> bên cạnh executable.
/// Sử dụng singleton <see cref="Instance"/>.
/// </summary>
public sealed class AppSettings
{
    // ─── Singleton ───────────────────────────────────────────────────────────

    private static AppSettings? _instance;

    /// <summary>Singleton instance, tự động tải từ file khi lần đầu truy cập.</summary>
    public static AppSettings Instance => _instance ??= Load();

    // ─── Đường dẫn file settings ─────────────────────────────────────────────

    private static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ─── Thuộc tính được lưu ─────────────────────────────────────────────────

    /// <summary>
    /// Vùng nhận diện tỏi (pixel, tọa độ frame camera thực tế).
    /// <c>null</c> nghĩa là nhận diện toàn bộ khung hình.
    /// </summary>
    public RegionDto? DetectionRegion { get; set; }

    /// <summary>
    /// Ngưỡng HSV cho bộ phân vùng.
    /// <c>null</c> nghĩa là dùng giá trị mặc định (<see cref="HsvDto.Default"/>).
    /// </summary>
    public HsvDto? Hsv { get; set; }

    /// <summary>
    /// Ngưỡng HSV thứ 2 dành cho tỏi hỏng (tối màu / nâu).
    /// Mask này được OR với mask chính để bắt cả tỏi hỏng không qua được ngưỡng trắng.
    /// <c>null</c> = dùng giá trị mặc định (<see cref="HsvDto.DefaultDamaged"/>).
    /// </summary>
    public HsvDto? HsvDamaged { get; set; }

    /// <summary>
    /// Thư mục lưu ảnh khi gán nhãn tỏi — nhớ lại lần chọn gần nhất.
    /// </summary>
    public string? LabelSaveFolder { get; set; }

    /// <summary>
    /// Ngưỡng circularity tối thiểu ∈ [0, 1] để một vùng được coi là tỏi hợp lệ.
    /// Vùng có circularity thấp hơn ngưỡng này sẽ không được gán nhãn.
    /// Mặc định 0.4 (vừa đủ chấp nhận tỏi oval nhẹ).
    /// </summary>
    public double MinCircularity { get; set; } = 0.4;

    // ─── Phân loại kích thước ────────────────────────────────────────────────

    /// <summary>
    /// Ngưỡng diện tích (px²) phân biệt tỏi to và tỏi nhỏ.
    /// Contour area ≥ giá trị này → Tỏi to; ngược lại → Tỏi nhỏ.
    /// Mặc định 5000 px².
    /// </summary>
    public int SizeThresholdPx { get; set; } = 5_000;

    /// <summary>
    /// Diện tích contour tối thiểu (px²) để một vùng được coi là tỏi hợp lệ.
    /// Vùng nhỏ hơn giá trị này bị loại bỏ khỏi kết quả phân vùng.
    /// Mặc định 500 px².
    /// </summary>
    public int MinContourArea { get; set; } = 500;

    /// <summary>
    /// Số pixel mở rộng thêm mỗi chiều khi crop ROI đưa vào SVM và khi vẽ khung bounding box.
    /// Giúp SVM thấy thêm ngữ cảnh xung quanh tỏi. Mặc định 20 px.
    /// </summary>
    public int RoiPaddingPx { get; set; } = 20;

    // ─── SVM / Huấn luyện ────────────────────────────────────────────────────

    /// <summary>Thư mục chứa ảnh đã gán nhãn dùng để huấn luyện SVM.</summary>
    public string? TrainDataFolder { get; set; }

    /// <summary>Đường dẫn đầy đủ đến file model SVM đã lưu (<c>.xml</c>).</summary>
    public string? SvmModelPath { get; set; }

    /// <summary>Tham số C (regularization) của SVM RBF. Mặc định 10.</summary>
    public double SvmC { get; set; } = 10.0;

    /// <summary>Tham số Gamma của SVM RBF. Mặc định 0.5.</summary>
    public double SvmGamma { get; set; } = 0.5;

    /// <summary>Kích thước ảnh ROI (cạnh) khi trích xuất đặc trưng. Mặc định 128.</summary>
    public int SvmTrainImageSize { get; set; } = 128;

    // ─── Robot / Serial Communication ────────────────────────────────────────

    /// <summary>Tên cổng COM để kết nối với robot (ví dụ: "COM3"). Mặc định "COM3".</summary>
    public string RobotPortName { get; set; } = "COM3";

    /// <summary>Tốc độ baud rate kết nối với robot. Mặc định 9600.</summary>
    public int RobotBaudRate { get; set; } = 9600;

    // ─── Camera / UI ─────────────────────────────────────────────────────────

    /// <summary>Label độ phân giải đã chọn lần cuối (ví dụ: "640 × 480"). Null = dùng mặc định.</summary>
    public string? LastResolutionLabel { get; set; }

    /// <summary>Bật/tắt chế độ nhận diện tự động (true = tự động, false = nhấn nút thủ công).</summary>
    public bool AutoDetect { get; set; } = true;

    // ─── Load / Save ─────────────────────────────────────────────────────────

    /// <summary>Đọc cài đặt từ <c>settings.json</c>. Trả về instance rỗng nếu file không tồn tại.</summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
                       ?? new AppSettings();
            }
        }
        catch
        {
            // File bị lỗi → dùng cài đặt mặc định, không crash app
        }

        return new AppSettings();
    }

    /// <summary>Ghi cài đặt hiện tại vào <c>settings.json</c>.</summary>
    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Lỗi ghi file — không crash app
        }
    }

    // ─── Helper lấy Rectangle ────────────────────────────────────────────────

    /// <summary>Trả về vùng nhận diện dưới dạng <see cref="Rectangle"/>; <c>null</c> nếu chưa thiết lập.</summary>
    [JsonIgnore]
    public Rectangle? DetectionRectangle => DetectionRegion?.ToRectangle();
}

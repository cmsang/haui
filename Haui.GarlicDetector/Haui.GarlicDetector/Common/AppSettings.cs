using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haui.GarlicDetector.Common;

/// <summary>DTO đại diện cho một hình chữ nhật, tương thích với System.Text.Json.</summary>
public sealed record RegionDto(int X, int Y, int Width, int Height)
{
    public Rectangle ToRectangle() => new(X, Y, Width, Height);
    public static RegionDto From(Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
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

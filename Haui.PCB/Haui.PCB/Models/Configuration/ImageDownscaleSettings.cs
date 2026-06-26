namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Cấu hình hạ kích thước ảnh sau khi grab — section <c>CameraDownscale</c> trong <c>setting.json</c>.
/// Khi bật, ảnh được thu nhỏ (giữ tỉ lệ) để vừa khung <see cref="Width"/>×<see cref="Height"/> trước khi xử lý tiếp.
/// </summary>
public sealed class ImageDownscaleSettings
{
    public const int DefaultWidth = 1920;
    public const int DefaultHeight = 1080;

    /// <summary>Bật/tắt hạ kích thước ảnh. Mặc định tắt.</summary>
    public bool Enabled { get; set; }

    /// <summary>Chiều rộng tối đa (px) khi hạ kích thước.</summary>
    public int Width { get; set; } = DefaultWidth;

    /// <summary>Chiều cao tối đa (px) khi hạ kích thước.</summary>
    public int Height { get; set; } = DefaultHeight;

    public ImageDownscaleSettings Clone() => new()
    {
        Enabled = Enabled,
        Width = Width,
        Height = Height
    };
}

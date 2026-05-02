namespace Haui.PCB.Models;

/// <summary>
/// Một vùng mẫu trên ảnh bo mạch.
/// Tọa độ lưu theo tỉ lệ tương đối (0..1) so với kích thước ảnh bo mạch.
/// </summary>
public class TemplateRegion
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Tọa độ X tương đối (0..1).</summary>
    public double RelX { get; set; }

    /// <summary>Tọa độ Y tương đối (0..1).</summary>
    public double RelY { get; set; }

    /// <summary>Chiều rộng tương đối (0..1).</summary>
    public double RelWidth { get; set; }

    /// <summary>Chiều cao tương đối (0..1).</summary>
    public double RelHeight { get; set; }
}

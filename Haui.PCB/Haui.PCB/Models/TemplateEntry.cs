namespace Haui.PCB.Models;

/// <summary>
/// Một mục ảnh mẫu trong thư viện mẫu.
/// </summary>
public class TemplateEntry
{
    /// <summary>Tên hiển thị của mẫu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Đường dẫn file ảnh bo mạch mẫu.</summary>
    public string BoardImagePath { get; set; } = string.Empty;

    /// <summary>Danh sách vùng được chọn trên mẫu này.</summary>
    public List<TemplateRegion> Regions { get; set; } = [];
}

namespace Haui.PCB.Models.Templates;

/// <summary>
/// File <c>{ảnh_mẫu}_regions.json</c> — tên hiển thị và danh sách vùng (không dùng index.json).
/// </summary>
public class TemplateRegionsDocument
{
    public string Name { get; set; } = string.Empty;

    public List<TemplateRegion> Regions { get; set; } = [];
}

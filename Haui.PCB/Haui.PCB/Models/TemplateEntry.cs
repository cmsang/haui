using System.Text.Json.Serialization;

namespace Haui.PCB.Models;

/// <summary>
/// Một mục ảnh mẫu trong thư viện (bộ nhớ). Vùng lưu file riêng, không nằm trong index.json.
/// </summary>
public class TemplateEntry
{
    /// <summary>Tên hiển thị của mẫu.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Đường dẫn file ảnh bo mạch mẫu.</summary>
    public string BoardImagePath { get; set; } = string.Empty;

    /// <summary>Đường dẫn file JSON danh sách vùng.</summary>
    public string RegionsFilePath { get; set; } = string.Empty;

    /// <summary>Danh sách vùng (nạp từ file phụ khi mở thư viện).</summary>
    [JsonIgnore]
    public List<TemplateRegion> Regions { get; set; } = [];
}

using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Lưu/tải danh sách vùng mẫu từ file JSON.
/// </summary>
public class TemplateRegionService : ITemplateRegionService
{
    private const string FilePath = "template_regions.json";

    public IReadOnlyList<TemplateRegion> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<TemplateRegion>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<TemplateRegion> regions)
    {
        try
        {
            var json = JsonSerializer.Serialize(regions.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }
}

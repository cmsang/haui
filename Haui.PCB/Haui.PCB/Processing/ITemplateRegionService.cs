using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện lưu/tải danh sách vùng mẫu.
/// </summary>
public interface ITemplateRegionService
{
    /// <summary>Tải danh sách vùng mẫu từ file.</summary>
    IReadOnlyList<TemplateRegion> Load();

    /// <summary>Lưu danh sách vùng mẫu xuống file.</summary>
    void Save(IEnumerable<TemplateRegion> regions);
}

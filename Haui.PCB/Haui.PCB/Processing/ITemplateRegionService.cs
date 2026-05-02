using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện lưu/tải danh sách vùng mẫu và ảnh bo mạch mẫu.
/// </summary>
public interface ITemplateRegionService
{
    /// <summary>Tải danh sách vùng mẫu từ file.</summary>
    IReadOnlyList<TemplateRegion> Load();

    /// <summary>Lưu danh sách vùng mẫu xuống file.</summary>
    void Save(IEnumerable<TemplateRegion> regions);

    /// <summary>Lưu ảnh bo mạch mẫu.</summary>
    void SaveBoardImage(OpenCvSharp.Mat boardImage);

    /// <summary>Tải ảnh bo mạch mẫu. Trả về null nếu chưa có.</summary>
    OpenCvSharp.Mat? LoadBoardImage();
}

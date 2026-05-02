using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện quản lý thư viện nhiều ảnh mẫu.
/// </summary>
public interface ITemplateLibraryService
{
    /// <summary>Tải danh sách tất cả mẫu từ thư viện.</summary>
    IReadOnlyList<TemplateEntry> LoadAll();

    /// <summary>Lưu toàn bộ danh sách mẫu.</summary>
    void SaveAll(IEnumerable<TemplateEntry> entries);

    /// <summary>Lưu ảnh bo mạch cho một mẫu và trả về đường dẫn file.</summary>
    string SaveBoardImage(string templateName, Mat boardImage);

    /// <summary>Tải ảnh bo mạch của một mẫu. Trả về null nếu không có.</summary>
    Mat? LoadBoardImage(string boardImagePath);
}

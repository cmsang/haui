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

    /// <summary>Thư mục thư viện đang dùng (mặc định <c>templates/</c> hoặc thư mục tùy chỉnh).</summary>
    string GetLibraryFolder();

    /// <summary>Cấu hình lưu thư viện — ghi <c>component_template_settings.json</c>.</summary>
    void ConfigureStorage(bool useCustomFolder, string? customFolder);

    /// <summary>Đọc cấu hình lưu hiện tại.</summary>
    (bool UseCustom, string Folder) GetStorageConfiguration();
}

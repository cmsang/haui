using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện quản lý thư viện nhiều ảnh mẫu.
/// </summary>
public interface ITemplateLibraryService
{
    /// <summary>Quét thư mục thư viện: mỗi <c>.png</c> kèm <c>*_regions.json</c>.</summary>
    IReadOnlyList<TemplateEntry> LoadAll();

    /// <summary>Ghi file vùng của từng mẫu (không tạo index.json).</summary>
    void SaveAll(IEnumerable<TemplateEntry> entries);

    /// <summary>Lưu ảnh bo mạch cho một mẫu và trả về đường dẫn file.</summary>
    string SaveBoardImage(string templateName, Mat boardImage);

    /// <summary>Tải ảnh bo mạch của một mẫu. Trả về null nếu không có.</summary>
    Mat? LoadBoardImage(string boardImagePath);

    /// <summary>Đường dẫn file vùng đi kèm ảnh bo mạch (<c>{base}_regions.json</c>).</summary>
    string GetRegionsFilePathForBoardImage(string boardImagePath);

    /// <summary>Lưu tên mẫu và danh sách vùng ra file JSON.</summary>
    void SaveRegions(string regionsFilePath, string templateName, IEnumerable<TemplateRegion> regions);

    /// <summary>Thư mục thư viện đang dùng (mặc định <c>templates/</c> hoặc thư mục tùy chỉnh).</summary>
    string GetLibraryFolder();

    /// <summary>Cấu hình lưu thư viện — ghi <c>appsettings.json</c> → ComponentTemplates.</summary>
    void ConfigureStorage(bool useCustomFolder, string? customFolder);

    /// <summary>Đọc cấu hình lưu hiện tại.</summary>
    (bool UseCustom, string Folder) GetStorageConfiguration();
}

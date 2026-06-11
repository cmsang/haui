using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

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

    /// <summary>Thư mục thư viện đang dùng (<c>setting.json</c> → ComponentTemplates.CustomFolder).</summary>
    string GetLibraryFolder();
}

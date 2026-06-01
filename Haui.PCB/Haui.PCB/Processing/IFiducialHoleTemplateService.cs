using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Lưu/tải thư viện mẫu lỗ tròn (nhiều ảnh, cùng hình dạng) và cấu hình thư mục.
/// </summary>
public interface IFiducialHoleTemplateService
{
    FiducialHoleSettings LoadSettings();
    void SaveSettings(FiducialHoleSettings settings);
    string GetTemplateFolder();

    /// <summary>Đã có ít nhất một file mẫu trong thư mục.</summary>
    bool HasTemplates();

    IReadOnlyList<string> ListTemplateFileNames();
    IReadOnlyList<Mat> LoadTemplates();
    string SaveTemplate(Mat template);
    void DeleteTemplate(string fileName);
}

using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

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

    /// <summary>Recognition counts keyed by template file name (missing entries = 0).</summary>
    IReadOnlyDictionary<string, int> GetRecognitionCounts();

    /// <summary>Templates sorted by recognition count descending; caller owns cloned <see cref="Mat"/> instances.</summary>
    IReadOnlyList<FiducialTemplateEntry> LoadTemplateEntries();

    IReadOnlyList<Mat> LoadTemplates();

    void UpdateRecognitionStats(IReadOnlyList<FiducialTemplateRecognitionOutcome> outcomes);

    string SaveTemplate(Mat template);
    void DeleteTemplate(string fileName);
}

using System.IO;
using System.Text.Json;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Quản lý thư viện nhiều ảnh mẫu — lưu trữ dưới thư mục "templates/".
/// File index: templates/index.json
/// Ảnh bo mạch: templates/{templateName}_{timestamp}.png
/// </summary>
public class TemplateLibraryService : ITemplateLibraryService
{
    private const string TemplateDir = "templates";
    private const string IndexFile = "templates/index.json";

    public IReadOnlyList<TemplateEntry> LoadAll()
    {
        try
        {
            if (!File.Exists(IndexFile)) return [];
            var json = File.ReadAllText(IndexFile);
            return JsonSerializer.Deserialize<List<TemplateEntry>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void SaveAll(IEnumerable<TemplateEntry> entries)
    {
        try
        {
            Directory.CreateDirectory(TemplateDir);
            var json = JsonSerializer.Serialize(entries.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(IndexFile, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public string SaveBoardImage(string templateName, Mat boardImage)
    {
        Directory.CreateDirectory(TemplateDir);
        // Tạo tên file an toàn từ tên mẫu
        var safeName = string.Concat(templateName.Select(c =>
            Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePath = Path.Combine(TemplateDir, $"{safeName}_{timestamp}.png");
        Cv2.ImWrite(filePath, boardImage);
        return filePath;
    }

    public Mat? LoadBoardImage(string boardImagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(boardImagePath) || !File.Exists(boardImagePath))
                return null;
            var mat = Cv2.ImRead(boardImagePath, ImreadModes.Color);
            return mat.Empty() ? null : mat;
        }
        catch
        {
            return null;
        }
    }
}

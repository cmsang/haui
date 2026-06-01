using System.IO;
using System.Text.Json;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Quản lý thư viện nhiều ảnh mẫu — mặc định <c>templates/</c>, có thể đổi thư mục qua cấu hình.
/// File index: {thư mục}/index.json
/// Ảnh bo mạch: {thư mục}/{templateName}_{timestamp}.png
/// </summary>
public class TemplateLibraryService : ITemplateLibraryService
{
    public string GetLibraryFolder()
    {
        var settings = ComponentTemplateSettingsStore.Load();
        return ResolveLibraryFolder(settings);
    }

    public void ConfigureStorage(bool useCustomFolder, string? customFolder)
    {
        var settings = ComponentTemplateSettingsStore.Load();
        settings.UseCustomFolder = useCustomFolder;
        settings.CustomFolder = customFolder?.Trim() ?? string.Empty;
        ComponentTemplateSettingsStore.Save(settings);
    }

    public (bool UseCustom, string Folder) GetStorageConfiguration()
    {
        var settings = ComponentTemplateSettingsStore.Load();
        var folder = settings.UseCustomFolder && !string.IsNullOrWhiteSpace(settings.CustomFolder)
            ? settings.CustomFolder
            : ComponentTemplateSettings.DefaultLibraryFolder;
        return (settings.UseCustomFolder, folder);
    }

    private static string ResolveLibraryFolder(ComponentTemplateSettings settings)
    {
        if (settings.UseCustomFolder && !string.IsNullOrWhiteSpace(settings.CustomFolder))
            return settings.CustomFolder;
        return ComponentTemplateSettings.DefaultLibraryFolder;
    }

    private string IndexFilePath => Path.Combine(GetLibraryFolder(), "index.json");

    public IReadOnlyList<TemplateEntry> LoadAll()
    {
        try
        {
            var indexFile = IndexFilePath;
            if (!File.Exists(indexFile)) return [];
            var json = File.ReadAllText(indexFile);
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
            var folder = GetLibraryFolder();
            Directory.CreateDirectory(folder);
            var json = JsonSerializer.Serialize(entries.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(IndexFilePath, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public string SaveBoardImage(string templateName, Mat boardImage)
    {
        var folder = GetLibraryFolder();
        Directory.CreateDirectory(folder);
        var safeName = string.Concat(templateName.Select(c =>
            Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePath = Path.Combine(folder, $"{safeName}_{timestamp}.png");
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

using System.IO;
using System.Text.Json;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Lưu/tải danh sách vùng mẫu từ file JSON, và lưu/tải ảnh bo mạch mẫu.
/// Mặc định file ở CWD; khi bật thư mục tùy chỉnh thì lưu cùng thư mục thư viện.
/// </summary>
public class TemplateRegionService : ITemplateRegionService
{
    private const string RegionsFileName = "template_regions.json";
    private const string BoardImageFileName = "template_board.png";

    public void ConfigureStorage(bool useCustomFolder, string? customFolder)
    {
        var settings = ComponentTemplateSettingsStore.Load();
        settings.UseCustomFolder = useCustomFolder;
        settings.CustomFolder = customFolder?.Trim() ?? string.Empty;
        ComponentTemplateSettingsStore.Save(settings);
    }

    private static string RegionsFilePath
    {
        get
        {
            var folder = GetActiveDataFolder();
            return string.IsNullOrEmpty(folder)
                ? RegionsFileName
                : Path.Combine(folder, RegionsFileName);
        }
    }

    private static string BoardImageFilePath
    {
        get
        {
            var folder = GetActiveDataFolder();
            return string.IsNullOrEmpty(folder)
                ? BoardImageFileName
                : Path.Combine(folder, BoardImageFileName);
        }
    }

    /// <summary>Thư mục chứa mẫu active; rỗng = CWD (hành vi cũ).</summary>
    private static string GetActiveDataFolder()
    {
        var settings = ComponentTemplateSettingsStore.Load();
        if (settings.UseCustomFolder && !string.IsNullOrWhiteSpace(settings.CustomFolder))
            return settings.CustomFolder;
        return string.Empty;
    }

    public IReadOnlyList<TemplateRegion> Load()
    {
        try
        {
            var path = RegionsFilePath;
            if (!File.Exists(path)) return [];
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<TemplateRegion>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<TemplateRegion> regions)
    {
        try
        {
            var path = RegionsFilePath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(regions.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public void SaveBoardImage(Mat boardImage)
    {
        try
        {
            var path = BoardImageFilePath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            Cv2.ImWrite(path, boardImage);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public Mat? LoadBoardImage()
    {
        try
        {
            var path = BoardImageFilePath;
            if (!File.Exists(path)) return null;
            var mat = Cv2.ImRead(path, ImreadModes.Color);
            return mat.Empty() ? null : mat;
        }
        catch
        {
            return null;
        }
    }
}

using System.IO;
using System.Text.Json;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Thư viện mẫu trong một thư mục — không dùng index.json.
/// Mỗi mẫu = <c>{name}_{timestamp}.png</c> + <c>{name}_{timestamp}_regions.json</c> (tên + vùng trong file JSON).
/// </summary>
public class TemplateLibraryService : ITemplateLibraryService
{
    private const string LegacyIndexFileName = "index.json";
    private const string RegionsFileSuffix = "_regions.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string GetLibraryFolder()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        return ResolveLibraryFolder(settings);
    }

    public void ConfigureStorage(bool useCustomFolder, string? customFolder)
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        settings.UseCustomFolder = useCustomFolder;
        settings.CustomFolder = customFolder?.Trim() ?? string.Empty;
        AppSettingsStore.SaveComponentTemplates(settings);
    }

    public (bool UseCustom, string Folder) GetStorageConfiguration()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
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

    public IReadOnlyList<TemplateEntry> LoadAll()
    {
        try
        {
            var folder = GetLibraryFolder();
            if (!Directory.Exists(folder))
                return [];

            MigrateLegacyIndexIfPresent(folder);

            var entries = new List<TemplateEntry>();
            foreach (var pngPath in Directory.EnumerateFiles(folder, "*.png"))
            {
                var regionsPath = GetRegionsFilePathForBoardImage(pngPath);
                var document = TryLoadRegionsDocument(regionsPath);
                var displayName = !string.IsNullOrWhiteSpace(document.Name)
                    ? document.Name
                    : Path.GetFileNameWithoutExtension(pngPath);

                entries.Add(new TemplateEntry
                {
                    Name = displayName,
                    BoardImagePath = pngPath,
                    RegionsFilePath = regionsPath,
                    Regions = document.Regions
                });
            }

            entries.Sort((a, b) =>
                string.Compare(b.BoardImagePath, a.BoardImagePath, StringComparison.OrdinalIgnoreCase));

            return entries;
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

            foreach (var entry in entries)
            {
                var regionsPath = ResolveRegionsFilePath(entry);
                SaveRegions(regionsPath, entry.Name, entry.Regions);
                entry.RegionsFilePath = regionsPath;
            }
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

    public string GetRegionsFilePathForBoardImage(string boardImagePath)
    {
        var dir = Path.GetDirectoryName(boardImagePath) ?? GetLibraryFolder();
        var baseName = Path.GetFileNameWithoutExtension(boardImagePath);
        return Path.Combine(dir, $"{baseName}{RegionsFileSuffix}");
    }

    public void SaveRegions(string regionsFilePath, string templateName, IEnumerable<TemplateRegion> regions)
    {
        try
        {
            var dir = Path.GetDirectoryName(regionsFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var document = new TemplateRegionsDocument
            {
                Name = templateName,
                Regions = regions.ToList()
            };
            var json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(regionsFilePath, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    private TemplateRegionsDocument TryLoadRegionsDocument(string regionsFilePath)
    {
        if (string.IsNullOrEmpty(regionsFilePath) || !File.Exists(regionsFilePath))
            return new TemplateRegionsDocument();

        try
        {
            var json = File.ReadAllText(regionsFilePath);
            var document = JsonSerializer.Deserialize<TemplateRegionsDocument>(json);
            if (document is not null && document.Regions.Count > 0)
                return document;

            // Định dạng cũ: mảng vùng thuần, không có trường Name
            var legacyRegions = JsonSerializer.Deserialize<List<TemplateRegion>>(json);
            if (legacyRegions is { Count: > 0 })
            {
                return new TemplateRegionsDocument
                {
                    Name = string.Empty,
                    Regions = legacyRegions
                };
            }

            return document ?? new TemplateRegionsDocument();
        }
        catch
        {
            return new TemplateRegionsDocument();
        }
    }

    private string ResolveRegionsFilePath(TemplateEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.RegionsFilePath))
            return entry.RegionsFilePath;

        if (!string.IsNullOrWhiteSpace(entry.BoardImagePath))
            return GetRegionsFilePathForBoardImage(entry.BoardImagePath);

        return Path.Combine(GetLibraryFolder(), $"{Guid.NewGuid():N}{RegionsFileSuffix}");
    }

    private void MigrateLegacyIndexIfPresent(string folder)
    {
        var indexPath = Path.Combine(folder, LegacyIndexFileName);
        if (!File.Exists(indexPath))
            return;

        try
        {
            var json = File.ReadAllText(indexPath);
            var rows = JsonSerializer.Deserialize<List<LegacyIndexRow>>(json);
            if (rows is null)
                return;

            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.BoardImagePath))
                    continue;

                var regionsPath = !string.IsNullOrWhiteSpace(row.RegionsFilePath)
                    ? row.RegionsFilePath
                    : GetRegionsFilePathForBoardImage(row.BoardImagePath);

                List<TemplateRegion> regions;
                if (row.Regions is { Count: > 0 })
                    regions = row.Regions;
                else if (File.Exists(regionsPath))
                    regions = TryLoadRegionsDocument(regionsPath).Regions;
                else
                    regions = [];

                SaveRegions(regionsPath, row.Name, regions);
            }

            File.Delete(indexPath);
        }
        catch { /* giữ index nếu migrate thất bại */ }
    }

    /// <summary>Chỉ dùng khi migrate <c>index.json</c> cũ.</summary>
    private sealed class LegacyIndexRow
    {
        public string Name { get; set; } = string.Empty;
        public string BoardImagePath { get; set; } = string.Empty;
        public string RegionsFilePath { get; set; } = string.Empty;
        public List<TemplateRegion>? Regions { get; set; }
    }
}

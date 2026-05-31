using System.IO;
using System.Text.Json;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Quản lý thư viện mẫu lỗ tròn (<c>hole_*.png</c>) — nhiều ảnh mẫu, cùng hình dạng lỗ.
/// </summary>
public class FiducialHoleTemplateService : IFiducialHoleTemplateService
{
    private const string SettingsFileName = "fiducial_settings.json";
    private const string TemplateSearchPattern = "hole_*.png";

    /// <summary>Số lỗ định vị cần tìm trên bo mạch (không phụ thuộc số file mẫu).</summary>
    public const int RequiredDetectionCount = 4;

    public FiducialHoleSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFileName)) return new FiducialHoleSettings();
            var json = File.ReadAllText(SettingsFileName);
            return JsonSerializer.Deserialize<FiducialHoleSettings>(json) ?? new FiducialHoleSettings();
        }
        catch
        {
            return new FiducialHoleSettings();
        }
    }

    public void SaveSettings(FiducialHoleSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFileName, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public string GetTemplateFolder()
    {
        var settings = LoadSettings();
        return string.IsNullOrWhiteSpace(settings.TemplateFolder)
            ? FiducialHoleSettings.DefaultTemplateFolder
            : settings.TemplateFolder;
    }

    public void SetTemplateFolder(string folderPath)
    {
        var settings = LoadSettings();
        settings.TemplateFolder = folderPath;
        SaveSettings(settings);
    }

    public bool HasTemplates() => ListTemplateFileNames().Count > 0;

    public IReadOnlyList<string> ListTemplateFileNames()
    {
        var folder = GetTemplateFolder();
        if (!Directory.Exists(folder)) return [];

        return Directory.GetFiles(folder, TemplateSearchPattern)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<Mat> LoadTemplates()
    {
        var folder = GetTemplateFolder();
        var templates = new List<Mat>();

        foreach (var fileName in ListTemplateFileNames())
        {
            var path = Path.Combine(folder, fileName);
            var mat = Cv2.ImRead(path, ImreadModes.Grayscale);
            if (mat.Empty())
            {
                mat.Dispose();
                continue;
            }

            templates.Add(mat);
        }

        return templates;
    }

    public string SaveTemplate(Mat template)
    {
        if (template.Empty())
            throw new ArgumentException("Mẫu lỗ tròn rỗng.", nameof(template));

        var folder = GetTemplateFolder();
        Directory.CreateDirectory(folder);

        var fileName = $"hole_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        Cv2.ImWrite(Path.Combine(folder, fileName), template);
        return fileName;
    }

    public void DeleteTemplate(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
            return;

        var path = Path.Combine(GetTemplateFolder(), fileName);
        if (File.Exists(path))
            File.Delete(path);
    }
}

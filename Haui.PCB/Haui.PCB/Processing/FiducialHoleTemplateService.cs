using System.IO;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Quản lý thư viện mẫu lỗ tròn (<c>hole_*.png</c>) — cache RAM, nhiều ảnh mẫu cùng hình dạng lỗ.
/// </summary>
public class FiducialHoleTemplateService : IFiducialHoleTemplateService
{
    private const string TemplateSearchPattern = "hole_*.png";

    /// <summary>Số lỗ định vị cần tìm trên bo mạch (không phụ thuộc số file mẫu).</summary>
    public const int RequiredDetectionCount = 4;

    private readonly List<(string FileName, Mat Template)> _cache = [];
    private string? _cachedFolder;
    private bool _cacheValid;

    public FiducialHoleSettings LoadSettings() => AppSettingsStore.LoadFiducialHoles();

    public void SaveSettings(FiducialHoleSettings settings) => AppSettingsStore.SaveFiducialHoles(settings);

    public string GetTemplateFolder()
    {
        var settings = LoadSettings();
        return string.IsNullOrWhiteSpace(settings.TemplateFolder)
            ? FiducialHoleSettings.DefaultTemplateFolder
            : settings.TemplateFolder;
    }

    public bool HasTemplates()
    {
        EnsureCacheLoaded();
        return _cache.Count > 0;
    }

    public IReadOnlyList<string> ListTemplateFileNames()
    {
        EnsureCacheLoaded();
        return _cache.Select(c => c.FileName).ToList();
    }

    public IReadOnlyList<Mat> LoadTemplates()
    {
        EnsureCacheLoaded();
        return _cache.Select(c => c.Template.Clone()).ToList();
    }

    public string SaveTemplate(Mat template)
    {
        if (template.Empty())
            throw new ArgumentException("Mẫu lỗ tròn rỗng.", nameof(template));

        var folder = GetTemplateFolder();
        Directory.CreateDirectory(folder);

        var fileName = $"hole_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        Cv2.ImWrite(Path.Combine(folder, fileName), template);
        InvalidateCache();
        return fileName;
    }

    public void DeleteTemplate(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains(".."))
            return;

        var path = Path.Combine(GetTemplateFolder(), fileName);
        if (File.Exists(path))
            File.Delete(path);

        InvalidateCache();
    }

    private void EnsureCacheLoaded()
    {
        var folder = GetTemplateFolder();
        if (_cacheValid && _cachedFolder == folder)
            return;

        InvalidateCache();
        _cachedFolder = folder;

        if (!Directory.Exists(folder))
        {
            _cacheValid = true;
            return;
        }

        foreach (var path in Directory.GetFiles(folder, TemplateSearchPattern).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName)) continue;

            var mat = Cv2.ImRead(path, ImreadModes.Grayscale);
            if (mat.Empty())
            {
                mat.Dispose();
                continue;
            }

            _cache.Add((fileName, mat));
        }

        _cacheValid = true;
    }

    private void InvalidateCache()
    {
        foreach (var (_, mat) in _cache)
            mat.Dispose();
        _cache.Clear();
        _cacheValid = false;
        _cachedFolder = null;
    }
}

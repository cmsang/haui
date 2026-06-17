using System.IO;
using System.Text.Json;
using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Quản lý thư viện mẫu lỗ tròn (<c>hole_*.png</c>) — cache RAM, nhiều ảnh mẫu cùng hình dạng lỗ.
/// </summary>
public class FiducialHoleTemplateService : IFiducialHoleTemplateService
{
    private const string TemplateSearchPattern = "hole_*.png";
    private const string RecognitionStatsFileName = "hole_recognition_stats.json";
    private const int RecognitionScoreModulus = 100_000_000;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

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

    public IReadOnlyDictionary<string, int> GetRecognitionCounts()
    {
        EnsureCacheLoaded();
        var stats = LoadStats();
        return _cache.ToDictionary(
            c => c.FileName,
            c => stats.GetValueOrDefault(c.FileName, 0));
    }

    public IReadOnlyList<FiducialTemplateEntry> LoadTemplateEntries()
    {
        EnsureCacheLoaded();
        var stats = LoadStats();

        return _cache
            .Select(c => new FiducialTemplateEntry
            {
                FileName = c.FileName,
                RecognitionCount = stats.GetValueOrDefault(c.FileName, 0),
                Template = c.Template.Clone()
            })
            .OrderByDescending(e => e.RecognitionCount)
            .ThenBy(e => e.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<Mat> LoadTemplates()
    {
        var entries = LoadTemplateEntries();
        try
        {
            return entries.Select(e => e.Template).ToList();
        }
        catch
        {
            foreach (var entry in entries)
                entry.Template.Dispose();
            throw;
        }
    }

    public void UpdateRecognitionStats(IReadOnlyList<FiducialTemplateRecognitionOutcome> outcomes)
    {
        if (outcomes.Count == 0)
            return;

        var stats = LoadStats();
        foreach (var outcome in outcomes)
        {
            if (string.IsNullOrWhiteSpace(outcome.FileName) || outcome.RecognizedHoleCount <= 0)
                continue;

            var current = stats.GetValueOrDefault(outcome.FileName, 0);
            stats[outcome.FileName] = (current + outcome.RecognizedHoleCount) % RecognitionScoreModulus;
        }

        SaveStats(stats);
    }

    public string SaveTemplate(Mat template)
    {
        if (template.Empty())
            throw new ArgumentException("Mẫu lỗ tròn rỗng.", nameof(template));

        var folder = GetTemplateFolder();
        Directory.CreateDirectory(folder);

        var fileName = $"hole_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        Cv2.ImWrite(Path.Combine(folder, fileName), template);

        var stats = LoadStats();
        stats[fileName] = 0;
        SaveStats(stats);

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

        var stats = LoadStats();
        if (stats.Remove(fileName))
            SaveStats(stats);

        InvalidateCache();
    }

    private string GetStatsFilePath() => Path.Combine(GetTemplateFolder(), RecognitionStatsFileName);

    private Dictionary<string, int> LoadStats()
    {
        var path = GetStatsFilePath();
        if (!File.Exists(path))
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOptions);
            if (loaded is null)
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var stats = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var (fileName, count) in loaded)
                stats[fileName] = Math.Max(0, count);

            return stats;
        }
        catch
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void SaveStats(Dictionary<string, int> stats)
    {
        var folder = GetTemplateFolder();
        Directory.CreateDirectory(folder);
        var path = GetStatsFilePath();
        var json = JsonSerializer.Serialize(stats, JsonOptions);
        File.WriteAllText(path, json);
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

using System.IO;

namespace Haui.PCB.Processing;

/// <summary>
/// Đường dẫn file cấu hình — lưu trong thư mục Config/ của project, không nằm trong bin/.
/// </summary>
public static class AppConfigPaths
{
    private static string? _configDir;

    public static string ConfigDirectory => _configDir ??= ResolveConfigDirectory();

    public static string SettingFile => Path.Combine(ConfigDirectory, "setting.json");

    private static string ResolveConfigDirectory()
    {
        var env = Environment.GetEnvironmentVariable("HAUI_PCB_CONFIG");
        if (!string.IsNullOrWhiteSpace(env))
        {
            Directory.CreateDirectory(env);
            return env;
        }

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Config");
            if (Directory.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Haui.PCB", "Config");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    /// <summary>Chuyển file cũ từ bin/ sang Config/ nếu có.</summary>
    public static void MigrateLegacyFile(string fileName)
    {
        var target = Path.Combine(ConfigDirectory, fileName);
        if (File.Exists(target)) return;

        var legacy = Path.Combine(AppContext.BaseDirectory, fileName);
        if (!File.Exists(legacy)) return;

        Directory.CreateDirectory(ConfigDirectory);
        File.Copy(legacy, target);
    }

    /// <summary>Thư mục có thể chứa file JSON cấu hình cũ (CWD, bin, walk-up).</summary>
    public static IEnumerable<string> LegacySearchDirectories() => EnumerateLegacyDirectories();

    /// <summary>Tìm file cấu hình cũ theo tên (ưu tiên gần exe, rồi walk-up).</summary>
    public static IEnumerable<string> FindLegacyConfigFiles(string fileName)
    {
        foreach (var dir in EnumerateLegacyDirectories())
        {
            var path = Path.Combine(dir, fileName);
            if (File.Exists(path))
                yield return path;
        }
    }

    private static IEnumerable<string> EnumerateLegacyDirectories()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (seen.Add(Path.GetFullPath(Directory.GetCurrentDirectory())))
            yield return Directory.GetCurrentDirectory();

        var baseDir = AppContext.BaseDirectory;
        if (seen.Add(Path.GetFullPath(baseDir)))
            yield return baseDir;

        var dirInfo = new DirectoryInfo(baseDir);
        while (dirInfo != null)
        {
            if (seen.Add(dirInfo.FullName))
                yield return dirInfo.FullName;
            dirInfo = dirInfo.Parent;
        }
    }
}

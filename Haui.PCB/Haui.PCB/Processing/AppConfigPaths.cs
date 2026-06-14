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
}

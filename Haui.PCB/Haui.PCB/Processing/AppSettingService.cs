using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Đọc / ghi setting.json — COM và STEPS_PER_DEG.
/// </summary>
public class AppSettingService : IAppSettingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSetting Load()
    {
        AppConfigPaths.MigrateLegacyFile("setting.json");

        try
        {
            if (!File.Exists(AppConfigPaths.SettingFile))
                return CreateDefaultFile();

            var json = File.ReadAllText(AppConfigPaths.SettingFile);
            return JsonSerializer.Deserialize<AppSetting>(json, JsonOptions)
                   ?? CreateDefaultFile();
        }
        catch
        {
            return new AppSetting();
        }
    }

    public void Save(AppSetting setting)
    {
        try
        {
            Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
            var json = JsonSerializer.Serialize(setting, JsonOptions);
            File.WriteAllText(AppConfigPaths.SettingFile, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    private static AppSetting CreateDefaultFile()
    {
        var setting = new AppSetting();
        try
        {
            Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
            var json = JsonSerializer.Serialize(setting, JsonOptions);
            File.WriteAllText(AppConfigPaths.SettingFile, json);
        }
        catch { /* bỏ qua */ }

        return setting;
    }
}

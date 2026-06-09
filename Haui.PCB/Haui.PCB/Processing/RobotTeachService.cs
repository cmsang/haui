using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Lưu / tải cấu hình teach robot từ robot_teach_config.json.
/// </summary>
public class RobotTeachService : IRobotTeachService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RobotTeachConfig Load()
    {
        AppConfigPaths.MigrateLegacyFile("robot_teach_config.json");

        try
        {
            if (!File.Exists(AppConfigPaths.RobotTeachFile))
                return new RobotTeachConfig();

            var json = File.ReadAllText(AppConfigPaths.RobotTeachFile);
            var config = JsonSerializer.Deserialize<RobotTeachConfig>(json, JsonOptions)
                           ?? new RobotTeachConfig();
            config.TeachPoints = RobotTeachPositions.Normalize(config.TeachPoints);
            return config;
        }
        catch
        {
            return new RobotTeachConfig();
        }
    }

    public void Save(RobotTeachConfig config)
    {
        try
        {
            Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(AppConfigPaths.RobotTeachFile, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }
}

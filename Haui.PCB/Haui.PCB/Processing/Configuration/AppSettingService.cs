using System.IO;
using System.Text.Json;
using Haui.PCB.Processing;

namespace Haui.PCB.Processing.Configuration;

/// <summary>
/// Đọc / ghi <c>Config/setting.json</c> — robot, camera, thư viện mẫu, lỗ định vị.
/// </summary>
public class AppSettingService : IAppSettingService
{
    private const string LegacyAppsettingsFile = "appsettings.json";
    private const string MigratedAppsettingsSuffix = ".migrated";

    private const string LegacyComponentFile = "component_template_settings.json";
    private const string LegacyFiducialFile = "fiducial_settings.json";
    private const string LegacyCameraFile = "camera_basler_defaults.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly CameraParameters CameraFallback = new()
    {
        DeviceIp = string.Empty,
        ExposureTimeUs = 15_000,
        GainDb = 0,
        Gamma = 1.0,
        Width = 1920,
        Height = 1200,
        BalanceWhiteAuto = "Off"
    };

    public AppSetting Load()
    {
        AppConfigPaths.MigrateLegacyFile("setting.json");
        MigrateFromAppsettingsJson();
        MigrateFromLegacySectionFiles();

        try
        {
            if (!File.Exists(AppConfigPaths.SettingFile))
                return CreateAndSaveDefault();

            var json = File.ReadAllText(AppConfigPaths.SettingFile);
            var setting = JsonSerializer.Deserialize<AppSetting>(json, JsonOptions) ?? new AppSetting();
            Normalize(setting);
            return setting;
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
            Normalize(setting);
            Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
            var json = JsonSerializer.Serialize(setting, JsonOptions);
            File.WriteAllText(AppConfigPaths.SettingFile, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public ComponentTemplateSettings LoadComponentTemplates()
    {
        var section = Load().ComponentTemplates;
        section.MinMatchSimilarityPercent = NormalizeMatchThreshold(section.MinMatchSimilarityPercent);
        return section;
    }

    public void SaveComponentTemplates(ComponentTemplateSettings settings)
    {
        var app = Load();
        settings.MinMatchSimilarityPercent = NormalizeMatchThreshold(settings.MinMatchSimilarityPercent);
        app.ComponentTemplates = settings;
        Save(app);
    }

    public double LoadMatchThresholdPercent()
        => NormalizeMatchThreshold(LoadComponentTemplates().MinMatchSimilarityPercent);

    public FiducialHoleSettings LoadFiducialHoles() => Load().FiducialHoles;

    public void SaveFiducialHoles(FiducialHoleSettings settings)
    {
        var app = Load();
        app.FiducialHoles = settings;
        Save(app);
    }

    public CameraParameters LoadCameraBasler()
    {
        var cam = Load().CameraBasler;
        if (cam.Width <= 0 || cam.Height <= 0)
            return CameraFallback.Clone();
        return cam.Clone();
    }

    public CameraCaptureSettings LoadCameraCapture() => Load().CameraCapture;

    private static AppSetting CreateAndSaveDefault()
    {
        var setting = new AppSetting();
        var service = new AppSettingService();
        service.Save(setting);
        return setting;
    }

    private static void Normalize(AppSetting setting)
    {
        if (!setting.DeveloperMode)
            setting.VirtualSerialPort = false;

        setting.ComponentTemplates.MinMatchSimilarityPercent =
            NormalizeMatchThreshold(setting.ComponentTemplates.MinMatchSimilarityPercent);
    }

    private static double NormalizeMatchThreshold(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;
        return Math.Clamp(value, 0, 100);
    }

    private static void MigrateFromAppsettingsJson()
    {
        foreach (var path in AppConfigPaths.FindLegacyConfigFiles(LegacyAppsettingsFile))
        {
            try
            {
                if (!File.Exists(path)) continue;

                var json = File.ReadAllText(path);
                var imported = JsonSerializer.Deserialize<AppSetting>(json, JsonOptions);
                if (imported == null) continue;

                var setting = File.Exists(AppConfigPaths.SettingFile)
                    ? JsonSerializer.Deserialize<AppSetting>(File.ReadAllText(AppConfigPaths.SettingFile), JsonOptions)
                      ?? new AppSetting()
                    : new AppSetting();

                MergeVisionSections(setting, imported);
                Normalize(setting);

                Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
                File.WriteAllText(AppConfigPaths.SettingFile, JsonSerializer.Serialize(setting, JsonOptions));

                var migratedPath = path + MigratedAppsettingsSuffix;
                if (File.Exists(migratedPath))
                    File.Delete(migratedPath);
                File.Move(path, migratedPath);
                return;
            }
            catch { /* thử path tiếp theo */ }
        }
    }

    private static void MigrateFromLegacySectionFiles()
    {
        var any = false;
        AppSetting setting;

        if (File.Exists(AppConfigPaths.SettingFile))
        {
            try
            {
                var json = File.ReadAllText(AppConfigPaths.SettingFile);
                setting = JsonSerializer.Deserialize<AppSetting>(json, JsonOptions) ?? new AppSetting();
            }
            catch
            {
                setting = new AppSetting();
            }
        }
        else
        {
            setting = new AppSetting();
        }

        foreach (var searchDir in AppConfigPaths.LegacySearchDirectories())
        {
            any |= TryImportLegacySection(
                Path.Combine(searchDir, LegacyComponentFile),
                json => setting.ComponentTemplates =
                    JsonSerializer.Deserialize<ComponentTemplateSettings>(json, JsonOptions)
                    ?? new ComponentTemplateSettings());

            any |= TryImportLegacySection(
                Path.Combine(searchDir, LegacyFiducialFile),
                json => setting.FiducialHoles =
                    JsonSerializer.Deserialize<FiducialHoleSettings>(json, JsonOptions)
                    ?? new FiducialHoleSettings());

            any |= TryImportLegacySection(
                Path.Combine(searchDir, LegacyCameraFile),
                json => setting.CameraBasler =
                    JsonSerializer.Deserialize<CameraParameters>(json, JsonOptions)
                    ?? CameraFallback.Clone());
        }

        if (!any) return;

        Normalize(setting);
        Directory.CreateDirectory(AppConfigPaths.ConfigDirectory);
        File.WriteAllText(AppConfigPaths.SettingFile, JsonSerializer.Serialize(setting, JsonOptions));
    }

    private static bool TryImportLegacySection(string path, Action<string> import)
    {
        if (!File.Exists(path)) return false;

        try
        {
            import(File.ReadAllText(path));
            ArchiveLegacyFile(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ArchiveLegacyFile(string path)
    {
        var archived = path + MigratedAppsettingsSuffix;
        if (File.Exists(archived))
            File.Delete(archived);
        File.Move(path, archived);
    }

    private static void MergeVisionSections(AppSetting target, AppSetting source)
    {
        if (source.ComponentTemplates.AllowedRegionNames.Count > 0
            || !string.IsNullOrWhiteSpace(source.ComponentTemplates.CustomFolder))
        {
            target.ComponentTemplates = source.ComponentTemplates;
        }

        if (!string.IsNullOrWhiteSpace(source.FiducialHoles.TemplateFolder)
            || source.FiducialHoles.MinMatchScore > 0
            || source.FiducialHoles.MaxMatchDimension > 0)
        {
            target.FiducialHoles = source.FiducialHoles;
        }

        if (source.CameraBasler.Width > 0 && source.CameraBasler.Height > 0)
            target.CameraBasler = source.CameraBasler;

        if (!string.IsNullOrWhiteSpace(source.CameraCapture.SaveFolder))
            target.CameraCapture = source.CameraCapture;
    }
}

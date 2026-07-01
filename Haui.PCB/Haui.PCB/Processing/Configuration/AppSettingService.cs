using System.IO;
using System.Text.Json;
using Haui.PCB.Processing;

namespace Haui.PCB.Processing.Configuration;

/// <summary>
/// Đọc / ghi <c>Config/setting.json</c> — robot, camera, thư viện mẫu.
/// </summary>
public class AppSettingService : IAppSettingService
{
    private const string LegacyAppsettingsFile = "appsettings.json";
    private const string MigratedAppsettingsSuffix = ".migrated";

    private const string LegacyComponentFile = "component_template_settings.json";
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
        PixelFormat = "Mono8",
        GainAuto = "Continuous",
        BalanceWhiteAuto = "Continuous"
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

    public ComponentDetectionSettings LoadComponentDetection()
    {
        var section = Load().ComponentDetection;
        NormalizeComponentDetection(section);
        return section;
    }

    public void SaveComponentDetection(ComponentDetectionSettings settings)
    {
        var app = Load();
        NormalizeComponentDetection(settings);
        app.ComponentDetection = settings;
        Save(app);
    }

    public SegmentationPipelineSettings LoadSegmentation()
    {
        var settings = Load().Segmentation;
        NormalizeSegmentation(settings);
        return settings;
    }

    public void SaveSegmentation(SegmentationPipelineSettings settings)
    {
        var app = Load();
        NormalizeSegmentation(settings);
        app.Segmentation = settings;
        Save(app);
    }

    public PcbBoardSettings LoadPcbBoard() => Load().PcbBoard;

    public void SavePcbBoard(PcbBoardSettings settings)
    {
        var app = Load();
        app.PcbBoard = settings;
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

    public ImageDownscaleSettings LoadCameraDownscale()
    {
        var settings = Load().CameraDownscale;
        if (settings.Width <= 0)
            settings.Width = ImageDownscaleSettings.DefaultWidth;
        if (settings.Height <= 0)
            settings.Height = ImageDownscaleSettings.DefaultHeight;
        return settings;
    }

    public HolderDetectionDownscaleSettings LoadHolderDetectionDownscale()
    {
        var settings = Load().HolderDetectionDownscale;
        if (settings.Width <= 0)
            settings.Width = HolderDetectionDownscaleSettings.DefaultWidth;
        if (settings.Height <= 0)
            settings.Height = HolderDetectionDownscaleSettings.DefaultHeight;
        return settings;
    }

    public CameraGigEStreamSettings LoadCameraGigEStream()
    {
        var settings = Load().CameraGigEStream.Clone();
        NormalizeCameraGigEStream(settings);
        return settings;
    }

    private static AppSetting CreateAndSaveDefault()
    {
        var setting = new AppSetting();
        var service = new AppSettingService();
        service.Save(setting);
        return setting;
    }

    private static void Normalize(AppSetting setting)
    {
        NormalizeComponentDetection(setting.ComponentDetection);
        NormalizeSegmentation(setting.Segmentation);
        NormalizeCameraGigEStream(setting.CameraGigEStream);
    }

    private static void NormalizeCameraGigEStream(CameraGigEStreamSettings settings)
    {
        if (settings.InterPacketDelay < 0)
            settings.InterPacketDelay = 0;

        if (settings.MaxNumBuffer <= 0)
            settings.MaxNumBuffer = CameraGigEStreamSettings.DefaultMaxNumBuffer;
        settings.MaxNumBuffer = Math.Clamp(settings.MaxNumBuffer, 5, 64);

        if (settings.OutputQueueSize <= 0)
            settings.OutputQueueSize = CameraGigEStreamSettings.DefaultOutputQueueSize;

        if (settings.MaxTransferSizeMb <= 0)
            settings.MaxTransferSizeMb = CameraGigEStreamSettings.DefaultMaxTransferSizeMb;

        if (settings.MaxBufferSizeMb <= 0)
            settings.MaxBufferSizeMb = CameraGigEStreamSettings.DefaultMaxBufferSizeMb;

        if (settings.GrabLoopThreadPriority < 0)
            settings.GrabLoopThreadPriority = 0;
        settings.GrabLoopThreadPriority = Math.Clamp(settings.GrabLoopThreadPriority, 0, 31);

        if (settings.ConsecutiveFailThreshold <= 0)
            settings.ConsecutiveFailThreshold = CameraGigEStreamSettings.DefaultConsecutiveFailThreshold;
    }

    private static void NormalizeComponentDetection(ComponentDetectionSettings settings)
    {
        settings.ConfThreshold = Math.Clamp(settings.ConfThreshold, 0.01, 0.99);
        settings.IouThreshold = Math.Clamp(settings.IouThreshold, 0.01, 0.99);

        if (settings.InputWidth <= 0)
            settings.InputWidth = ComponentDetectionSettings.DefaultInputWidth;
        if (settings.InputHeight <= 0)
            settings.InputHeight = ComponentDetectionSettings.DefaultInputHeight;

        if (string.IsNullOrWhiteSpace(settings.ModelPath))
            settings.ModelPath = ComponentDetectionSettings.DefaultModelPath;

        if (settings.ClassNames is null || settings.ClassNames.Count == 0)
            settings.ClassNames = [.. ComponentDetectionSettings.DefaultClassNames];

        if (string.IsNullOrWhiteSpace(settings.DefaultGroupSplit))
            settings.DefaultGroupSplit = ComponentDetectionSettings.DefaultGroupSplitDirection;

        if (settings.ComponentGroups is null || settings.ComponentGroups.Count == 0)
        {
            settings.ComponentGroups = ComponentDetectionSettings.DefaultComponentGroups
                .Select(g => new ComponentGroup
                {
                    Parent = g.Parent,
                    Children = [.. g.Children],
                    Split = g.Split
                })
                .ToList();
        }
        else
        {
            foreach (var group in settings.ComponentGroups)
            {
                group.Parent = group.Parent?.Trim() ?? string.Empty;
                group.Children = group.Children?
                    .Select(c => c.Trim())
                    .Where(c => c.Length > 0)
                    .ToList() ?? [];
            }

            settings.ComponentGroups = settings.ComponentGroups
                .Where(g => g.Parent.Length > 0 && g.Children.Count > 0)
                .ToList();
        }
    }

    private static void NormalizeSegmentation(SegmentationPipelineSettings settings)
    {
        if (settings.CannyThreshold1 <= 0)
            settings.CannyThreshold1 = SegmentationPipelineSettings.DefaultCannyThreshold1;
        if (settings.CannyThreshold2 <= 0)
            settings.CannyThreshold2 = SegmentationPipelineSettings.DefaultCannyThreshold2;
        if (settings.CannyThreshold2 < settings.CannyThreshold1)
            settings.CannyThreshold2 = settings.CannyThreshold1;
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
                json =>
                {
                    // Legacy component template settings are no longer used at runtime.
                });

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
        if (source.CameraBasler.Width > 0 && source.CameraBasler.Height > 0)
            target.CameraBasler = source.CameraBasler;

        if (!string.IsNullOrWhiteSpace(source.CameraCapture.SaveFolder))
            target.CameraCapture = source.CameraCapture;
    }
}

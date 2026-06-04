using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Đọc/ghi <c>appsettings.json</c> — camera, thư viện mẫu linh kiện, mẫu lỗ định vị.
/// </summary>
internal static class AppSettingsStore
{
    public const string SettingsFileName = "appsettings.json";

    private const string LegacyComponentFile = "component_template_settings.json";
    private const string LegacyFiducialFile = "fiducial_settings.json";
    private const string LegacyCameraFile = "camera_basler_defaults.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly CameraParameters CameraFallback = new()
    {
        ExposureTimeUs = 15_000,
        GainDb = 0,
        Gamma = 1.0,
        Width = 1920,
        Height = 1200,
        BalanceWhiteAuto = "Off"
    };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFileName))
            {
                var json = File.ReadAllText(SettingsFileName);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                Normalize(settings);
                return settings;
            }

            if (TryLoadLegacyFiles(out var migrated))
            {
                Save(migrated);
                return migrated;
            }

            return new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Normalize(settings);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFileName, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public static ComponentTemplateSettings LoadComponentTemplates()
    {
        var section = Load().ComponentTemplates;
        section.MinMatchSimilarityPercent = NormalizeMatchThreshold(section.MinMatchSimilarityPercent);
        return section;
    }

    public static void SaveComponentTemplates(ComponentTemplateSettings settings)
    {
        var app = Load();
        settings.MinMatchSimilarityPercent = NormalizeMatchThreshold(settings.MinMatchSimilarityPercent);
        app.ComponentTemplates = settings;
        Save(app);
    }

    public static double LoadMatchThresholdPercent()
        => NormalizeMatchThreshold(LoadComponentTemplates().MinMatchSimilarityPercent);

    public static FiducialHoleSettings LoadFiducialHoles() => Load().FiducialHoles;

    public static void SaveFiducialHoles(FiducialHoleSettings settings)
    {
        var app = Load();
        app.FiducialHoles = settings;
        Save(app);
    }

    public static CameraParameters LoadCameraBasler()
    {
        var cam = Load().CameraBasler;
        if (cam.Width <= 0 || cam.Height <= 0)
            return CameraFallback.Clone();
        return cam.Clone();
    }

    public static CameraCaptureSettings LoadCameraCapture() => Load().CameraCapture;

    private static void Normalize(AppSettings settings)
    {
        settings.ComponentTemplates.MinMatchSimilarityPercent =
            NormalizeMatchThreshold(settings.ComponentTemplates.MinMatchSimilarityPercent);
    }

    private static double NormalizeMatchThreshold(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;
        return Math.Clamp(value, 0, 100);
    }

    private static bool TryLoadLegacyFiles(out AppSettings settings)
    {
        settings = new AppSettings();
        var any = false;

        if (File.Exists(LegacyComponentFile))
        {
            try
            {
                var json = File.ReadAllText(LegacyComponentFile);
                settings.ComponentTemplates =
                    JsonSerializer.Deserialize<ComponentTemplateSettings>(json, JsonOptions)
                    ?? new ComponentTemplateSettings();
                any = true;
            }
            catch { /* bỏ qua */ }
        }

        if (File.Exists(LegacyFiducialFile))
        {
            try
            {
                var json = File.ReadAllText(LegacyFiducialFile);
                settings.FiducialHoles =
                    JsonSerializer.Deserialize<FiducialHoleSettings>(json, JsonOptions)
                    ?? new FiducialHoleSettings();
                any = true;
            }
            catch { /* bỏ qua */ }
        }

        if (File.Exists(LegacyCameraFile))
        {
            try
            {
                var json = File.ReadAllText(LegacyCameraFile);
                settings.CameraBasler =
                    JsonSerializer.Deserialize<CameraParameters>(json, JsonOptions)
                    ?? CameraFallback.Clone();
                any = true;
            }
            catch { /* bỏ qua */ }
        }

        if (any)
            Normalize(settings);

        return any;
    }
}

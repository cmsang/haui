using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Đọc tham số khuyến nghị Basler từ camera_basler_defaults.json (CWD).
/// </summary>
public static class CameraDefaultsLoader
{
    public const string DefaultsFileName = "camera_basler_defaults.json";

    private static readonly CameraParameters Fallback = new()
    {
        ExposureTimeUs = 15_000,
        GainDb = 0,
        Gamma = 1.0,
        Width = 1920,
        Height = 1200,
        BalanceWhiteAuto = "Off"
    };

    public static CameraParameters LoadRecommended()
    {
        try
        {
            if (!File.Exists(DefaultsFileName))
                return Fallback.Clone();

            var json = File.ReadAllText(DefaultsFileName);
            var loaded = JsonSerializer.Deserialize<CameraParameters>(json, JsonOptions);
            return loaded ?? Fallback.Clone();
        }
        catch
        {
            return Fallback.Clone();
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}

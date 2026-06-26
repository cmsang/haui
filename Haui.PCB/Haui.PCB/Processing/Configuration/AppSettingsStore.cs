
namespace Haui.PCB.Processing.Configuration;

/// <summary>
/// Truy cập section vision trong <c>setting.json</c> — wrapper tĩnh cho code không dùng DI.
/// </summary>
internal static class AppSettingsStore
{
    private static readonly AppSettingService Service = new();

    public static ComponentTemplateSettings LoadComponentTemplates() => Service.LoadComponentTemplates();

    public static void SaveComponentTemplates(ComponentTemplateSettings settings)
        => Service.SaveComponentTemplates(settings);

    public static double LoadMatchThresholdPercent() => Service.LoadMatchThresholdPercent();

    public static PcbBoardSettings LoadPcbBoard() => Service.LoadPcbBoard();

    public static CameraParameters LoadCameraBasler() => Service.LoadCameraBasler();

    public static CameraCaptureSettings LoadCameraCapture() => Service.LoadCameraCapture();

    public static ImageDownscaleSettings LoadCameraDownscale() => Service.LoadCameraDownscale();
}

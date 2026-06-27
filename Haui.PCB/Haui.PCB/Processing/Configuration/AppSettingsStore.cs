
namespace Haui.PCB.Processing.Configuration;

/// <summary>
/// Truy cập section vision trong <c>setting.json</c> — wrapper tĩnh cho code không dùng DI.
/// </summary>
internal static class AppSettingsStore
{
    private static readonly AppSettingService Service = new();

    public static ComponentDetectionSettings LoadComponentDetection() => Service.LoadComponentDetection();

    public static void SaveComponentDetection(ComponentDetectionSettings settings)
        => Service.SaveComponentDetection(settings);

    public static SegmentationPipelineSettings LoadSegmentation() => Service.LoadSegmentation();

    public static PcbBoardSettings LoadPcbBoard() => Service.LoadPcbBoard();

    public static CameraParameters LoadCameraBasler() => Service.LoadCameraBasler();

    public static CameraCaptureSettings LoadCameraCapture() => Service.LoadCameraCapture();

    public static ImageDownscaleSettings LoadCameraDownscale() => Service.LoadCameraDownscale();

    public static HolderDetectionDownscaleSettings LoadHolderDetectionDownscale()
        => Service.LoadHolderDetectionDownscale();
}

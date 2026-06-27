using Haui.PCB.Models.Configuration;

namespace Haui.PCB.Processing;

/// <summary>App settings — robot and vision share <c>Config/setting.json</c>.</summary>
public interface IAppSettingService
{
    AppSetting Load();
    void Save(AppSetting setting);

    ComponentDetectionSettings LoadComponentDetection();
    void SaveComponentDetection(ComponentDetectionSettings settings);

    SegmentationPipelineSettings LoadSegmentation();
    void SaveSegmentation(SegmentationPipelineSettings settings);

    PcbBoardSettings LoadPcbBoard();
    void SavePcbBoard(PcbBoardSettings settings);

    CameraParameters LoadCameraBasler();
    CameraCaptureSettings LoadCameraCapture();
    ImageDownscaleSettings LoadCameraDownscale();
    HolderDetectionDownscaleSettings LoadHolderDetectionDownscale();
}

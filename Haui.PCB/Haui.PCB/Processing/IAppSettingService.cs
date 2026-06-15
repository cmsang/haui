using Haui.PCB.Models.Configuration;

namespace Haui.PCB.Processing;

/// <summary>App settings — robot and vision share <c>Config/setting.json</c>.</summary>
public interface IAppSettingService
{
    AppSetting Load();
    void Save(AppSetting setting);

    ComponentTemplateSettings LoadComponentTemplates();
    void SaveComponentTemplates(ComponentTemplateSettings settings);
    double LoadMatchThresholdPercent();

    FiducialHoleSettings LoadFiducialHoles();
    void SaveFiducialHoles(FiducialHoleSettings settings);

    CameraParameters LoadCameraBasler();
    CameraCaptureSettings LoadCameraCapture();
}

using Haui.PCB.Models;

namespace Haui.PCB.Processing;

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

using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Tham số khuyến nghị Basler từ <c>setting.json</c> → <see cref="AppSetting.CameraBasler"/>.
/// </summary>
public static class CameraDefaultsLoader
{
    public static CameraParameters LoadRecommended() => AppSettingsStore.LoadCameraBasler();
}

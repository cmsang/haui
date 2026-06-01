using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Tham số khuyến nghị Basler từ <c>appsettings.json</c> → <see cref="AppSettings.CameraBasler"/>.
/// </summary>
public static class CameraDefaultsLoader
{
    public static CameraParameters LoadRecommended() => AppSettingsStore.LoadCameraBasler();
}

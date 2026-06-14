namespace Haui.PCB.Models.Camera;

/// <summary>
/// Thông tin camera Basler phát hiện được qua pylon.
/// </summary>
public record CameraInfo(int Index, string Name, string DeviceId)
{
    public string DisplayName => Name;
}

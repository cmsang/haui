namespace Haui.PCB.Processing;

/// <summary>
/// Thông tin camera Basler phát hiện được qua pylon.
/// </summary>
public record CameraInfo(int Index, string Name, string DeviceId)
{
    public string DisplayName => Name;
}

/// <summary>
/// Độ phân giải hỗ trợ của camera.
/// </summary>
public record ResolutionInfo(int Width, int Height)
{
    public string Label => $"{Width}×{Height}";
}

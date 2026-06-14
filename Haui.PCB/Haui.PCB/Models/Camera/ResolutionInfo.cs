namespace Haui.PCB.Models.Camera;

/// <summary>
/// Độ phân giải hỗ trợ của camera.
/// </summary>
public record ResolutionInfo(int Width, int Height)
{
    public string Label => $"{Width}×{Height}";
}

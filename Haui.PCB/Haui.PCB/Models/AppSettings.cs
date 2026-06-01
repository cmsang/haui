namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình ứng dụng — đọc/ghi từ <c>appsettings.json</c> (CWD).
/// </summary>
public sealed class AppSettings
{
    public ComponentTemplateSettings ComponentTemplates { get; set; } = new();
    public FiducialHoleSettings FiducialHoles { get; set; } = new();
    public CameraParameters CameraBasler { get; set; } = new();
}

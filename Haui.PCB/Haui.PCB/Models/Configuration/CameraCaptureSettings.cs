namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Cấu hình lưu ảnh chụp nhanh — section <c>CameraCapture</c> trong <c>setting.json</c>.
/// </summary>
public sealed class CameraCaptureSettings
{
    public const string DefaultSaveFolder = "captures";

    /// <summary>Thư mục lưu ảnh chụp (đường dẫn tuyệt đối hoặc tương đối CWD). Rỗng → <see cref="DefaultSaveFolder"/>.</summary>
    public string SaveFolder { get; set; } = string.Empty;
}

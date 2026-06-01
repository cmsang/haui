namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình thư mục lưu mẫu linh kiện (thư viện + mẫu active cho Test).
/// </summary>
public sealed class ComponentTemplateSettings
{
    public const string DefaultLibraryFolder = "templates";

    /// <summary>Bật lưu vào <see cref="CustomFolder"/> thay vì mặc định.</summary>
    public bool UseCustomFolder { get; set; }

    /// <summary>Thư mục tùy chỉnh (đường dẫn tuyệt đối hoặc tương đối CWD).</summary>
    public string CustomFolder { get; set; } = string.Empty;
}

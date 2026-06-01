namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình thư mục lưu mẫu linh kiện (thư viện + mẫu active cho Test).
/// </summary>
public sealed class ComponentTemplateSettings
{
    public const string DefaultLibraryFolder = "templates";

    /// <summary>Số vùng linh kiện bắt buộc trên mỗi ảnh mẫu (Viewer thư viện).</summary>
    public const int DefaultRequiredRegionCount = 18;

    /// <summary>Tên vùng hợp lệ khi tạo mẫu (mặc định theo sơ đồ linh kiện).</summary>
    public static readonly string[] DefaultAllowedRegionNames =
    [
        "KF1", "KF2", "KF3", "C2", "AMS1", "L3", "L1", "R1", "R2",
        "C3", "C6", "C5", "D2", "D1", "C1", "LM1", "C4"
    ];

    /// <summary>Bật lưu vào <see cref="CustomFolder"/> thay vì mặc định.</summary>
    public bool UseCustomFolder { get; set; }

    /// <summary>Thư mục tùy chỉnh (đường dẫn tuyệt đối hoặc tương đối CWD).</summary>
    public string CustomFolder { get; set; } = string.Empty;

    /// <summary>Số vùng linh kiện tối thiểu để lưu từ Viewer thư viện.</summary>
    public int RequiredRegionCount { get; set; } = DefaultRequiredRegionCount;

    /// <summary>Danh sách tên vùng được phép khi tạo / chỉnh sửa mẫu.</summary>
    public List<string> AllowedRegionNames { get; set; } =
        [.. DefaultAllowedRegionNames];
}

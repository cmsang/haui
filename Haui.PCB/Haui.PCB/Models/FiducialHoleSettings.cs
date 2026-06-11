namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình mẫu lỗ định vị — section <c>FiducialHoles</c> trong <c>setting.json</c>.
/// </summary>
public sealed class FiducialHoleSettings
{
    public const string DefaultTemplateFolder = "fiducial_holes";
    public const double DefaultMinMatchScore = 0.55;
    public const int DefaultMaxMatchDimension = 1280;

    /// <summary>Thư mục lưu <c>hole_*.png</c> — chỉnh trong <c>setting.json</c> → FiducialHoles.</summary>
    public string TemplateFolder { get; set; } = DefaultTemplateFolder;
    public double MinMatchScore { get; set; } = DefaultMinMatchScore;

    /// <summary>Cạnh dài nhất của ảnh dùng cho template matching (px); ảnh lớn hơn sẽ downscale.</summary>
    public int MaxMatchDimension { get; set; } = DefaultMaxMatchDimension;
}

namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Cấu hình mẫu lỗ định vị — section <c>FiducialHoles</c> trong <c>setting.json</c>.
/// </summary>
public sealed class FiducialHoleSettings
{
    public const string DefaultTemplateFolder = "fiducial_holes";
    public const double DefaultMinMatchScore = 0.55;
    public const int DefaultMaxMatchDimension = 1280;
    public const double DefaultAspectRatioTolerance = 0.15;
    public const int DefaultMaxQuadSearchCandidates = 15;
    public const double DefaultMinQuadRectangularity = 0.75;

    /// <summary>Thư mục lưu <c>hole_*.png</c> — chỉnh trong <c>setting.json</c> → FiducialHoles.</summary>
    public string TemplateFolder { get; set; } = DefaultTemplateFolder;
    public double MinMatchScore { get; set; } = DefaultMinMatchScore;

    /// <summary>Cạnh dài nhất của ảnh dùng cho template matching (px); ảnh lớn hơn sẽ downscale.</summary>
    public int MaxMatchDimension { get; set; } = DefaultMaxMatchDimension;

    /// <summary>Max relative aspect-ratio error when <see cref="PcbBoardSettings"/> dimensions are set.</summary>
    public double AspectRatioTolerance { get; set; } = DefaultAspectRatioTolerance;

    /// <summary>Max distinct match candidates considered for quad combinatorial search.</summary>
    public int MaxQuadSearchCandidates { get; set; } = DefaultMaxQuadSearchCandidates;

    /// <summary>Minimum opposite-side similarity for a valid fiducial quad (0..1).</summary>
    public double MinQuadRectangularity { get; set; } = DefaultMinQuadRectangularity;
}

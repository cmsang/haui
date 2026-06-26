namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Fiducial hole template settings — <c>FiducialHoles</c> section in <c>setting.json</c>.
/// </summary>
public sealed class FiducialHoleSettings
{
    public const string DefaultTemplateFolder = "fiducial_holes";
    public const double DefaultMinMatchScore = 0.55;
    public const int DefaultMaxMatchDimension = 1280;
    public const double DefaultAspectRatioTolerance = 0.15;
    public const int DefaultMaxQuadSearchCandidates = 15;
    public const double DefaultMinQuadRectangularity = 0.75;

    /// <summary>Folder for <c>hole_*.png</c> templates — <c>setting.json</c> → FiducialHoles.</summary>
    public string TemplateFolder { get; set; } = DefaultTemplateFolder;
    public double MinMatchScore { get; set; } = DefaultMinMatchScore;

    /// <summary>Longest edge used for template matching (px); larger images are downscaled.</summary>
    public int MaxMatchDimension { get; set; } = DefaultMaxMatchDimension;

    /// <summary>Max relative aspect-ratio error when <see cref="PcbBoardSettings"/> dimensions are set.</summary>
    public double AspectRatioTolerance { get; set; } = DefaultAspectRatioTolerance;

    /// <summary>Max distinct match candidates considered for quad combinatorial search.</summary>
    public int MaxQuadSearchCandidates { get; set; } = DefaultMaxQuadSearchCandidates;

    /// <summary>Minimum opposite-side similarity for a valid fiducial quad (0..1).</summary>
    public double MinQuadRectangularity { get; set; } = DefaultMinQuadRectangularity;
}

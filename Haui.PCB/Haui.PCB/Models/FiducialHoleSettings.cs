namespace Haui.PCB.Models;

/// <summary>
/// Cấu hình thư mục và ngưỡng nhận diện 4 lỗ tròn định vị PCB.
/// </summary>
public sealed class FiducialHoleSettings
{
    public const string DefaultTemplateFolder = "fiducial_holes";
    public const double DefaultMinMatchScore = 0.55;

    public string TemplateFolder { get; set; } = DefaultTemplateFolder;
    public double MinMatchScore { get; set; } = DefaultMinMatchScore;
}

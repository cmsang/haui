namespace Haui.ShapesDetector.Models;

public class DetectionResult
{
    public string ClassName { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public BoundingBox BoundingBox { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.Now;
}

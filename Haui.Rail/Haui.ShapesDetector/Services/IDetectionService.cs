using Haui.ShapesDetector.Models;

namespace Haui.ShapesDetector.Services;

public interface IDetectionService
{
    Task InitializeAsync(string modelPath, string classesPath);
    Task<List<DetectionResult>> DetectAsync(byte[] imageData, int width, int height);
    void SetConfidenceThreshold(float threshold);
    void SetIouThreshold(float threshold);
    bool IsInitialized { get; }
}

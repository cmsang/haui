using Haui.PCB.Models.Detection;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>
/// Maps YOLO detections to missing-component inspection results.
/// Each detection box represents one missing component location.
/// </summary>
public sealed class MissingComponentDetectionService : IComponentInspectionService
{
    private readonly IOnnxYoloDetector _detector;

    public MissingComponentDetectionService(IOnnxYoloDetector detector)
    {
        _detector = detector;
    }

    public MissingComponentDetectionService()
        : this(new OnnxYoloDetector())
    {
    }

    public ComponentInspectionResult Inspect(Mat board)
    {
        var settings = AppSettingsStore.LoadComponentDetection();
        var detections = _detector.Detect(board);

        // Each label appears at most once per board: keep the highest-confidence detection.
        var missing = detections
            .GroupBy(d => d.Label)
            .Select(g => g.OrderByDescending(d => d.Confidence).First())
            .OrderByDescending(d => d.Confidence)
            .Select(d => new MissingComponent(d.Label, d.Confidence, d.Box))
            .ToList();

        return new ComponentInspectionResult
        {
            Missing = missing,
            ConfThreshold = settings.ConfThreshold
        };
    }
}

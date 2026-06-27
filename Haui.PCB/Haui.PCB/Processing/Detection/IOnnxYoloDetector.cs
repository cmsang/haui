using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>Runs YOLO ONNX inference and returns raw detections before missing-component mapping.</summary>
public interface IOnnxYoloDetector
{
    IReadOnlyList<YoloDetection> Detect(Mat board);
}

/// <summary>Single YOLO bounding-box detection.</summary>
public sealed record YoloDetection(int ClassId, string Label, float Confidence, Rect Box);

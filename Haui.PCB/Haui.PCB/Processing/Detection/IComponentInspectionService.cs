using Haui.PCB.Models.Detection;
using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>Detects missing component locations on a warped board using YOLO ONNX.</summary>
public interface IComponentInspectionService
{
    ComponentInspectionResult Inspect(Mat board);
}

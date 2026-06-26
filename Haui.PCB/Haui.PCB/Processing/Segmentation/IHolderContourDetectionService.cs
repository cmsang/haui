using Haui.PCB.Models.Configuration;
using Haui.PCB.Models.Segmentation;
using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Detects the outer rectangular holder frame from morphology-close edge pixels.
/// </summary>
public interface IHolderContourDetectionService
{
    HolderContourResult Detect(
        Mat closedEdges,
        Rect? edgeSearchRoi,
        PcbBoardSettings holderSettings);
}

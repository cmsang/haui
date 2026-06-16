using Haui.PCB.Models.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Detects four fiducial holes via template matching on the Morphology Close image;
/// geometry and board aspect ratio select the correct outer quad.
/// </summary>
public interface IFiducialHoleDetectionService
{
    /// <param name="searchImage">Image after Morphology Close (grayscale).</param>
    /// <param name="templates">Templates sorted by recognition score descending.</param>
    FiducialDetectionResult Detect(
        Mat searchImage,
        IReadOnlyList<FiducialTemplateEntry> templates,
        FiducialHoleSettings fiducialSettings,
        PcbBoardSettings boardSettings);
}

using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// Finds board orientation by comparing the configured orientation marker region
/// against template images that define that marker (CCoeffNormed).
/// </summary>
public interface IBoardOrientationDetectionService
{
    /// <summary>
    /// Tries all templates with an orientation marker; rotates 180° and retries when no match.
    /// Returns <see cref="BoardOrientationDetectionResult.Skipped"/> when orientation is not configured.
    /// </summary>
    BoardOrientationDetectionResult Detect(Mat newBoard);
}

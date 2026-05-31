using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Nhận diện 4 lỗ tròn định vị bằng template matching trên ảnh Morphology Close.
/// </summary>
public interface IFiducialHoleDetectionService
{
    /// <param name="searchImage">Ảnh sau bước Morphology Close (grayscale).</param>
    FiducialDetectionResult Detect(
        Mat searchImage,
        IReadOnlyList<Mat> templates,
        double minMatchScore,
        int maxMatchDimension);
}

using OpenCvSharp;

namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Nhận diện 4 lỗ tròn định vị bằng template matching trên ảnh Morphology Close.
/// </summary>
public interface IFiducialHoleDetectionService
{
    /// <param name="searchImage">Ảnh sau bước Morphology Close (grayscale).</param>
    /// <param name="templates">Mẫu đã sắp theo điểm nhận diện giảm dần.</param>
    FiducialDetectionResult Detect(
        Mat searchImage,
        IReadOnlyList<FiducialTemplateEntry> templates,
        double minMatchScore,
        int maxMatchDimension);
}

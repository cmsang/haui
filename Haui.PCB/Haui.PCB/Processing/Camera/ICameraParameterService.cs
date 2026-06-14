
namespace Haui.PCB.Processing.Camera;

/// <summary>
/// Điều khiển tham số GenICam trên camera Basler.
/// </summary>
public interface ICameraParameterService
{
    CameraParameters GetRecommendedParameters();
    CameraParameters ReadCurrentParameters();
    void ApplyParameters(CameraParameters parameters);
    void ResetToRecommended();
    void PrepareForStart(CameraParameters parameters);
}

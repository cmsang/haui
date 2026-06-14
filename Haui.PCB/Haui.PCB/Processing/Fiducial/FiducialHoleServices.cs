namespace Haui.PCB.Processing.Fiducial;

/// <summary>
/// Dịch vụ mẫu lỗ tròn dùng chung toàn app — cache RAM được chia sẻ giữa các cửa sổ.
/// </summary>
public static class FiducialHoleServices
{
    public static IFiducialHoleTemplateService TemplateService { get; } = new FiducialHoleTemplateService();
}

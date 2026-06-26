namespace Haui.PCB.Models.Templates;

/// <summary>
/// File <c>{ảnh_mẫu}_orientation.json</c> — vị trí thành phần xác định chiều mạch (tách khỏi vùng linh kiện).
/// </summary>
public class TemplateOrientationDocument
{
    public double RelX { get; set; }

    public double RelY { get; set; }

    public double RelWidth { get; set; }

    public double RelHeight { get; set; }

    public TemplateRegion ToRegion(string componentName) => new()
    {
        Name = componentName,
        RelX = RelX,
        RelY = RelY,
        RelWidth = RelWidth,
        RelHeight = RelHeight
    };

    public static TemplateOrientationDocument? FromRegion(TemplateRegion? region)
    {
        if (region is null)
            return null;

        return new TemplateOrientationDocument
        {
            RelX = region.RelX,
            RelY = region.RelY,
            RelWidth = region.RelWidth,
            RelHeight = region.RelHeight
        };
    }
}

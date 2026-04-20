namespace Haui.GarlicDetector.Models;

/// <summary>Một vùng được người dùng khoanh chọn kèm nhãn phân loại tỏi.</summary>
public sealed record LabeledRegion(Rectangle ImageRect, GarlicLabel Label);

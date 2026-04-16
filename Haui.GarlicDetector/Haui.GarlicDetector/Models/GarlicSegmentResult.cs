using OpenCvSharp;

namespace Haui.GarlicDetector.Models;

/// <summary>
/// Kết quả phân vùng của một contour tỏi:
/// bounding rect, diện tích contour thực tế và độ tròn.
/// </summary>
public sealed record GarlicSegmentResult(
    Rect   BoundingRect,
    double Area,
    double Circularity);

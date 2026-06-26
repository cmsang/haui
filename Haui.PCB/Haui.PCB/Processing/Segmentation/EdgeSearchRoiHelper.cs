using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Axis-aligned bounding box of non-zero edge pixels after morphology close.
/// </summary>
public static class EdgeSearchRoiHelper
{
    public static Rect? ComputeBoundingRect(Mat closed)
    {
        if (closed.Empty())
            return null;

        using var points = new Mat();
        Cv2.FindNonZero(closed, points);
        if (points.Empty() || points.Total() == 0)
            return null;

        return Cv2.BoundingRect(points);
    }

    /// <summary>
    /// Crops <paramref name="closed"/> to the edge AABB ROI; returns null when no edge pixels exist.
    /// </summary>
    public static Mat? CropToEdgeSearchRoi(Mat closed)
    {
        var roi = ComputeBoundingRect(closed);
        if (roi is null)
            return null;

        using var patch = new Mat(closed, roi.Value);
        return patch.Clone();
    }
}

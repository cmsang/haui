using Haui.PCB.Models.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Downscales frames for holder detection and maps coordinates back to full-resolution space.
/// </summary>
internal static class DetectionImageHelper
{
    /// <summary>
    /// Fits <paramref name="source"/> within max bounds (aspect preserved). Returns a new Mat and
    /// <paramref name="scale"/> = detectionWidth / sourceWidth (uniform; equals detectionHeight / sourceHeight).
    /// When downscale is disabled or not needed, returns a clone and scale = 1.
    /// </summary>
    public static (Mat Image, double Scale) DownscaleForDetection(
        Mat source,
        HolderDetectionDownscaleSettings settings)
    {
        if (source.Empty())
            return (source.Clone(), 1.0);

        if (!settings.Enabled)
            return (source.Clone(), 1.0);

        int maxWidth = settings.Width;
        int maxHeight = settings.Height;
        if (maxWidth <= 0 || maxHeight <= 0)
            return (source.Clone(), 1.0);

        double scale = Math.Min(
            (double)maxWidth / source.Width,
            (double)maxHeight / source.Height);

        if (scale >= 1.0)
            return (source.Clone(), 1.0);

        int targetWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
        int targetHeight = Math.Max(1, (int)Math.Round(source.Height * scale));

        var resized = new Mat();
        Cv2.Resize(source, resized, new Size(targetWidth, targetHeight), 0, 0, InterpolationFlags.Area);
        return (resized, scale);
    }

    public static Point2f[] ScaleCornersToSource(Point2f[] corners, double invScale)
    {
        if (Math.Abs(invScale - 1.0) < 1e-9)
            return corners;

        var scaled = new Point2f[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            scaled[i] = new Point2f(
                (float)(corners[i].X * invScale),
                (float)(corners[i].Y * invScale));
        }

        return scaled;
    }

    public static Rect ScaleRectToSource(Rect roi, double invScale)
    {
        if (Math.Abs(invScale - 1.0) < 1e-9)
            return roi;

        return new Rect(
            (int)Math.Round(roi.X * invScale),
            (int)Math.Round(roi.Y * invScale),
            Math.Max(1, (int)Math.Round(roi.Width * invScale)),
            Math.Max(1, (int)Math.Round(roi.Height * invScale)));
    }
}

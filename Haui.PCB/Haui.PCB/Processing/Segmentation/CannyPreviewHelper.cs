using System.Diagnostics;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Shared grayscale → blur → downscale → Canny → Morphology Close stages used by
/// <see cref="PcbSegmentationService"/> and the Canny threshold tuner window.
/// </summary>
public static class CannyPreviewHelper
{
    private const int MorphKernelSize = 5;

    /// <summary>Output mats from the shared edge-detection stages (caller owns and disposes).</summary>
    public sealed class Result
    {
        public required Mat Gray { get; init; }
        public required Mat Blurred { get; init; }
        public required Mat Edges { get; init; }
        public required Mat Closed { get; init; }
        public Size? DetectionSize { get; init; }
        public double DetectionScale { get; init; }
        public double InvDetectionScale => DetectionScale > 0 ? 1.0 / DetectionScale : 1.0;

        public void DisposeAll()
        {
            Gray.Dispose();
            Blurred.Dispose();
            Edges.Dispose();
            Closed.Dispose();
        }
    }

    /// <summary>
    /// Runs the same pre-processing and edge stages as the main segmentation pipeline.
    /// </summary>
    /// <param name="includeIntermediateMats">
    /// When true, returns clones of grayscale and Gaussian-blur mats for pipeline gallery debug.
    /// </param>
    /// <param name="timings">Optional per-step elapsed times (GaussianBlur, Canny, MorphologyClose).</param>
    public static Result Compute(
        Mat source,
        double threshold1,
        double threshold2,
        bool includeIntermediateMats = false,
        Dictionary<string, TimeSpan>? timings = null)
    {
        var sw = new Stopwatch();

        using var grayWork = new Mat();
        using var blurredWork = new Mat();
        using var kernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(MorphKernelSize, MorphKernelSize));

        if (source.Channels() == 1)
            source.CopyTo(grayWork);
        else
            Cv2.CvtColor(source, grayWork, ColorConversionCodes.BGR2GRAY);

        Mat cannyInput;
        if (source.Channels() == 1)
        {
            // Mono8 frames are pre-blurred in BaslerCameraService.
            cannyInput = grayWork;
        }
        else
        {
            sw.Restart();
            Cv2.GaussianBlur(grayWork, blurredWork, new Size(5, 5), 0);
            RecordTiming(timings, SegmentationPipelineSteps.GaussianBlur, sw.Elapsed);
            cannyInput = blurredWork;
        }

        var holderDownscale = AppSettingsStore.LoadHolderDetectionDownscale();
        var (detectionInputMat, detectionScale) = DetectionImageHelper.DownscaleForDetection(
            cannyInput,
            holderDownscale);
        using var detectionInput = detectionInputMat;

        Size? detectionSize = Math.Abs(detectionScale - 1.0) > 1e-9
            ? new Size(detectionInput.Width, detectionInput.Height)
            : null;

        sw.Restart();
        var edges = new Mat();
        Cv2.Canny(detectionInput, edges, threshold1, threshold2);
        RecordTiming(timings, SegmentationPipelineSteps.Canny, sw.Elapsed);

        sw.Restart();
        var closed = new Mat();
        Cv2.MorphologyEx(edges, closed, MorphTypes.Close, kernel, iterations: 3);
        RecordTiming(timings, SegmentationPipelineSteps.MorphologyClose, sw.Elapsed);

        return new Result
        {
            Gray = includeIntermediateMats ? grayWork.Clone() : new Mat(),
            Blurred = includeIntermediateMats && source.Channels() != 1 ? blurredWork.Clone() : new Mat(),
            Edges = edges,
            Closed = closed,
            DetectionSize = detectionSize,
            DetectionScale = detectionScale
        };
    }

    private static void RecordTiming(
        Dictionary<string, TimeSpan>? timings,
        string key,
        TimeSpan elapsed)
    {
        if (timings is not null)
            timings[key] = elapsed;
    }
}

using System.Diagnostics;
using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Segments and straightens a PCB board from the camera frame using the holder support frame contour.
/// Canny + morphology close prepare the search image; the largest valid external quad defines the warp.
/// Implements <see cref="IPcbSegmentationService"/>.
/// </summary>
public class PcbSegmentationService : IPcbSegmentationService
{
    private readonly SegmentationParameters _parameters;
    private readonly IHolderContourDetectionService _holderContourDetection;

    public PcbSegmentationService()
        : this(SegmentationSettings.Current, new HolderContourDetectionService())
    {
    }

    public PcbSegmentationService(SegmentationParameters parameters)
        : this(parameters, new HolderContourDetectionService())
    {
    }

    public PcbSegmentationService(
        SegmentationParameters parameters,
        IHolderContourDetectionService holderContourDetection)
    {
        _parameters = parameters;
        _holderContourDetection = holderContourDetection;
    }

    private const int MorphKernelSize = 5;
    private const int EdgePadding = 2;

    /// <inheritdoc />
    public Mat? Segment(Mat source)
    {
        using var pipeline = RunPipelineCore(source, includeDebugMats: false);
        if (pipeline.Warped is null || pipeline.Warped.Empty())
            return null;
        return pipeline.Warped.Clone();
    }

    /// <inheritdoc />
    public SegmentationPipelineResult RunPipeline(Mat source)
        => RunPipelineCore(source, includeDebugMats: true);

    private SegmentationPipelineResult RunPipelineCore(Mat source, bool includeDebugMats)
    {
        var timings = includeDebugMats ? new Dictionary<string, TimeSpan>() : null;
        var sw = new Stopwatch();

        using var grayWork = new Mat();
        using var blurredWork = new Mat();
        using var edgesWork = new Mat();
        using var closedWork = new Mat();
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

        double t1 = _parameters.CannyThreshold1;
        double t2 = _parameters.CannyThreshold2;

        sw.Restart();
        Cv2.Canny(cannyInput, edgesWork, t1, t2);
        RecordTiming(timings, SegmentationPipelineSteps.Canny, sw.Elapsed);

        sw.Restart();
        Cv2.MorphologyEx(edgesWork, closedWork, MorphTypes.Close, kernel, iterations: 3);
        RecordTiming(timings, SegmentationPipelineSteps.MorphologyClose, sw.Elapsed);

        Point2f[]? warpQuadCorners = null;
        string? contourDescription = null;
        Rect? edgeSearchRoi = null;
        Mat? warped = null;

        edgeSearchRoi = EdgeSearchRoiHelper.ComputeBoundingRect(closedWork);

        if (edgeSearchRoi is null)
        {
            contourDescription = "Không có pixel biên sau Morphology Close.";
            RecordTiming(timings, SegmentationPipelineSteps.HolderContour, TimeSpan.Zero);
        }
        else
        {
            var holderSettings = AppSettingsStore.LoadPcbBoard();

            sw.Restart();
            var contourResult = _holderContourDetection.Detect(
                closedWork,
                edgeSearchRoi,
                holderSettings);
            RecordTiming(timings, SegmentationPipelineSteps.HolderContour, sw.Elapsed);

            contourDescription = contourResult.Message ?? "Không tìm được khung hộp đỡ.";

            if (contourResult.Success && contourResult.Corners is { Length: 4 } corners)
            {
                warpQuadCorners = corners;

                sw.Restart();
                warped = WarpPerspective(source, warpQuadCorners);
                RecordTiming(timings, SegmentationPipelineSteps.Warp, sw.Elapsed);
            }
        }

        return new SegmentationPipelineResult
        {
            Gray = includeDebugMats ? grayWork.Clone() : new Mat(),
            Blurred = includeDebugMats && source.Channels() != 1 ? blurredWork.Clone() : new Mat(),
            Edges = includeDebugMats ? edgesWork.Clone() : new Mat(),
            Closed = includeDebugMats ? closedWork.Clone() : new Mat(),
            CannyThreshold1 = t1,
            CannyThreshold2 = t2,
            WarpQuadCorners = warpQuadCorners,
            ContourDescription = contourDescription,
            EdgeSearchRoi = edgeSearchRoi,
            Warped = warped,
            StepTimings = timings ?? new Dictionary<string, TimeSpan>()
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

    private static Mat WarpPerspective(Mat source, Point2f[] quad)
    {
        var ordered = QuadOrdering.OrderCorners(quad);

        float width = Math.Max(
            Distance(ordered[0], ordered[1]),
            Distance(ordered[3], ordered[2]));

        float height = Math.Max(
            Distance(ordered[0], ordered[3]),
            Distance(ordered[1], ordered[2]));

        float paddedWidth = width + EdgePadding * 2;
        float paddedHeight = height + EdgePadding * 2;

        var dst = new Point2f[]
        {
            new(EdgePadding, EdgePadding),
            new(paddedWidth - 1 - EdgePadding, EdgePadding),
            new(paddedWidth - 1 - EdgePadding, paddedHeight - 1 - EdgePadding),
            new(EdgePadding, paddedHeight - 1 - EdgePadding)
        };

        using var M = Cv2.GetPerspectiveTransform(ordered, dst);
        var warped = new Mat();
        Cv2.WarpPerspective(source, warped, M, new Size((int)paddedWidth, (int)paddedHeight));

        if (warped.Height > warped.Width)
        {
            var rotated = new Mat();
            Cv2.Transpose(warped, rotated);
            Cv2.Flip(rotated, rotated, FlipMode.Y);
            warped.Dispose();
            return rotated;
        }

        return warped;
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

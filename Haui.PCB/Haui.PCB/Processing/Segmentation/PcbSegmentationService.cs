using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Segments and straightens a PCB board from the camera frame using fiducial hole template matching.
/// Canny + morphology close prepare the search image; four matched hole centers define the warp quad.
/// Implements <see cref="IPcbSegmentationService"/>.
/// </summary>
public class PcbSegmentationService : IPcbSegmentationService
{
    private readonly SegmentationParameters _parameters;
    private readonly IFiducialHoleTemplateService? _fiducialTemplates;
    private readonly IFiducialHoleDetectionService? _fiducialDetection;

    public PcbSegmentationService()
        : this(SegmentationSettings.Current, FiducialHoleServices.TemplateService, new FiducialHoleDetectionService())
    {
    }

    public PcbSegmentationService(SegmentationParameters parameters)
        : this(parameters, FiducialHoleServices.TemplateService, new FiducialHoleDetectionService())
    {
    }

    public PcbSegmentationService(
        SegmentationParameters parameters,
        IFiducialHoleTemplateService? fiducialTemplates,
        IFiducialHoleDetectionService? fiducialDetection)
    {
        _parameters = parameters;
        _fiducialTemplates = fiducialTemplates;
        _fiducialDetection = fiducialDetection;
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
        using var grayWork = new Mat();
        using var blurredWork = new Mat();
        using var edgesWork = new Mat();
        using var closedWork = new Mat();
        using var kernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(MorphKernelSize, MorphKernelSize));

        Cv2.CvtColor(source, grayWork, ColorConversionCodes.BGR2GRAY);
        Cv2.GaussianBlur(grayWork, blurredWork, new Size(5, 5), 0);

        double t1 = _parameters.CannyThreshold1;
        double t2 = _parameters.CannyThreshold2;
        Cv2.Canny(blurredWork, edgesWork, t1, t2);
        Cv2.MorphologyEx(edgesWork, closedWork, MorphTypes.Close, kernel, iterations: 3);

        Point2f[]? fiducialCenters = null;
        string? fiducialDescription = null;
        Mat? warped = null;

        if (_fiducialTemplates?.HasTemplates() == true && _fiducialDetection is not null)
        {
            var settings = _fiducialTemplates.LoadSettings();
            var templates = _fiducialTemplates.LoadTemplates();
            try
            {
                var fiducialResult = _fiducialDetection.Detect(
                    closedWork,
                    templates,
                    settings.MinMatchScore,
                    settings.MaxMatchDimension);

                if (fiducialResult.Success && fiducialResult.Centers is not null)
                {
                    fiducialCenters = fiducialResult.Centers;
                    fiducialDescription = fiducialResult.Message;
                    warped = WarpPerspective(source, fiducialCenters);
                }
                else
                {
                    fiducialDescription = fiducialResult.Message ?? "Không nhận diện được 4 lỗ định vị.";
                }
            }
            finally
            {
                foreach (var template in templates)
                    template.Dispose();
            }
        }
        else
        {
            fiducialDescription = "Chưa có mẫu lỗ định vị.";
        }

        return new SegmentationPipelineResult
        {
            Gray = includeDebugMats ? grayWork.Clone() : new Mat(),
            Blurred = includeDebugMats ? blurredWork.Clone() : new Mat(),
            Edges = includeDebugMats ? edgesWork.Clone() : new Mat(),
            Closed = includeDebugMats ? closedWork.Clone() : new Mat(),
            CannyThreshold1 = t1,
            CannyThreshold2 = t2,
            FiducialCenters = fiducialCenters,
            FiducialDescription = fiducialDescription,
            Warped = warped
        };
    }

    private static Mat WarpPerspective(Mat source, Point2f[] quad)
    {
        var ordered = OrderPoints(quad);

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

    private static Point2f[] OrderPoints(Point2f[] pts)
    {
        var sums = pts.Select(p => p.X + p.Y).ToArray();
        var diffs = pts.Select(p => p.Y - p.X).ToArray();

        return
        [
            pts[Array.IndexOf(sums, sums.Min())],
            pts[Array.IndexOf(diffs, diffs.Min())],
            pts[Array.IndexOf(sums, sums.Max())],
            pts[Array.IndexOf(diffs, diffs.Max())]
        ];
    }

    private static float Distance(Point2f a, Point2f b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}

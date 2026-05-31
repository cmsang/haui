using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Dịch vụ phân vùng và cắt bo mạch PCB từ ảnh nền bằng thuật toán phát hiện đường biên.
/// Hỗ trợ bo mạch hình chữ nhật / vuông xoay 360° — sử dụng MinAreaRect để
/// lấy hình chữ nhật nhỏ nhất bao quanh PCB, sau đó warp perspective căn thẳng.
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
    private const double MinAreaRatio = 0.01;
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
        bool usedFiducial = false;

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
                    usedFiducial = true;
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

        Point[][] contours = [];
        Point[]? bestContour = null;
        double bestArea = 0;

        bool skipContour = usedFiducial && !includeDebugMats;
        if (!skipContour)
        {
            Cv2.FindContours(
                closedWork,
                out contours,
                out _,
                RetrievalModes.External,
                ContourApproximationModes.ApproxSimple);

            if (contours.Length > 0)
            {
                double imageArea = source.Rows * source.Cols;
                double minArea = imageArea * MinAreaRatio;

                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);
                    if (area > minArea && area > bestArea)
                    {
                        bestArea = area;
                        bestContour = contour;
                    }
                }
            }
        }

        Point2f[]? quad = null;
        string? boxDesc = null;
        Mat? warped = null;

        if (fiducialCenters is not null)
        {
            quad = fiducialCenters;
            boxDesc = "4 lỗ tròn định vị (template matching)";
            warped = WarpPerspective(source, quad);
        }
        else if (bestContour is not null)
        {
            double epsilon = 0.02 * Cv2.ArcLength(bestContour, true);
            var approx = Cv2.ApproxPolyDP(bestContour, epsilon, true);

            if (approx.Length == 4)
            {
                quad = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();
                boxDesc = "Xấp xỉ tứ giác (4 góc phát hiện rõ)";
            }
            else
            {
                var rotatedRect = Cv2.MinAreaRect(bestContour);
                quad = Cv2.BoxPoints(rotatedRect);
                boxDesc = $"MinAreaRect (góc xoay ≈ {rotatedRect.Angle:F1}°)";
            }

            warped = WarpPerspective(source, quad);
        }

        return new SegmentationPipelineResult
        {
            Gray = includeDebugMats ? grayWork.Clone() : new Mat(),
            Blurred = includeDebugMats ? blurredWork.Clone() : new Mat(),
            Edges = includeDebugMats ? edgesWork.Clone() : new Mat(),
            Closed = includeDebugMats ? closedWork.Clone() : new Mat(),
            CannyThreshold1 = t1,
            CannyThreshold2 = t2,
            Contours = contours,
            BestContour = bestContour,
            BestArea = bestArea,
            Quad = quad,
            BoundingBoxDescription = boxDesc,
            UsedFiducialDetection = usedFiducial,
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

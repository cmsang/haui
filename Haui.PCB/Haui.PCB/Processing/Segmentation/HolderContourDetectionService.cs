using Haui.PCB.Models.Configuration;
using Haui.PCB.Models.Segmentation;
using OpenCvSharp;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Finds the holder support quad from morphology-close edges using dilated masks
/// and convex hull + minAreaRect.
/// </summary>
public sealed class HolderContourDetectionService : IHolderContourDetectionService
{
    private const int DilateKernelSize = 9;
    private const int DilateIterations = 2;
    private const double SoftAspectMultiplier = 2.0;

    public HolderContourResult Detect(
        Mat closedEdges,
        Rect? edgeSearchRoi,
        PcbBoardSettings holderSettings)
    {
        if (closedEdges.Empty())
            return HolderContourResult.Fail("Ảnh Morphology Close rỗng.");

        using var dilated = PrepareDilatedMask(closedEdges);

        var hullResult = TryDetectFromConvexHull(
            dilated,
            edgeSearchRoi,
            holderSettings,
            strictAspect: true)
            ?? TryDetectFromConvexHull(
                dilated,
                edgeSearchRoi,
                holderSettings,
                strictAspect: false);

        if (hullResult is not null)
            return hullResult;

        if (holderSettings.HasAspectConstraint)
        {
            return HolderContourResult.Fail(
                $"Không tìm được khung hộp đỡ {holderSettings.WidthMm:0}×{holderSettings.HeightMm:0} mm.");
        }

        return HolderContourResult.Fail("Không tìm được khung hộp đỡ hình chữ nhật hợp lệ.");
    }

    private static Mat PrepareDilatedMask(Mat closedEdges)
    {
        using var binary = EnsureBinary(closedEdges);
        using var kernel = Cv2.GetStructuringElement(
            MorphShapes.Rect,
            new Size(DilateKernelSize, DilateKernelSize));

        var dilated = new Mat();
        Cv2.Dilate(binary, dilated, kernel, iterations: DilateIterations);
        Cv2.MorphologyEx(dilated, dilated, MorphTypes.Close, kernel, iterations: 1);
        return dilated;
    }

    private static HolderContourResult? TryDetectFromConvexHull(
        Mat dilated,
        Rect? edgeSearchRoi,
        PcbBoardSettings holderSettings,
        bool strictAspect)
    {
        if (edgeSearchRoi is null)
            return null;

        var padded = PadRoi(edgeSearchRoi.Value, dilated.Width, dilated.Height, paddingFraction: 0.01);
        using var roiPatch = new Mat(dilated, padded);

        using var pointsMat = new Mat();
        Cv2.FindNonZero(roiPatch, pointsMat);
        if (pointsMat.Empty() || pointsMat.Total() < 4)
            return null;

        pointsMat.GetArray(out Point[] points);

        int offsetX = padded.X;
        int offsetY = padded.Y;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].X += offsetX;
            points[i].Y += offsetY;
        }

        int[] hullIndices = Cv2.ConvexHullIndices(points, clockwise: true);
        if (hullIndices.Length < 4)
            return null;

        var hull = hullIndices.Select(i => points[i]).ToArray();

        var rotated = Cv2.MinAreaRect(hull);
        var corners = rotated.Points();
        if (corners.Length != 4)
            return null;

        return TryAcceptQuad(
            corners,
            Cv2.ContourArea(hull),
            dilated.Width * dilated.Height,
            holderSettings,
            strictAspect,
            sourceLabel: "hull");
    }

    private static HolderContourResult? TryAcceptQuad(
        Point2f[] rawCorners,
        double area,
        double imageArea,
        PcbBoardSettings holderSettings,
        bool strictAspect,
        string sourceLabel)
    {
        if (!TryScoreQuad(
                rawCorners,
                area,
                imageArea,
                holderSettings,
                strictAspect,
                out var candidate))
            return null;

        var result = ToResult(candidate, holderSettings);
        return HolderContourResult.Ok(
            candidate.Corners,
            result.Message!.Replace("Khung hộp đỡ:", $"Khung hộp đỡ ({sourceLabel}):", StringComparison.Ordinal));
    }

    private static bool TryScoreQuad(
        Point2f[] rawCorners,
        double area,
        double imageArea,
        PcbBoardSettings holderSettings,
        bool strictAspect,
        out CandidateQuad candidate)
    {
        candidate = default!;

        float minEdgeLength = (float)(Math.Sqrt(imageArea) * 0.05);
        if (!QuadOrdering.AreDistinctCorners(rawCorners, minEdgeLength * 0.25f))
            return false;

        var ordered = QuadOrdering.OrderCorners(rawCorners);

        if (!QuadGeometry.MeetsRectTolerance(
                ordered,
                holderSettings.QuadRectTolerancePercent,
                out var metrics))
            return false;

        if (!QuadGeometry.MeetsAngleTolerance(
                ordered,
                holderSettings.QuadAngleToleranceDegrees,
                out _))
            return false;

        double aspectErr = 0;
        if (holderSettings.HasAspectConstraint)
        {
            aspectErr = QuadGeometry.AspectRatioError(
                metrics.Width,
                metrics.Height,
                holderSettings.WidthMm,
                holderSettings.HeightMm);

            double limit = holderSettings.AspectRatioTolerance
                           * (strictAspect ? 1.0 : SoftAspectMultiplier);
            if (aspectErr > limit)
                return false;
        }

        double areaNorm = area / Math.Max(imageArea, 1);
        candidate = new CandidateQuad(ordered, metrics, area, areaNorm, aspectErr, strictAspect);
        return true;
    }

    private static HolderContourResult ToResult(CandidateQuad candidate, PcbBoardSettings holderSettings)
    {
        string aspectText = holderSettings.HasAspectConstraint
            ? $" {holderSettings.WidthMm:0}×{holderSettings.HeightMm:0} mm"
            : string.Empty;

        string strictNote = candidate.StrictAspect ? string.Empty : " (sai số tỷ lệ nới lỏng)";

        return HolderContourResult.Ok(
            candidate.Corners,
            $"Khung hộp đỡ: {candidate.Metrics.Width:0}×{candidate.Metrics.Height:0} px, " +
            $"diện tích {candidate.AreaNorm:P0}{aspectText}{strictNote}.");
    }

    private static Rect PadRoi(Rect roi, int imageWidth, int imageHeight, double paddingFraction)
    {
        int padX = Math.Max(1, (int)(roi.Width * paddingFraction));
        int padY = Math.Max(1, (int)(roi.Height * paddingFraction));

        int x = Math.Clamp(roi.X - padX, 0, Math.Max(0, imageWidth - 1));
        int y = Math.Clamp(roi.Y - padY, 0, Math.Max(0, imageHeight - 1));
        int right = Math.Clamp(roi.X + roi.Width + padX, 1, imageWidth);
        int bottom = Math.Clamp(roi.Y + roi.Height + padY, 1, imageHeight);

        return new Rect(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
    }

    private static Mat EnsureBinary(Mat closed)
    {
        if (closed.Channels() == 1)
            return closed.Clone();

        var gray = new Mat();
        Cv2.CvtColor(closed, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private sealed record CandidateQuad(
        Point2f[] Corners,
        QuadGeometry.QuadMetrics Metrics,
        double Area,
        double AreaNorm,
        double AspectError,
        bool StrictAspect);
}

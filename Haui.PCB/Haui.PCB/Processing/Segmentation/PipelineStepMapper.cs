using System.Diagnostics;
using Haui.PCB.ViewModels.Pipeline;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Maps <see cref="SegmentationPipelineResult"/> to UI <see cref="PipelineStep"/> items for debug galleries.
/// </summary>
public static class PipelineStepMapper
{
    public static IReadOnlyList<PipelineStep> MapSteps(SegmentationPipelineResult pipeline, Mat source)
    {
        var steps = new List<PipelineStep>();
        var cloneSw = Stopwatch.StartNew();
        using var original = source.Clone();
        cloneSw.Stop();
        steps.Add(MakeStep(
            "Ảnh gốc",
            original,
            source.Channels() == 1
                ? "Ảnh Mono8 từ camera (đã làm mờ 5×5 khi grab)"
                : "Ảnh đầu vào từ camera / vùng đã chọn",
            cloneSw.Elapsed));

        var timings = pipeline.StepTimings;

        if (source.Channels() != 1)
        {
            steps.Add(MakeStep(
                "Gaussian Blur",
                pipeline.Blurred.Clone(),
                "Làm mờ để giảm nhiễu (kernel 5×5)",
                GetTiming(timings, SegmentationPipelineSteps.GaussianBlur)));
        }

        steps.Add(MakeStep(
            "Canny Edges",
            pipeline.Edges.Clone(),
            $"Phát hiện cạnh Canny (threshold1={pipeline.CannyThreshold1}, threshold2={pipeline.CannyThreshold2})",
            GetTiming(timings, SegmentationPipelineSteps.Canny)));

        steps.Add(MakeStep(
            "Morphology Close",
            BuildMorphologyCloseVisualization(pipeline),
            BuildMorphologyCloseDescription(pipeline),
            GetTiming(timings, SegmentationPipelineSteps.MorphologyClose)));

        if (!string.IsNullOrWhiteSpace(pipeline.ContourDescription))
        {
            const int maxDisplayDim = 1920;
            Mat holderVis;
            Point2f[]? drawCorners = pipeline.WarpQuadCorners;

            int maxDim = Math.Max(source.Width, source.Height);
            if (maxDim > maxDisplayDim)
            {
                double displayScale = maxDisplayDim / (double)maxDim;
                using var bgrSource = EnsureBgr(source);
                holderVis = new Mat();
                Cv2.Resize(bgrSource, holderVis, new Size(), displayScale, displayScale, InterpolationFlags.Area);
                if (drawCorners is { Length: 4 } scaledCorners)
                {
                    drawCorners = scaledCorners
                        .Select(c => new Point2f((float)(c.X * displayScale), (float)(c.Y * displayScale)))
                        .ToArray();
                }
            }
            else
            {
                holderVis = EnsureBgr(source);
            }

            if (drawCorners is { Length: 4 } corners)
                DrawHolderQuad(holderVis, corners, success: pipeline.Warped is not null);

            steps.Add(MakeStep(
                "Khung hộp đỡ",
                holderVis,
                pipeline.ContourDescription,
                GetTiming(timings, SegmentationPipelineSteps.HolderContour)));
        }

        if (pipeline.Warped is not null)
        {
            steps.Add(MakeStep(
                "Warp Perspective",
                pipeline.Warped.Clone(),
                "Bo mạch sau khi căn thẳng bằng perspective transform",
                GetTiming(timings, SegmentationPipelineSteps.Warp)));
        }
        else
        {
            steps.Add(MakeStep(
                "Kết quả",
                source.Clone(),
                "Không phát hiện được bo mạch",
                TimeSpan.Zero));
        }

        return steps;
    }

    private static Mat EnsureBgr(Mat source)
    {
        if (source.Channels() != 1)
            return source.Clone();

        var bgr = new Mat();
        Cv2.CvtColor(source, bgr, ColorConversionCodes.GRAY2BGR);
        return bgr;
    }

    private static Mat BuildMorphologyCloseVisualization(SegmentationPipelineResult pipeline)
    {
        using var closed = pipeline.Closed.Clone();
        var vis = new Mat();
        if (closed.Channels() == 1)
            Cv2.CvtColor(closed, vis, ColorConversionCodes.GRAY2BGR);
        else
            closed.CopyTo(vis);

        if (pipeline.EdgeSearchRoi is { } roi)
        {
            int thickness = Math.Clamp(Math.Min(vis.Width, vis.Height) / 180, 2, 6);
            Cv2.Rectangle(vis, roi, new Scalar(0, 165, 255), thickness);
        }

        return vis;
    }

    private static string BuildMorphologyCloseDescription(SegmentationPipelineResult pipeline)
    {
        const string baseText = "Đóng kín khoảng hở trên biên (Close, 3 lần lặp)";
        if (pipeline.EdgeSearchRoi is not { } roi)
            return baseText;

        return $"{baseText}. ROI biên: {roi.Width}×{roi.Height} px tại ({roi.X},{roi.Y})";
    }

    private static TimeSpan GetTiming(IReadOnlyDictionary<string, TimeSpan> timings, string key)
        => timings.TryGetValue(key, out var elapsed) ? elapsed : TimeSpan.Zero;

    private static PipelineStep MakeStep(string name, Mat mat, string description, TimeSpan elapsed)
    {
        var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
        bitmap.Freeze();
        mat.Dispose();
        return PipelineStep.WithTiming(name, bitmap, description, elapsed);
    }

    private static void DrawQuad(Mat img, Point2f[] pts, Scalar color, int thickness)
    {
        for (int i = 0; i < 4; i++)
        {
            var p1 = new Point((int)pts[i].X, (int)pts[i].Y);
            var p2 = new Point((int)pts[(i + 1) % 4].X, (int)pts[(i + 1) % 4].Y);
            Cv2.Line(img, p1, p2, color, thickness);
        }
    }

    private static void DrawHolderQuad(Mat img, Point2f[] corners, bool success)
    {
        int minDim = Math.Min(img.Width, img.Height);
        int thickness = Math.Clamp(minDim / 180, 2, 6);
        double fontScale = Math.Clamp(minDim / 900.0, 0.9, 3.0);
        int fontThickness = Math.Clamp(thickness, 2, 8);
        var quadColor = success ? new Scalar(0, 220, 0) : new Scalar(0, 200, 255);

        DrawQuad(img, corners, quadColor, thickness);

        for (int i = 0; i < 4; i++)
        {
            var center = new Point((int)corners[i].X, (int)corners[i].Y);
            var label = (i + 1).ToString();
            var labelPos = new Point(center.X + 8, center.Y - 8);
            Cv2.PutText(img, label, labelPos,
                HersheyFonts.HersheySimplex, fontScale, Scalar.All(0), fontThickness + 2, LineTypes.AntiAlias);
            Cv2.PutText(img, label, labelPos,
                HersheyFonts.HersheySimplex, fontScale, quadColor, fontThickness, LineTypes.AntiAlias);
        }
    }
}

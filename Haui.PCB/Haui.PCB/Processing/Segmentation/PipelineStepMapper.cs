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
            "Ảnh đầu vào từ camera / vùng đã chọn",
            cloneSw.Elapsed));

        var timings = pipeline.StepTimings;

        steps.Add(MakeStep(
            "Grayscale",
            pipeline.Gray.Clone(),
            "Chuyển ảnh màu sang ảnh xám (BGR → GRAY)",
            GetTiming(timings, SegmentationPipelineSteps.Grayscale)));

        steps.Add(MakeStep(
            "Gaussian Blur",
            pipeline.Blurred.Clone(),
            "Làm mờ để giảm nhiễu (kernel 5×5)",
            GetTiming(timings, SegmentationPipelineSteps.GaussianBlur)));

        steps.Add(MakeStep(
            "Canny Edges",
            pipeline.Edges.Clone(),
            $"Phát hiện cạnh Canny (threshold1={pipeline.CannyThreshold1}, threshold2={pipeline.CannyThreshold2})",
            GetTiming(timings, SegmentationPipelineSteps.Canny)));

        steps.Add(MakeStep(
            "Morphology Close",
            pipeline.Closed.Clone(),
            "Đóng kín khoảng hở trên biên (Close, 3 lần lặp)",
            GetTiming(timings, SegmentationPipelineSteps.MorphologyClose)));

        if (!string.IsNullOrWhiteSpace(pipeline.FiducialDescription))
        {
            const int maxFiducialDisplayDim = 1920;
            Mat fiducialVis;
            Point2f[] drawCenters;
            double[]? drawScores = pipeline.FiducialMatchScores;

            if (pipeline.FiducialCenters is { Length: > 0 } centers)
            {
                int maxDim = Math.Max(source.Width, source.Height);
                if (maxDim > maxFiducialDisplayDim)
                {
                    double displayScale = maxFiducialDisplayDim / (double)maxDim;
                    fiducialVis = new Mat();
                    Cv2.Resize(source, fiducialVis, new Size(), displayScale, displayScale, InterpolationFlags.Area);
                    drawCenters = centers
                        .Select(c => new Point2f((float)(c.X * displayScale), (float)(c.Y * displayScale)))
                        .ToArray();
                }
                else
                {
                    fiducialVis = source.Clone();
                    drawCenters = centers;
                }

                DrawFiducialHoles(fiducialVis, drawCenters, drawScores);
            }
            else
            {
                fiducialVis = source.Clone();
            }

            steps.Add(MakeStep(
                "Lỗ định vị",
                fiducialVis,
                pipeline.FiducialDescription,
                GetTiming(timings, SegmentationPipelineSteps.Fiducial)));
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

    private static TimeSpan GetTiming(IReadOnlyDictionary<string, TimeSpan> timings, string key)
        => timings.TryGetValue(key, out var elapsed) ? elapsed : TimeSpan.Zero;

    private static PipelineStep MakeStep(string name, Mat mat, string description, TimeSpan elapsed)
    {
        var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
        bitmap.Freeze();
        mat.Dispose();
        return PipelineStep.WithTiming(name, bitmap, description, elapsed);
    }

    private static void DrawQuad(Mat img, Point2f[] pts, Scalar color)
    {
        int thickness = Math.Clamp(Math.Min(img.Width, img.Height) / 180, 2, 6);
        for (int i = 0; i < 4; i++)
        {
            var p1 = new Point((int)pts[i].X, (int)pts[i].Y);
            var p2 = new Point((int)pts[(i + 1) % 4].X, (int)pts[(i + 1) % 4].Y);
            Cv2.Line(img, p1, p2, color, thickness);
        }
    }

    private static void DrawFiducialHoles(Mat img, Point2f[] centers, double[]? matchScores)
    {
        const int RequiredHoleCount = 4;
        int minDim = Math.Min(img.Width, img.Height);
        int radius = Math.Clamp(minDim / 22, 20, 120);
        int thickness = Math.Clamp(minDim / 70, 4, 16);
        double fontScale = Math.Clamp(minDim / 900.0, 0.9, 3.0);
        int fontThickness = Math.Clamp(thickness - 1, 2, 8);

        bool complete = centers.Length >= RequiredHoleCount;
        var holeColor = complete ? new Scalar(0, 220, 0) : new Scalar(0, 200, 255);
        var quadColor = new Scalar(0, 165, 255);

        for (int i = 0; i < centers.Length; i++)
        {
            var center = new Point((int)centers[i].X, (int)centers[i].Y);
            Cv2.Circle(img, center, radius + 2, Scalar.All(0), thickness + 2);
            Cv2.Circle(img, center, radius, holeColor, thickness);
            Cv2.Circle(img, center, Math.Max(4, radius / 5), holeColor, -1);

            var label = (i + 1).ToString();
            if (matchScores is not null && i < matchScores.Length)
                label += $" {matchScores[i]:P0}";

            var labelPos = new Point(center.X + radius + 6, center.Y + radius / 3);
            Cv2.PutText(img, label, labelPos,
                HersheyFonts.HersheySimplex, fontScale, Scalar.All(0), fontThickness + 2, LineTypes.AntiAlias);
            Cv2.PutText(img, label, labelPos,
                HersheyFonts.HersheySimplex, fontScale, holeColor, fontThickness, LineTypes.AntiAlias);
        }

        if (complete)
            DrawQuad(img, centers, quadColor);
    }
}

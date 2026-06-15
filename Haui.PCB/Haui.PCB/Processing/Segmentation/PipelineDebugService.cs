using System.Diagnostics;
using Haui.PCB.ViewModels.Pipeline;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.Processing.Segmentation;

/// <summary>
/// Chạy pipeline phân vùng PCB theo từng bước và thu thập ảnh trung gian để hiển thị debug.
/// Logic xử lý ảnh nằm trong <see cref="PcbSegmentationService"/>; lớp này chỉ chuyển kết quả sang UI.
/// </summary>
public class PipelineDebugService : IPipelineDebugService
{
    private readonly IPcbSegmentationService _segmentation;

    public PipelineDebugService(IPcbSegmentationService segmentation)
    {
        _segmentation = segmentation;
    }

    public PipelineDebugService()
        : this(new PcbSegmentationService())
    {
    }

    public IReadOnlyList<PipelineStep> RunSteps(Mat source)
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

        using var pipeline = _segmentation.RunPipeline(source);
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

        if (pipeline.FiducialCenters is not null)
        {
            using var fiducialVis = new Mat();
            Cv2.CvtColor(pipeline.Closed, fiducialVis, ColorConversionCodes.GRAY2BGR);
            DrawFiducialHoles(fiducialVis, pipeline.FiducialCenters);
            steps.Add(MakeStep(
                "Fiducial Matching",
                fiducialVis,
                pipeline.FiducialDescription ?? "Template matching 4 lỗ trên ảnh Morphology Close",
                GetTiming(timings, SegmentationPipelineSteps.Fiducial)));
        }
        else if (!string.IsNullOrWhiteSpace(pipeline.FiducialDescription))
        {
            steps.Add(MakeStep(
                "Fiducial Matching",
                pipeline.Closed.Clone(),
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
        for (int i = 0; i < 4; i++)
        {
            var p1 = new Point((int)pts[i].X, (int)pts[i].Y);
            var p2 = new Point((int)pts[(i + 1) % 4].X, (int)pts[(i + 1) % 4].Y);
            Cv2.Line(img, p1, p2, color, 2);
        }
    }

    private static void DrawFiducialHoles(Mat img, Point2f[] centers)
    {
        for (int i = 0; i < centers.Length; i++)
        {
            var center = new Point((int)centers[i].X, (int)centers[i].Y);
            Cv2.Circle(img, center, 12, new Scalar(0, 255, 255), 2);
            Cv2.PutText(img, (i + 1).ToString(),
                new Point(center.X + 14, center.Y + 5),
                HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 255), 2);
        }

        DrawQuad(img, centers, new Scalar(255, 128, 0));
    }
}

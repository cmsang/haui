using Haui.PCB.Models;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.Processing;

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

        steps.Add(MakeStep("Ảnh gốc", source.Clone(), "Ảnh đầu vào từ camera / vùng đã chọn"));

        using var pipeline = _segmentation.RunPipeline(source);

        steps.Add(MakeStep("Grayscale", pipeline.Gray.Clone(),
            "Chuyển ảnh màu sang ảnh xám (BGR → GRAY)"));

        steps.Add(MakeStep("Gaussian Blur", pipeline.Blurred.Clone(),
            "Làm mờ để giảm nhiễu (kernel 5×5)"));

        steps.Add(MakeStep("Canny Edges", pipeline.Edges.Clone(),
            $"Phát hiện cạnh Canny (threshold1={pipeline.CannyThreshold1}, threshold2={pipeline.CannyThreshold2})"));

        steps.Add(MakeStep("Morphology Close", pipeline.Closed.Clone(),
            "Đóng kín khoảng hở trên biên (Close, 3 lần lặp)"));

        if (pipeline.FiducialCenters is not null)
        {
            using var fiducialVis = new Mat();
            Cv2.CvtColor(pipeline.Closed, fiducialVis, ColorConversionCodes.GRAY2BGR);
            DrawFiducialHoles(fiducialVis, pipeline.FiducialCenters, pipeline.Quad);
            steps.Add(MakeStep("Fiducial Matching", fiducialVis,
                pipeline.FiducialDescription ?? "Template matching 4 lỗ trên ảnh Morphology Close"));
        }
        else if (!string.IsNullOrWhiteSpace(pipeline.FiducialDescription))
        {
            steps.Add(MakeStep("Fiducial Matching", pipeline.Closed.Clone(),
                pipeline.FiducialDescription));
        }

        using var contourVis = source.Clone();
        if (pipeline.BestContour is not null)
        {
            Cv2.DrawContours(contourVis, pipeline.Contours, -1, new Scalar(0, 200, 0), 1);
            Cv2.DrawContours(contourVis, [pipeline.BestContour], -1, new Scalar(0, 0, 255), 3);
        }

        steps.Add(MakeStep("Contour Detection", contourVis,
            $"Tìm contour — xanh: tất cả, đỏ: lớn nhất ({(pipeline.BestContour is null ? "không tìm thấy" : $"area≈{pipeline.BestArea:F0}px²")})"));

        if (pipeline.Quad is not null && pipeline.BoundingBoxDescription is not null)
        {
            using var boxVis = source.Clone();
            DrawQuad(boxVis, pipeline.Quad, new Scalar(255, 128, 0));
            steps.Add(MakeStep("Bounding Quad", boxVis, pipeline.BoundingBoxDescription));

            if (pipeline.Warped is not null)
            {
                steps.Add(MakeStep("Warp Perspective", pipeline.Warped.Clone(),
                    "Bo mạch sau khi căn thẳng bằng perspective transform"));
            }
        }
        else
        {
            steps.Add(MakeStep("Kết quả", source.Clone(), "Không phát hiện được bo mạch"));
        }

        return steps;
    }

    private static PipelineStep MakeStep(string name, Mat mat, string description)
    {
        var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
        bitmap.Freeze();
        mat.Dispose();
        return new PipelineStep { StepName = name, Image = bitmap, Description = description };
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

    private static void DrawFiducialHoles(Mat img, Point2f[] centers, Point2f[]? quad)
    {
        for (int i = 0; i < centers.Length; i++)
        {
            var center = new Point((int)centers[i].X, (int)centers[i].Y);
            Cv2.Circle(img, center, 12, new Scalar(0, 255, 255), 2);
            Cv2.PutText(img, (i + 1).ToString(),
                new Point(center.X + 14, center.Y + 5),
                HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 255), 2);
        }

        if (quad is not null)
            DrawQuad(img, quad, new Scalar(255, 128, 0));
    }
}

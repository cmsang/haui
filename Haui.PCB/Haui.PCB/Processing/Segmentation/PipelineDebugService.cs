using Haui.PCB.ViewModels.Pipeline;
using OpenCvSharp;

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
        using var pipeline = _segmentation.RunPipeline(source);
        return PipelineStepMapper.MapSteps(pipeline, source);
    }
}

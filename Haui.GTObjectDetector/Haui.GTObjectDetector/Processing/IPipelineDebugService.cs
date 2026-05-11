using Haui.GTObjectDetector.Models;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Giao diện dịch vụ chạy pipeline phân vùng PCB theo từng bước,
/// trả về danh sách ảnh trung gian để hiển thị debug.
/// </summary>
public interface IPipelineDebugService
{
    /// <summary>
    /// Chạy toàn bộ pipeline trên <paramref name="source"/> và trả về
    /// danh sách các bước, mỗi bước chứa tên và ảnh trung gian.
    /// </summary>
    IReadOnlyList<PipelineStep> RunSteps(Mat source);
}

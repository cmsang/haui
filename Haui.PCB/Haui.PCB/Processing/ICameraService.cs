using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Giao diện trừu tượng cho dịch vụ camera Basler: dò tìm, kết nối, lấy frame.
/// </summary>
public interface ICameraService : IDisposable
{
    bool IsRunning { get; }

    event Action<Mat>? FrameArrived;

    Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync();

    Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(string serialNumber);

    void Start(string serialNumber, int width, int height);

    void Stop();

    Mat? GrabFrame();

    Task<Mat?> CaptureSharpestFrameAsync(int durationMs = 1500, CancellationToken cancellationToken = default);
}

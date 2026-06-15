using OpenCvSharp;

namespace Haui.PCB.Processing.Camera;

/// <summary>
/// Abstraction for Basler camera: discover, connect, grab frames.
/// </summary>
public interface ICameraService : IDisposable
{
    bool IsRunning { get; }

    event Action<Mat>? FrameArrived;

    Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync();

    Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(CameraInfo camera);

    Task StartAsync(CameraInfo camera, int width, int height, CancellationToken cancellationToken = default);

    void Start(CameraInfo camera, int width, int height);

    void Stop();

    Mat? GrabFrame();

    Task<Mat?> CaptureSharpestFrameAsync(int durationMs = 1500, CancellationToken cancellationToken = default);
}

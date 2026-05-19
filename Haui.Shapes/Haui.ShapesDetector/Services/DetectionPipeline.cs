using Haui.ShapesDetector.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Haui.ShapesDetector.Services;

/// <summary>Raised on the camera thread for every new frame — no detection latency.</summary>
public sealed record FrameReadyEventArgs(Bitmap Frame, List<DetectionResult> CachedDetections);

/// <summary>Raised on a thread-pool thread when YOLO finishes processing a frame.</summary>
public sealed record DetectionCompletedEventArgs(Bitmap Frame, List<DetectionResult> Detections);

/// <summary>
/// Orchestrates the camera capture and YOLO detection pipeline:
/// <list type="bullet">
///   <item>Displays each frame immediately (no YOLO wait)</item>
///   <item>Runs YOLO in parallel, always on the latest available frame</item>
///   <item>Caches the last detection results for live overlay</item>
/// </list>
/// </summary>
public sealed class DetectionPipeline : IDisposable
{
    private readonly IDetectionService _detectionService;
    private readonly CameraService _cameraService;

    private Bitmap? _latestRawFrame;
    private Bitmap? _pendingYoloFrame;
    private readonly object _frameLock = new();
    private volatile List<DetectionResult> _cachedDetections = [];
    private int _isDetecting = 0; // 0 = idle, 1 = running (Interlocked)
    private float _confidenceThreshold = 0.75f;

    /// <summary>Fired on the camera background thread (~30 fps) with the raw frame and last known detections.</summary>
    public event EventHandler<FrameReadyEventArgs>? FrameReady;

    /// <summary>Fired on a thread-pool thread after YOLO finishes — frame is the LATEST from camera.</summary>
    public event EventHandler<DetectionCompletedEventArgs>? DetectionCompleted;

    /// <summary>Fired when any internal error occurs.</summary>
    public event EventHandler<string>? ErrorOccurred;

    public bool IsInitialized => _detectionService.IsInitialized;
    public bool IsRunning     => _cameraService.IsRunning;

    /// <summary>The most recently cached detection results (volatile reference — thread-safe read).</summary>
    public List<DetectionResult> CachedDetections => _cachedDetections;

    /// <summary>Current confidence threshold (0.01 – 1.0). Default 0.75.</summary>
    public float ConfidenceThreshold => _confidenceThreshold;

    public DetectionPipeline(IDetectionService detectionService, CameraService cameraService)
    {
        _detectionService = detectionService;
        _cameraService    = cameraService;

        _cameraService.FrameCaptured += OnFrameCaptured;
        _cameraService.ErrorOccurred += (_, msg) => ErrorOccurred?.Invoke(this, msg);
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    public Task InitializeAsync(string modelPath, string classesPath)
        => _detectionService.InitializeAsync(modelPath, classesPath);

    public void Start() => _cameraService.Start();
    public void Stop()  => _cameraService.Stop();

    public void SwitchCamera(int index) => _cameraService.SwitchCamera(index);

    public static List<CameraInfo> GetAvailableCameras() => CameraService.GetAvailableCameras();

    public Bitmap? CaptureSnapshot() => _cameraService.CaptureSnapshot();

    /// <summary>
    /// Updates the confidence threshold and forwards it to the underlying detection service.
    /// </summary>
    public void SetConfidenceThreshold(float threshold)
    {
        _confidenceThreshold = Math.Clamp(threshold, 0.01f, 1.0f);
        _detectionService.SetConfidenceThreshold(_confidenceThreshold);
    }

    /// <summary>
    /// Detects objects on a manually captured snapshot and updates <see cref="CachedDetections"/>.
    /// </summary>
    public async Task<List<DetectionResult>> DetectSnapshotAsync(Bitmap bitmap)
    {
        var detections = await DetectCoreAsync(bitmap);
        _cachedDetections = detections;
        return detections;
    }

    // ─── Internal pipeline ─────────────────────────────────────────────────────

    private void OnFrameCaptured(object? sender, Bitmap bitmap)
    {
        if (!_detectionService.IsInitialized) return;

        // Separate clones — different lifetimes (_latestRawFrame outlives _pendingYoloFrame)
        var rawClone  = (Bitmap)bitmap.Clone();
        var yoloClone = (Bitmap)bitmap.Clone();
        Bitmap? oldRaw;

        lock (_frameLock)
        {
            oldRaw          = _latestRawFrame;
            _latestRawFrame = rawClone;

            _pendingYoloFrame?.Dispose();   // discard superseded frame
            _pendingYoloFrame = yoloClone;
        }
        oldRaw?.Dispose();

        // Notify subscriber immediately — no YOLO wait
        FrameReady?.Invoke(this, new FrameReadyEventArgs((Bitmap)bitmap.Clone(), _cachedDetections));

        // Wake YOLO worker (only one instance allowed at a time)
        if (Interlocked.CompareExchange(ref _isDetecting, 1, 0) == 0)
            _ = RunDetectionLoopAsync();
    }

    private async Task RunDetectionLoopAsync()
    {
        try
        {
            while (true)
            {
                Bitmap? frame;
                lock (_frameLock)
                {
                    frame             = _pendingYoloFrame;
                    _pendingYoloFrame = null;
                }

                if (frame == null)
                {
                    // Release before exiting — guard against a race where a new frame
                    // arrived between the null-check above and this release.
                    Interlocked.Exchange(ref _isDetecting, 0);
                    lock (_frameLock)
                    {
                        if (_pendingYoloFrame == null) break;  // truly empty → exit
                    }
                    // A new frame snuck in; try to reclaim the worker slot
                    if (Interlocked.CompareExchange(ref _isDetecting, 1, 0) != 0) break;
                    continue;
                }

                try
                {
                    var detections = await DetectCoreAsync(frame);
                    _cachedDetections = detections;

                    // Overlay boxes on the LATEST raw frame, not the one YOLO just processed
                    Bitmap? latestDisplay;
                    lock (_frameLock)
                    {
                        latestDisplay = _latestRawFrame != null
                            ? (Bitmap)_latestRawFrame.Clone()
                            : null;
                    }

                    if (latestDisplay != null)
                        DetectionCompleted?.Invoke(this, new DetectionCompletedEventArgs(latestDisplay, detections));
                }
                finally
                {
                    frame.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _isDetecting, 0);
            ErrorOccurred?.Invoke(this, $"Detection error: {ex.Message}");
        }
    }

    private async Task<List<DetectionResult>> DetectCoreAsync(Bitmap bitmap)
    {
        // Convert to grayscale via OpenCvSharp (SIMD-accelerated, BT.601)
        // Encode as PNG lossless — tránh artifact JPEG ảnh hưởng YOLO
        using var colorMat = BitmapConverter.ToMat(bitmap);
        using var grayMat  = new Mat();
        var code = colorMat.Channels() == 4
            ? ColorConversionCodes.BGRA2GRAY
            : ColorConversionCodes.BGR2GRAY;
        Cv2.CvtColor(colorMat, grayMat, code);
        Cv2.ImEncode(".png", grayMat, out var pngBytes);
        return await _detectionService.DetectAsync(pngBytes, bitmap.Width, bitmap.Height);
    }

    // ───────────────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _cameraService.FrameCaptured -= OnFrameCaptured;
        _cameraService.Dispose();
        (_detectionService as IDisposable)?.Dispose();

        lock (_frameLock)
        {
            _latestRawFrame?.Dispose();
            _pendingYoloFrame?.Dispose();
            _latestRawFrame  = null;
            _pendingYoloFrame = null;
        }
    }
}

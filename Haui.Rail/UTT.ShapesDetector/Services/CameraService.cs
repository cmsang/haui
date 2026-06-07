using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;

namespace UTT.ShapesDetector.Services;

public class CameraService : IDisposable
{
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private Task? _captureTask;
    private int _currentDeviceIndex = 0;

    public event EventHandler<Bitmap>? FrameCaptured;
    public event EventHandler<string>? ErrorOccurred;
    public bool IsRunning { get; private set; }

    /// <summary>
    /// L?y danh s�ch c�c camera c� s?n
    /// </summary>
    public static List<CameraInfo> GetAvailableCameras()
    {
        var cameras = new List<CameraInfo>();

        for (int i = 0; i < 10; i++)
        {
            try
            {
                using var capture = new VideoCapture(i, VideoCaptureAPIs.DSHOW);
                if (capture.IsOpened())
                {
                    cameras.Add(new CameraInfo
                    {
                        Index = i,
                        Name = $"Camera {i}"
                    });
                    capture.Release();
                }
            }
            catch
            {
                // Camera kh�ng kh? d?ng
            }
        }

        return cameras;
    }

    public void Start(int deviceIndex = 0)
    {
        if (IsRunning) return;

        try
        {
            _currentDeviceIndex = deviceIndex;
            _capture = new VideoCapture(deviceIndex, VideoCaptureAPIs.DSHOW);
            
            if (!_capture.IsOpened())
            {
                ErrorOccurred?.Invoke(this, "Cannot open camera");
                return;
            }

            // Set camera properties
            //_capture.Set(VideoCaptureProperties.FrameWidth, 1280);
            //_capture.Set(VideoCaptureProperties.FrameHeight, 720);
            _capture.Set(VideoCaptureProperties.Fps, 30);

            _cts = new CancellationTokenSource();
            _captureTask = Task.Run(() => CaptureLoop(_cts.Token));
            IsRunning = true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Camera error: {ex.Message}");
        }
    }

    private async Task CaptureLoop(CancellationToken token)
    {
        using var frame = new Mat();

        while (!token.IsCancellationRequested && _capture?.IsOpened() == true)
        {
            try
            {
                if (_capture.Read(frame) && !frame.Empty())
                {
                    var bitmap = BitmapConverter.ToBitmap(frame);
                    FrameCaptured?.Invoke(this, bitmap);
                }

                await Task.Delay(33, token); // ~30 FPS
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Frame capture error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _captureTask?.Wait(1000);
        _capture?.Release();
        _cts?.Dispose();
        IsRunning = false;
    }

    /// <summary>
    /// Chuy?n ??i sang camera kh�c
    /// </summary>
    public void SwitchCamera(int deviceIndex)
    {
        if (_currentDeviceIndex == deviceIndex) return;

        var wasRunning = IsRunning;

        if (wasRunning)
        {
            Stop();
        }

        if (wasRunning)
        {
            Start(deviceIndex);
        }
        else
        {
            _currentDeviceIndex = deviceIndex;
        }
    }

    public Bitmap? CaptureSnapshot()
    {
        if (_capture?.IsOpened() != true)
            return null;

        using var frame = new Mat();
        if (_capture.Read(frame) && !frame.Empty())
        {
            return BitmapConverter.ToBitmap(frame);
        }

        return null;
    }

    public void Dispose()
    {
        Stop();
        _capture?.Dispose();
    }
}

/// <summary>
/// Th�ng tin camera
/// </summary>
public class CameraInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}

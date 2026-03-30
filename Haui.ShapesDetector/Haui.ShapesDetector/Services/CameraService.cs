using AForge.Video.DirectShow;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;

namespace Haui.ShapesDetector.Services;

public class CameraService : IDisposable
{
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private Task? _captureTask;

    public event EventHandler<Bitmap>? FrameCaptured;
    public event EventHandler<string>? ErrorOccurred;
    public bool IsRunning { get; private set; }
    VideoCaptureDevice captureDevice;
    Bitmap _crbitmap;

    public void Start(int deviceIndex = 0)
    {
        if (IsRunning) return;
        try
        {
            FilterInfoCollection filterInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            captureDevice = new VideoCaptureDevice(filterInfo[deviceIndex].MonikerString);
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();

            IsRunning = true;

        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tìm thấy thông tin camera. Vui lòng kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine($"{ex}");
            Application.Exit();
        }
    }

    private void CaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
    {
        try
        {
            // Clone bitmap từ event args
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            FrameCaptured?.Invoke(this, bitmap);
            _crbitmap = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera error: {ex.Message}");
        }

    }
    public Bitmap? CaptureSnapshot()
    {
        if (IsRunning != true)
            return null;

        if (_crbitmap != null)
            return _crbitmap;
        else return null;
    }

    public void Dispose()
    {
        Stop();
    }

    public void Stop()
    {
        try
        {
            if (!IsRunning) return;
            captureDevice.Stop();
            IsRunning = false;
        }
        catch (Exception)
        {

        }

    }

    #region Luồng cũ

    public void Start1(int deviceIndex = 0)
    {
        if (IsRunning) return;

        try
        {
            _capture = new VideoCapture(deviceIndex, VideoCaptureAPIs.DSHOW);

            if (!_capture.IsOpened())
            {
                ErrorOccurred?.Invoke(this, "Cannot open camera");
                return;
            }

            // Set camera properties
            _capture.Set(VideoCaptureProperties.FrameWidth, 1280);
            _capture.Set(VideoCaptureProperties.FrameHeight, 720);
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

    public void Stop1()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        _captureTask?.Wait(1000);
        _capture?.Release();
        _cts?.Dispose();
        IsRunning = false;
    }

    public Bitmap? CaptureSnapshot1()
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

    public void Dispose1()
    {
        Stop();
        _capture?.Dispose();
    }
    #endregion
}

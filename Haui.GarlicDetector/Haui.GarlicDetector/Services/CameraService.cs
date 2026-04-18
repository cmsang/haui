using OpenCvSharp;

namespace Haui.GarlicDetector.Services;

/// <summary>Thông tin một camera được tìm thấy trong hệ thống.</summary>
public sealed class CameraInfo
{
    public int    Index { get; init; }
    public string Name  { get; init; } = string.Empty;
    public override string ToString() => Name;
}

/// <summary>Độ phân giải camera (chiều rộng × chiều cao).</summary>
public sealed class CameraResolution
{
    public int    Width  { get; init; }
    public int    Height { get; init; }
    public string Label  { get; init; } = string.Empty;
    public override string ToString() => Label;

    /// <summary>Danh sách các độ phân giải phổ biến.</summary>
    public static List<CameraResolution> GetPresets() =>
    [
        new() { Width = 320,  Height = 240,  Label = "320 × 240" },
        new() { Width = 640,  Height = 480,  Label = "640 × 480" },
        new() { Width = 1280, Height = 720,  Label = "1280 × 720 (HD)" },
        new() { Width = 1920, Height = 1080, Label = "1920 × 1080 (FHD)" },
    ];
}

/// <summary>
/// Dịch vụ capture ảnh từ camera với tốc độ ~30 fps.
/// Phát sự kiện <see cref="FrameCaptured"/> mỗi khi có frame mới
/// và <see cref="ErrorOccurred"/> khi có lỗi.
/// </summary>
public sealed class CameraService : IDisposable
{
    private VideoCapture?             _capture;
    private CancellationTokenSource?  _cts;
    private Task?                     _captureTask;
    private readonly object           _captureLock = new();

    /// <summary>Sự kiện phát ra mỗi khi có frame mới từ camera.</summary>
    public event EventHandler<Bitmap>? FrameCaptured;

    /// <summary>Sự kiện phát ra khi có lỗi trong quá trình capture.</summary>
    public event EventHandler<string>? ErrorOccurred;

    /// <summary>Camera có đang capture hay không.</summary>
    public bool IsRunning { get; private set; }

    // ─── API công khai ────────────────────────────────────────────────────────

    /// <summary>
    /// Bắt đầu capture từ camera chỉ định với độ phân giải tùy chọn.
    /// Nếu camera đang chạy sẽ dừng trước rồi khởi động lại.
    /// </summary>
    public void Start(int deviceIndex = 0, CameraResolution? resolution = null)
    {
        Stop();

        lock (_captureLock)
        {
            // Mở camera qua DirectShow (DSHOW) — hỗ trợ đa camera trên Windows
            _capture = new VideoCapture(deviceIndex, VideoCaptureAPIs.DSHOW);

            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                ErrorOccurred?.Invoke(this, $"Không thể mở camera {deviceIndex}.");
                return;
            }

            // Thiết lập độ phân giải nếu được chỉ định
            if (resolution != null)
            {
                _capture.Set(VideoCaptureProperties.FrameWidth,  resolution.Width);
                _capture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
            }

            _cts      = new CancellationTokenSource();
            IsRunning = true;
        }

        _captureTask = Task.Run(() => CaptureLoop(_cts!.Token));
    }

    /// <summary>Dừng vòng lặp capture và giải phóng VideoCapture.</summary>
    public void Stop()
    {
        lock (_captureLock)
        {
            if (!IsRunning) return;
            IsRunning = false;
            _cts?.Cancel();
        }

        try { _captureTask?.Wait(TimeSpan.FromSeconds(2)); } catch { /* bỏ qua khi hủy */ }

        lock (_captureLock)
        {
            _capture?.Dispose();
            _capture = null;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>Chuyển sang camera khác (dừng rồi khởi động lại với index mới).</summary>
    public void SwitchCamera(int deviceIndex, CameraResolution? resolution = null)
        => Start(deviceIndex, resolution);

    /// <summary>Thay đổi độ phân giải camera đang chạy (dừng rồi khởi động lại).</summary>
    public void SwitchResolution(int deviceIndex, CameraResolution resolution)
        => Start(deviceIndex, resolution);

    /// <summary>Chụp một ảnh tĩnh từ camera hiện tại.</summary>
    public Bitmap? CaptureSnapshot()
    {
        lock (_captureLock)
        {
            if (_capture == null || !_capture.IsOpened()) return null;
            using var mat = new Mat();
            _capture.Read(mat);
            return mat.Empty() ? null : OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        }
    }

    /// <summary>
    /// Quét các camera khả dụng bằng cách thử mở tối đa 10 thiết bị.
    /// Dừng ngay khi gặp thiết bị đầu tiên không tồn tại.
    /// </summary>
    public static List<CameraInfo> GetAvailableCameras()
    {
        var list = new List<CameraInfo>();

        for (int i = 0; i < 10; i++)
        {
            using var cap = new VideoCapture(i, VideoCaptureAPIs.DSHOW);
            if (cap.IsOpened())
                list.Add(new CameraInfo { Index = i, Name = $"Camera {i}" });
            else
                break; // dừng khi gặp thiết bị đầu tiên không tồn tại
        }

        // Đảm bảo luôn có ít nhất một mục trong danh sách
        if (list.Count == 0)
            list.Add(new CameraInfo { Index = 0, Name = "Camera 0 (mặc định)" });

        return list;
    }

    // ─── Vòng lặp capture nội bộ ─────────────────────────────────────────────

    private void CaptureLoop(CancellationToken token)
    {
        try
        {
            using var mat = new Mat();

            while (!token.IsCancellationRequested)
            {
                VideoCapture? cap;
                lock (_captureLock) { cap = _capture; }
                if (cap == null) break;

                if (!cap.Read(mat) || mat.Empty())
                {
                    Thread.Sleep(10);
                    continue;
                }

                // Chuyển Mat → Bitmap, phát sự kiện rồi dispose ngay —
                // subscribers phải clone nếu cần giữ lại (GarlicPipeline đã làm vậy)
                using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                FrameCaptured?.Invoke(this, bitmap);

                // Giới hạn ~30 fps
                Thread.Sleep(33);
            }
        }
        catch (OperationCanceledException) { /* dừng bình thường */ }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Lỗi vòng lặp capture: {ex.Message}");
        }
    }

    // ─── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose() => Stop();
}

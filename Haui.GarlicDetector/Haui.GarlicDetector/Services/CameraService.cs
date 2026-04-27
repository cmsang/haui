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
    /// Kích hoạt lấy nét tự động của camera (nếu hỗ trợ) rồi chụp <paramref name="frameCount"/>
    /// khung hình liên tiếp, trả về khung nét nhất dựa trên phương sai Laplacian.
    /// <paramref name="roi"/> giới hạn vùng tính độ nét về vùng nhận diện đã chọn.
    /// </summary>
    public Bitmap? CaptureSharpestFrame(int frameCount = 10, Rectangle? roi = null)
    {
        // Thử kích hoạt autofocus của camera (một số camera DSHOW hỗ trợ; bỏ qua nếu không hỗ trợ)
        lock (_captureLock)
        {
            if (_capture != null && _capture.IsOpened())
            {
                try { _capture.Set(VideoCaptureProperties.AutoFocus, 1); } catch { /* bỏ qua */ }
            }
        }

        // Đợi camera điều chỉnh tiêu cự
        Thread.Sleep(300);

        Bitmap? sharpest = null;
        double maxVariance = -1;

        for (int i = 0; i < frameCount; i++)
        {
            var bmp = CaptureSnapshot();
            if (bmp == null) { Thread.Sleep(50); continue; }

            using var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            double variance = ComputeLaplacianVariance(mat, roi);

            if (variance > maxVariance)
            {
                maxVariance = variance;
                sharpest?.Dispose();
                sharpest = bmp;
            }
            else
            {
                bmp.Dispose();
            }

            // Khoảng cách ~30 fps giữa các lần chụp
            Thread.Sleep(33);
        }

        return sharpest;
    }

    /// <summary>
    /// Tính phương sai Laplacian (chỉ số độ nét) trên vùng <paramref name="roi"/> của ảnh BGR.
    /// Giá trị càng cao → ảnh càng nét.
    /// </summary>
    private static double ComputeLaplacianVariance(Mat bgrMat, Rectangle? roi)
    {
        Mat? roiMat = null;
        Mat matToUse = bgrMat;

        if (roi.HasValue)
        {
            var r = roi.Value;
            int x = Math.Clamp(r.X, 0, bgrMat.Width - 1);
            int y = Math.Clamp(r.Y, 0, bgrMat.Height - 1);
            int w = Math.Clamp(r.Width,  1, bgrMat.Width  - x);
            int h = Math.Clamp(r.Height, 1, bgrMat.Height - y);
            if (w >= 8 && h >= 8)
            {
                roiMat    = new Mat(bgrMat, new Rect(x, y, w, h));
                matToUse  = roiMat;
            }
        }

        try
        {
            using var gray = new Mat();
            Cv2.CvtColor(matToUse, gray, ColorConversionCodes.BGR2GRAY);
            using var lap = new Mat();
            Cv2.Laplacian(gray, lap, MatType.CV_64F);
            Cv2.MeanStdDev(lap, out _, out Scalar stdDev);
            return stdDev.Val0 * stdDev.Val0; // phương sai = độ nét
        }
        finally
        {
            roiMat?.Dispose();
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

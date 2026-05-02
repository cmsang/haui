using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Haui.PCB.Processing;

/// <summary>
/// Thông tin camera phát hiện được trên máy qua DirectShow.
/// </summary>
public record CameraInfo(int Index, string Name, string MonikerString);

/// <summary>
/// Độ phân giải hỗ trợ của camera.
/// </summary>
public record ResolutionInfo(int Width, int Height)
{
    public string Label => $"{Width}×{Height}";
}

/// <summary>
/// Dịch vụ quản lý camera: dò tìm, kết nối và lấy frame đều dùng AForge.Video.DirectShow.
/// Implements <see cref="ICameraService"/>.
/// </summary>
public class CameraService : ICameraService
{
    private VideoCaptureDevice? _device;
    private Mat? _lastFrame;
    private readonly object _frameLock = new();
    private bool _disposed;

    public bool IsRunning { get; private set; }

    /// <summary>
    /// Sự kiện được phát mỗi khi có frame mới từ camera.
    /// </summary>
    public event Action<Mat>? FrameArrived;

    /// <summary>
    /// Dò tìm tất cả camera có sẵn trên máy bằng DirectShow (bất đồng bộ).
    /// </summary>
    public Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync()
        => Task.Run<IReadOnlyList<CameraInfo>>(() =>
        {
            var result = new List<CameraInfo>();
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (int i = 0; i < devices.Count; i++)
                result.Add(new CameraInfo(i, devices[i].Name, devices[i].MonikerString));
            return result;
        });

    /// <summary>
    /// Lấy danh sách độ phân giải thực sự mà camera hỗ trợ qua DirectShow VideoCapabilities (bất đồng bộ).
    /// </summary>
    public Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(string monikerString)
        => Task.Run<IReadOnlyList<ResolutionInfo>>(() =>
        {
            var result = new List<ResolutionInfo>();
            var seen = new HashSet<(int, int)>();
            try
            {
                var device = new VideoCaptureDevice(monikerString);
                foreach (var cap in device.VideoCapabilities)
                {
                    int w = cap.FrameSize.Width;
                    int h = cap.FrameSize.Height;
                    if (w > 0 && h > 0 && seen.Add((w, h)))
                        result.Add(new ResolutionInfo(w, h));
                }
            }
            catch
            {
                // Bỏ qua nếu không truy vấn được capabilities
            }

            // Sắp xếp tăng dần theo diện tích
            result.Sort((a, b) => (a.Width * a.Height).CompareTo(b.Width * b.Height));
            return result;
        });

    /// <summary>
    /// Bắt đầu kết nối camera bằng AForge VideoCaptureDevice theo monikerString và độ phân giải.
    /// </summary>
    public void Start(string monikerString, int width, int height)
    {
        Stop();

        _device = new VideoCaptureDevice(monikerString);

        // Chọn VideoCapabilities khớp với độ phân giải yêu cầu
        var matchedCap = _device.VideoCapabilities
            .FirstOrDefault(c => c.FrameSize.Width == width && c.FrameSize.Height == height)
            ?? _device.VideoCapabilities.FirstOrDefault();

        if (matchedCap is not null)
            _device.VideoResolution = matchedCap;

        _device.NewFrame += OnNewFrame;
        _device.Start();
        IsRunning = true;
    }

    /// <summary>
    /// Trả về clone của frame cuối cùng nhận được (dùng khi bấm nút Test).
    /// </summary>
    public Mat? GrabFrame()
    {
        lock (_frameLock)
            return _lastFrame?.Clone();
    }

    /// <summary>
    /// Thu thập frame trong <paramref name="durationMs"/> mili-giây, tính phương sai Laplacian
    /// để đo độ sắc nét từng frame, rồi trả về frame có độ sắc nét cao nhất (clone).
    /// Phương sai Laplacian càng lớn → ảnh càng nét.
    /// </summary>
    public async Task<Mat?> CaptureSharpestFrameAsync(int durationMs = 1500, CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
            return null;

        Mat? bestFrame = null;
        double bestSharpness = -1;
        var tcs = new TaskCompletionSource();

        void OnFrame(Mat frame)
        {
            double sharpness = ComputeLaplacianVariance(frame);
            lock (_frameLock)
            {
                if (sharpness > bestSharpness)
                {
                    bestSharpness = sharpness;
                    bestFrame?.Dispose();
                    bestFrame = frame.Clone();
                }
            }
        }

        FrameArrived += OnFrame;
        try
        {
            await Task.Delay(durationMs, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Trả về frame tốt nhất đã thu thập được dù bị huỷ
        }
        finally
        {
            FrameArrived -= OnFrame;
        }

        lock (_frameLock)
            return bestFrame;
    }

    /// <summary>
    /// Tính phương sai của ảnh Laplacian — thước đo độ sắc nét của frame.
    /// Giá trị càng lớn → ảnh càng nét, ít mờ.
    /// </summary>
    private static double ComputeLaplacianVariance(Mat src)
    {
        using var gray = new Mat();
        using var lap = new Mat();

        // Chuyển sang ảnh xám nếu ảnh màu
        if (src.Channels() > 1)
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
        else
            src.CopyTo(gray);

        // Áp dụng bộ lọc Laplacian để phát hiện cạnh / chi tiết
        Cv2.Laplacian(gray, lap, MatType.CV_64F);

        // Tính mean và stddev; phương sai = stddev²
        Cv2.MeanStdDev(lap, out _, out var stdDev);
        double sigma = stdDev.Val0;
        return sigma * sigma;
    }

    public void Stop()
    {
        if (!IsRunning) return;

        if (_device is not null)
        {
            _device.NewFrame -= OnNewFrame;
            _device.SignalToStop();
            _device.WaitForStop();
            _device = null;
        }

        lock (_frameLock)
        {
            _lastFrame?.Dispose();
            _lastFrame = null;
        }

        IsRunning = false;
    }

    /// <summary>
    /// Xử lý frame mới từ AForge: convert Bitmap → Mat rồi phát sự kiện FrameArrived.
    /// </summary>
    private void OnNewFrame(object sender, NewFrameEventArgs e)
    {
        // Clone bitmap vì AForge dispose nó ngay sau khi event trả về
        using var bitmap = (Bitmap)e.Frame.Clone();
        using var mat = BitmapConverter.ToMat(bitmap);

        // Lưu frame cuối cùng để GrabFrame() dùng
        lock (_frameLock)
        {
            _lastFrame?.Dispose();
            _lastFrame = mat.Clone();
        }

        FrameArrived?.Invoke(mat);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
    }
}


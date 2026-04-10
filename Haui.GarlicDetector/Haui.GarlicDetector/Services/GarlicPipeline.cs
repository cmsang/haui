using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Vision;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Haui.GarlicDetector.Services;

/// <summary>Tham số sự kiện phát ra khi có frame mới từ camera (~30 fps).</summary>
public sealed record FrameReadyEventArgs(Bitmap Frame, List<GarlicRegion> CachedRegions);

/// <summary>Tham số sự kiện phát ra khi thuật toán HSV hoàn thành phân vùng.</summary>
public sealed record SegmentationCompletedEventArgs(Bitmap Frame, List<GarlicRegion> Regions);

/// <summary>
/// Điều phối pipeline camera + phân vùng HSV theo mô hình "luôn xử lý frame mới nhất":
/// <list type="bullet">
///   <item>Hiển thị mỗi frame ngay lập tức (không chờ phân vùng)</item>
///   <item>Phân vùng chạy song song, luôn lấy frame mới nhất từ hàng đợi</item>
///   <item>Cache kết quả gần nhất để overlay lên frame live</item>
/// </list>
/// </summary>
public sealed class GarlicPipeline : IDisposable
{
    private readonly CameraService _cameraService;
    private readonly HsvSegmenter  _segmenter;

    // Frame gốc mới nhất — dùng để overlay kết quả phân vùng
    private Bitmap? _latestRawFrame;

    // Frame đang chờ phân vùng — luôn được thay bằng frame mới hơn nếu có
    private Bitmap? _pendingFrame;

    private readonly object _frameLock = new();

    // Kết quả phân vùng được cache (volatile — đọc từ thread khác an toàn)
    private volatile List<GarlicRegion> _cachedRegions = [];

    // Cờ Interlocked: 0 = rảnh, 1 = đang xử lý (chỉ 1 worker tại một thời điểm)
    private int _isProcessing = 0;

    // Index camera hiện tại — cần lưu lại khi chuyển độ phân giải
    private int _currentCameraIndex = 0;

    // ─── Sự kiện công khai ───────────────────────────────────────────────────

    /// <summary>Phát trên thread camera (~30 fps) — frame thô kèm vùng tỏi đã cache.</summary>
    public event EventHandler<FrameReadyEventArgs>? FrameReady;

    /// <summary>Phát trên thread-pool sau khi phân vùng xong — frame là frame MỚI NHẤT.</summary>
    public event EventHandler<SegmentationCompletedEventArgs>? SegmentationCompleted;

    /// <summary>Phát khi có lỗi nội bộ trong pipeline.</summary>
    public event EventHandler<string>? ErrorOccurred;

    // ─── Thuộc tính ──────────────────────────────────────────────────────────

    /// <summary>Camera pipeline có đang chạy hay không.</summary>
    public bool IsRunning => _cameraService.IsRunning;

    /// <summary>Thuật toán phân vùng HSV — cho phép điều chỉnh ngưỡng từ UI.</summary>
    public HsvSegmenter Segmenter => _segmenter;

    /// <summary>Các vùng tỏi được phát hiện trong lần phân vùng gần nhất.</summary>
    public List<GarlicRegion> CachedRegions => _cachedRegions;

    public GarlicPipeline(CameraService cameraService, HsvSegmenter segmenter)
    {
        _cameraService = cameraService;
        _segmenter     = segmenter;

        // Đăng ký sự kiện từ camera service
        _cameraService.FrameCaptured += OnFrameCaptured;
        _cameraService.ErrorOccurred += (_, msg) => ErrorOccurred?.Invoke(this, msg);
    }

    // ─── API công khai ────────────────────────────────────────────────────────

    /// <summary>Bắt đầu pipeline với camera và độ phân giải chỉ định.</summary>
    public void Start(int deviceIndex = 0, CameraResolution? resolution = null)
    {
        _currentCameraIndex = deviceIndex;
        _cameraService.Start(deviceIndex, resolution);
    }

    /// <summary>Dừng toàn bộ pipeline.</summary>
    public void Stop() => _cameraService.Stop();

    /// <summary>Chuyển sang camera khác, giữ nguyên độ phân giải tùy chọn.</summary>
    public void SwitchCamera(int deviceIndex, CameraResolution? resolution = null)
    {
        _currentCameraIndex = deviceIndex;
        _cameraService.SwitchCamera(deviceIndex, resolution);
    }

    /// <summary>Thay đổi độ phân giải của camera đang chạy.</summary>
    public void SwitchResolution(CameraResolution resolution)
        => _cameraService.SwitchResolution(_currentCameraIndex, resolution);

    /// <summary>Lấy danh sách camera khả dụng trên máy.</summary>
    public static List<CameraInfo> GetAvailableCameras()
        => CameraService.GetAvailableCameras();

    /// <summary>Chụp ảnh tĩnh từ camera hiện tại.</summary>
    public Bitmap? CaptureSnapshot() => _cameraService.CaptureSnapshot();

    // ─── Pipeline nội bộ ─────────────────────────────────────────────────────

    private void OnFrameCaptured(object? sender, Bitmap bitmap)
    {
        // Tạo hai bản clone độc lập: rawClone (hiển thị) và pendingClone (phân vùng)
        var rawClone     = (Bitmap)bitmap.Clone();
        var pendingClone = (Bitmap)bitmap.Clone();
        Bitmap? oldRaw;

        lock (_frameLock)
        {
            oldRaw          = _latestRawFrame;
            _latestRawFrame = rawClone;

            // Loại bỏ frame cũ đang chờ — thay bằng frame mới hơn
            _pendingFrame?.Dispose();
            _pendingFrame = pendingClone;
        }
        oldRaw?.Dispose();

        // Thông báo UI ngay lập tức — không chờ phân vùng
        FrameReady?.Invoke(this, new FrameReadyEventArgs((Bitmap)bitmap.Clone(), _cachedRegions));

        // Khởi động worker phân vùng nếu chưa có instance nào đang chạy
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
            _ = RunSegmentationLoopAsync();
    }

    private async Task RunSegmentationLoopAsync()
    {
        try
        {
            while (true)
            {
                // Lấy frame đang chờ để xử lý
                Bitmap? frame;
                lock (_frameLock)
                {
                    frame         = _pendingFrame;
                    _pendingFrame = null;
                }

                if (frame == null)
                {
                    // Giải phóng cờ trước — tránh race condition khi frame mới vừa tới
                    Interlocked.Exchange(ref _isProcessing, 0);
                    lock (_frameLock)
                    {
                        if (_pendingFrame == null) break; // thực sự rỗng → thoát vòng lặp
                    }
                    // Có frame mới lọt vào giữa lúc kiểm tra — thử nhận lại worker slot
                    if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0) break;
                    continue;
                }

                try
                {
                    // Chạy phân vùng HSV trên thread-pool
                    var regions = await Task.Run(() => SegmentFrame(frame));
                    _cachedRegions = regions;

                    // Overlay lên frame MỚI NHẤT từ camera (không phải frame đã phân vùng)
                    Bitmap? latestDisplay;
                    lock (_frameLock)
                    {
                        latestDisplay = _latestRawFrame != null
                            ? (Bitmap)_latestRawFrame.Clone()
                            : null;
                    }

                    if (latestDisplay != null)
                    {
                        DrawRegions(latestDisplay, regions);
                        SegmentationCompleted?.Invoke(
                            this, new SegmentationCompletedEventArgs(latestDisplay, regions));
                    }
                }
                finally
                {
                    frame.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _isProcessing, 0);
            ErrorOccurred?.Invoke(this, $"Lỗi phân vùng: {ex.Message}");
        }
    }

    /// <summary>Chạy thuật toán HSV trên bitmap và chuyển kết quả thành danh sách GarlicRegion.</summary>
    private List<GarlicRegion> SegmentFrame(Bitmap bitmap)
    {
        using var mat = BitmapConverter.ToMat(bitmap);
        var rects = _segmenter.Segment(mat);

        return rects.Select(r => new GarlicRegion
        {
            BoundingBox = new Rectangle(r.X, r.Y, r.Width, r.Height),
            Area        = r.Width * (double)r.Height,
            DetectedAt  = DateTime.Now,
        }).ToList();
    }

    /// <summary>
    /// Vẽ hình chữ nhật màu vàng và nhãn lên bitmap (thay đổi trực tiếp).
    /// </summary>
    public static void DrawRegions(Bitmap bitmap, List<GarlicRegion> regions)
    {
        using var g   = Graphics.FromImage(bitmap);
        using var pen = new Pen(Color.Yellow, 2);

        foreach (var region in regions)
        {
            var box = region.BoundingBox;
            g.DrawRectangle(pen, box);

            // Vẽ nhãn với nền bán trong suốt phía trên bounding box
            string label    = $"Tỏi ({region.Area:N0}px²)";
            var    font     = SystemFonts.SmallCaptionFont;
            var    textSize = g.MeasureString(label, font);
            var    labelRect = new RectangleF(
                box.X, box.Y - textSize.Height,
                textSize.Width + 4, textSize.Height);

            // Tránh nhãn bị cắt ở cạnh trên màn hình
            if (labelRect.Y < 0) labelRect.Y = box.Y;

            using var bgBrush = new SolidBrush(Color.FromArgb(140, Color.Black));
            g.FillRectangle(bgBrush, labelRect);
            g.DrawString(label, font, Brushes.Yellow, labelRect.Location);
        }
    }

    // ─── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        _cameraService.FrameCaptured -= OnFrameCaptured;
        _cameraService.Dispose();

        lock (_frameLock)
        {
            _latestRawFrame?.Dispose();
            _pendingFrame?.Dispose();
            _latestRawFrame = null;
            _pendingFrame   = null;
        }
    }
}

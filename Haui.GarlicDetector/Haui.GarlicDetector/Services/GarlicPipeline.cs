using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.ML;
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
    private readonly IImagePreprocessor _preprocessor;
    private readonly IGarlicSegmentor _segmenter;
    private readonly IPredictor? _predictor;
    private readonly IFeatureExtractor? _featureExtractor;

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

    /// <summary>Thuật toán phân vùng — có thể ép kiểu sang implementation cụ thể nếu cần.</summary>
    public IGarlicSegmentor Segmenter => _segmenter;

    /// <summary>Các vùng tỏi được phát hiện trong lần phân vùng gần nhất.</summary>
    public List<GarlicRegion> CachedRegions => _cachedRegions;

    /// <summary>
    /// Vùng nhận diện (pixel, tọa độ frame camera thực tế).
    /// Khi được đặt, pipeline chỉ phân vùng bên trong hình chữ nhật này.
    /// <c>null</c> = nhận diện toàn bộ khung hình.
    /// </summary>
    public Rectangle? DetectionRegion { get; set; }

    public GarlicPipeline(
        CameraService cameraService,
        IImagePreprocessor preprocessor,
        IGarlicSegmentor segmenter,
        IPredictor? predictor = null,
        IFeatureExtractor? featureExtractor = null)
    {
        _cameraService = cameraService;
        _preprocessor = preprocessor;
        _segmenter = segmenter;
        _predictor = predictor;
        _featureExtractor = featureExtractor;

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
        var rawClone = (Bitmap)bitmap.Clone();
        var pendingClone = (Bitmap)bitmap.Clone();
        Bitmap? oldRaw;

        lock (_frameLock)
        {
            oldRaw = _latestRawFrame;
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
                    frame = _pendingFrame;
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
                    // Snapshot DetectionRegion để tránh thay đổi giữa chừng
                    var region = DetectionRegion;

                    // Chạy phân vùng HSV trên thread-pool
                    var regions = await Task.Run(() => SegmentFrame(frame, region));
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
                        // Chỉ vẽ bounding box tỏi — viền vùng nhận diện do frmMain.PicCamera_Paint đảm nhận
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

    /// <summary>
    /// Tiền xử lý + phân vùng bitmap.
    /// Nếu <paramref name="detectionRegion"/> được đặt, chỉ phân vùng trong vùng đó;
    /// bounding box kết quả được offset về tọa độ frame đầy đủ.
    /// </summary>
    private List<GarlicRegion> SegmentFrame(Bitmap bitmap, Rectangle? detectionRegion)
    {
        using var bgrMat = BitmapConverter.ToMat(bitmap);

        int offsetX = 0;
        int offsetY = 0;
        Mat matToProcess;
        Mat? roiMat = null;

        if (detectionRegion.HasValue)
        {
            var dr = detectionRegion.Value;

            // Clamp vùng vào kích thước frame thực tế
            int x = Math.Clamp(dr.X, 0, bgrMat.Width - 1);
            int y = Math.Clamp(dr.Y, 0, bgrMat.Height - 1);
            int w = Math.Clamp(dr.Width, 1, bgrMat.Width - x);
            int h = Math.Clamp(dr.Height, 1, bgrMat.Height - y);

            // Bỏ qua ROI quá nhỏ để tránh lỗi OpenCV (kernel > image)
            if (w < 20 || h < 20) return [];

            roiMat = new Mat(bgrMat, new Rect(x, y, w, h));
            matToProcess = roiMat;
            offsetX = x;
            offsetY = y;
        }
        else
        {
            matToProcess = bgrMat;
        }

        try
        {
            using var preprocessedMat = _preprocessor.Preprocess(matToProcess);
            var segmentResults = _segmenter.Segment(preprocessedMat);

            var regions = new List<GarlicRegion>(segmentResults.Count);
            foreach (var r in segmentResults)
            {
                var region = new GarlicRegion
                {
                    // Cộng offset để bounding box luôn ở tọa độ frame gốc
                    BoundingBox = new Rectangle(
                        r.BoundingRect.X + offsetX,
                        r.BoundingRect.Y + offsetY,
                        r.BoundingRect.Width,
                        r.BoundingRect.Height),
                    Area = r.Area,
                    Circularity = r.Circularity,
                    DetectedAt = DateTime.Now,
                };

                // ── Stage 1: SVM phân loại bình thường / hỏng ────────────────
                // Crop ROI BGR từ mat gốc — mở rộng thêm RoiPaddingPx mỗi chiều để SVM có thêm ngữ cảnh
                int pad = AppSettings.Instance.RoiPaddingPx;
                int rx = Math.Clamp(r.BoundingRect.X - pad, 0, matToProcess.Width - 1);
                int ry = Math.Clamp(r.BoundingRect.Y - pad, 0, matToProcess.Height - 1);
                int rw = Math.Clamp(r.BoundingRect.Width + pad * 2, 1, matToProcess.Width - rx);
                int rh = Math.Clamp(r.BoundingRect.Height + pad * 2, 1, matToProcess.Height - ry);
                var roiRect = new Rect(rx, ry, rw, rh);

                using var cropRoi = new Mat(matToProcess, roiRect);
                float[] features = _featureExtractor.Extract(cropRoi);
                int svmLabel = _predictor.Predict(features);

                // ── Stage 2: phân kích thước (chỉ khi bình thường) ───
                region.FinalLabel = svmLabel == 1
                    ? GarlicLabel.ToHong
                    : (r.Area >= AppSettings.Instance.SizeThresholdPx
                        ? GarlicLabel.ToTo
                        : GarlicLabel.ToNho);

                regions.Add(region);
            }

            return regions;
        }
        finally
        {
            roiMat?.Dispose();
        }
    }

    /// <summary>
    /// Vẽ bounding box màu theo nhãn phân loại + text nhãn lên bitmap (in-place).<br/>
    /// • Tỏi to   → khung xanh lá<br/>
    /// • Tỏi nhỏ  → khung vàng<br/>
    /// • Tỏi hỏng → khung đỏ<br/>
    /// • Chưa phân loại (null) → khung trắng (model chưa nạp)<br/>
    /// Viền vùng nhận diện được vẽ riêng qua <c>PicCamera_Paint</c> trên UI thread.
    /// </summary>
    public static void DrawRegions(Bitmap bitmap, List<GarlicRegion> regions)
    {
        using var g = Graphics.FromImage(bitmap);
        var font = SystemFonts.SmallCaptionFont ?? SystemFonts.DefaultFont;

        foreach (var region in regions)
        {
            var p = AppSettings.Instance.RoiPaddingPx;
            var raw = region.BoundingBox;
            // Mở rộng khung vẽ ra ngoài giống padding dùng khi crop SVM
            var box = Rectangle.FromLTRB(
                Math.Max(0, raw.Left - p),
                Math.Max(0, raw.Top - p),
                Math.Min(bitmap.Width, raw.Right + p),
                Math.Min(bitmap.Height, raw.Bottom + p));

            // Chọn màu khung và nhãn text theo FinalLabel
            Color penColor;
            string labelText;

            switch (region.FinalLabel)
            {
                case GarlicLabel.ToTo:
                    penColor = Color.LimeGreen;
                    labelText = "Tỏi to";
                    break;
                case GarlicLabel.ToNho:
                    penColor = Color.Yellow;
                    labelText = "Tỏi nhỏ";
                    break;
                case GarlicLabel.ToHong:
                    penColor = Color.Red;
                    labelText = "Tỏi hỏng";
                    break;
                default:
                    // Model SVM chưa được nạp — vẽ khung trắng, hiện diện tích
                    penColor = Color.White;
                    labelText = $"A:{region.Area:N0}px²";
                    break;
            }

            using var pen = new Pen(penColor, 2);
            g.DrawRectangle(pen, box);

            // Bỏ qua nhãn text nếu circularity quá thấp
            if (region.Circularity < AppSettings.Instance.MinCircularity) continue;

            var textSize = g.MeasureString(labelText, font);
            var labelRect = new RectangleF(
                box.X, box.Y - textSize.Height,
                textSize.Width + 4, textSize.Height);

            if (labelRect.Y < 0) labelRect.Y = box.Y;

            using var bgBrush = new SolidBrush(Color.FromArgb(160, Color.Black));
            using var textBrush = new SolidBrush(penColor);
            g.FillRectangle(bgBrush, labelRect);
            g.DrawString(labelText, font, textBrush, labelRect.Location);
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
            _pendingFrame = null;
        }
    }
}

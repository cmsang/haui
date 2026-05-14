using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Haui.GTObjectDetector.Models;
using Haui.GTObjectDetector.Processing;
using OpenCvSharp;

namespace Haui.GTObjectDetector.ViewModels;

/// <summary>
/// ViewModel cho MainWindow — chứa toàn bộ logic nghiệp vụ liên quan đến camera.
/// Tách biệt hoàn toàn khỏi UI (WPF), tuân theo SOLID: SRP, DIP, OCP.
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ICameraService _cameraService;
    private readonly IObjectDetectionService? _detectionService;

    private IReadOnlyList<CameraInfo> _cameras = [];
    private IReadOnlyList<ResolutionInfo> _resolutions = [];
    private string _statusText = string.Empty;
    private bool _isBusy;
    private int _frameCount;
    private int _currentFps;
    private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();
    private bool _disposed;

    // Bật/tắt tính năng nhận diện đối tượng YOLO
    private bool _isDetectionEnabled;

    // Ngưỡng confidence và NMS
    private float _confidenceThreshold = 0.45f;
    private float _nmsThreshold = 0.45f;

    // Tùy chọn tiền xử lý ảnh trước khi detect
    private readonly PreprocessOptions _preprocessOptions = new();

    // Vùng nhận diện (tọa độ tương đối 0..1 so với kích thước frame thực)
    private OpenCvSharp.Rect? _selectedRegion;
    private const string RegionSettingsPath = "last_region.json";

    // Kích thước frame thực tế mới nhất để tính toán vùng
    private int _lastFrameWidth;
    private int _lastFrameHeight;

    // ──── Sự kiện ────────────────────────────────────────────────────────────

    /// <summary>Phát khi có frame mới sẵn sàng để hiển thị (đã Freeze).</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource>? FrameReady;

    /// <summary>Phát khi người dùng nhấn Tạo mẫu — truyền frame để mở CreateTemplateWindow.</summary>
    public event Action<Mat>? TemplateFrameCaptured;

    /// <summary>Phát khi người dùng nhấn Test — truyền frame để mở TestPipelineWindow.</summary>
    public event Action<Mat>? TestFrameCaptured;

    /// <summary>Phát khi người dùng nhấn Test 2 — truyền frame để mở PipelineStepsWindow.</summary>
    public event Action<Mat>? Test2FrameCaptured;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public IReadOnlyList<CameraInfo> Cameras
    {
        get => _cameras;
        private set { _cameras = value; OnPropertyChanged(); }
    }

    public IReadOnlyList<ResolutionInfo> Resolutions
    {
        get => _resolutions;
        private set { _resolutions = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); }
    }

    public int CurrentFps
    {
        get => _currentFps;
        private set { _currentFps = value; OnPropertyChanged(); }
    }

    public bool IsRunning => _cameraService.IsRunning;

    /// <summary>Bật/tắt nhận diện đối tượng YOLO trên luồng camera.</summary>
    public bool IsDetectionEnabled
    {
        get => _isDetectionEnabled;
        set { _isDetectionEnabled = value; OnPropertyChanged(); }
    }

    /// <summary>Ngưỡng confidence tối thiểu (0.0–1.0).</summary>
    public float ConfidenceThreshold
    {
        get => _confidenceThreshold;
        set { _confidenceThreshold = Math.Clamp(value, 0.01f, 1f); OnPropertyChanged(); }
    }

    /// <summary>Ngưỡng NMS (0.0–1.0).</summary>
    public float NmsThreshold
    {
        get => _nmsThreshold;
        set { _nmsThreshold = Math.Clamp(value, 0.01f, 1f); OnPropertyChanged(); }
    }

    /// <summary>True nếu detection service đã được nạp thành công.</summary>
    public bool IsDetectionAvailable => _detectionService is not null;

    /// <summary>Tùy chọn tiền xử lý (CLAHE, Denoise) — có thể điều chỉnh từ ngoài.</summary>
    public PreprocessOptions PreprocessOptions => _preprocessOptions;

    /// <summary>Vùng nhận diện trên frame thực (pixel). Null = toàn bộ khung hình.</summary>
    public OpenCvSharp.Rect? SelectedRegion
    {
        get => _selectedRegion;
        set
        {
            _selectedRegion = value;
            OnPropertyChanged();
            SaveRegion();
        }
    }

    /// <summary>Kích thước frame thực tế mới nhất (để View tính toán tỉ lệ).</summary>
    public int LastFrameWidth => _lastFrameWidth;
    public int LastFrameHeight => _lastFrameHeight;

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public MainViewModel(ICameraService cameraService, IObjectDetectionService? detectionService = null)
    {
        _cameraService = cameraService;
        _detectionService = detectionService;
        _cameraService.FrameArrived += OnFrameArrived;
        LoadRegion();
    }

    // ──── Commands / Actions ──────────────────────────────────────────────────

    /// <summary>Dò tìm camera thực tế trên máy và nạp vào danh sách.</summary>
    public async Task RefreshCamerasAsync()
    {
        IsBusy = true;
        StatusText = "Đang dò tìm camera...";
        Cameras = [];
        Resolutions = [];

        var cameras = await _cameraService.EnumerateCamerasAsync();
        Cameras = cameras;

        if (cameras.Count == 0)
        {
            StatusText = "Không phát hiện camera nào.";
        }
        else
        {
            StatusText = $"Tìm thấy {cameras.Count} camera.";
        }

        IsBusy = false;
    }

    /// <summary>Tải độ phân giải hỗ trợ của camera được chọn.</summary>
    public async Task LoadResolutionsAsync(string monikerString)
    {
        IsBusy = true;
        Resolutions = [];
        StatusText = "Đang đọc độ phân giải...";

        var resolutions = await _cameraService.GetSupportedResolutionsAsync(monikerString);
        Resolutions = resolutions;

        if (resolutions.Count == 0)
            StatusText = "Camera không phản hồi độ phân giải.";
        else
            StatusText = $"Sẵn sàng. {resolutions.Count} độ phân giải hỗ trợ.";

        IsBusy = false;
    }

    /// <summary>
    /// Tìm index mặc định ưu tiên 1280×720 cho danh sách độ phân giải hiện tại.
    /// </summary>
    public int GetDefaultResolutionIndex()
    {
        int idx = Resolutions
            .Select((r, i) => (r, i))
            .FirstOrDefault(t => t.r.Width == 1280 && t.r.Height == 720, (null!, -1)).i;
        return idx >= 0 ? idx : Resolutions.Count / 2;
    }

    /// <summary>Bắt đầu camera với camera và độ phân giải đã chọn.</summary>
    public void StartCamera(string monikerString, int width, int height)
    {
        _cameraService.Start(monikerString, width, height);
        OnPropertyChanged(nameof(IsRunning));
        StatusText = $"Camera đang chạy ({width}×{height})";
    }

    /// <summary>Dừng camera.</summary>
    public void StopCamera()
    {
        _cameraService.Stop();
        OnPropertyChanged(nameof(IsRunning));
        _frameCount = 0;
        CurrentFps = 0;
        StatusText = "Camera đã dừng.";
    }

    /// <summary>Chụp frame hiện tại và phát sự kiện TestFrameCaptured.</summary>
    public async Task CaptureTestFrameAsync()
    {
        StatusText = "Đang chụp ảnh...";

        var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            frame?.Dispose();
            StatusText = "Không thể chụp ảnh từ camera.";
            return;
        }

        // Crop theo vùng đã chọn nếu có
        if (_selectedRegion.HasValue)
        {
            var roi = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
            if (roi.Width > 0 && roi.Height > 0)
            {
                var cropped = new Mat(frame, roi);
                frame.Dispose();
                frame = cropped.Clone();
                cropped.Dispose();
            }
        }

        StatusText = "Đã mở Test Pipeline.";
        TestFrameCaptured?.Invoke(frame);
    }

    /// <summary>Chụp frame từ vùng đã chọn và phát sự kiện Test2FrameCaptured để mở PipelineStepsWindow.</summary>
    public async Task CaptureTest2FrameAsync()
    {
        StatusText = "Đang chụp ảnh (Test 2)...";

        var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            frame?.Dispose();
            StatusText = "Không thể chụp ảnh từ camera.";
            return;
        }

        // Crop theo vùng đã chọn nếu có
        if (_selectedRegion.HasValue)
        {
            var roi = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
            if (roi.Width > 0 && roi.Height > 0)
            {
                var cropped = new Mat(frame, roi);
                frame.Dispose();
                frame = cropped.Clone();
                cropped.Dispose();
            }
        }

        StatusText = "Đã mở Pipeline Debug.";
        Test2FrameCaptured?.Invoke(frame);
    }

    /// <summary>Chụp frame hiện tại và phát sự kiện TemplateFrameCaptured để mở form tạo mẫu.</summary>
    public async Task CaptureTemplateFrameAsync()
    {
        StatusText = "Đang chụp ảnh để tạo mẫu...";

        var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            frame?.Dispose();
            StatusText = "Không thể chụp ảnh từ camera.";
            return;
        }

        StatusText = "Đã mở form tạo mẫu.";
        TemplateFrameCaptured?.Invoke(frame);
    }

    // ──── Xử lý frame ────────────────────────────────────────────────────────

    private void OnFrameArrived(Mat frame)
    {
        // Đo FPS
        _frameCount++;
        if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
        {
            CurrentFps = _frameCount;
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }

        // Lưu kích thước frame để View tính tỉ lệ
        if (frame.Width != _lastFrameWidth || frame.Height != _lastFrameHeight)
        {
            _lastFrameWidth = frame.Width;
            _lastFrameHeight = frame.Height;
        }

        // ── Nhận diện đối tượng YOLO (nếu bật) ───────────────────────────
        if (_isDetectionEnabled && _detectionService is not null)
        {
            // Xác định vùng cần detect
            Mat detectMat = frame;
            OpenCvSharp.Rect roiForDetect = default;
            bool hasRoi = _selectedRegion.HasValue;

            if (hasRoi)
            {
                roiForDetect = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
                hasRoi = roiForDetect.Width > 0 && roiForDetect.Height > 0;
            }

            if (hasRoi)
                detectMat = new Mat(frame, roiForDetect);

            try
            {
                var detections = _detectionService.Detect(detectMat, _confidenceThreshold, _nmsThreshold, _preprocessOptions);

                // Nếu detect trên ROI thì bù offset tọa độ về frame gốc
                if (hasRoi && detections.Count > 0)
                {
                    var shifted = detections.Select(d => d with
                    {
                        BoundingBox = new OpenCvSharp.Rect(
                            d.BoundingBox.X + roiForDetect.X,
                            d.BoundingBox.Y + roiForDetect.Y,
                            d.BoundingBox.Width,
                            d.BoundingBox.Height)
                    }).ToList();
                    YoloDetectionService.DrawDetections(frame, shifted);
                }
                else
                {
                    YoloDetectionService.DrawDetections(frame, detections);
                }
            }
            finally
            {
                if (hasRoi) detectMat.Dispose();
            }
        }

        // Vẽ hình chữ nhật xanh cho vùng nhận diện
        if (_selectedRegion.HasValue)
        {
            var roi = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
            if (roi.Width > 0 && roi.Height > 0)
                Cv2.Rectangle(frame, roi, new Scalar(0, 200, 0), 2);
        }

        // Convert sang BitmapSource trên thread hiện tại (background), freeze để cross-thread an toàn
        var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(frame);
        bitmap.Freeze();
        FrameReady?.Invoke(bitmap);
    }

    private static OpenCvSharp.Rect ClampRect(OpenCvSharp.Rect r, int w, int h)
    {
        int x = Math.Max(0, r.X);
        int y = Math.Max(0, r.Y);
        int width = Math.Min(r.Width, w - x);
        int height = Math.Min(r.Height, h - y);
        return new OpenCvSharp.Rect(x, y, Math.Max(0, width), Math.Max(0, height));
    }

    private void SaveRegion()
    {
        try
        {
            if (_selectedRegion.HasValue)
            {
                var r = _selectedRegion.Value;
                var json = JsonSerializer.Serialize(new { r.X, r.Y, r.Width, r.Height });
                File.WriteAllText(RegionSettingsPath, json);
            }
            else
            {
                File.Delete(RegionSettingsPath);
            }
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    private void LoadRegion()
    {
        try
        {
            if (!File.Exists(RegionSettingsPath)) return;
            var json = File.ReadAllText(RegionSettingsPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            int x = root.GetProperty("X").GetInt32();
            int y = root.GetProperty("Y").GetInt32();
            int width = root.GetProperty("Width").GetInt32();
            int height = root.GetProperty("Height").GetInt32();
            if (width > 0 && height > 0)
                _selectedRegion = new OpenCvSharp.Rect(x, y, width, height);
        }
        catch { /* bỏ qua lỗi đọc file */ }
    }

    // ──── INotifyPropertyChanged ──────────────────────────────────────────────

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ──── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _cameraService.FrameArrived -= OnFrameArrived;
        _cameraService.Dispose();
        _detectionService?.Dispose();
        _disposed = true;
    }
}

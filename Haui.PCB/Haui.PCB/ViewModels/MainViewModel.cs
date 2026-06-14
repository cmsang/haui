using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Haui.PCB.Processing;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho MainWindow — camera và pipeline chụp ảnh PCB.
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ICameraService _cameraService;
    private readonly IMaterialTransferService? _materialTransfer;

    private IReadOnlyList<CameraInfo> _cameras = [];
    private IReadOnlyList<ResolutionInfo> _resolutions = [];
    private string _statusText = string.Empty;
    private bool _isBusy;
    private int _frameCount;
    private int _currentFps;
    private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();
    private bool _disposed;

    private OpenCvSharp.Rect? _selectedRegion;
    private const string RegionSettingsPath = "last_region.json";

    private int _lastFrameWidth;
    private int _lastFrameHeight;

    public event Action<System.Windows.Media.Imaging.BitmapSource>? FrameReady;
    public event Action<Mat>? TemplateFrameCaptured;
    public event Action<Mat>? TestFrameCaptured;
    public event Action<Mat>? Test2FrameCaptured;
    public event PropertyChangedEventHandler? PropertyChanged;

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

    public int LastFrameWidth => _lastFrameWidth;
    public int LastFrameHeight => _lastFrameHeight;

    public bool IsMaterialTransferRunning => _materialTransfer?.IsRunning ?? false;

    public MainViewModel(ICameraService cameraService, IMaterialTransferService? materialTransfer = null)
    {
        _cameraService = cameraService;
        _materialTransfer = materialTransfer;
        _cameraService.FrameArrived += OnFrameArrived;
        LoadRegion();
    }

    public void CancelMaterialTransfer() => _materialTransfer?.Cancel();

    /// <summary>Pass — PickUp → ô OK (xoay vòng OK1–OK4).</summary>
    public async Task TransferPassMaterial()
    {
        if (_materialTransfer == null)
        {
            StatusText = "Chưa cấu hình dịch vụ chuyển material.";
            return;
        }

        if (_materialTransfer.IsRunning)
        {
            StatusText = "Chu trình chuyển material đang chạy.";
            return;
        }

        try
        {
            await _materialTransfer.TransferPassAsync(msg => StatusText = msg);
        }
        finally
        {
            OnPropertyChanged(nameof(IsMaterialTransferRunning));
        }
    }

    /// <summary>Fail — PickUp → ô NG (xoay vòng NG1–NG4).</summary>
    public async Task TransferFailMaterial()
    {
        if (_materialTransfer == null)
        {
            StatusText = "Chưa cấu hình dịch vụ chuyển material.";
            return;
        }

        if (_materialTransfer.IsRunning)
        {
            StatusText = "Chu trình chuyển material đang chạy.";
            return;
        }

        try
        {
            await _materialTransfer.TransferFailAsync(msg => StatusText = msg);
        }
        finally
        {
            OnPropertyChanged(nameof(IsMaterialTransferRunning));
        }
    }

    public async Task RefreshCamerasAsync()
    {
        IsBusy = true;
        StatusText = "Đang dò tìm camera...";
        Cameras = [];
        Resolutions = [];

        var cameras = await _cameraService.EnumerateCamerasAsync();
        Cameras = cameras;

        StatusText = cameras.Count == 0
            ? "Không phát hiện camera nào."
            : $"Tìm thấy {cameras.Count} camera.";

        IsBusy = false;
    }

    public async Task LoadResolutionsAsync(string monikerString)
    {
        IsBusy = true;
        Resolutions = [];
        StatusText = "Đang đọc độ phân giải...";

        var resolutions = await _cameraService.GetSupportedResolutionsAsync(monikerString);
        Resolutions = resolutions;

        StatusText = resolutions.Count == 0
            ? "Camera không phản hồi độ phân giải."
            : $"Sẵn sàng. {resolutions.Count} độ phân giải hỗ trợ.";

        IsBusy = false;
    }

    public int GetDefaultResolutionIndex()
    {
        int idx = Resolutions
            .Select((r, i) => (r, i))
            .FirstOrDefault(t => t.r.Width == 1280 && t.r.Height == 720, (null!, -1)).i;
        return idx >= 0 ? idx : Resolutions.Count / 2;
    }

    public void StartCamera(string monikerString, int width, int height)
    {
        _cameraService.Start(monikerString, width, height);
        OnPropertyChanged(nameof(IsRunning));
        StatusText = $"Camera đang chạy ({width}×{height})";
    }

    public void StopCamera()
    {
        _cameraService.Stop();
        OnPropertyChanged(nameof(IsRunning));
        _frameCount = 0;
        CurrentFps = 0;
        StatusText = "Camera đã dừng.";
    }

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

        frame = CropToSelectedRegion(frame);
        StatusText = "Đã mở Test Pipeline.";
        TestFrameCaptured?.Invoke(frame);
    }

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

        frame = CropToSelectedRegion(frame);
        StatusText = "Đã mở Pipeline Debug.";
        Test2FrameCaptured?.Invoke(frame);
    }

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

    private Mat CropToSelectedRegion(Mat frame)
    {
        if (!_selectedRegion.HasValue)
            return frame;

        var roi = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
        if (roi.Width <= 0 || roi.Height <= 0)
            return frame;

        var cropped = new Mat(frame, roi);
        frame.Dispose();
        var result = cropped.Clone();
        cropped.Dispose();
        return result;
    }

    private void OnFrameArrived(Mat frame)
    {
        _frameCount++;
        if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
        {
            CurrentFps = _frameCount;
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }

        if (frame.Width != _lastFrameWidth || frame.Height != _lastFrameHeight)
        {
            _lastFrameWidth = frame.Width;
            _lastFrameHeight = frame.Height;
        }

        if (_selectedRegion.HasValue)
        {
            var roi = ClampRect(_selectedRegion.Value, frame.Width, frame.Height);
            if (roi.Width > 0 && roi.Height > 0)
                Cv2.Rectangle(frame, roi, new Scalar(0, 200, 0), 2);
        }

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

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        if (_disposed) return;
        _cameraService.FrameArrived -= OnFrameArrived;
        _cameraService.Dispose();
        _disposed = true;
    }
}

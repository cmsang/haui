using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Haui.PCB.Processing;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho MainWindow — camera Basler và pipeline chụp ảnh PCB.
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IMaterialTransferService? _materialTransfer;
    private BaslerCameraService? _cameraService;

    private IReadOnlyList<CameraInfo> _cameras = [];
    private IReadOnlyList<ResolutionInfo> _resolutions = [];
    private CameraInfo? _selectedCamera;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private int _frameCount;
    private int _currentFps;
    private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();
    private bool _disposed;

    private double _exposureTimeUs = 15_000;
    private double _gainDb;
    private double _gamma = 1.0;

    private readonly SegmentationParameters _pipelineParameters = SegmentationSettings.Current;
    private readonly IFiducialHoleTemplateService _fiducialTemplateService = FiducialHoleServices.TemplateService;
    private readonly CameraCaptureService _cameraCaptureService = new();

    private OpenCvSharp.Rect? _selectedRegion;
    private const string RegionSettingsPath = "last_region.json";

    private int _lastFrameWidth;
    private int _lastFrameHeight;
    private int _previewProcessing;

    public event Action<System.Windows.Media.Imaging.BitmapSource>? FrameReady;
    public event Action<Mat>? TemplateFrameCaptured;
    public event Action<Mat>? FiducialTemplateFrameCaptured;
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

    public bool IsRunning => _cameraService?.IsRunning ?? false;

    public bool CanEditCameraParameters => IsRunning && _cameraService is not null;

    public double ExposureTimeUs
    {
        get => _exposureTimeUs;
        set { _exposureTimeUs = value; OnPropertyChanged(); }
    }

    public double GainDb
    {
        get => _gainDb;
        set { _gainDb = value; OnPropertyChanged(); }
    }

    public double Gamma
    {
        get => _gamma;
        set { _gamma = value; OnPropertyChanged(); }
    }

    public double CannyThreshold1
    {
        get => _pipelineParameters.CannyThreshold1;
        set
        {
            _pipelineParameters.CannyThreshold1 = value;
            OnPropertyChanged();
        }
    }

    public double CannyThreshold2
    {
        get => _pipelineParameters.CannyThreshold2;
        set
        {
            _pipelineParameters.CannyThreshold2 = value;
            OnPropertyChanged();
        }
    }

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

    public bool HasFiducialTemplates => _fiducialTemplateService.HasTemplates();

    public bool IsMaterialTransferRunning => _materialTransfer?.IsRunning ?? false;

    public MainViewModel(IMaterialTransferService? materialTransfer = null)
    {
        _materialTransfer = materialTransfer;
        LoadRegion();
        LoadRecommendedParametersToUi();
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

    public void OnCameraSelected(CameraInfo? camera)
    {
        _selectedCamera = camera;
        OnPropertyChanged(nameof(CanEditCameraParameters));
        if (camera is not null)
            LoadRecommendedParametersToUi();
    }

    public async Task RefreshCamerasAsync()
    {
        IsBusy = true;
        StatusText = "Đang dò tìm camera Basler...";
        Cameras = [];
        Resolutions = [];

        using var discovery = new BaslerCameraService();
        Cameras = await discovery.EnumerateCamerasAsync();

        StatusText = Cameras.Count == 0
            ? "Không phát hiện camera Basler nào."
            : $"Tìm thấy {Cameras.Count} camera Basler.";

        IsBusy = false;
    }

    public async Task LoadResolutionsAsync(CameraInfo camera)
    {
        IsBusy = true;
        Resolutions = [];
        StatusText = "Đang đọc độ phân giải...";
        OnCameraSelected(camera);

        using var probe = new BaslerCameraService();
        Resolutions = await probe.GetSupportedResolutionsAsync(camera);

        StatusText = Resolutions.Count == 0
            ? "Camera không phản hồi độ phân giải."
            : $"Sẵn sàng. {Resolutions.Count} độ phân giải hỗ trợ.";

        IsBusy = false;
    }

    public int GetDefaultResolutionIndex()
        => Resolutions.Count > 0 ? Resolutions.Count - 1 : 0;

    public async Task StartCameraAsync(CameraInfo camera, int width, int height)
    {
        StopCameraInternal();

        _cameraService = new BaslerCameraService();
        _cameraService.FrameArrived += OnFrameArrived;
        _cameraService.GrabStatusChanged += OnGrabStatusChanged;
        _selectedCamera = camera;

        _cameraService.PrepareForStart(BuildParametersFromUi(width, height));
        IsBusy = true;
        StatusText = "Đang kết nối camera...";

        try
        {
            await _cameraService.StartAsync(camera, width, height).ConfigureAwait(true);
            SyncParametersFromCamera();

            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(CanEditCameraParameters));
            StatusText = $"Camera đang chạy ({width}×{height})";
        }
        catch (Exception ex)
        {
            StopCameraInternal();
            OnPropertyChanged(nameof(IsRunning));
            StatusText = $"Lỗi kết nối camera: {ex.Message}";
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnGrabStatusChanged(string message)
        => StatusText = message;

    public void StopCamera()
    {
        StopCameraInternal();
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(CanEditCameraParameters));
        _frameCount = 0;
        CurrentFps = 0;
        StatusText = "Camera đã dừng.";
    }

    public void LoadRecommendedParametersToUi()
    {
        var defaults = CameraDefaultsLoader.LoadRecommended();
        ExposureTimeUs = defaults.ExposureTimeUs;
        GainDb = defaults.GainDb;
        Gamma = defaults.Gamma;
    }

    public void ApplyCameraParameters()
    {
        if (_cameraService is null || !IsRunning)
        {
            StatusText = "Chỉ áp dụng tham số khi camera đang chạy.";
            return;
        }

        try
        {
            var resolution = GetCurrentResolutionFromRunningCamera();
            _cameraService.ApplyParameters(BuildParametersFromUi(resolution.Width, resolution.Height));
            SyncParametersFromCamera();
            StatusText = $"Đã áp dụng tham số (Exposure {ExposureTimeUs:F0} µs, Gain {GainDb:F1} dB).";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi áp dụng tham số: {ex.Message}";
        }
    }

    public void ResetPipelineParameters()
    {
        _pipelineParameters.ResetToDefaults();
        OnPropertyChanged(nameof(CannyThreshold1));
        OnPropertyChanged(nameof(CannyThreshold2));
        StatusText = "Đã đặt lại tham số Canny (50 / 150).";
    }

    public void ResetCameraParameters()
    {
        if (_cameraService is null)
        {
            LoadRecommendedParametersToUi();
            StatusText = "Đã tải tham số khuyến nghị.";
            return;
        }

        try
        {
            _cameraService.ResetToRecommended();
            SyncParametersFromCamera();
            StatusText = "Đã đặt lại và áp dụng tham số khuyến nghị.";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi đặt lại tham số: {ex.Message}";
        }
    }

    /// <summary>Grab one frame from the running camera (caller owns the returned <see cref="Mat"/>).</summary>
    public async Task<Mat?> CaptureFrameAsync()
    {
        StatusText = "Đang chụp ảnh...";

        if (_cameraService is null)
        {
            StatusText = "Camera chưa khởi động.";
            return null;
        }

        var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            frame?.Dispose();
            StatusText = "Không thể chụp ảnh từ camera.";
            return null;
        }

        return CropToSelectedRegion(frame);
    }

    public async Task CaptureTest2FrameAsync()
    {
        StatusText = "Đang chụp ảnh (Test 2)...";

        if (_cameraService is null)
        {
            StatusText = "Camera chưa khởi động.";
            return;
        }

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

        if (_cameraService is null)
        {
            StatusText = "Camera chưa khởi động.";
            return;
        }

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

    public async Task CaptureFiducialTemplateFrameAsync()
    {
        StatusText = "Đang chụp ảnh và chạy Morphology Close...";

        if (_cameraService is null)
        {
            StatusText = "Camera chưa khởi động.";
            return;
        }

        var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            frame?.Dispose();
            StatusText = "Không thể chụp ảnh từ camera.";
            return;
        }

        frame = CropToSelectedRegion(frame);

        Mat? closedImage = await Task.Run(() =>
        {
            var segmentation = new PcbSegmentationService(_pipelineParameters);
            using var pipeline = segmentation.RunPipeline(frame);
            return pipeline.Closed.Clone();
        });
        frame.Dispose();

        if (closedImage is null || closedImage.Empty())
        {
            closedImage?.Dispose();
            StatusText = "Không tạo được ảnh Morphology Close.";
            return;
        }

        StatusText = "Đã mở form tạo mẫu 4 lỗ tròn (Morphology Close).";
        FiducialTemplateFrameCaptured?.Invoke(closedImage);
    }

    public async Task CaptureAndSaveFrameAsync()
    {
        StatusText = "Đang chụp và lưu ảnh...";

        if (_cameraService is null)
        {
            StatusText = "Camera chưa khởi động.";
            return;
        }

        using var frame = await Task.Run(() => _cameraService.GrabFrame());

        if (frame is null || frame.Empty())
        {
            StatusText = "Không thể chụp ảnh từ camera.";
            return;
        }

        try
        {
            var filePath = await Task.Run(() => _cameraCaptureService.SaveFrame(frame));
            StatusText = $"Đã lưu ảnh: {filePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi lưu ảnh: {ex.Message}";
        }
    }

    public void RefreshFiducialTemplateStatus()
    {
        OnPropertyChanged(nameof(HasFiducialTemplates));
    }

    private CameraParameters BuildParametersFromUi(int width, int height) => new()
    {
        ExposureTimeUs = ExposureTimeUs,
        GainDb = GainDb,
        Gamma = Gamma,
        Width = width,
        Height = height,
        BalanceWhiteAuto = CameraDefaultsLoader.LoadRecommended().BalanceWhiteAuto
    };

    private void SyncParametersFromCamera()
    {
        if (_cameraService is null) return;

        var current = _cameraService.ReadCurrentParameters();
        ExposureTimeUs = current.ExposureTimeUs;
        GainDb = current.GainDb;
        Gamma = current.Gamma;
    }

    private (int Width, int Height) GetCurrentResolutionFromRunningCamera()
    {
        if (_cameraService is not null)
        {
            var current = _cameraService.ReadCurrentParameters();
            return (current.Width, current.Height);
        }

        if (_lastFrameWidth > 0 && _lastFrameHeight > 0)
            return (_lastFrameWidth, _lastFrameHeight);

        var defaults = CameraDefaultsLoader.LoadRecommended();
        return (defaults.Width, defaults.Height);
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

    private void StopCameraInternal()
    {
        if (_cameraService is null) return;

        _cameraService.FrameArrived -= OnFrameArrived;
        _cameraService.GrabStatusChanged -= OnGrabStatusChanged;
        _cameraService.Stop();
        _cameraService.Dispose();
        _cameraService = null;
    }

    private const int PreviewMaxDimension = 1920;

    private void OnFrameArrived(Mat frame)
    {
        _frameCount++;
        if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
        {
            CurrentFps = _frameCount;
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }

        _lastFrameWidth = frame.Width;
        _lastFrameHeight = frame.Height;

        // Drop frames while preview conversion is still running — avoids blocking pylon grab thread.
        if (Interlocked.CompareExchange(ref _previewProcessing, 1, 0) != 0)
            return;

        Mat frameCopy;
        OpenCvSharp.Rect? selectedRegion;
        try
        {
            frameCopy = frame.Clone();
            selectedRegion = _selectedRegion;
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _previewProcessing, 0);
            StatusText = $"Hiển thị preview lỗi: {ex.Message}";
            return;
        }

        _ = Task.Run(() =>
        {
            try
            {
                var bitmap = BuildPreviewBitmap(frameCopy, selectedRegion);
                if (bitmap is not null)
                    FrameReady?.Invoke(bitmap);
            }
            catch (Exception ex)
            {
                StatusText = $"Hiển thị preview lỗi: {ex.Message}";
            }
            finally
            {
                frameCopy.Dispose();
                Interlocked.Exchange(ref _previewProcessing, 0);
            }
        });
    }

    private static System.Windows.Media.Imaging.BitmapSource? BuildPreviewBitmap(
        Mat frame, OpenCvSharp.Rect? selectedRegion)
    {
        Mat? previewMat = null;
        try
        {
            Mat displaySource = frame;
            int maxDim = Math.Max(frame.Width, frame.Height);
            if (maxDim > PreviewMaxDimension)
            {
                previewMat = new Mat();
                double scale = PreviewMaxDimension / (double)maxDim;
                Cv2.Resize(frame, previewMat, new Size(), scale, scale, InterpolationFlags.Area);
                displaySource = previewMat;
            }

            using var annotated = displaySource.Clone();

            if (selectedRegion.HasValue)
            {
                double scaleX = (double)displaySource.Width / frame.Width;
                double scaleY = (double)displaySource.Height / frame.Height;
                var roi = selectedRegion.Value;
                var scaled = new OpenCvSharp.Rect(
                    (int)(roi.X * scaleX),
                    (int)(roi.Y * scaleY),
                    (int)(roi.Width * scaleX),
                    (int)(roi.Height * scaleY));
                roi = ClampRect(scaled, annotated.Width, annotated.Height);
                if (roi.Width > 0 && roi.Height > 0)
                    Cv2.Rectangle(annotated, roi, new Scalar(0, 200, 0), 2);
            }

            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(annotated);
            bitmap.Freeze();
            return bitmap;
        }
        finally
        {
            previewMat?.Dispose();
        }
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
        StopCameraInternal();
        _disposed = true;
    }
}

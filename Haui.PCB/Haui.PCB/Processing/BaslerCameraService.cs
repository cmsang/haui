using Basler.Pylon;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Dịch vụ camera Basler qua pylon .NET SDK (GigE / USB3).
/// </summary>
public class BaslerCameraService : ICameraService, ICameraParameterService
{
    private Camera? _camera;
    private Mat? _lastFrame;
    private PixelDataConverter? _converter;
    private readonly object _frameLock = new();
    private bool _disposed;
    private CameraParameters _pendingParameters = CameraDefaultsLoader.LoadRecommended();
    private string? _lastGrabError;
    private int _grabFailCount;

    /// <summary>Thông báo lỗi grab (hiển thị lên status bar).</summary>
    public event Action<string>? GrabStatusChanged;

    public bool IsRunning { get; private set; }

    public event Action<Mat>? FrameArrived;

    public Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync()
        => Task.Run<IReadOnlyList<CameraInfo>>(() =>
        {
            var result = new List<CameraInfo>();
            try
            {
                int i = 0;
                foreach (var info in CameraFinder.Enumerate())
                {
                    string name = info[CameraInfoKey.FriendlyName] ?? "Basler Camera";
                    string serial = info[CameraInfoKey.SerialNumber] ?? string.Empty;
                    if (string.IsNullOrEmpty(serial))
                        continue;
                    result.Add(new CameraInfo(i++, name, serial));
                }
            }
            catch
            {
                // pylon chưa cài hoặc runtime không khả dụng
            }

            return result;
        });

    public Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(string serialNumber)
        => Task.Run<IReadOnlyList<ResolutionInfo>>(() =>
        {
            var result = new List<ResolutionInfo>();
            try
            {
                using var probe = new Camera(serialNumber);
                probe.Open();

                long maxW = probe.Parameters[PLCamera.WidthMax].GetValue();
                long maxH = probe.Parameters[PLCamera.HeightMax].GetValue();
                result.Add(new ResolutionInfo((int)maxW, (int)maxH));

                AddIfValid(result, (int)maxW, (int)maxH, 1536, 1024);
                AddIfValid(result, (int)maxW, (int)maxH, 2304, 1536);
                AddIfValid(result, (int)maxW, (int)maxH, 1280, 720);
                AddIfValid(result, (int)maxW, (int)maxH, 1920, 1080);
                AddIfValid(result, (int)maxW, (int)maxH, 1920, 1280);

                probe.Close();
            }
            catch
            {
                // Trả về defaults từ file cấu hình
                var defaults = CameraDefaultsLoader.LoadRecommended();
                result.Add(new ResolutionInfo(defaults.Width, defaults.Height));
            }

            result = result
                .DistinctBy(r => (r.Width, r.Height))
                .OrderBy(r => r.Width * r.Height)
                .ToList();
            return result;
        });

    public void Start(string serialNumber, int width, int height)
    {
        Stop();

        _camera = new Camera(serialNumber);
        _camera.Open();
        Configuration.AcquireContinuous(_camera, null);
        ConfigureStream();
        TryConfigurePixelFormat();

        _pendingParameters.Width = width;
        _pendingParameters.Height = height;
        ConfigureRoi(width, height);
        ApplyExposureParameters(_pendingParameters);

        _camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
        _camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
        IsRunning = true;
        _grabFailCount = 0;
        _lastGrabError = null;
    }

    private void ConfigureStream()
    {
        if (_camera is null) return;

        if (_camera.Parameters[PLStream.AutoPacketSize].IsWritable)
            _camera.Parameters[PLStream.AutoPacketSize].SetValue(true);

        if (_camera.Parameters[PLCameraInstance.MaxNumBuffer].IsWritable)
            _camera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(16);

        if (_camera.Parameters[PLStream.MaxTransferSize].IsWritable)
            _camera.Parameters[PLStream.MaxTransferSize].SetValue(4 * 1024 * 1024);

        if (_camera.Parameters[PLStream.MaxBufferSize].IsWritable)
            _camera.Parameters[PLStream.MaxBufferSize].SetValue(64 * 1024 * 1024);
    }

    private void TryConfigurePixelFormat()
    {
        if (_camera is null) return;

        var pixelFormat = _camera.Parameters[PLCamera.PixelFormat];
        if (!pixelFormat.IsWritable) return;

        ReadOnlySpan<string> preferred =
        [
            PLCamera.PixelFormat.BayerRG8,
            PLCamera.PixelFormat.BayerBG8,
            PLCamera.PixelFormat.BayerGR8,
            PLCamera.PixelFormat.BayerGB8,
            PLCamera.PixelFormat.BGR8,
            PLCamera.PixelFormat.RGB8,
            PLCamera.PixelFormat.Mono8
        ];

        foreach (string format in preferred)
        {
            if (pixelFormat.TrySetValue(format))
                return;
        }
    }

    private void ConfigureRoi(int targetWidth, int targetHeight)
    {
        if (_camera is null) return;

        var p = _camera.Parameters;
        p[PLCamera.OffsetX].TrySetToMinimum();
        p[PLCamera.OffsetY].TrySetToMinimum();

        long maxW = p[PLCamera.WidthMax].GetValue();
        long maxH = p[PLCamera.HeightMax].GetValue();

        if (targetWidth <= 0 || targetHeight <= 0 || targetWidth >= maxW && targetHeight >= maxH)
        {
            p[PLCamera.Width].TrySetToMaximum();
            p[PLCamera.Height].TrySetToMaximum();
            return;
        }

        p[PLCamera.Width].TrySetValue(targetWidth, IntegerValueCorrection.Nearest);
        p[PLCamera.Height].TrySetValue(targetHeight, IntegerValueCorrection.Nearest);

        long actualW = p[PLCamera.Width].GetValue();
        long actualH = p[PLCamera.Height].GetValue();
        int offsetX = (int)((maxW - actualW) / 2);
        int offsetY = (int)((maxH - actualH) / 2);
        p[PLCamera.OffsetX].TrySetValue(offsetX, IntegerValueCorrection.Nearest);
        p[PLCamera.OffsetY].TrySetValue(offsetY, IntegerValueCorrection.Nearest);
    }

    public Mat? GrabFrame()
    {
        lock (_frameLock)
            return _lastFrame?.Clone();
    }

    public async Task<Mat?> CaptureSharpestFrameAsync(
        int durationMs = 1500,
        CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
            return null;

        Mat? bestFrame = null;
        double bestSharpness = -1;

        void OnFrame(Mat frame)
        {
            double sharpness = FrameSharpnessHelper.ComputeLaplacianVariance(frame);
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
            // Trả về frame tốt nhất đã thu thập
        }
        finally
        {
            FrameArrived -= OnFrame;
        }

        lock (_frameLock)
            return bestFrame;
    }

    public void Stop()
    {
        if (!IsRunning && _camera is null)
            return;

        if (_camera is not null)
        {
            try
            {
                _camera.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
                if (_camera.StreamGrabber.IsGrabbing)
                    _camera.StreamGrabber.Stop();
            }
            catch
            {
                // Bỏ qua lỗi khi dừng grabber
            }

            try
            {
                if (_camera.IsOpen)
                    _camera.Close();
            }
            catch
            {
                // Bỏ qua lỗi khi đóng camera
            }

            _camera.Dispose();
            _camera = null;
        }

        lock (_frameLock)
        {
            _lastFrame?.Dispose();
            _lastFrame = null;
        }

        _converter?.Dispose();
        _converter = null;
        IsRunning = false;
    }

    public CameraParameters GetRecommendedParameters()
        => CameraDefaultsLoader.LoadRecommended().Clone();

    public CameraParameters ReadCurrentParameters()
    {
        if (_camera is null || !_camera.IsOpen)
            return _pendingParameters.Clone();

        var p = _pendingParameters.Clone();
        var parameters = _camera.Parameters;

        TryReadDouble(parameters[PLCamera.ExposureTime], v => p.ExposureTimeUs = v);
        TryReadDouble(parameters[PLCamera.Gain], v => p.GainDb = v);
        TryReadDouble(parameters[PLCamera.Gamma], v => p.Gamma = v);
        TryReadInt(parameters[PLCamera.Width], v => p.Width = v);
        TryReadInt(parameters[PLCamera.Height], v => p.Height = v);

        var balanceWhite = parameters[PLCamera.BalanceWhiteAuto];
        if (balanceWhite.IsReadable)
            p.BalanceWhiteAuto = balanceWhite.GetValue();

        return p;
    }

    public void ApplyParameters(CameraParameters parameters)
    {
        _pendingParameters = parameters.Clone();
        if (_camera is not null && _camera.IsOpen)
            ApplyParametersInternal(_pendingParameters, restartGrabber: true);
    }

    public void PrepareForStart(CameraParameters parameters)
        => _pendingParameters = parameters.Clone();

    public void ResetToRecommended()
    {
        var defaults = GetRecommendedParameters();
        ApplyParameters(defaults);
    }

    private void ApplyParametersInternal(CameraParameters settings, bool restartGrabber)
    {
        if (_camera is null || !_camera.IsOpen)
            return;

        bool wasGrabbing = _camera.StreamGrabber.IsGrabbing;
        if (restartGrabber && wasGrabbing)
            _camera.StreamGrabber.Stop();

        ConfigureRoi(settings.Width, settings.Height);
        ApplyExposureParameters(settings);

        if (restartGrabber && wasGrabbing && IsRunning)
            _camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
    }

    private void ApplyExposureParameters(CameraParameters settings)
    {
        if (_camera is null || !_camera.IsOpen) return;

        var cameraParams = _camera.Parameters;
        SetBalanceWhiteAuto(settings.BalanceWhiteAuto);
        SetDoubleParameter(cameraParams[PLCamera.ExposureTime], settings.ExposureTimeUs);
        SetDoubleParameter(cameraParams[PLCamera.Gain], settings.GainDb);
        SetDoubleParameter(cameraParams[PLCamera.Gamma], settings.Gamma);
    }

    private void SetBalanceWhiteAuto(string value)
    {
        if (_camera is null) return;
        var param = _camera.Parameters[PLCamera.BalanceWhiteAuto];
        if (!param.IsWritable) return;

        string normalized = value.Trim();
        if (string.Equals(normalized, "Once", StringComparison.OrdinalIgnoreCase))
            param.SetValue(PLCamera.BalanceWhiteAuto.Once);
        else if (string.Equals(normalized, "Continuous", StringComparison.OrdinalIgnoreCase))
            param.SetValue(PLCamera.BalanceWhiteAuto.Continuous);
        else
            param.SetValue(PLCamera.BalanceWhiteAuto.Off);
    }

    private void SetDoubleParameter(IFloatParameter? param, double value)
    {
        if (param is null || !param.IsWritable) return;

        double min = param.GetMinimum();
        double max = param.GetMaximum();
        param.SetValue(Math.Clamp(value, min, max));
    }

    private static void TryReadDouble(IFloatParameter? param, Action<double> setter)
    {
        if (param is not null && param.IsReadable)
            setter(param.GetValue());
    }

    private static void TryReadInt(IIntegerParameter? param, Action<int> setter)
    {
        if (param is not null && param.IsReadable)
            setter((int)param.GetValue());
    }

    private void OnImageGrabbed(object? sender, ImageGrabbedEventArgs e)
    {
        IGrabResult grabResult = e.GrabResult;
        try
        {
            if (!grabResult.GrabSucceeded)
            {
                _grabFailCount++;
                _lastGrabError = $"{grabResult.ErrorCode}: {grabResult.ErrorDescription}";
                if (_grabFailCount <= 3 || _grabFailCount % 30 == 0)
                    GrabStatusChanged?.Invoke($"Grab lỗi: {_lastGrabError}");
                return;
            }

            using var mat = ConvertGrabResultToMat(grabResult);
            if (mat.Empty())
                return;

            _grabFailCount = 0;

            lock (_frameLock)
            {
                _lastFrame?.Dispose();
                _lastFrame = mat.Clone();
            }

            FrameArrived?.Invoke(mat);
        }
        catch (Exception ex)
        {
            _grabFailCount++;
            if (_grabFailCount <= 3)
                GrabStatusChanged?.Invoke($"Convert lỗi: {ex.Message}");
        }
        finally
        {
            grabResult.Dispose();
        }
    }

    private Mat ConvertGrabResultToMat(IGrabResult grabResult)
    {
        _converter ??= new PixelDataConverter();
        _converter.OutputPixelFormat = PixelType.BGR8packed;

        int width = grabResult.Width;
        int height = grabResult.Height;
        var mat = new Mat(height, width, MatType.CV_8UC3);
        int bufferSize = (int)_converter.GetBufferSizeForConversion(grabResult);
        if (bufferSize <= 0)
            bufferSize = (int)(mat.Step() * mat.Rows);

        _converter.Convert(mat.Data, bufferSize, grabResult);
        return mat;
    }

    private static void AddIfValid(List<ResolutionInfo> list, int maxW, int maxH, int w, int h)
    {
        if (w <= maxW && h <= maxH)
            list.Add(new ResolutionInfo(w, h));
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
    }
}

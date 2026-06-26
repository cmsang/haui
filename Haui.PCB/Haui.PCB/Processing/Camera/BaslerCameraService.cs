using Basler.Pylon;
using OpenCvSharp;
using PylonCamera = Basler.Pylon.Camera;

namespace Haui.PCB.Processing.Camera;

/// <summary>
/// Basler camera service via pylon .NET SDK (GigE / USB3).
/// GigE fast path: announce + ICameraInfo connect; resolution probe cached per device.
/// </summary>
public class BaslerCameraService : ICameraService, ICameraParameterService
{
    private const int GrabGaussianBlurKernelSize = 5;

    private PylonCamera? _camera;
    private Mat? _lastFrame;
    private PixelDataConverter? _converter;
    private readonly object _frameLock = new();
    private bool _disposed;
    private CameraParameters _pendingParameters = CameraDefaultsLoader.LoadRecommended();
    private string? _lastGrabError;
    private int _grabFailCount;

    /// <summary>Grab errors for the status bar.</summary>
    public event Action<string>? GrabStatusChanged;

    public bool IsRunning { get; private set; }

    public event Action<Mat>? FrameArrived;

    public Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync()
        => Task.Run<IReadOnlyList<CameraInfo>>(EnumerateCameras);

    private static IReadOnlyList<CameraInfo> EnumerateCameras()
    {
        var result = new List<CameraInfo>();
        try
        {
            var config = CameraDefaultsLoader.LoadRecommended();
            string configuredIp = config.DeviceIp.Trim();
            if (!string.IsNullOrEmpty(configuredIp))
            {
                BaslerPylonRuntime.AnnounceIp(configuredIp);
                ICameraInfo? found = BaslerPylonRuntime.FindGigEByIp(configuredIp);
                if (found is not null)
                {
                    string serial = found[CameraInfoKey.SerialNumber] ?? configuredIp;
                    string name = found[CameraInfoKey.FriendlyName] ?? $"Basler GigE ({configuredIp})";
                    result.Add(new CameraInfo(0, name, serial, configuredIp));
                    return result;
                }

                result.Add(new CameraInfo(0, $"Basler GigE ({configuredIp})", configuredIp, configuredIp));
                return result;
            }

            int index = 0;
            foreach (var info in BaslerPylonRuntime.EnumerateGigE())
            {
                string name = info[CameraInfoKey.FriendlyName] ?? "Basler Camera";
                string serial = info[CameraInfoKey.SerialNumber] ?? string.Empty;
                if (string.IsNullOrEmpty(serial))
                    continue;

                string? ip = info[CameraInfoKey.DeviceIpAddress];
                result.Add(new CameraInfo(index++, name, serial, ip));
            }
        }
        catch
        {
            // pylon not installed or runtime unavailable
        }

        return result;
    }

    public Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(CameraInfo camera)
        => Task.Run(() => QueryResolutions(camera));

    private static IReadOnlyList<ResolutionInfo> QueryResolutions(CameraInfo camera)
    {
        string cacheKey = camera.DeviceId;
        if (BaslerPylonRuntime.TryGetCachedResolution(cacheKey, out int cachedW, out int cachedH))
            return BuildResolutionList(cachedW, cachedH);

        try
        {
            using var probe = OpenCamera(camera);
            int maxW = (int)probe.Parameters[PLCamera.WidthMax].GetValue();
            int maxH = (int)probe.Parameters[PLCamera.HeightMax].GetValue();
            BaslerPylonRuntime.CacheResolution(cacheKey, maxW, maxH);
            probe.Close();
            return BuildResolutionList(maxW, maxH);
        }
        catch
        {
            var defaults = CameraDefaultsLoader.LoadRecommended();
            return BuildResolutionList(defaults.Width, defaults.Height);
        }
    }

    private static IReadOnlyList<ResolutionInfo> BuildResolutionList(int maxW, int maxH)
    {
        var defaults = CameraDefaultsLoader.LoadRecommended();
        var result = new List<ResolutionInfo>();
        AddIfValid(result, maxW, maxH, maxW, maxH);
        AddIfValid(result, maxW, maxH, defaults.Width, defaults.Height);
        AddIfValid(result, maxW, maxH, 2304, 1536);
        AddIfValid(result, maxW, maxH, 1920, 1280);
        AddIfValid(result, maxW, maxH, 1920, 1080);
        AddIfValid(result, maxW, maxH, 1536, 1024);
        AddIfValid(result, maxW, maxH, 1280, 720);

        return result
            .DistinctBy(r => (r.Width, r.Height))
            .OrderBy(r => r.Width * r.Height)
            .ToList();
    }

    public Task StartAsync(CameraInfo camera, int width, int height, CancellationToken cancellationToken = default)
        => Task.Run(() => StartCore(camera, width, height), cancellationToken);

    public void Start(CameraInfo camera, int width, int height)
        => StartCore(camera, width, height);

    private void StartCore(CameraInfo camera, int width, int height)
    {
        Stop();

        var opened = OpenCamera(camera);
        _camera = opened;
        Basler.Pylon.Configuration.AcquireContinuous(opened, null);
        ConfigureStream();
        TryConfigurePixelFormat();

        _pendingParameters.Width = width;
        _pendingParameters.Height = height;
        ConfigureRoi(width, height);
        ApplyExposureParameters(_pendingParameters, includeAutoWhiteBalance: true);

        opened.StreamGrabber.ImageGrabbed += OnImageGrabbed;
        opened.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
        IsRunning = true;
        _grabFailCount = 0;
        _lastGrabError = null;
    }

    private static PylonCamera OpenCamera(CameraInfo camera)
    {
        if (BaslerPylonRuntime.TryGetCachedDevice(camera.DeviceId, out ICameraInfo deviceInfo)
            || (!string.IsNullOrWhiteSpace(camera.IpAddress)
                && BaslerPylonRuntime.TryGetCachedDevice(camera.IpAddress, out deviceInfo)))
        {
            var byInfo = new PylonCamera(deviceInfo);
            byInfo.Open();
            return byInfo;
        }

        string configuredIp = CameraDefaultsLoader.LoadRecommended().DeviceIp.Trim();
        string? announceIp = !string.IsNullOrWhiteSpace(camera.IpAddress)
            ? camera.IpAddress.Trim()
            : string.Equals(camera.DeviceId, configuredIp, StringComparison.OrdinalIgnoreCase)
                ? configuredIp
                : null;

        if (!string.IsNullOrEmpty(announceIp))
        {
            BaslerPylonRuntime.AnnounceIp(announceIp);
            ICameraInfo? found = BaslerPylonRuntime.FindGigEByIp(announceIp);
            if (found is not null)
            {
                var byIpInfo = new PylonCamera(found);
                byIpInfo.Open();
                return byIpInfo;
            }
        }

        var bySerial = new PylonCamera(camera.DeviceId);
        bySerial.Open();
        return bySerial;
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

        if (!pixelFormat.TrySetValue(PLCamera.PixelFormat.Mono8))
            GrabStatusChanged?.Invoke("Không thể đặt pixel format Mono8.");
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
            // Return best frame collected so far
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
                // Ignore stop errors
            }

            try
            {
                if (_camera.IsOpen)
                    _camera.Close();
            }
            catch
            {
                // Ignore close errors
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

        var gainAuto = parameters[PLCamera.GainAuto];
        if (gainAuto.IsReadable)
            p.GainAuto = gainAuto.GetValue();

        var pixelFormat = parameters[PLCamera.PixelFormat];
        if (pixelFormat.IsReadable)
            p.PixelFormat = pixelFormat.GetValue();

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
        ApplyExposureParameters(settings, includeAutoWhiteBalance: true);

        if (restartGrabber && wasGrabbing && IsRunning)
            _camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
    }

    private void ApplyExposureParameters(CameraParameters settings, bool includeAutoWhiteBalance)
    {
        if (_camera is null || !_camera.IsOpen) return;

        var cameraParams = _camera.Parameters;
        SetGainAuto(settings.GainAuto);
        if (includeAutoWhiteBalance)
            SetBalanceWhiteAuto(settings.BalanceWhiteAuto);

        SetDoubleParameter(cameraParams[PLCamera.ExposureTime], settings.ExposureTimeUs);
        SetDoubleParameter(cameraParams[PLCamera.Gamma], settings.Gamma);

        var gainAuto = cameraParams[PLCamera.GainAuto];
        if (gainAuto.IsReadable
            && string.Equals(gainAuto.GetValue(), PLCamera.GainAuto.Off, StringComparison.OrdinalIgnoreCase))
        {
            SetDoubleParameter(cameraParams[PLCamera.Gain], settings.GainDb);
        }
    }

    private void SetGainAuto(string value)
    {
        if (_camera is null) return;
        var param = _camera.Parameters[PLCamera.GainAuto];
        if (!param.IsWritable) return;

        string normalized = value.Trim();
        if (string.Equals(normalized, PLCamera.GainAuto.Once, StringComparison.OrdinalIgnoreCase))
            param.SetValue(PLCamera.GainAuto.Once);
        else if (string.Equals(normalized, PLCamera.GainAuto.Continuous, StringComparison.OrdinalIgnoreCase))
            param.SetValue(PLCamera.GainAuto.Continuous);
        else
            param.SetValue(PLCamera.GainAuto.Off);
    }

    private void SetBalanceWhiteAuto(string value)
    {
        if (_camera is null) return;
        var param = _camera.Parameters[PLCamera.BalanceWhiteAuto];
        if (!param.IsWritable) return;

        string normalized = value.Trim();
        if (string.Equals(normalized, PLCamera.BalanceWhiteAuto.Once, StringComparison.OrdinalIgnoreCase))
            param.SetValue(PLCamera.BalanceWhiteAuto.Once);
        else if (string.Equals(normalized, PLCamera.BalanceWhiteAuto.Continuous, StringComparison.OrdinalIgnoreCase))
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

            using var raw = ConvertGrabResultToMat(grabResult);
            if (raw.Empty())
                return;

            using var blurred = new Mat();
            Cv2.GaussianBlur(
                raw,
                blurred,
                new Size(GrabGaussianBlurKernelSize, GrabGaussianBlurKernelSize),
                0);

            _grabFailCount = 0;

            lock (_frameLock)
            {
                _lastFrame?.Dispose();
                _lastFrame = blurred.Clone();
            }

            FrameArrived?.Invoke(blurred);
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
        _converter.OutputPixelFormat = PixelType.Mono8;

        int width = grabResult.Width;
        int height = grabResult.Height;
        var mat = new Mat(height, width, MatType.CV_8UC1);
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

using System.Threading.Channels;
using Basler.Pylon;
using Haui.PCB.Models.Configuration;
using OpenCvSharp;
using PylonCamera = Basler.Pylon.Camera;

namespace Haui.PCB.Processing.Camera;

/// <summary>
/// Basler camera service via pylon .NET SDK (GigE / USB3).
/// GigE fast path: announce + ICameraInfo connect; resolution probe cached per device.
/// Grab processing runs on a background worker to avoid pylon buffer underruns.
/// </summary>
public class BaslerCameraService : ICameraService, ICameraParameterService
{
    private const int GrabGaussianBlurKernelSize = 5;
    private const uint IncompleteGrabErrorCode = 3_774_873_620u;
    private const double GrabStatsWindowSeconds = 60;
    private const double RecoveryCooldownSeconds = 30;

    private PylonCamera? _camera;
    private Mat? _lastFrame;
    private PixelDataConverter? _converter;
    private readonly object _frameLock = new();
    private readonly object _recoveryLock = new();
    private bool _disposed;
    private CameraParameters _pendingParameters = CameraDefaultsLoader.LoadRecommended();
    private ImageDownscaleSettings _downscale = CameraDefaultsLoader.LoadDownscale();
    private CameraGigEStreamSettings _gigEStream = CameraDefaultsLoader.LoadGigEStream();
    private string? _lastGrabError;
    private int _consecutiveFailCount;
    private int _incompleteGrabCount;
    private int _windowOkCount;
    private int _windowFailCount;
    private DateTime _windowStartUtc = DateTime.UtcNow;
    private DateTime _lastRecoveryUtc = DateTime.MinValue;

    private CancellationTokenSource? _workerCts;
    private Task? _workerTask;
    private Channel<IGrabResult>? _frameChannel;

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

        _downscale = CameraDefaultsLoader.LoadDownscale();
        _gigEStream = CameraDefaultsLoader.LoadGigEStream();
        ResetGrabMetrics();

        var opened = OpenCamera(camera);
        _camera = opened;
        Basler.Pylon.Configuration.AcquireContinuous(opened, null);
        ConfigureStream();
        TryConfigurePixelFormat();

        _pendingParameters.Width = width;
        _pendingParameters.Height = height;
        ConfigureRoi(width, height);
        ApplyExposureParameters(_pendingParameters, includeAutoWhiteBalance: true);

        StartFrameWorker();

        opened.StreamGrabber.ImageGrabbed += OnImageGrabbed;
        opened.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
        IsRunning = true;
    }

    private void ResetGrabMetrics()
    {
        _consecutiveFailCount = 0;
        _incompleteGrabCount = 0;
        _windowOkCount = 0;
        _windowFailCount = 0;
        _windowStartUtc = DateTime.UtcNow;
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

        var stream = _gigEStream;
        var parameters = _camera.Parameters;

        var autoPacketSize = parameters[PLStream.AutoPacketSize];
        if (autoPacketSize.IsWritable)
            autoPacketSize.SetValue(stream.AutoPacketSize);

        if (!stream.AutoPacketSize && stream.PacketSize > 0)
            TrySetIntegerParameter(parameters, "GevSCPSPacketSize", stream.PacketSize);

        if (stream.InterPacketDelay > 0)
            TrySetIntegerParameter(parameters, "GevSCPD", stream.InterPacketDelay);

        var maxNumBuffer = parameters[PLCameraInstance.MaxNumBuffer];
        if (maxNumBuffer.IsWritable)
            maxNumBuffer.SetValue(Math.Clamp((long)stream.MaxNumBuffer, maxNumBuffer.GetMinimum(), maxNumBuffer.GetMaximum()));

        var outputQueueSize = parameters[PLCameraInstance.OutputQueueSize];
        if (outputQueueSize.IsWritable)
            outputQueueSize.SetValue(Math.Clamp((long)stream.OutputQueueSize, outputQueueSize.GetMinimum(), outputQueueSize.GetMaximum()));

        var maxTransferSize = parameters[PLStream.MaxTransferSize];
        if (maxTransferSize.IsWritable)
            maxTransferSize.SetValue((long)stream.MaxTransferSizeMb * 1024 * 1024);

        var maxBufferSize = parameters[PLStream.MaxBufferSize];
        if (maxBufferSize.IsWritable)
            maxBufferSize.SetValue((long)stream.MaxBufferSizeMb * 1024 * 1024);

        if (stream.GrabLoopThreadPriority > 0)
        {
            TrySetIntegerParameter(parameters, "GrabLoopThreadPriority", stream.GrabLoopThreadPriority);
            TrySetIntegerParameter(parameters, "InternalGrabEngineThreadPriority", stream.GrabLoopThreadPriority);
        }
    }

    private static void TrySetIntegerParameter(IParameterCollection parameters, string name, long value)
    {
        if (!parameters.Contains(name))
            return;

        var param = parameters[name];
        if (param is not IIntegerParameter intParam || !intParam.IsWritable)
            return;

        long min = intParam.GetMinimum();
        long max = intParam.GetMaximum();
        intParam.SetValue(Math.Clamp(value, min, max));
    }

    private void StartFrameWorker()
    {
        StopFrameWorker();

        _frameChannel = Channel.CreateBounded<IGrabResult>(new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        _workerCts = new CancellationTokenSource();
        var token = _workerCts.Token;
        _workerTask = Task.Run(() => FrameWorkerLoop(token), token);
    }

    private void StopFrameWorker()
    {
        if (_workerCts is not null)
        {
            _workerCts.Cancel();
            _frameChannel?.Writer.TryComplete();
            try
            {
                _workerTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Worker cancelled during stop
            }

            _workerCts.Dispose();
            _workerCts = null;
            _workerTask = null;
        }

        if (_frameChannel is not null)
        {
            while (_frameChannel.Reader.TryRead(out IGrabResult? pending))
                pending.Dispose();

            _frameChannel = null;
        }
    }

    private async Task FrameWorkerLoop(CancellationToken cancellationToken)
    {
        if (_frameChannel is null)
            return;

        try
        {
            await foreach (IGrabResult grabResult in _frameChannel.Reader.ReadAllAsync(cancellationToken))
            {
                using (grabResult)
                {
                    try
                    {
                        ProcessGrabResult(grabResult);
                    }
                    catch (Exception ex)
                    {
                        RecordGrabFailure(isConversionError: true, ex.Message);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on stop
        }
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

        StopFrameWorker();

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
                _lastGrabError = $"{grabResult.ErrorCode}: {grabResult.ErrorDescription}";
                if (IsIncompleteBufferError(grabResult))
                    _incompleteGrabCount++;

                RecordGrabFailure(isConversionError: false, _lastGrabError);
                TryAutoRecoverGrabber();
                return;
            }

            IGrabResult cloned = grabResult.Clone();
            Channel<IGrabResult>? channel = _frameChannel;
            if (channel is null)
            {
                cloned.Dispose();
                return;
            }

            if (!channel.Writer.TryWrite(cloned))
            {
                // Worker busy — drop frame but keep grab thread unblocked.
                cloned.Dispose();
            }
        }
        finally
        {
            grabResult.Dispose();
        }
    }

    private void ProcessGrabResult(IGrabResult grabResult)
    {
        using var raw = ConvertGrabResultToMat(grabResult);
        if (raw.Empty())
            return;

        using var blurred = new Mat();
        Cv2.GaussianBlur(
            raw,
            blurred,
            new Size(GrabGaussianBlurKernelSize, GrabGaussianBlurKernelSize),
            0);

        using var processed = ApplyDownscale(blurred);

        RecordGrabSuccess();

        lock (_frameLock)
        {
            _lastFrame?.Dispose();
            _lastFrame = processed.Clone();
        }

        FrameArrived?.Invoke(processed);
    }

    private void RecordGrabSuccess()
    {
        RollGrabStatsWindowIfNeeded();
        _windowOkCount++;
        _consecutiveFailCount = 0;
    }

    private void RecordGrabFailure(bool isConversionError, string message)
    {
        RollGrabStatsWindowIfNeeded();
        _windowFailCount++;
        _consecutiveFailCount++;

        if (isConversionError)
            GrabStatusChanged?.Invoke($"Convert lỗi: {message}");
        else
            PublishGrabFailureStatus();
    }

    private void PublishGrabFailureStatus()
    {
        int total = _windowOkCount + _windowFailCount;
        if (total <= 0)
        {
            GrabStatusChanged?.Invoke($"Grab lỗi: {_lastGrabError}");
            return;
        }

        int percent = (int)Math.Round(100.0 * _windowFailCount / total);
        GrabStatusChanged?.Invoke($"Grab lỗi: {_windowFailCount}/{total} frame ({percent}%)");
    }

    private void RollGrabStatsWindowIfNeeded()
    {
        if ((DateTime.UtcNow - _windowStartUtc).TotalSeconds < GrabStatsWindowSeconds)
            return;

        _windowOkCount = 0;
        _windowFailCount = 0;
        _windowStartUtc = DateTime.UtcNow;
    }

    private void TryAutoRecoverGrabber()
    {
        if (!_gigEStream.EnableAutoRecovery)
            return;

        if (_consecutiveFailCount < _gigEStream.ConsecutiveFailThreshold)
            return;

        lock (_recoveryLock)
        {
            if ((DateTime.UtcNow - _lastRecoveryUtc).TotalSeconds < RecoveryCooldownSeconds)
                return;

            if (_camera is null || !_camera.IsOpen || !IsRunning)
                return;

            _lastRecoveryUtc = DateTime.UtcNow;
            _gigEStream = CameraDefaultsLoader.LoadGigEStream();

            try
            {
                GrabStatusChanged?.Invoke("Đang khôi phục luồng camera...");

                bool wasGrabbing = _camera.StreamGrabber.IsGrabbing;
                if (wasGrabbing)
                    _camera.StreamGrabber.Stop();

                ConfigureStream();

                if (wasGrabbing)
                    _camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

                _consecutiveFailCount = 0;
            }
            catch (Exception ex)
            {
                GrabStatusChanged?.Invoke($"Khôi phục camera lỗi: {ex.Message}");
            }
        }
    }

    private static bool IsIncompleteBufferError(IGrabResult grabResult)
    {
        if ((uint)grabResult.ErrorCode == IncompleteGrabErrorCode)
            return true;

        return grabResult.ErrorDescription?.Contains("incompletely grabbed", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Scales <paramref name="source"/> down so it fits within the configured bounds
    /// (<c>CameraDownscale</c>), keeping the aspect ratio. Returns a clone when the flag is
    /// disabled or no downscaling is needed.
    /// </summary>
    private Mat ApplyDownscale(Mat source)
    {
        if (!_downscale.Enabled || source.Empty())
            return source.Clone();

        int maxWidth = _downscale.Width;
        int maxHeight = _downscale.Height;
        if (maxWidth <= 0 || maxHeight <= 0)
            return source.Clone();

        double scale = Math.Min(
            (double)maxWidth / source.Width,
            (double)maxHeight / source.Height);

        if (scale >= 1.0)
            return source.Clone();

        int targetWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
        int targetHeight = Math.Max(1, (int)Math.Round(source.Height * scale));

        var resized = new Mat();
        Cv2.Resize(source, resized, new Size(targetWidth, targetHeight), 0, 0, InterpolationFlags.Area);
        return resized;
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

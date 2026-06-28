namespace Haui.PCB.Processing;

/// <summary>
/// Giao tiếp cổng warehouse trên một kết nối Serial bền vững — lắng nghe CAPx (yêu cầu chụp),
/// gửi CMx/chờ COx trước khi robot gắp hàng, gửi C1x/C2x khi buffer đầy.
/// </summary>
public class WarehouseSerialService : IDisposable
{
    private static readonly TimeSpan ReadyResponseTimeout = TimeSpan.FromSeconds(120);

    private readonly IAppSettingService _appSettingService;
    private readonly RobotSerialService _serial = new();
    private readonly object _connectGate = new();
    private bool _disposed;

    public WarehouseSerialService(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
        _serial.FrameReceived += OnFrameReceived;
    }

    /// <summary>Nhà kho gửi CAPx — yêu cầu PC chụp ảnh và kiểm tra bo mạch.</summary>
    public event Action? CaptureRequested;

    public bool IsConnected => _serial.IsConnected;

    /// <summary>Mở cổng warehouse và bắt đầu lắng nghe CAPx. False nếu chưa cấu hình / mở lỗi.</summary>
    public bool TryStartListening(out string warehouseCom, out string? error)
        => EnsureConnected(out warehouseCom, out error);

    /// <summary>Gửi CMx, chờ COx từ nhà kho trước khi chạy robot.</summary>
    public async Task RequestMaterialTransferAsync(
        Action<string> reportStatus,
        CancellationToken ct)
    {
        if (!EnsureConnected(out var warehouseCom, out var connectError))
            throw new InvalidOperationException(connectError ?? "Không mở được cổng warehouse.");

        reportStatus($"Gửi {RobotSerialProtocol.WarehouseMaterialRequest}x → {warehouseCom}, chờ {RobotSerialProtocol.WarehouseMaterialReady}x...");

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnFrame(string frame)
        {
            if (IsReadyResponse(frame))
                tcs.TrySetResult();
        }

        _serial.FrameReceived += OnFrame;

        try
        {
            _serial.SendAscii(RobotSerialProtocol.WarehouseMaterialRequest);

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(ReadyResponseTimeout);

            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(Timeout.InfiniteTimeSpan, linked.Token));

            if (completed != tcs.Task)
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException(ct);

                throw new TimeoutException(
                    $"Timeout — không nhận được {RobotSerialProtocol.WarehouseMaterialReady}x từ nhà kho ({warehouseCom}).");
            }

            await tcs.Task;
            reportStatus($"Nhà kho phản hồi {RobotSerialProtocol.WarehouseMaterialReady}x — bắt đầu robot.");
        }
        finally
        {
            _serial.FrameReceived -= OnFrame;
        }
    }

    public bool TrySendCommand(string command, out string warehouseCom, out string? error)
    {
        if (!EnsureConnected(out warehouseCom, out error))
            return false;

        try
        {
            _serial.SendAscii(command);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool EnsureConnected(out string warehouseCom, out string? error)
    {
        error = null;

        var setting = _appSettingService.Load();
        warehouseCom = setting.WarehouseCom?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(warehouseCom))
        {
            error = "Chưa cấu hình warehouseCom trong setting.json.";
            return false;
        }

        try
        {
            lock (_connectGate)
            {
                if (!_serial.IsConnected)
                    _serial.Connect(warehouseCom, setting.BaudRate);
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private void OnFrameReceived(string frame)
    {
        if (IsCaptureRequest(frame))
            CaptureRequested?.Invoke();
    }

    public static bool IsReadyResponse(string text)
        => HasFrame(text, IsCoFrame);

    public static bool IsCaptureRequest(string text)
        => HasFrame(text, IsCapFrame);

    private static bool HasFrame(string text, Func<string, bool> predicate)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        foreach (var segment in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (predicate(segment.Trim()))
                return true;
        }

        return predicate(text.Trim());
    }

    private static bool IsCoFrame(string frame)
    {
        if (frame.Length < 2)
            return false;

        return frame[0] == 'C'
               && char.ToUpperInvariant(frame[1]) == 'O';
    }

    private static bool IsCapFrame(string frame)
    {
        var body = frame.EndsWith(RobotSerialProtocol.FrameTerminator, StringComparison.OrdinalIgnoreCase)
            ? frame[..^1]
            : frame;

        return body.Equals(RobotSerialProtocol.WarehouseCaptureRequest, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _serial.FrameReceived -= OnFrameReceived;
        _serial.Dispose();
    }
}

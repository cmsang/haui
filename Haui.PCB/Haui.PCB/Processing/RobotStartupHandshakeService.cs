namespace Haui.PCB.Processing;

/// <summary>
/// Khởi động app: Rx → Yx → H0x → chờ Dx (homing xong) mới sẵn sàng thao tác.
/// Lặp Rx mỗi 1s nếu chưa nhận Yx.
/// </summary>
public class RobotStartupHandshakeService
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);

    private readonly IRobotSerialService _serialService;
    private CancellationTokenSource? _cts;
    private bool _completed;

    public RobotStartupHandshakeService(IRobotSerialService serialService)
    {
        _serialService = serialService;
    }

    public bool IsRunning { get; private set; }

    /// <summary>Đã nhận Yx và homing H0 hoàn tất (Dx).</summary>
    public bool IsCompleted => _completed;

    public async Task RunAsync(Action<string>? reportStatus = null, CancellationToken ct = default)
    {
        if (_completed || IsRunning)
            return;

        if (!_serialService.IsConnected)
        {
            reportStatus?.Invoke("Chưa kết nối Serial — kiểm tra cổng COM.");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        IsRunning = true;

        var readyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnFrame(string frame)
        {
            if (IsReadySignal(frame))
                readyTcs.TrySetResult();
        }

        _serialService.FrameReceived += OnFrame;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                reportStatus?.Invoke("Đang kết nối robot — chờ phản hồi...");
                _serialService.SendAscii(RobotSerialProtocol.StartupHandshake);

                var delayTask = Task.Delay(RetryInterval, _cts.Token);
                var completed = await Task.WhenAny(readyTcs.Task, delayTask);

                if (completed != readyTcs.Task)
                    continue;

                await readyTcs.Task;
                _serialService.FrameReceived -= OnFrame;

                reportStatus?.Invoke("Robot phản hồi — đang homing (H0x)...");
                try
                {
                    var executor = new RobotPickPlaceExecutor(_serialService);
                    await executor.HomeAllAxesAsync(reportStatus, _cts.Token);
                    _completed = true;
                    reportStatus?.Invoke("Robot sẵn sàng — homing hoàn tất.");
                }
                catch (TimeoutException)
                {
                    reportStatus?.Invoke("Homing quá thời gian — robot chưa sẵn sàng.");
                }

                return;
            }
        }
        catch (OperationCanceledException)
        {
            reportStatus?.Invoke("Kết nối robot bị hủy.");
        }
        finally
        {
            _serialService.FrameReceived -= OnFrame;
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Cancel()
    {
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    public static bool IsReadySignal(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var s = text.Trim();
        return s.Length >= 2
               && s[0] == RobotSerialProtocol.StartupReadyResponse
               && s[^1] == 'x';
    }
}

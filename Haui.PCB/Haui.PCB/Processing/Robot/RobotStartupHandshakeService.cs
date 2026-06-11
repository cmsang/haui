namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Handshake khi mở app: gửi Rx → chờ Yx → gửi H0x. Lặp Rx mỗi 1s nếu chưa nhận Yx.
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

    public bool IsCompleted => _completed;

    public async Task RunAsync(Action<string>? reportStatus = null, CancellationToken ct = default)
    {
        if (_completed || IsRunning)
            return;

        if (!_serialService.IsConnected)
        {
            reportStatus?.Invoke("Handshake — chưa kết nối Serial.");
            return;
        }

        if (_serialService.IsVirtual)
        {
            _completed = true;
            reportStatus?.Invoke("Serial ảo — bỏ qua handshake (Rx/Y/H0).");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        IsRunning = true;

        var readyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnData(string chunk)
        {
            if (IsReadySignal(chunk))
                readyTcs.TrySetResult();
        }

        void OnLine(string line)
        {
            if (IsReadySignal(line))
                readyTcs.TrySetResult();
        }

        _serialService.DataReceived += OnData;
        _serialService.LineReceived += OnLine;

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                reportStatus?.Invoke("Handshake — TX Rx, chờ Yx...");
                _serialService.SendAscii(RobotSerialProtocol.StartupHandshake);

                var delayTask = Task.Delay(RetryInterval, _cts.Token);
                var completed = await Task.WhenAny(readyTcs.Task, delayTask);

                if (completed == readyTcs.Task)
                {
                    await readyTcs.Task;
                    reportStatus?.Invoke("Handshake — nhận Yx, TX H0x...");
                    _serialService.SendHome(0);
                    _completed = true;
                    reportStatus?.Invoke("Handshake hoàn tất — đã gửi H0x (homing tất cả trục).");
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            reportStatus?.Invoke("Handshake bị hủy.");
        }
        finally
        {
            _serialService.DataReceived -= OnData;
            _serialService.LineReceived -= OnLine;
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

        foreach (var segment in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var s = segment.Trim();
            if (s.Length >= 1 && s[0] == RobotSerialProtocol.StartupReadyResponse)
                return true;
        }

        var trimmed = text.Trim();
        return trimmed.Length >= 1 && trimmed[0] == RobotSerialProtocol.StartupReadyResponse;
    }
}

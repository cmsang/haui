
namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Chu trình Pick &amp; Place: G90 → PickUp → G0 → Wait → Destination → G90 → Wait → G0.
/// Mỗi bước chờ phản hồi Dx từ robot (giống Manual Control).
/// </summary>
public class RobotPickPlaceExecutor
{
    private const int GripperOpenAngle = 90;
    private const int GripperCloseAngle = 0;
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(120);

    private readonly IRobotSerialService _serialService;

    public RobotPickPlaceExecutor(IRobotSerialService serialService)
    {
        _serialService = serialService;
    }

    /// <summary>Di chuyển tới một vị trí teach và chờ Dx.</summary>
    public async Task MoveToPointAsync(
        RobotTeachPoint point,
        Action<string>? reportStatus,
        CancellationToken ct)
    {
        reportStatus?.Invoke($"Di chuyển về {point.Name} — gửi lệnh...");
        SendMove(point);
        reportStatus?.Invoke($"Di chuyển về {point.Name} — chờ Dx...");
        await WaitForDoneAsync(StepTimeout, ct);
        reportStatus?.Invoke($"Đã tới {point.Name}.");
    }

    /// <summary>Homing tất cả trục (H0x) và chờ Dx.</summary>
    public async Task HomeAllAxesAsync(Action<string>? reportStatus, CancellationToken ct)
    {
        reportStatus?.Invoke("Homing tất cả trục (H0x) — gửi lệnh...");
        _serialService.SendHome(0);
        reportStatus?.Invoke("Homing tất cả trục — chờ Dx...");
        await WaitForDoneAsync(StepTimeout, ct);
        reportStatus?.Invoke("Homing hoàn tất — nhận Dx.");
    }

    public async Task RunPickUpToDestinationAsync(
        RobotTeachPoint pickUp,
        RobotTeachPoint wait,
        RobotTeachPoint destination,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        await RunStepAsync("1/8 — Mở gripper 90° (G90x)",
            () => SendGripper(GripperOpenAngle), reportStatus, ct);

        await RunStepAsync("2/8 — Move → PickUp",
            () => SendMove(pickUp), reportStatus, ct);

        await RunStepAsync("3/8 — Đóng gripper 0° (G0x)",
            () => SendGripper(GripperCloseAngle), reportStatus, ct);

        await RunStepAsync("4/8 — Move → Wait",
            () => SendMove(wait), reportStatus, ct);

        await RunStepAsync($"5/8 — Move → {destination.Name}",
            () => SendMove(destination), reportStatus, ct);

        await RunStepAsync("6/8 — Mở gripper 90° (G90x)",
            () => SendGripper(GripperOpenAngle), reportStatus, ct);

        await RunStepAsync("7/8 — Move → Wait",
            () => SendMove(wait), reportStatus, ct);

        await RunStepAsync("8/8 — Đóng gripper 0° (G0x)",
            () => SendGripper(GripperCloseAngle), reportStatus, ct);
    }

    public static bool IsDoneSignal(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        foreach (var segment in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var s = segment.Trim();
            if (s.Length >= 2 && s[0] == 'D')
                return true;
        }

        var trimmed = text.Trim();
        return trimmed.Length >= 2 && trimmed[0] == 'D';
    }

    private void SendGripper(int angleDegrees)
    {
        var cmd = RobotSerialProtocol.GripperCommand(angleDegrees);
        _serialService.SendAscii(cmd);
    }

    private void SendMove(RobotTeachPoint point)
    {
        var cmd = RobotSerialProtocol.MoveCommand(
            point.J1, point.J2, point.J3, point.J4, point.J5);
        _serialService.SendAscii(cmd);
    }

    private async Task RunStepAsync(
        string label,
        Action send,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        reportStatus($"{label} — gửi lệnh...");
        send();
        reportStatus($"{label} — chờ Dx...");
        await WaitForDoneAsync(StepTimeout, ct);
        reportStatus($"{label} — nhận Dx, chuyển bước tiếp.");
    }

    private async Task WaitForDoneAsync(TimeSpan timeout, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnLine(string line)
        {
            if (IsDoneSignal(line))
                tcs.TrySetResult();
        }

        void OnData(string chunk)
        {
            if (IsDoneSignal(chunk))
                tcs.TrySetResult();
        }

        _serialService.LineReceived += OnLine;
        _serialService.DataReceived += OnData;

        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(timeout);

            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(Timeout.InfiniteTimeSpan, linked.Token));

            if (completed != tcs.Task)
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException(ct);

                throw new TimeoutException("Timeout — không nhận được Dx từ robot.");
            }

            await tcs.Task;
        }
        finally
        {
            _serialService.LineReceived -= OnLine;
            _serialService.DataReceived -= OnData;
        }
    }
}

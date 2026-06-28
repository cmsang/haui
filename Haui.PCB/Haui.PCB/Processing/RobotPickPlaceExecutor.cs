using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Chu trình Pick &amp; Place 13 bước — không gọi H0x (homing chỉ lúc khởi động app).
/// </summary>
public class RobotPickPlaceExecutor
{
    private const int GripperOpenAngle = 40;
    private const int GripperCloseAngle = 20;
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(120);

    private readonly IRobotSerialService _serialService;

    public RobotPickPlaceExecutor(IRobotSerialService serialService)
    {
        _serialService = serialService;
    }

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

    public async Task HomeAllAxesAsync(Action<string>? reportStatus, CancellationToken ct)
    {
        reportStatus?.Invoke("Homing tất cả trục (H0x) — gửi lệnh...");
        _serialService.SendHome(0);
        reportStatus?.Invoke("Homing tất cả trục — chờ Dx...");
        await WaitForDoneAsync(StepTimeout, ct);
        reportStatus?.Invoke("Homing hoàn tất — nhận Dx.");
    }

    /// <summary>
    /// Mở gripper → Wait PickUp → PickUp → đóng → Wait PickUp → Wait Place
    /// → Place → mở → Wait Place → Wait → đóng gripper.
    /// Điểm Wait chỉ dùng ở cuối: robot về Wait rồi đóng gripper.
    /// </summary>
    public async Task RunPickUpToDestinationAsync(
        RobotTeachPoint pickUp,
        RobotTeachPoint waitPickUp,
        RobotTeachPoint waitPlace,
        RobotTeachPoint destination,
        RobotTeachPoint wait,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        await RunStepAsync("1/11 — Mở gripper (G90x)",
            () => SendGripper(GripperOpenAngle), reportStatus, ct);
        await RunStepAsync($"2/11 — Move → {waitPickUp.Name}",
            () => SendMove(waitPickUp), reportStatus, ct);
        await RunStepAsync("3/11 — Move → PickUp",
            () => SendMove(pickUp), reportStatus, ct);
        await RunStepAsync("4/11 — Đóng gripper (G0x)",
            () => SendGripper(GripperCloseAngle), reportStatus, ct);
        await RunStepAsync($"5/11 — Move → {waitPickUp.Name} (rút lui)",
            () => SendMove(waitPickUp), reportStatus, ct);
        await RunStepAsync($"6/11 — Move → {waitPlace.Name}",
            () => SendMove(waitPlace), reportStatus, ct);
        await RunStepAsync($"7/11 — Move → {destination.Name} (Place)",
            () => SendMove(destination), reportStatus, ct);
        await RunStepAsync("8/11 — Mở gripper (G90x)",
            () => SendGripper(GripperOpenAngle), reportStatus, ct);
        await RunStepAsync($"9/11 — Move → {waitPlace.Name} (rút lui)",
            () => SendMove(waitPlace), reportStatus, ct);
        await RunStepAsync($"10/11 — Move → {wait.Name}",
            () => SendMove(wait), reportStatus, ct);
        await RunStepAsync("11/11 — Đóng gripper (G0x)",
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

        void OnFrame(string frame)
        {
            if (IsDoneSignal(frame))
                tcs.TrySetResult();
        }

        _serialService.FrameReceived += OnFrame;

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
            _serialService.FrameReceived -= OnFrame;
        }
    }
}

using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Chu trình Pick &amp; Place 11 bước — không gọi H0x (homing chỉ lúc khởi động app).
/// Bước 6 và 9 dùng Wait OKx / Wait NGx riêng theo slot đích (tránh va đập).
/// </summary>
public class RobotPickPlaceExecutor
{
    private const int GripperOpenAngle = 15;
    private const int GripperCloseAngle = 4;
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan GripperDoneTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan GripperSettleDelay = TimeSpan.FromMilliseconds(500);

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
        await WaitForDoneAsync(
            StepTimeout,
            ct,
            () => SendMove(point),
            onSent: () => reportStatus?.Invoke($"Di chuyển về {point.Name} — chờ Dx..."));
        reportStatus?.Invoke($"Đã tới {point.Name}.");
    }

    public async Task HomeAllAxesAsync(Action<string>? reportStatus, CancellationToken ct)
    {
        reportStatus?.Invoke("Homing tất cả trục (H0x) — gửi lệnh...");
        await WaitForDoneAsync(
            StepTimeout,
            ct,
            () => _serialService.SendHome(0),
            onSent: () => reportStatus?.Invoke("Homing tất cả trục — chờ Dx..."));
        reportStatus?.Invoke("Homing hoàn tất — nhận Dx.");
    }

    /// <summary>
    /// Mở gripper → Wait PickUp → PickUp → đóng → Pick Done
    /// → Wait OKx/NGx (theo slot) → Place → mở → Wait OKx/NGx → Wait → đóng gripper.
    /// </summary>
    public async Task RunPickUpToDestinationAsync(
        RobotTeachPoint pickUp,
        RobotTeachPoint waitPickUp,
        RobotTeachPoint pickDone,
        RobotTeachPoint waitPlace,
        RobotTeachPoint destination,
        RobotTeachPoint wait,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        await RunGripperStepAsync("1/11 — Mở gripper", GripperOpenAngle, reportStatus, ct);
        await RunMoveStepAsync($"2/11 — Move → {waitPickUp.Name} (từ vị trí hiện tại)",
            waitPickUp, reportStatus, ct);
        await RunMoveStepAsync("3/11 — Move → PickUp",
            pickUp, reportStatus, ct);
        await RunGripperStepAsync("4/11 — Đóng gripper", GripperCloseAngle, reportStatus, ct);
        await RunMoveStepAsync($"5/11 — Move → {pickDone.Name} (sau gắp)",
            pickDone, reportStatus, ct);
        await RunMoveStepAsync($"6/11 — Move → {waitPlace.Name}",
            waitPlace, reportStatus, ct);
        await RunMoveStepAsync($"7/11 — Move → {destination.Name} (Place)",
            destination, reportStatus, ct);
        await RunGripperStepAsync("8/11 — Mở gripper", GripperOpenAngle, reportStatus, ct);
        await RunMoveStepAsync($"9/11 — Move → {waitPlace.Name} (rút lui)",
            waitPlace, reportStatus, ct);
        await RunMoveStepAsync($"10/11 — Move → {wait.Name}",
            wait, reportStatus, ct);
        await RunGripperStepAsync("11/11 — Đóng gripper", GripperCloseAngle, reportStatus, ct);
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

    private async Task RunMoveStepAsync(
        string label,
        RobotTeachPoint point,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        reportStatus($"{label} — gửi lệnh {FormatMove(point)}...");

        try
        {
            await WaitForDoneAsync(
                StepTimeout,
                ct,
                () => SendMove(point),
                onSent: () => reportStatus($"{label} — chờ Dx..."));
        }
        catch (TimeoutException)
        {
            reportStatus($"{label} — timeout, thử gửi lại lệnh...");
            await WaitForDoneAsync(
                StepTimeout,
                ct,
                () => SendMove(point),
                onSent: () => reportStatus($"{label} — chờ Dx (lần 2)..."));
        }

        reportStatus($"{label} — nhận Dx, chuyển bước tiếp.");
    }

    private async Task RunGripperStepAsync(
        string label,
        int angleDegrees,
        Action<string> reportStatus,
        CancellationToken ct)
    {
        reportStatus($"{label} — gửi lệnh G{angleDegrees}x...");
        var gotDx = await TryWaitForDoneAsync(
            GripperDoneTimeout, ct, () => SendGripper(angleDegrees));

        if (!gotDx)
            reportStatus($"{label} — không nhận Dx từ firmware.");

        reportStatus($"{label} — chờ gripper ổn định...");
        await Task.Delay(GripperSettleDelay, ct);
        reportStatus($"{label} — hoàn thành, chuyển bước tiếp.");
    }

    private Task WaitForDoneAsync(TimeSpan timeout, CancellationToken ct, Action send, Action? onSent = null)
        => WaitForDoneCoreAsync(timeout, ct, send, onSent, requireDone: true);

    private async Task<bool> TryWaitForDoneAsync(
        TimeSpan timeout,
        CancellationToken ct,
        Action send,
        Action? onSent = null)
        => await WaitForDoneCoreAsync(timeout, ct, send, onSent, requireDone: false);

    private async Task<bool> WaitForDoneCoreAsync(
        TimeSpan timeout,
        CancellationToken ct,
        Action send,
        Action? onSent,
        bool requireDone)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var armed = false;

        void OnFrame(string frame)
        {
            if (!armed || !IsDoneSignal(frame))
                return;

            tcs.TrySetResult();
        }

        _serialService.FrameReceived += OnFrame;

        try
        {
            send();
            armed = true;
            onSent?.Invoke();

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(timeout);

            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(Timeout.InfiniteTimeSpan, linked.Token));

            if (completed == tcs.Task)
            {
                await tcs.Task;
                return true;
            }

            if (ct.IsCancellationRequested)
                throw new OperationCanceledException(ct);

            if (requireDone)
                throw new TimeoutException("Timeout — không nhận được Dx từ robot.");

            return false;
        }
        finally
        {
            _serialService.FrameReceived -= OnFrame;
        }
    }

    private static string FormatMove(RobotTeachPoint point)
        => RobotSerialProtocol.MoveCommand(point.J1, point.J2, point.J3, point.J4, point.J5);
}

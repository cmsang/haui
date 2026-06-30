namespace Haui.PCB.Processing;

/// <summary>Vị trí logic của cánh tay robot (suy ra từ hành động cuối, không phải đọc trực tiếp firmware).</summary>
public enum RobotArmPosition
{
    /// <summary>Không rõ — vừa jog / move / go to tới vị trí bất kỳ, hoặc chu trình lỗi giữa chừng.</summary>
    Unknown,

    /// <summary>Đã homing (H0x) — ở vị trí gốc.</summary>
    Home,

    /// <summary>Đứng ở điểm Wait sau khi hoàn tất một chu trình Pick &amp; Place.</summary>
    Wait
}

/// <summary>
/// Theo dõi vị trí logic của robot để chặn chu trình Pick &amp; Place khi không an toàn.
/// Chu trình chỉ được bắt đầu khi cánh tay ở <see cref="RobotArmPosition.Home"/> hoặc
/// <see cref="RobotArmPosition.Wait"/> — tránh lao thẳng tới Wait PickUp từ vị trí bất kỳ.
/// </summary>
public class RobotPositionTracker
{
    private readonly object _gate = new();
    private RobotArmPosition _current = RobotArmPosition.Unknown;

    public RobotArmPosition Current
    {
        get { lock (_gate) return _current; }
    }

    /// <summary>Chỉ cho phép bắt đầu chu trình khi ở Home hoặc Wait.</summary>
    public bool CanStartCycle
    {
        get { lock (_gate) return _current is RobotArmPosition.Home or RobotArmPosition.Wait; }
    }

    public void SetHome() => Set(RobotArmPosition.Home);

    public void SetWait() => Set(RobotArmPosition.Wait);

    public void SetUnknown() => Set(RobotArmPosition.Unknown);

    private void Set(RobotArmPosition value)
    {
        lock (_gate) _current = value;
    }

    public static string Describe(RobotArmPosition position) => position switch
    {
        RobotArmPosition.Home => "Home",
        RobotArmPosition.Wait => "Wait",
        _ => "Không xác định"
    };
}

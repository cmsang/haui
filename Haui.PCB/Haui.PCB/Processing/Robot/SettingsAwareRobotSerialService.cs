namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Delegates to real or virtual serial based on current <c>setting.json</c> on each connect/send.
/// Avoids requiring an app restart after toggling Virtual Serial Port.
/// </summary>
public sealed class SettingsAwareRobotSerialService : IRobotSerialService
{
    private readonly IAppSettingService _appSettingService;
    private readonly RobotSerialService _real = new();
    private readonly VirtualRobotSerialService _virtual = new();
    private bool _disposed;

    public SettingsAwareRobotSerialService(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
        _real.DataReceived += ForwardDataReceived;
        _real.LineReceived += ForwardLineReceived;
        _virtual.DataReceived += ForwardDataReceived;
        _virtual.LineReceived += ForwardLineReceived;
    }

    public bool IsVirtual => _appSettingService.Load().UseVirtualSerial();

    public bool IsConnected => _real.IsConnected || _virtual.IsConnected;

    public event Action<string>? DataReceived;

    public event Action<string>? LineReceived;

    public IReadOnlyList<string> GetAvailablePorts()
        => IsVirtual ? _virtual.GetAvailablePorts() : _real.GetAvailablePorts();

    public void Connect(string portName, int baudRate)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SettingsAwareRobotSerialService));

        if (IsVirtual)
        {
            if (_real.IsConnected)
                _real.Disconnect();

            _virtual.Connect(VirtualRobotSerialService.VirtualPortName, baudRate);
            return;
        }

        if (_virtual.IsConnected)
            _virtual.Disconnect();

        _real.Connect(portName, baudRate);
    }

    public void Disconnect()
    {
        _real.Disconnect();
        _virtual.Disconnect();
    }

    public void SendRaw(byte[] packet) => ActiveBackend().SendRaw(packet);

    public void SendAscii(string command) => ActiveBackend().SendAscii(command);

    public void SendJog(string axisName, bool positive, double stepDegrees)
        => ActiveBackend().SendJog(axisName, positive, stepDegrees);

    public void SendHome(byte axis = 0) => ActiveBackend().SendHome(axis);

    public void SendGripperCommand(int angleDegrees)
        => ActiveBackend().SendGripperCommand(angleDegrees);

    public void SendGripperAngle(double angleDegrees)
        => ActiveBackend().SendGripperAngle(angleDegrees);

    public void SendTuningForAllAxes(int speedPercent, int stepsPerDeg,
        ushort acceleration = RobotSerialProtocol.DefaultAcceleration)
        => ActiveBackend().SendTuningForAllAxes(speedPercent, stepsPerDeg, acceleration);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _real.DataReceived -= ForwardDataReceived;
        _real.LineReceived -= ForwardLineReceived;
        _virtual.DataReceived -= ForwardDataReceived;
        _virtual.LineReceived -= ForwardLineReceived;

        _real.Dispose();
        _virtual.Dispose();
    }

    private IRobotSerialService ActiveBackend()
    {
        if (_virtual.IsConnected)
            return _virtual;

        if (_real.IsConnected)
            return _real;

        throw new InvalidOperationException("SerialPort chưa kết nối.");
    }

    private void ForwardDataReceived(string chunk) => DataReceived?.Invoke(chunk);

    private void ForwardLineReceived(string line) => LineReceived?.Invoke(line);
}

using System.Text;

namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Simulates robot firmware serial I/O without opening a physical COM port.
/// Auto-acknowledges move/home/gripper commands with Dx for pick-and-place flows.
/// </summary>
public sealed class VirtualRobotSerialService : IRobotSerialService
{
    private const int AckDelayMs = 80;
    public const string VirtualPortName = "VIRTUAL";

    private bool _connected;
    private bool _disposed;

    public bool IsVirtual => true;

    public bool IsConnected => _connected && !_disposed;

    public event Action<string>? DataReceived;

    public event Action<string>? LineReceived;

    public IReadOnlyList<string> GetAvailablePorts() => [VirtualPortName];

    public void Connect(string portName, int baudRate)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(VirtualRobotSerialService));

        _connected = true;
        EmitTx($"[Virtual] Connect {portName} @ {baudRate}");
    }

    public void Disconnect() => _connected = false;

    public void SendRaw(byte[] packet)
    {
        EnsureConnected();
        var ascii = Encoding.ASCII.GetString(packet);
        EmitTx($"[Virtual] TX raw: {ascii}");
        ScheduleResponsesForPayload(ascii, packet);
    }

    public void SendAscii(string command)
    {
        EnsureConnected();
        EmitTx($"[Virtual] TX: {command}x");
        ScheduleResponsesForCommand(command);
    }

    public void SendJog(string axisName, bool positive, double stepDegrees)
        => SendAscii(RobotSerialProtocol.JogCommand(axisName, positive, stepDegrees));

    public void SendHome(byte axis = 0)
        => SendAscii(RobotSerialProtocol.HomeCommand(axis));

    public void SendGripperCommand(int angleDegrees)
        => SendAscii(RobotSerialProtocol.GripperCommand(angleDegrees));

    public void SendGripperAngle(double angleDegrees)
        => SendRaw(RobotSerialProtocol.BuildGripper(RobotSerialProtocol.GripperAngleToVal(angleDegrees)));

    public void SendTuningForAllAxes(int speedPercent, int stepsPerDeg,
        ushort acceleration = RobotSerialProtocol.DefaultAcceleration)
    {
        EnsureConnected();
        EmitTx($"[Virtual] TX tuning speed={speedPercent}% stepsPerDeg={stepsPerDeg}");
        ScheduleLine("Dx");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connected = false;
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("SerialPort chưa kết nối.");
    }

    private void EmitTx(string message) => DataReceived?.Invoke(message + "\n");

    private void ScheduleResponsesForCommand(string command)
    {
        if (string.Equals(command, RobotSerialProtocol.StartupHandshake, StringComparison.Ordinal))
        {
            ScheduleLine("Y");
            return;
        }

        if (command.Length > 0)
        {
            var op = command[0];
            if (op is 'M' or 'G' or 'J' or 'H')
                ScheduleLine("Dx");
        }
    }

    private void ScheduleResponsesForPayload(string ascii, byte[] packet)
    {
        if (packet.Length > 0 && packet[0] == RobotSerialProtocol.CmdTuning)
            ScheduleLine("Dx");
        else if (packet.Length > 0 && packet[0] == RobotSerialProtocol.CmdGripper)
            ScheduleLine("Dx");
        else if (ascii.Length > 0)
            ScheduleResponsesForCommand(ascii.TrimEnd('x'));
    }

    private void ScheduleLine(string line)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(AckDelayMs).ConfigureAwait(false);
            if (!IsConnected) return;

            var payload = line + "\n";
            DataReceived?.Invoke(payload);
            LineReceived?.Invoke(line);
        });
    }
}

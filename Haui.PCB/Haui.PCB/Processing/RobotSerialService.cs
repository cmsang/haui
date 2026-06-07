using System.IO.Ports;
using System.Text;

namespace Haui.PCB.Processing;

/// <summary>
/// Gửi lệnh M/J/H/S/G tới firmware robot qua SerialPort.
/// </summary>
public class RobotSerialService : IRobotSerialService
{
    private SerialPort? _port;
    private readonly StringBuilder _rxBuffer = new();
    private bool _disposed;

    public bool IsConnected => _port?.IsOpen == true;

    public event Action<string>? LineReceived;

    public IReadOnlyList<string> GetAvailablePorts()
        => SerialPort.GetPortNames().OrderBy(p => p).ToList();

    public void Connect(string portName, int baudRate)
    {
        Disconnect();

        _port = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 500,
            WriteTimeout = 500,
            NewLine = "\n",
            Encoding = Encoding.ASCII
        };
        _port.DataReceived += Port_DataReceived;
        _port.Open();
    }

    public void Disconnect()
    {
        if (_port == null) return;

        try
        {
            if (_port.IsOpen)
            {
                _port.DataReceived -= Port_DataReceived;
                _port.Close();
            }
        }
        catch { /* bỏ qua */ }
        finally
        {
            _port.Dispose();
            _port = null;
        }
    }

    public void SendRaw(byte[] packet)
    {
        if (_port?.IsOpen != true)
            throw new InvalidOperationException("SerialPort chưa kết nối.");

        _port.Write(packet, 0, packet.Length);
    }

    public void SendAscii(string command)
        => SendRaw(Encoding.ASCII.GetBytes(command));

    public void SendJog(string axisName, bool positive, double stepDegrees)
        => SendAscii(RobotSerialProtocol.JogCommand(axisName, positive, stepDegrees));

    public void SendHome(byte axis = 0)
        => SendAscii(RobotSerialProtocol.HomeCommand(axis));

    public void SendGripperAngle(double angleDegrees)
        => SendRaw(RobotSerialProtocol.BuildGripper(RobotSerialProtocol.GripperAngleToVal(angleDegrees)));

    public void SendTuningForAllAxes(int speedPercent, int stepsPerDeg,
        ushort acceleration = RobotSerialProtocol.DefaultAcceleration)
    {
        var velocity = RobotSerialProtocol.SpeedPercentToVelocity(speedPercent, stepsPerDeg);
        for (byte axis = 1; axis <= 5; axis++)
            SendRaw(RobotSerialProtocol.BuildTuning(axis, velocity, acceleration));
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_port?.IsOpen != true) return;

            var chunk = _port.ReadExisting();
            if (string.IsNullOrEmpty(chunk)) return;

            _rxBuffer.Append(chunk);
            FlushLines();
        }
        catch { /* bỏ qua lỗi đọc */ }
    }

    private void FlushLines()
    {
        while (true)
        {
            var text = _rxBuffer.ToString();
            var idx = text.IndexOfAny(['\r', '\n']);
            if (idx < 0) break;

            var line = text[..idx].Trim();
            var skip = idx + 1;
            if (skip < text.Length && text[idx] == '\r' && text[skip] == '\n')
                skip++;

            _rxBuffer.Clear();
            _rxBuffer.Append(text[skip..]);

            if (line.Length > 0)
                LineReceived?.Invoke(line);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
    }
}

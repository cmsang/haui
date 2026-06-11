using System.IO.Ports;
using System.Text;

namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Gửi/nhận lệnh M/J/H/S/G tới firmware robot qua SerialPort.
/// </summary>
public class RobotSerialService : IRobotSerialService
{
    private SerialPort? _port;
    private readonly StringBuilder _rxLineBuffer = new();
    private bool _disposed;

    public bool IsConnected => _port?.IsOpen == true;



    /// <summary>Mỗi lần có byte vào COM — kể cả không có ký tự xuống dòng.</summary>

    public event Action<string>? DataReceived;



    /// <summary>Một dòng text hoàn chỉnh (kết thúc bằng CR/LF).</summary>

    public event Action<string>? LineReceived;

    public IReadOnlyList<string> GetAvailablePorts()
        => SerialPort.GetPortNames().OrderBy(p => p).ToList();

    public void Connect(string portName, int baudRate)
    {
        Disconnect();

        var ports = GetAvailablePorts();
        if (!ports.Contains(portName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Không tìm thấy cổng {portName}. Hiện có: {(ports.Count > 0 ? string.Join(", ", ports) : "(trống)")}");
        }
        _port = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 500,
            WriteTimeout = 500,
            NewLine = "\n",
            Encoding = Encoding.ASCII,
            ReceivedBytesThreshold = 1,
            DtrEnable = true,
            RtsEnable = true
        };
        _port.DataReceived += Port_DataReceived;
        _port.Open();
        _rxLineBuffer.Clear();
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
            _rxLineBuffer.Clear();
        }
    }

    public void SendRaw(byte[] packet)
    {
        if (_port?.IsOpen != true)
            throw new InvalidOperationException("SerialPort chưa kết nối.");

        _port.Write(packet, 0, packet.Length);
    }

    public void SendAscii(string command)
        => SendRaw(Encoding.ASCII.GetBytes(command + "x"));

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
        var velocity = RobotSerialProtocol.SpeedPercentToVelocity(speedPercent, stepsPerDeg);
        for (byte axis = 1; axis <= 5; axis++)
            SendRaw(RobotSerialProtocol.BuildTuning(axis, velocity, acceleration));
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_port?.IsOpen != true) return;

            while (_port.BytesToRead > 0)
            {
                var count = _port.BytesToRead;
                var buf = new byte[count];
                var read = _port.Read(buf, 0, count);
                if (read <= 0) break;
                var chunk = Encoding.ASCII.GetString(buf, 0, read);
                DataReceived?.Invoke(chunk);
                _rxLineBuffer.Append(chunk);

                FlushLines();
            }
        }
        catch (Exception ex)
        {
            DataReceived?.Invoke($"[Lỗi đọc Serial] {ex.Message}");
        }
    }

    private void FlushLines()
    {
        while (true)
        {
            var text = _rxLineBuffer.ToString();

            var idx = text.IndexOfAny(['\r', '\n']);
            if (idx < 0) break;

            var line = text[..idx].Trim();
            var skip = idx + 1;
            if (skip < text.Length && text[idx] == '\r' && text[skip] == '\n')
                skip++;

            _rxLineBuffer.Clear();
            _rxLineBuffer.Append(text[skip..]);

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

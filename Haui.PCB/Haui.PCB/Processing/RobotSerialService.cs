using System.IO;
using System.IO.Ports;
using System.Text;

namespace Haui.PCB.Processing;

/// <summary>
/// Gửi/nhận lệnh M/J/H/S/G tới firmware robot qua SerialPort.
/// RX: đọc frame bằng <see cref="SerialPort.ReadLine"/> (NewLine = "x") — tương đương ReadTo("x").
/// </summary>
public class RobotSerialService : IRobotSerialService
{
    private SerialPort? _port;
    private Thread? _readThread;
    private volatile bool _reading;
    private bool _disposed;

    public bool IsConnected => _port?.IsOpen == true;

    public event Action<string>? FrameReceived;

    public event Action<string>? DataSent;

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
            ReadTimeout = SerialPort.InfiniteTimeout,
            WriteTimeout = 500,
            NewLine = RobotSerialProtocol.FrameTerminator,
            Encoding = Encoding.ASCII,
            DtrEnable = true,
            RtsEnable = true
        };
        _port.Open();

        _reading = true;
        _readThread = new Thread(ReadFramesLoop)
        {
            IsBackground = true,
            Name = "RobotSerialReadToX"
        };
        _readThread.Start();
    }

    public void Disconnect()
    {
        _reading = false;

        if (_port == null)
            return;

        try
        {
            if (_port.IsOpen)
                _port.Close();
        }
        catch { /* bỏ qua */ }
        finally
        {
            try { _readThread?.Join(1000); }
            catch { /* bỏ qua */ }

            _readThread = null;
            _port.Dispose();
            _port = null;
        }
    }

    public void SendRaw(byte[] packet)
    {
        if (_port?.IsOpen != true)
            throw new InvalidOperationException("SerialPort chưa kết nối.");

        _port.Write(packet, 0, packet.Length);
        DataSent?.Invoke(Encoding.ASCII.GetString(packet, 0, packet.Length));
    }

    public void SendAscii(string command)
        => SendRaw(Encoding.ASCII.GetBytes(command + RobotSerialProtocol.FrameTerminator));

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

    /// <summary>Đọc liên tục ReadTo("x") — chờ đủ frame dù STM32 HAL gửi từng byte.</summary>
    private void ReadFramesLoop()
    {
        while (_reading)
        {
            var port = _port;
            if (port is not { IsOpen: true })
                break;

            try
            {
                var body = port.ReadLine();
                if (string.IsNullOrEmpty(body))
                    continue;

                var frame = body + RobotSerialProtocol.FrameTerminator;
                FrameReceived?.Invoke(frame);
            }
            catch (TimeoutException)
            {
                if (!_reading) break;
            }
            catch (IOException)
            {
                break;
            }
            catch (InvalidOperationException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (_reading)
                    FrameReceived?.Invoke($"[Lỗi đọc Serial] {ex.Message}");
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
    }
}

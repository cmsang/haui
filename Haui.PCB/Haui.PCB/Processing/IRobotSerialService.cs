namespace Haui.PCB.Processing;

public interface IRobotSerialService : IDisposable
{
    bool IsConnected { get; }

    IReadOnlyList<string> GetAvailablePorts();

    void Connect(string portName, int baudRate);

    void Disconnect();

    void SendRaw(byte[] packet);

    void SendAscii(string command);

    void SendJog(string axisName, bool positive, double stepDegrees);

    void SendHome(byte axis = 0);

    void SendGripperCommand(int angleDegrees);

    void SendGripperAngle(double angleDegrees);

    void SendTuningForAllAxes(int speedPercent, int stepsPerDeg, ushort acceleration = RobotSerialProtocol.DefaultAcceleration);

    /// <summary>Frame hoàn chỉnh từ robot (đọc ReadTo "x").</summary>
    event Action<string>? FrameReceived;

    /// <summary>Byte/chuỗi thô vừa ghi ra COM.</summary>
    event Action<string>? DataSent;
}

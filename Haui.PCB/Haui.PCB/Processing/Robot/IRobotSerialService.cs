namespace Haui.PCB.Processing.Robot;

public interface IRobotSerialService : IDisposable
{
    /// <summary>True when no physical COM port is used (developer simulation).</summary>
    bool IsVirtual { get; }

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

    /// <summary>Byte/chuỗi thô vừa đọc từ COM (mọi lần có dữ liệu).</summary>
    event Action<string>? DataReceived;

    /// <summary>Một dòng hoàn chỉnh (CR/LF).</summary>
    event Action<string>? LineReceived;
}

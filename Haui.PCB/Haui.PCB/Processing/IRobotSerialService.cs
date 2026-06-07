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

    void SendGripperAngle(double angleDegrees);

    void SendTuningForAllAxes(int speedPercent, int stepsPerDeg, ushort acceleration = RobotSerialProtocol.DefaultAcceleration);

    event Action<string>? LineReceived;
}

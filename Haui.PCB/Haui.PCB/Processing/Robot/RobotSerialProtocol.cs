namespace Haui.PCB.Processing.Robot;

using System.Globalization;
using System.Text;

/// <summary>
/// Mã hóa lệnh Serial theo giao thức firmware robot: M, J, H, S, G.
/// Lệnh H/J/M gửi dạng chuỗi ASCII: "H1", "J1+10", "M1,2,3,4,5", ...
/// </summary>
public static class RobotSerialProtocol
{
    public const byte CmdMove = (byte)'M';     // ASCII: M + J1,J2,J3,J4,J5 (độ)
    public const byte CmdJog = (byte)'J';      // ASCII: J + tên trục + +/- + bước (độ)
    public const byte CmdHome = (byte)'H';     //  2 bytes ASCII: H + axis
    public const byte CmdTuning = (byte)'S';   //  6 bytes: S + Axis ASCII + V + A (uint16 LE)
    public const byte CmdGripper = (byte)'G';  //  2 bytes: G + Val (0–255)

    /// <summary>Warehouse: C1 = tất cả ô OK full, C2 = tất cả ô NG full.</summary>
    public const string WarehouseOkBufferFull = "C1";
    public const string WarehouseNgBufferFull = "C2";

    /// <summary>Khởi động: gửi Rx, robot trả Yx → gửi H0x.</summary>
    public const string StartupHandshake = "R";
    public const char StartupReadyResponse = 'Y';

    public const ushort MaxVelocityStepsPerSec = 40_000;
    public const ushort DefaultAcceleration = 10_000;

    /// <summary>0 → "H0", 1–5 → "H1".."H5".</summary>
    public static string HomeCommand(int axis)
        => axis is >= 1 and <= 5 ? $"H{axis}" : "H0";

    /// <summary>VD: J1+10, J2-1.5, JG+5 (tên trục: 1–5 hoặc G).</summary>
    public static string JogCommand(string axisName, bool positive, double stepDegrees)
    {
        var dir = positive ? "+" : "-";
        var step = stepDegrees.ToString("0.##", CultureInfo.InvariantCulture);
        return $"J{axisName}{dir}{step}";
    }

    /// <summary>VD: M10.0,20.5,-15.0,0.0,45.0</summary>
    public static string MoveCommand(double j1, double j2, double j3, double j4, double j5)
    {
        static string A(double angle) =>
            angle.ToString("0.##", CultureInfo.InvariantCulture);

        return $"M{A(j1)},{A(j2)},{A(j3)},{A(j4)},{A(j5)}";
    }

    /// <summary>VD: G180 → gửi G180x (mở), G0 → G0x (đóng).</summary>
    public static string GripperCommand(int angleDegrees)
        => $"G{angleDegrees}";

    public static byte[] AsciiBytes(string command)
        => Encoding.ASCII.GetBytes(command);

    /// <param name="axisName">Tên trục: "1".."5" hoặc "G".</param>
    public static byte[] BuildJog(string axisName, bool positive, double stepDegrees)
        => AsciiBytes(JogCommand(axisName, positive, stepDegrees));

    /// <param name="axis">0 = tất cả trục, 1–5 = trục cụ thể.</param>
    public static byte[] BuildHome(byte axis)
        => AsciiBytes(HomeCommand(axis));

    private static byte AxisToAscii(byte axis)
        => (byte)(axis is >= 1 and <= 5 ? '0' + axis : '0');

    public static byte[] BuildTuning(byte axis, ushort velocity, ushort acceleration)
    {
        var buf = new byte[6];
        buf[0] = CmdTuning;
        buf[1] = AxisToAscii(axis);
        WriteUInt16Le(buf, 2, velocity);
        WriteUInt16Le(buf, 4, acceleration);
        return buf;
    }

    /// <param name="val">0–255 → PWM 1000–2000 µs.</param>
    public static byte[] BuildGripper(byte val)
        => [(byte)'G', val];

    public static byte GripperAngleToVal(double angleDegrees)
        => (byte)Math.Clamp(Math.Round(angleDegrees / 180.0 * 255.0), 0, 255);

    public static ushort SpeedPercentToVelocity(int speedPercent, int stepsPerDeg)
    {
        var min = (ushort)Math.Max(1, stepsPerDeg);
        var max = MaxVelocityStepsPerSec;
        var pct = Math.Clamp(speedPercent, 1, 100) / 100.0;
        return (ushort)Math.Clamp(Math.Round(min + (max - min) * pct), min, max);
    }

    public static string FormatPacket(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0) return string.Empty;
        var cmd = (char)data[0];
        return cmd switch
        {
            'M' => Encoding.ASCII.GetString(data),
            'J' => Encoding.ASCII.GetString(data),
            'H' when data.Length >= 2 => Encoding.ASCII.GetString(data),
            'S' when data.Length >= 6 =>
                $"S Axis='{(char)data[1]}' V={ReadUInt16Le(data, 2)} A={ReadUInt16Le(data, 4)}",
            'G' when data.Length >= 2 => $"G Val={data[1]}",
            _ => BitConverter.ToString(data.ToArray())
        };
    }

    public static string FormatHex(ReadOnlySpan<byte> data)
        => data.Length == 0 ? string.Empty : BitConverter.ToString(data.ToArray());

    private static void WriteUInt16Le(byte[] buf, int offset, ushort value)
    {
        buf[offset] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static ushort ReadUInt16Le(ReadOnlySpan<byte> buf, int offset)
        => (ushort)(buf[offset] | (buf[offset + 1] << 8));
}

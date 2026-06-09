namespace Haui.PCB.Models;

/// <summary>
/// Một vị trí đã teach — 5 khớp quay RRRRR + góc gripper.
/// </summary>
public class RobotTeachPoint
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Nhóm hiển thị: Chung, OK, NG, Khác.</summary>
    public string Group { get; set; } = string.Empty;

    public double J1 { get; set; }
    public double J2 { get; set; }
    public double J3 { get; set; }
    public double J4 { get; set; }
    public double J5 { get; set; }

    /// <summary>Góc xoay gripper (độ).</summary>
    public double GripperAngle { get; set; }

    public double[] ToJointArray() => [J1, J2, J3, J4, J5, GripperAngle];

    public static RobotTeachPoint FromJointArray(string name, IReadOnlyList<double> angles) => new()
    {
        Name = name,
        J1 = angles.Count > 0 ? angles[0] : 0,
        J2 = angles.Count > 1 ? angles[1] : 0,
        J3 = angles.Count > 2 ? angles[2] : 0,
        J4 = angles.Count > 3 ? angles[3] : 0,
        J5 = angles.Count > 4 ? angles[4] : 0,
        GripperAngle = angles.Count > 5 ? angles[5] : 0
    };
}

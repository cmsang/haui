namespace Haui.PCB.Models.Robot;

public class RobotJointLimits
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double MinAngle { get; set; } = -180;
    public double MaxAngle { get; set; } = 180;
    public bool IsGripper { get; set; }

    public static List<RobotJointLimits> CreateDefault() =>
    [
        new() { Key = "J1", Label = "J1 — Base (Quay đế)", MinAngle = -180, MaxAngle = 180 },
        new() { Key = "J2", Label = "J2 — Shoulder (Vai)", MinAngle = -90, MaxAngle = 90 },
        new() { Key = "J3", Label = "J3 — Elbow (Khuỷu)", MinAngle = -135, MaxAngle = 135 },
        new() { Key = "J4", Label = "J4 — Wrist 1 (Cổ tay 1)", MinAngle = -180, MaxAngle = 180 },
        new() { Key = "J5", Label = "J5 — Wrist 2 (Cổ tay 2)", MinAngle = -180, MaxAngle = 180 }
    ];
}

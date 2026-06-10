namespace BL.PCBDetect.Models;

/// <summary>
/// Vị trí teach robot — dùng trong tầng nghiệp vụ (chỉ J1–J5).
/// </summary>
public class RobotTeachPointInfo
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public double J1 { get; set; }
    public double J2 { get; set; }
    public double J3 { get; set; }
    public double J4 { get; set; }
    public double J5 { get; set; }

    /// <summary>EMPTY hoặc FULL — chỉ áp dụng slot OK/NG.</summary>
    public string FullState { get; set; } = SlotFullState.Empty;
}

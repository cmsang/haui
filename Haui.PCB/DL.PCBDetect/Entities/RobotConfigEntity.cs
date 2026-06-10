namespace DL.PCBDetect.Entities;

/// <summary>
/// Một dòng trong bảng RobotConfig.
/// </summary>
public class RobotConfigEntity
{
    public string PosName { get; set; } = string.Empty;
    public string PosGroup { get; set; } = string.Empty;
    public string J1 { get; set; } = "0";
    public string J2 { get; set; } = "0";
    public string J3 { get; set; } = "0";
    public string J4 { get; set; } = "0";
    public string J5 { get; set; } = "0";
    public string FullState { get; set; } = string.Empty;
    public DateTime UpdateTime { get; set; }
}

namespace Haui.PCB.Models;

/// <summary>
/// Giá trị gợi ý cho điểm Wait — offset J2/J3/J4 so với điểm gốc (dùng khi chưa teach).
/// </summary>
public static class RobotTeachPointOffsets
{
    public const string WaitPickUpName = "Wait PickUp";
    public const string WaitPlaceOkName = "Wait OK";
    public const string WaitPlaceNgName = "Wait NG";

    public const double Joint234OffsetDegrees = -20.0;

    public static RobotTeachPoint CreateWaitPickUp(RobotTeachPoint pickUp)
        => OffsetJoint234(pickUp, WaitPickUpName, Joint234OffsetDegrees);

    public static RobotTeachPoint CreateWaitPlaceOk(RobotTeachPoint ok1Reference)
        => OffsetJoint234(ok1Reference, WaitPlaceOkName, Joint234OffsetDegrees);

    public static RobotTeachPoint CreateWaitPlaceNg(RobotTeachPoint ng1Reference)
        => OffsetJoint234(ng1Reference, WaitPlaceNgName, Joint234OffsetDegrees);

    private static RobotTeachPoint OffsetJoint234(
        RobotTeachPoint source,
        string name,
        double deltaJ234)
        => new()
        {
            Name = name,
            Group = RobotTeachPositions.GetGroup(name),
            J1 = source.J1,
            J2 = source.J2 + deltaJ234,
            J3 = source.J3 + deltaJ234,
            J4 = source.J4,
            J5 = source.J5,
            FullState = "EMPTY"
        };
}

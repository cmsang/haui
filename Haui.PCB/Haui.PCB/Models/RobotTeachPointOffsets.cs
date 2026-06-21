namespace Haui.PCB.Models;

/// <summary>
/// Offset góc cho điểm Wait — J2/J3/J4 so với điểm gốc (PickUp / OK1 / NG1).
/// </summary>
public static class RobotTeachPointOffsets
{
    public const string WaitPickUpName = "Wait PickUp";
    public const string WaitPlaceOkName = "Wait OK";
    public const string WaitPlaceNgName = "Wait NG";
    public const double DefaultJoint234OffsetDegrees = -20.0;

    public static RobotTeachPoint CreateWaitPickUp(
        RobotTeachPoint pickUp,
        double joint234OffsetDegrees = DefaultJoint234OffsetDegrees)
        => OffsetJoint234(pickUp, WaitPickUpName, joint234OffsetDegrees);

    public static RobotTeachPoint CreateWaitPlaceOk(
        RobotTeachPoint ok1Reference,
        double joint234OffsetDegrees = DefaultJoint234OffsetDegrees)
        => OffsetJoint234(ok1Reference, WaitPlaceOkName, joint234OffsetDegrees);

    public static RobotTeachPoint CreateWaitPlaceNg(
        RobotTeachPoint ng1Reference,
        double joint234OffsetDegrees = DefaultJoint234OffsetDegrees)
        => OffsetJoint234(ng1Reference, WaitPlaceNgName, joint234OffsetDegrees);

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
            J4 = source.J4 + deltaJ234,
            J5 = source.J5,
            FullState = "EMPTY"
        };
}

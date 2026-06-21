namespace Haui.PCB.Models;

/// <summary>
/// Danh sách vị trí teach chuẩn của hệ thống Pick &amp; Place PCB.
/// </summary>
public static class RobotTeachPositions
{
    public const string PickUp = "PickUp";
    public const string Wait = "Wait";
    public const string WaitPickUp = RobotTeachPointOffsets.WaitPickUpName;
    public const string WaitPlaceOk = RobotTeachPointOffsets.WaitPlaceOkName;
    public const string WaitPlaceNg = RobotTeachPointOffsets.WaitPlaceNgName;
    public const string Ok1 = "OK1";
    public const string Ng1 = "NG1";

    public static readonly IReadOnlyList<string> StandardNames =
    [
        PickUp, WaitPickUp, Wait, WaitPlaceOk, WaitPlaceNg,
        Ok1, "OK2", "OK3", "OK4",
        Ng1, "NG2", "NG3", "NG4"
    ];

    public static readonly IReadOnlyList<string> OkSlotNames =
        ["OK1", "OK2", "OK3", "OK4"];

    public static readonly IReadOnlyList<string> NgSlotNames =
        ["NG1", "NG2", "NG3", "NG4"];

    public static bool IsStandard(string name)
        => StandardNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static bool IsNgSlot(string name)
        => NgSlotNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static bool IsOkSlot(string name)
        => OkSlotNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static string GetGroup(string name) => name.ToUpperInvariant() switch
    {
        "PICKUP" => "Chung",
        _ when name.Equals(Wait, StringComparison.OrdinalIgnoreCase) => "Chung",
        _ when name.Equals(WaitPickUp, StringComparison.OrdinalIgnoreCase) => "Chung",
        _ when name.Equals(WaitPlaceOk, StringComparison.OrdinalIgnoreCase) => "Chung",
        _ when name.Equals(WaitPlaceNg, StringComparison.OrdinalIgnoreCase) => "Chung",
        _ when name.StartsWith("OK", StringComparison.OrdinalIgnoreCase) => "OK",
        _ when name.StartsWith("NG", StringComparison.OrdinalIgnoreCase) => "NG",
        _ => "Khác"
    };

    public static List<RobotTeachPoint> CreateDefault(
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var points = StandardNames.Select(name => new RobotTeachPoint
        {
            Name = name,
            Group = GetGroup(name)
        }).ToList();

        ApplyDefaultWaitPointsIfUntaught(points, joint234OffsetDegrees);
        return points;
    }

    /// <summary>
    /// Bổ sung vị trí chuẩn còn thiếu và sắp xếp theo thứ tự quy định.
    /// </summary>
    public static List<RobotTeachPoint> Normalize(
        IEnumerable<RobotTeachPoint> saved,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var map = saved.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        var result = new List<RobotTeachPoint>();

        foreach (var name in StandardNames)
        {
            if (map.TryGetValue(name, out var point))
            {
                point.Name = name;
                point.Group = GetGroup(name);
                result.Add(point);
                map.Remove(name);
            }
            else
            {
                result.Add(new RobotTeachPoint { Name = name, Group = GetGroup(name) });
            }
        }

        foreach (var extra in map.Values.OrderBy(p => p.Name))
        {
            if (IsLegacyIgnoredName(extra.Name))
                continue;

            extra.Group = GetGroup(extra.Name);
            result.Add(extra);
        }

        ApplyDefaultWaitPointsIfUntaught(result, joint234OffsetDegrees);
        return result;
    }

    public static bool IsUntaught(RobotTeachPoint point)
        => point.J1 == 0 && point.J2 == 0 && point.J3 == 0 && point.J4 == 0 && point.J5 == 0;

    /// <summary>Gợi ý Wait PickUp / Wait OK/NG khi DB chưa có tọa độ (toàn 0).</summary>
    public static void ApplyDefaultWaitPointsIfUntaught(
        IList<RobotTeachPoint> points,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var pickUp = points.FirstOrDefault(p =>
            p.Name.Equals(PickUp, StringComparison.OrdinalIgnoreCase));
        var ok1 = points.FirstOrDefault(p =>
            p.Name.Equals(Ok1, StringComparison.OrdinalIgnoreCase));
        var ng1 = points.FirstOrDefault(p =>
            p.Name.Equals(Ng1, StringComparison.OrdinalIgnoreCase));

        if (pickUp != null)
            ApplyDefaultIfUntaught(points, RobotTeachPointOffsets.CreateWaitPickUp(pickUp, joint234OffsetDegrees));

        if (ok1 != null)
            ApplyDefaultIfUntaught(points, RobotTeachPointOffsets.CreateWaitPlaceOk(ok1, joint234OffsetDegrees));

        if (ng1 != null)
            ApplyDefaultIfUntaught(points, RobotTeachPointOffsets.CreateWaitPlaceNg(ng1, joint234OffsetDegrees));
    }

    public static RobotTeachPoint? CreateDerivedWaitPoint(
        RobotTeachPoint reference,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        if (reference.Name.Equals(PickUp, StringComparison.OrdinalIgnoreCase))
            return RobotTeachPointOffsets.CreateWaitPickUp(reference, joint234OffsetDegrees);

        if (reference.Name.Equals(Ok1, StringComparison.OrdinalIgnoreCase))
            return RobotTeachPointOffsets.CreateWaitPlaceOk(reference, joint234OffsetDegrees);

        if (reference.Name.Equals(Ng1, StringComparison.OrdinalIgnoreCase))
            return RobotTeachPointOffsets.CreateWaitPlaceNg(reference, joint234OffsetDegrees);

        return null;
    }

    public static bool IsWaitReferencePoint(string name)
        => name.Equals(PickUp, StringComparison.OrdinalIgnoreCase)
           || name.Equals(Ok1, StringComparison.OrdinalIgnoreCase)
           || name.Equals(Ng1, StringComparison.OrdinalIgnoreCase);

    public static RobotTeachPoint? ResolveWaitPickUp(
        IEnumerable<RobotTeachPoint> points,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var list = points as IList<RobotTeachPoint> ?? points.ToList();
        var waitPickUp = list.FirstOrDefault(p =>
            p.Name.Equals(WaitPickUp, StringComparison.OrdinalIgnoreCase));

        if (waitPickUp != null && !IsUntaught(waitPickUp))
            return waitPickUp;

        var pickUp = list.FirstOrDefault(p =>
            p.Name.Equals(PickUp, StringComparison.OrdinalIgnoreCase));

        return pickUp == null ? null : RobotTeachPointOffsets.CreateWaitPickUp(pickUp, joint234OffsetDegrees);
    }

    /// <summary>Wait Place theo nhóm đích: OK → OK1, NG → NG1 (offset J2–J4 từ cài đặt).</summary>
    public static RobotTeachPoint? CreateWaitPlaceForDestination(
        IEnumerable<RobotTeachPoint> points,
        RobotTeachPoint destination,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var list = points as IList<RobotTeachPoint> ?? points.ToList();
        var isNg = IsNgSlot(destination.Name);

        var reference = list.FirstOrDefault(p =>
            p.Name.Equals(isNg ? Ng1 : Ok1, StringComparison.OrdinalIgnoreCase));

        if (reference == null)
            return null;

        return isNg
            ? RobotTeachPointOffsets.CreateWaitPlaceNg(reference, joint234OffsetDegrees)
            : RobotTeachPointOffsets.CreateWaitPlaceOk(reference, joint234OffsetDegrees);
    }

    public static RobotTeachPoint? FindWaitPlaceForDestination(
        IEnumerable<RobotTeachPoint> points,
        RobotTeachPoint destination,
        double joint234OffsetDegrees = RobotTeachPointOffsets.DefaultJoint234OffsetDegrees)
    {
        var list = points as IList<RobotTeachPoint> ?? points.ToList();
        var name = IsNgSlot(destination.Name) ? WaitPlaceNg : WaitPlaceOk;
        var waitPlace = list.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (waitPlace != null && !IsUntaught(waitPlace))
            return waitPlace;

        return CreateWaitPlaceForDestination(list, destination, joint234OffsetDegrees);
    }

    private static bool IsLegacyIgnoredName(string name)
        => name.Equals("Home", StringComparison.OrdinalIgnoreCase)
           || name.Equals("Wait Place", StringComparison.OrdinalIgnoreCase);

    private static void ApplyDefaultIfUntaught(IList<RobotTeachPoint> points, RobotTeachPoint defaultPoint)
    {
        for (var i = 0; i < points.Count; i++)
        {
            if (!points[i].Name.Equals(defaultPoint.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            if (IsUntaught(points[i]))
                points[i] = defaultPoint;
            return;
        }

        points.Add(defaultPoint);
    }
}

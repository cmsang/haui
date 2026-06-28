namespace Haui.PCB.Models;

/// <summary>
/// Danh sách vị trí teach chuẩn của hệ thống Pick &amp; Place PCB.
/// Điểm Wait chung chỉ dùng ở cuối chu trình (robot về Wait rồi đóng gripper).
/// </summary>
public static class RobotTeachPositions
{
    public const string PickUp = "PickUp";
    public const string Wait = "Wait";
    public const string WaitPickUp = "Wait PickUp";
    public const string WaitPlaceOk = "Wait OK";
    public const string WaitPlaceNg = "Wait NG";
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

    public static List<RobotTeachPoint> CreateDefault()
        => StandardNames.Select(name => new RobotTeachPoint
        {
            Name = name,
            Group = GetGroup(name)
        }).ToList();

    /// <summary>
    /// Bổ sung vị trí chuẩn còn thiếu và sắp xếp theo thứ tự quy định.
    /// </summary>
    public static List<RobotTeachPoint> Normalize(IEnumerable<RobotTeachPoint> saved)
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

        return result;
    }

    public static bool IsUntaught(RobotTeachPoint point)
        => point.J1 == 0 && point.J2 == 0 && point.J3 == 0 && point.J4 == 0 && point.J5 == 0;

    /// <summary>Trả về Wait PickUp đã teach; null nếu chưa teach.</summary>
    public static RobotTeachPoint? ResolveWaitPickUp(IEnumerable<RobotTeachPoint> points)
    {
        var waitPickUp = points.FirstOrDefault(p =>
            p.Name.Equals(WaitPickUp, StringComparison.OrdinalIgnoreCase));

        return waitPickUp != null && !IsUntaught(waitPickUp) ? waitPickUp : null;
    }

    /// <summary>Trả về Wait OK / Wait NG (theo nhóm đích) đã teach; null nếu chưa teach.</summary>
    public static RobotTeachPoint? FindWaitPlaceForDestination(
        IEnumerable<RobotTeachPoint> points,
        RobotTeachPoint destination)
    {
        var name = IsNgSlot(destination.Name) ? WaitPlaceNg : WaitPlaceOk;
        var waitPlace = points.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return waitPlace != null && !IsUntaught(waitPlace) ? waitPlace : null;
    }

    private static bool IsLegacyIgnoredName(string name)
        => name.Equals("Home", StringComparison.OrdinalIgnoreCase)
           || name.Equals("Wait Place", StringComparison.OrdinalIgnoreCase);
}

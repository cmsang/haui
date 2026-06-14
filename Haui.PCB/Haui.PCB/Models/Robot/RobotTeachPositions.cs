namespace Haui.PCB.Models.Robot;

/// <summary>
/// Danh sách vị trí teach chuẩn của hệ thống Pick &amp; Place PCB.
/// </summary>
public static class RobotTeachPositions
{
    public const string PickUp = "PickUp";
    public const string Home = "Home";
    public const string Wait = "Wait";

    public static readonly IReadOnlyList<string> StandardNames =
    [
        PickUp, Home, Wait,
        "OK1", "OK2", "OK3", "OK4", "OK5", "OK6",
        "NG1", "NG2", "NG3", "NG4", "NG5", "NG6"
    ];

    public static readonly IReadOnlyList<string> OkSlotNames =
        ["OK1", "OK2", "OK3", "OK4", "OK5", "OK6"];

    public static readonly IReadOnlyList<string> NgSlotNames =
        ["NG1", "NG2", "NG3", "NG4", "NG5", "NG6"];

    public static bool IsStandard(string name)
        => StandardNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    public static string GetGroup(string name) => name.ToUpperInvariant() switch
    {
        "PICKUP" or "HOME" or "WAIT" => "Chung",
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
            extra.Group = GetGroup(extra.Name);
            result.Add(extra);
        }

        return result;
    }
}


namespace Haui.PCB.Processing.Configuration;

/// <summary>Kiểm tra tên vùng mẫu theo <c>setting.json</c> → ComponentTemplates.</summary>
internal static class ComponentTemplateRegionNames
{
    public static HashSet<string> LoadAllowedNames()
        => LoadAllowedNamesInOrder().ToHashSet(StringComparer.Ordinal);

    /// <summary>Thứ tự như trong <c>setting.json</c> → AllowedRegionNames.</summary>
    public static IReadOnlyList<string> LoadAllowedNamesInOrder()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        var names = settings.AllowedRegionNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim());
        if (!names.Any())
            return ComponentTemplateSettings.DefaultAllowedRegionNames;
        return names.Distinct(StringComparer.Ordinal).ToList();
    }

    public static bool IsAllowedName(string name, HashSet<string> allowedNames)
    {
        var trimmed = name.Trim();
        return !string.IsNullOrEmpty(trimmed) && allowedNames.Contains(trimmed);
    }

    public static bool TryGetInvalidNames(
        IEnumerable<string> regionNames,
        HashSet<string> allowedNames,
        out IReadOnlyList<string> invalidNames)
    {
        invalidNames = regionNames
            .Where(n => !IsAllowedName(n, allowedNames))
            .Select(n => string.IsNullOrWhiteSpace(n) ? "(trống)" : n.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return invalidNames.Count == 0;
    }

    public static string FormatAllowedNamesHint(HashSet<string> allowedNames)
        => string.Join(", ", allowedNames.OrderBy(n => n, StringComparer.Ordinal));
}

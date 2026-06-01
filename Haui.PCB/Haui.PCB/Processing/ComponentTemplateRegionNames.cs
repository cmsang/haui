using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>Kiểm tra tên vùng mẫu theo <c>component_template_settings.json</c>.</summary>
internal static class ComponentTemplateRegionNames
{
    public static HashSet<string> LoadAllowedNames()
        => LoadAllowedNamesInOrder().ToHashSet(StringComparer.Ordinal);

    /// <summary>Thứ tự như trong <c>component_template_settings.json</c>.</summary>
    public static IReadOnlyList<string> LoadAllowedNamesInOrder()
    {
        var settings = ComponentTemplateSettingsStore.Load();
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

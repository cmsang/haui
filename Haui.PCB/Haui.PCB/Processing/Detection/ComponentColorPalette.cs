using Haui.PCB.Models.Configuration;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>
/// Stable distinct color per component label for marking boxes on the board image.
/// Uses a curated palette of highly saturated, mutually contrasting colors that pop
/// on a grayscale board (no near-gray, dark, or pastel tones). Each label keeps the
/// same color across frames; the palette cycles only if labels exceed its size.
/// </summary>
public static class ComponentColorPalette
{
    private static readonly Scalar Fallback = new(75, 25, 230); // vivid red (BGR)

    // Vivid, high-contrast colors authored in RGB then stored as BGR Scalars.
    // Ordered so neighbours differ strongly (hue + brightness) for adjacent labels.
    private static readonly Scalar[] Palette =
    [
        FromRgb(230, 25, 75),    // red
        FromRgb(60, 180, 75),    // green
        FromRgb(0, 130, 255),    // blue
        FromRgb(245, 130, 48),   // orange
        FromRgb(240, 50, 230),   // magenta
        FromRgb(60, 210, 240),   // cyan
        FromRgb(255, 215, 20),   // yellow
        FromRgb(150, 40, 200),   // purple
        FromRgb(160, 220, 40),   // lime
        FromRgb(255, 90, 165),   // hot pink
        FromRgb(0, 190, 170),    // teal
        FromRgb(125, 80, 235),   // violet
        FromRgb(0, 210, 120),    // spring green
        FromRgb(0, 165, 255),    // amber
        FromRgb(120, 200, 255),  // sky blue
        FromRgb(210, 100, 0),    // burnt orange
    ];

    private static readonly object SyncRoot = new();
    private static Dictionary<string, Scalar>? _map;

    /// <summary>BGR color for a component label (box + text).</summary>
    public static Scalar GetColor(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Fallback;

        var map = EnsureMap();
        if (map.TryGetValue(label, out var color))
            return color;

        // Unknown label (not in config): pick a stable palette slot from the name hash.
        return Palette[StableHash(label) % Palette.Length];
    }

    /// <summary>Clears the cached map so the next call rebuilds from current settings.</summary>
    public static void Invalidate()
    {
        lock (SyncRoot)
            _map = null;
    }

    private static Dictionary<string, Scalar> EnsureMap()
    {
        lock (SyncRoot)
        {
            if (_map is not null)
                return _map;

            var labels = BuildLabelUniverse(AppSettingsStore.LoadComponentDetection());
            var map = new Dictionary<string, Scalar>(StringComparer.Ordinal);
            for (var i = 0; i < labels.Count; i++)
                map[labels[i]] = Palette[i % Palette.Length];

            _map = map;
            return _map;
        }
    }

    private static List<string> BuildLabelUniverse(ComponentDetectionSettings settings)
    {
        var groupByParent = settings.ComponentGroups
            .Where(g => !string.IsNullOrWhiteSpace(g.Parent) && g.Children.Count > 0)
            .ToDictionary(g => g.Parent.Trim(), g => g.Children, StringComparer.Ordinal);

        var labels = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        void Add(string name)
        {
            var trimmed = name.Trim();
            if (trimmed.Length > 0 && seen.Add(trimmed))
                labels.Add(trimmed);
        }

        foreach (var className in settings.ClassNames)
        {
            if (groupByParent.TryGetValue(className.Trim(), out var children))
            {
                foreach (var child in children)
                    Add(child);
            }
            else
            {
                Add(className);
            }
        }

        // Include any group children whose parent is not listed in ClassNames.
        foreach (var children in groupByParent.Values)
            foreach (var child in children)
                Add(child);

        return labels;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var c in value)
                hash = hash * 31 + c;
            return hash & 0x7FFFFFFF;
        }
    }

    private static Scalar FromRgb(int r, int g, int b) => new(b, g, r);
}

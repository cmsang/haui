using Haui.PCB.Models.Configuration;
using Haui.PCB.Models.Detection;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Detection;

/// <summary>
/// Maps YOLO detections to missing-component inspection results.
/// Parent detections with configured child groups are split into per-child boxes.
/// </summary>
public sealed class MissingComponentDetectionService : IComponentInspectionService
{
    private readonly IOnnxYoloDetector _detector;

    public MissingComponentDetectionService(IOnnxYoloDetector detector)
    {
        _detector = detector;
    }

    public MissingComponentDetectionService()
        : this(new OnnxYoloDetector())
    {
    }

    public ComponentInspectionResult Inspect(Mat board)
    {
        var settings = AppSettingsStore.LoadComponentDetection();
        var detections = _detector.Detect(board);
        var groupLookup = BuildGroupLookup(settings);

        // Each parent label appears at most once per board: keep the highest-confidence detection.
        var parentDetections = detections
            .GroupBy(d => d.Label)
            .Select(g => g.OrderByDescending(d => d.Confidence).First())
            .OrderByDescending(d => d.Confidence);

        var missing = new List<MissingComponent>();
        foreach (var detection in parentDetections)
        {
            if (groupLookup.TryGetValue(detection.Label, out var group))
                missing.AddRange(ExpandGroup(detection, group, settings.DefaultGroupSplit));
            else
                missing.Add(new MissingComponent(detection.Label, detection.Confidence, detection.Box));
        }

        return new ComponentInspectionResult
        {
            Missing = missing,
            ConfThreshold = settings.ConfThreshold
        };
    }

    private static Dictionary<string, ComponentGroup> BuildGroupLookup(ComponentDetectionSettings settings)
    {
        var lookup = new Dictionary<string, ComponentGroup>(StringComparer.Ordinal);
        foreach (var group in settings.ComponentGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Parent) || group.Children.Count == 0)
                continue;

            lookup[group.Parent.Trim()] = group;
        }

        return lookup;
    }

    private static IEnumerable<MissingComponent> ExpandGroup(
        YoloDetection detection,
        ComponentGroup group,
        string defaultSplit)
    {
        var vertical = IsVerticalSplit(group.Split, defaultSplit);
        var cells = SplitBox(detection.Box, group.Children.Count, vertical);

        for (var i = 0; i < group.Children.Count; i++)
        {
            var childLabel = group.Children[i];
            var cell = i < cells.Count ? cells[i] : detection.Box;
            yield return new MissingComponent(childLabel, detection.Confidence, cell);
        }
    }

    private static bool IsVerticalSplit(string? split, string defaultSplit)
    {
        var effective = string.IsNullOrWhiteSpace(split) ? defaultSplit : split.Trim();
        return string.Equals(effective, ComponentGroup.SplitVertical, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<Rect> SplitBox(Rect box, int count, bool vertical)
    {
        if (count <= 1)
            return [box];

        if (box.Width <= 0 || box.Height <= 0)
            return Enumerable.Repeat(box, count).ToList();

        var cells = new List<Rect>(count);
        if (vertical)
        {
            var baseHeight = box.Height / count;
            var remainder = box.Height % count;
            var y = box.Y;
            for (var i = 0; i < count; i++)
            {
                var height = baseHeight + (i < remainder ? 1 : 0);
                cells.Add(new Rect(box.X, y, box.Width, height));
                y += height;
            }
        }
        else
        {
            var baseWidth = box.Width / count;
            var remainder = box.Width % count;
            var x = box.X;
            for (var i = 0; i < count; i++)
            {
                var width = baseWidth + (i < remainder ? 1 : 0);
                cells.Add(new Rect(x, box.Y, width, box.Height));
                x += width;
            }
        }

        return cells;
    }
}

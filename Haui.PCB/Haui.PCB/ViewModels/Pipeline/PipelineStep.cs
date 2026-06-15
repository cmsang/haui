using System.Windows.Media.Imaging;

namespace Haui.PCB.ViewModels.Pipeline;

/// <summary>
/// One segmentation pipeline debug step: label, output image, and elapsed time.
/// </summary>
public sealed class PipelineStep
{
    public string StepName { get; init; } = string.Empty;
    public BitmapSource? Image { get; init; }
    public string Description { get; init; } = string.Empty;
    public double ElapsedMs { get; init; }
    public string ElapsedText { get; init; } = string.Empty;

    public static PipelineStep WithTiming(
        string stepName,
        BitmapSource? image,
        string description,
        TimeSpan elapsed)
    {
        var ms = elapsed.TotalMilliseconds;
        return new PipelineStep
        {
            StepName = stepName,
            Image = image,
            Description = description,
            ElapsedMs = ms,
            ElapsedText = FormatElapsed(ms)
        };
    }

    public static string FormatElapsed(double elapsedMs)
    {
        if (elapsedMs >= 1000)
            return $"{elapsedMs / 1000:F2} s";
        if (elapsedMs >= 100)
            return $"{elapsedMs:F0} ms";
        if (elapsedMs >= 10)
            return $"{elapsedMs:F1} ms";
        return $"{elapsedMs:F2} ms";
    }

    public static string FormatElapsed(TimeSpan elapsed) => FormatElapsed(elapsed.TotalMilliseconds);
}

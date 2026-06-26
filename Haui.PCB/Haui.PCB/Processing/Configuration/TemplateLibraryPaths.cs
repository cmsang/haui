namespace Haui.PCB.Processing.Configuration;

/// <summary>
/// Resolves active template library folder from <see cref="ComponentTemplateSettings"/>.
/// </summary>
public static class TemplateLibraryPaths
{
    public static bool IsWhiteCircuitMode(ComponentTemplateSettings settings)
        => settings.TrainWhiteCircuit;

    public static string ResolveFolder(ComponentTemplateSettings settings, bool whiteCircuit)
    {
        if (whiteCircuit)
        {
            if (!string.IsNullOrWhiteSpace(settings.WhiteCircuitCustomFolder))
                return settings.WhiteCircuitCustomFolder.Trim();
            return ComponentTemplateSettings.DefaultWhiteCircuitLibraryFolder;
        }

        if (!string.IsNullOrWhiteSpace(settings.CustomFolder))
            return settings.CustomFolder.Trim();
        return ComponentTemplateSettings.DefaultLibraryFolder;
    }

    public static string ResolveActiveFolder(ComponentTemplateSettings settings)
        => ResolveFolder(settings, IsWhiteCircuitMode(settings));
}

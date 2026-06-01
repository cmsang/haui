using System.IO;
using System.Text.Json;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Đọc/ghi <c>component_template_settings.json</c> — dùng chung cho thư viện và mẫu active.
/// </summary>
internal static class ComponentTemplateSettingsStore
{
    private const string SettingsFileName = "component_template_settings.json";

    public static ComponentTemplateSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFileName))
                return new ComponentTemplateSettings();
            var json = File.ReadAllText(SettingsFileName);
            return JsonSerializer.Deserialize<ComponentTemplateSettings>(json)
                   ?? new ComponentTemplateSettings();
        }
        catch
        {
            return new ComponentTemplateSettings();
        }
    }

    public static void Save(ComponentTemplateSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFileName, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }
}

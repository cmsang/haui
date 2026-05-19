using System.IO;
using System.Text.Json;
using Haui.GTObjectDetector.Models;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Lưu/tải danh sách vùng mẫu từ file JSON, và lưu/tải ảnh bo mạch mẫu.
/// </summary>
public class TemplateRegionService : ITemplateRegionService
{
    private const string FilePath = "template_regions.json";
    private const string BoardImagePath = "template_board.png";

    public IReadOnlyList<TemplateRegion> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<TemplateRegion>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<TemplateRegion> regions)
    {
        try
        {
            var json = JsonSerializer.Serialize(regions.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public void SaveBoardImage(Mat boardImage)
    {
        try
        {
            Cv2.ImWrite(BoardImagePath, boardImage);
        }
        catch { /* bỏ qua lỗi ghi file */ }
    }

    public Mat? LoadBoardImage()
    {
        try
        {
            if (!File.Exists(BoardImagePath)) return null;
            var mat = Cv2.ImRead(BoardImagePath, ImreadModes.Color);
            return mat.Empty() ? null : mat;
        }
        catch
        {
            return null;
        }
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Một vùng chờ lưu mẫu lỗ tròn trên ảnh Morphology Close.
/// </summary>
public sealed class FiducialHoleRegionItem : INotifyPropertyChanged
{
    public int Index { get; set; }
    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }
    public Color RegionColor { get; set; }

    public string Label => $"Vùng {Index}";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Saved fiducial hole template with recognition score for library list display.
/// </summary>
public sealed class FiducialSavedTemplateItem
{
    public required string FileName { get; init; }
    public int RecognitionCount { get; init; }

    public string DisplayText => RecognitionCount >= 0
        ? $"{FileName} (+{RecognitionCount})"
        : $"{FileName} ({RecognitionCount})";
}

/// <summary>
/// ViewModel tạo thư viện mẫu lỗ tròn — chọn nhiều vùng, lưu một lần.
/// </summary>
public sealed class FiducialTemplateViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IFiducialHoleTemplateService _templateService;
    private Mat? _sourceImage;
    private string _statusText = string.Empty;
    private bool _disposed;

    private static readonly Color[] RegionPalette =
    [
        Color.FromRgb(255, 215, 40),
        Color.FromRgb(80, 160, 255),
        Color.FromRgb(60, 210, 110),
        Color.FromRgb(210, 80, 255),
        Color.FromRgb(255, 140, 40),
        Color.FromRgb(40, 215, 215),
        Color.FromRgb(255, 100, 180),
        Color.FromRgb(160, 230, 60)
    ];

    public event Action<BitmapSource>? ImageReady;
    public event Action? SavedTemplatesChanged;
    public event Action? PendingRegionsChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FiducialHoleRegionItem> PendingRegions { get; } = [];
    public ObservableCollection<FiducialSavedTemplateItem> SavedTemplates { get; } = [];

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public int ImageWidth => _sourceImage?.Width ?? 0;
    public int ImageHeight => _sourceImage?.Height ?? 0;
    public bool HasPendingRegions => PendingRegions.Count > 0;

    public FiducialTemplateViewModel(IFiducialHoleTemplateService templateService)
    {
        _templateService = templateService;
    }

    public void LoadFrame(Mat sourceFrame)
    {
        _sourceImage?.Dispose();
        _sourceImage = sourceFrame.Clone();
        PendingRegions.Clear();
        OnPropertyChanged(nameof(HasPendingRegions));
        PendingRegionsChanged?.Invoke();

        var bitmap = BitmapSourceConverter.ToBitmapSource(_sourceImage);
        bitmap.Freeze();
        ImageReady?.Invoke(bitmap);

        RefreshSavedTemplates();
        StatusText = "Ảnh Morphology Close — kéo thả nhiều vùng quanh lỗ tròn, rồi bấm Lưu mẫu.";
        OnPropertyChanged(nameof(ImageWidth));
        OnPropertyChanged(nameof(ImageHeight));
    }

    public void RefreshSavedTemplates()
    {
        SavedTemplates.Clear();
        var counts = _templateService.GetRecognitionCounts();
        foreach (var fileName in _templateService.ListTemplateFileNames()
                     .OrderByDescending(f => counts.GetValueOrDefault(f, 0))
                     .ThenBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            SavedTemplates.Add(new FiducialSavedTemplateItem
            {
                FileName = fileName,
                RecognitionCount = counts.GetValueOrDefault(fileName, 0)
            });
        }

        SavedTemplatesChanged?.Invoke();
    }

    public void AddRegion(double relX, double relY, double relW, double relH)
    {
        int index = PendingRegions.Count + 1;
        PendingRegions.Add(new FiducialHoleRegionItem
        {
            Index = index,
            RelX = relX,
            RelY = relY,
            RelWidth = relW,
            RelHeight = relH,
            RegionColor = RegionPalette[(index - 1) % RegionPalette.Length]
        });

        OnPropertyChanged(nameof(HasPendingRegions));
        PendingRegionsChanged?.Invoke();
        StatusText = PendingRegions.Count == 1
            ? "Đã chọn 1 vùng — thêm vùng khác hoặc bấm Lưu mẫu."
            : $"Đã chọn {PendingRegions.Count} vùng — tiếp tục kéo thả hoặc bấm Lưu mẫu.";
    }

    public void RemovePendingRegion(FiducialHoleRegionItem item)
    {
        PendingRegions.Remove(item);
        for (int i = 0; i < PendingRegions.Count; i++)
        {
            PendingRegions[i].Index = i + 1;
            PendingRegions[i].RegionColor = RegionPalette[i % RegionPalette.Length];
            PendingRegions[i].NotifyChanged(nameof(FiducialHoleRegionItem.Label));
        }

        OnPropertyChanged(nameof(HasPendingRegions));
        PendingRegionsChanged?.Invoke();
        StatusText = PendingRegions.Count > 0
            ? $"Còn {PendingRegions.Count} vùng chờ lưu."
            : "Kéo thả vùng quanh lỗ tròn để thêm mẫu.";
    }

    public void ClearPendingRegions()
    {
        PendingRegions.Clear();
        OnPropertyChanged(nameof(HasPendingRegions));
        PendingRegionsChanged?.Invoke();
        StatusText = "Đã xóa tất cả vùng chờ lưu.";
    }

    public void SavePendingTemplates()
    {
        if (_sourceImage is null || _sourceImage.Empty())
        {
            StatusText = "Chưa có ảnh.";
            return;
        }

        if (PendingRegions.Count == 0)
        {
            StatusText = "Chọn ít nhất một vùng trước khi lưu.";
            return;
        }

        var savedNames = new List<string>();
        var patches = new List<Mat>();

        try
        {
            foreach (var region in PendingRegions)
            {
                var patch = CropRegion(region);
                if (patch is null || patch.Empty())
                    continue;
                patches.Add(patch);
            }

            if (patches.Count == 0)
            {
                StatusText = "Không có vùng hợp lệ để lưu.";
                return;
            }

            foreach (var patch in patches)
                savedNames.Add(_templateService.SaveTemplate(patch));

            PendingRegions.Clear();
            OnPropertyChanged(nameof(HasPendingRegions));
            PendingRegionsChanged?.Invoke();
            RefreshSavedTemplates();

            StatusText = savedNames.Count == 1
                ? $"Đã lưu 1 mẫu \"{savedNames[0]}\" ({SavedTemplates.Count} mẫu trong thư mục)."
                : $"Đã lưu {savedNames.Count} mẫu ({SavedTemplates.Count} mẫu trong thư mục).";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi lưu mẫu: {ex.Message}";
        }
        finally
        {
            foreach (var patch in patches)
                patch.Dispose();
        }
    }

    public void DeleteSavedTemplate(string fileName)
    {
        try
        {
            _templateService.DeleteTemplate(fileName);
            RefreshSavedTemplates();
            StatusText = SavedTemplates.Count > 0
                ? $"Đã xóa \"{fileName}\" — còn {SavedTemplates.Count} mẫu."
                : $"Đã xóa \"{fileName}\" — thư mục trống.";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi xóa mẫu: {ex.Message}";
        }
    }

    private Mat? CropRegion(FiducialHoleRegionItem region)
    {
        if (_sourceImage is null) return null;

        int x = (int)(region.RelX * _sourceImage.Width);
        int y = (int)(region.RelY * _sourceImage.Height);
        int w = Math.Max(1, (int)(region.RelWidth * _sourceImage.Width));
        int h = Math.Max(1, (int)(region.RelHeight * _sourceImage.Height));

        x = Math.Clamp(x, 0, _sourceImage.Width - 1);
        y = Math.Clamp(y, 0, _sourceImage.Height - 1);
        w = Math.Min(w, _sourceImage.Width - x);
        h = Math.Min(h, _sourceImage.Height - y);

        using var roi = new Mat(_sourceImage, new OpenCvSharp.Rect(x, y, w, h));
        if (roi.Channels() == 1)
            return roi.Clone();

        var gray = new Mat();
        Cv2.CvtColor(roi, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        if (_disposed) return;
        _sourceImage?.Dispose();
        _disposed = true;
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Má»™t vÃ¹ng chá» lÆ°u máº«u lá»— trÃ²n trÃªn áº£nh Morphology Close.
/// </summary>
public sealed class FiducialHoleRegionItem : INotifyPropertyChanged
{
    public int Index { get; set; }
    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }
    public Color RegionColor { get; set; }

    public string Label => $"VÃ¹ng {Index}";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// ViewModel táº¡o thÆ° viá»‡n máº«u lá»— trÃ²n â€” chá»n nhiá»u vÃ¹ng, lÆ°u má»™t láº§n.
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
    public ObservableCollection<string> SavedTemplateFiles { get; } = [];

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
        StatusText = "áº¢nh Morphology Close â€” kÃ©o tháº£ nhiá»u vÃ¹ng quanh lá»— trÃ²n, rá»“i báº¥m LÆ°u máº«u.";
        OnPropertyChanged(nameof(ImageWidth));
        OnPropertyChanged(nameof(ImageHeight));
    }

    public void RefreshSavedTemplates()
    {
        SavedTemplateFiles.Clear();
        foreach (var fileName in _templateService.ListTemplateFileNames())
            SavedTemplateFiles.Add(fileName);
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
            ? "ÄÃ£ chá»n 1 vÃ¹ng â€” thÃªm vÃ¹ng khÃ¡c hoáº·c báº¥m LÆ°u máº«u."
            : $"ÄÃ£ chá»n {PendingRegions.Count} vÃ¹ng â€” tiáº¿p tá»¥c kÃ©o tháº£ hoáº·c báº¥m LÆ°u máº«u.";
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
            ? $"CÃ²n {PendingRegions.Count} vÃ¹ng chá» lÆ°u."
            : "KÃ©o tháº£ vÃ¹ng quanh lá»— trÃ²n Ä‘á»ƒ thÃªm máº«u.";
    }

    public void ClearPendingRegions()
    {
        PendingRegions.Clear();
        OnPropertyChanged(nameof(HasPendingRegions));
        PendingRegionsChanged?.Invoke();
        StatusText = "ÄÃ£ xÃ³a táº¥t cáº£ vÃ¹ng chá» lÆ°u.";
    }

    public void SavePendingTemplates()
    {
        if (_sourceImage is null || _sourceImage.Empty())
        {
            StatusText = "ChÆ°a cÃ³ áº£nh.";
            return;
        }

        if (PendingRegions.Count == 0)
        {
            StatusText = "Chá»n Ã­t nháº¥t má»™t vÃ¹ng trÆ°á»›c khi lÆ°u.";
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
                StatusText = "KhÃ´ng cÃ³ vÃ¹ng há»£p lá»‡ Ä‘á»ƒ lÆ°u.";
                return;
            }

            foreach (var patch in patches)
                savedNames.Add(_templateService.SaveTemplate(patch));

            PendingRegions.Clear();
            OnPropertyChanged(nameof(HasPendingRegions));
            PendingRegionsChanged?.Invoke();
            RefreshSavedTemplates();

            StatusText = savedNames.Count == 1
                ? $"ÄÃ£ lÆ°u 1 máº«u \"{savedNames[0]}\" ({SavedTemplateFiles.Count} máº«u trong thÆ° má»¥c)."
                : $"ÄÃ£ lÆ°u {savedNames.Count} máº«u ({SavedTemplateFiles.Count} máº«u trong thÆ° má»¥c).";
        }
        catch (Exception ex)
        {
            StatusText = $"Lá»—i lÆ°u máº«u: {ex.Message}";
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
            StatusText = SavedTemplateFiles.Count > 0
                ? $"ÄÃ£ xÃ³a \"{fileName}\" â€” cÃ²n {SavedTemplateFiles.Count} máº«u."
                : $"ÄÃ£ xÃ³a \"{fileName}\" â€” thÆ° má»¥c trá»‘ng.";
        }
        catch (Exception ex)
        {
            StatusText = $"Lá»—i xÃ³a máº«u: {ex.Message}";
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

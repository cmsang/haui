using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Äáº¡i diá»‡n má»™t vÃ¹ng máº«u trong danh sÃ¡ch â€” há»— trá»£ sá»­a tÃªn trá»±c tiáº¿p.
/// </summary>
public class TemplateRegionItem : INotifyPropertyChanged
{
    private static readonly SolidColorBrush ValidNameBrush = Brushes.Black;
    private static readonly SolidColorBrush InvalidNameBrush = Brushes.Red;

    private string _name = string.Empty;
    private int _stt;

    /// <summary>GÃ¡n tá»« <see cref="CreateTemplateViewModel"/> Ä‘á»ƒ tÃ´ mÃ u tÃªn há»£p lá»‡ / khÃ´ng há»£p lá»‡.</summary>
    public Func<string, bool>? NameValidator { get; set; }

    public int Stt
    {
        get => _stt;
        set { _stt = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNameValid));
            OnPropertyChanged(nameof(NameForeground));
        }
    }

    public bool IsNameValid => NameValidator?.Invoke(_name) ?? false;

    public Brush NameForeground => IsNameValid ? ValidNameBrush : InvalidNameBrush;

    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }

    /// <summary>MÃ u hiá»ƒn thá»‹ cá»§a vÃ¹ng nÃ y trÃªn canvas vÃ  trong grid.</summary>
    public Color RegionColor { get; set; } = Colors.LimeGreen;

    /// <summary>Brush tá»« RegionColor Ä‘á»ƒ bind trong XAML.</summary>
    public SolidColorBrush RegionBrush => new(RegionColor);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshNameValidation()
    {
        OnPropertyChanged(nameof(IsNameValid));
        OnPropertyChanged(nameof(NameForeground));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public TemplateRegion ToModel() => new()
    {
        Name = Name,
        RelX = RelX,
        RelY = RelY,
        RelWidth = RelWidth,
        RelHeight = RelHeight
    };

    public static TemplateRegionItem FromModel(TemplateRegion m, Color color) => new()
    {
        Name = m.Name,
        RelX = m.RelX,
        RelY = m.RelY,
        RelWidth = m.RelWidth,
        RelHeight = m.RelHeight,
        RegionColor = color
    };
}

/// <summary>
/// ViewModel cho CreateTemplateWindow â€” xá»­ lÃ½ logic cáº¯t bo máº¡ch,
/// quáº£n lÃ½ danh sÃ¡ch vÃ¹ng, lÆ°u vÃ  táº£i vÃ¹ng máº«u.
/// </summary>
public class CreateTemplateViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentationService;
    private readonly ITemplateLibraryService _libraryService;
    private Mat? _boardImage;
    private BitmapSource? _boardBitmap;
    private string _statusText = string.Empty;
    private readonly HashSet<string> _allowedRegionNames;
    private readonly string _allowedNamesHint;
    private bool _disposed;

    // â”€â”€â”€â”€ Events â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>PhÃ¡t khi áº£nh bo máº¡ch sáºµn sÃ ng Ä‘á»ƒ hiá»ƒn thá»‹.</summary>
    public event Action<BitmapSource>? BoardImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // â”€â”€â”€â”€ Properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public ObservableCollection<TemplateRegionItem> Regions { get; } = [];

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    /// <summary>KÃ­ch thÆ°á»›c áº£nh bo máº¡ch (Ä‘á»ƒ View tÃ­nh tá»‰ lá»‡ vÃ¹ng chá»n).</summary>
    public int BoardWidth => _boardImage?.Width ?? 0;
    public int BoardHeight => _boardImage?.Height ?? 0;

    public int RegionCount => Regions.Count;

    /// <summary>Chá»‰ cho lÆ°u khi cÃ³ áº£nh bo máº¡ch, Ã­t nháº¥t má»™t vÃ¹ng vÃ  má»i tÃªn náº±m trong danh sÃ¡ch cáº¥u hÃ¬nh.</summary>
    public bool CanSave =>
        _boardImage is not null
        && Regions.Count > 0
        && ComponentTemplateRegionNames.TryGetInvalidNames(
            Regions.Select(r => r.Name), _allowedRegionNames, out _);

    /// <summary>Cho phÃ©p xoay khi Ä‘Ã£ cÃ³ áº£nh bo máº¡ch.</summary>
    public bool CanRotateBoard => _boardImage is not null && !_boardImage.Empty();

    /// <summary>Hiá»ƒn thá»‹ tiáº¿n Ä‘á»™ Ä‘Ã¡nh dáº¥u vÃ¹ng trÃªn UI.</summary>
    public string RegionProgressText =>
        CanSave
            ? $"VÃ¹ng linh kiá»‡n: {Regions.Count} â€” cÃ³ thá»ƒ lÆ°u."
            : $"VÃ¹ng linh kiá»‡n: {Regions.Count} â€” tÃªn há»£p lá»‡: {_allowedNamesHint}";

    // â”€â”€â”€â”€ Báº£ng 50 mÃ u phÃ¢n biá»‡t â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    internal static readonly Color[] RegionPalette =
    [
        Color.FromRgb(255,  80,  80),  //  1 Ä‘á»
        Color.FromRgb( 80, 160, 255),  //  2 xanh dÆ°Æ¡ng
        Color.FromRgb(255, 215,  40),  //  3 vÃ ng
        Color.FromRgb( 60, 210, 110),  //  4 xanh lÃ¡
        Color.FromRgb(210,  80, 255),  //  5 tÃ­m
        Color.FromRgb(255, 140,  40),  //  6 cam
        Color.FromRgb( 40, 215, 215),  //  7 cyan
        Color.FromRgb(255, 100, 180),  //  8 há»“ng
        Color.FromRgb(160, 230,  60),  //  9 xanh lÃ¡ nÃµn
        Color.FromRgb(255, 180, 100),  // 10 cam nháº¡t
        Color.FromRgb(100, 100, 255),  // 11 xanh má»±c
        Color.FromRgb(255,  50, 150),  // 12 há»“ng Ä‘áº­m
        Color.FromRgb( 50, 200, 170),  // 13 tÃ­m xanh
        Color.FromRgb(200, 160,  40),  // 14 vÃ ng Ä‘á»“ng
        Color.FromRgb(255, 120, 120),  // 15 Ä‘á» há»“ng
        Color.FromRgb( 40, 180, 255),  // 16 xanh trá»i
        Color.FromRgb(180, 255,  80),  // 17 vÃ ng xanh
        Color.FromRgb(255,  80, 200),  // 18 tÃ­m há»“ng
        Color.FromRgb( 80, 255, 160),  // 19 xanh báº¡c hÃ 
        Color.FromRgb(255, 200,  60),  // 20 vÃ ng sÃ¡ng
        Color.FromRgb(200,  80,  80),  // 21 Ä‘á» Ä‘áº­m
        Color.FromRgb( 60, 120, 220),  // 22 xanh navy nháº¡t
        Color.FromRgb(220, 200,  50),  // 23 vÃ ng Ã´ liu
        Color.FromRgb( 80, 220,  60),  // 24 xanh cá»
        Color.FromRgb(180,  60, 220),  // 25 tÃ­m há»“ng
        Color.FromRgb(255, 160,  60),  // 26 cam vÃ ng
        Color.FromRgb( 60, 220, 220),  // 27 ngá»c lam
        Color.FromRgb(220, 100, 160),  // 28 há»“ng Ä‘á»
        Color.FromRgb(140, 220,  80),  // 29 xanh lÃ¡ sÃ¡ng
        Color.FromRgb(255, 140, 180),  // 30 há»“ng nháº¡t
        Color.FromRgb( 80, 200,  80),  // 31 xanh lÃ¡ vá»«a
        Color.FromRgb(200, 120, 255),  // 32 tÃ­m nháº¡t
        Color.FromRgb(255, 220, 100),  // 33 vÃ ng nháº¡t
        Color.FromRgb( 80, 140, 200),  // 34 xanh xÃ¡m
        Color.FromRgb(240,  80,  40),  // 35 Ä‘á» cam
        Color.FromRgb(100, 240, 200),  // 36 xanh lÃ¡ biá»ƒn
        Color.FromRgb(240, 160, 240),  // 37 há»“ng tÃ­m nháº¡t
        Color.FromRgb(200, 240,  80),  // 38 vÃ ng xanh nháº¡t
        Color.FromRgb(255,  80, 100),  // 39 Ä‘á» há»“ng Ä‘áº­m
        Color.FromRgb( 60, 200, 240),  // 40 xanh sÃ¡ng
        Color.FromRgb(255, 180, 200),  // 41 há»“ng pháº¥n
        Color.FromRgb(160, 255, 120),  // 42 xanh lÃ¡ nÃµn nháº¡t
        Color.FromRgb(255, 120,  60),  // 43 cam Ä‘áº­m
        Color.FromRgb(120, 120, 240),  // 44 Ä‘á» tÃ­m
        Color.FromRgb( 80, 240, 140),  // 45 báº¡c hÃ  sÃ¡ng
        Color.FromRgb(240, 200,  80),  // 46 vÃ ng kim
        Color.FromRgb(200,  60, 100),  // 47 Ä‘á» rÆ°á»£u
        Color.FromRgb( 60, 180, 160),  // 48 xanh rÃªu
        Color.FromRgb(240, 120, 200),  // 49 há»“ng neon
        Color.FromRgb(180, 240, 180),  // 50 xanh lÃ¡ pastel
    ];

    /// <summary>
    /// Chá»n mÃ u tiáº¿p theo chÆ°a Ä‘Æ°á»£c dÃ¹ng bá»Ÿi báº¥t ká»³ vÃ¹ng nÃ o Ä‘ang cÃ³ trong danh sÃ¡ch.
    /// </summary>
    internal Color GetNextColor()
    {
        var usedColors = Regions.Select(r => r.RegionColor).ToHashSet();
        foreach (var color in RegionPalette)
            if (!usedColors.Contains(color))
                return color;

        // Táº¥t cáº£ 50 mÃ u Ä‘Ã£ dÃ¹ng háº¿t â€” quay vÃ²ng theo index
        return RegionPalette[Regions.Count % RegionPalette.Length];
    }

    // â”€â”€â”€â”€ Khá»Ÿi táº¡o â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public CreateTemplateViewModel(
        IPcbSegmentationService segmentationService,
        ITemplateLibraryService libraryService)
    {
        _segmentationService = segmentationService;
        _libraryService = libraryService;
        _allowedRegionNames = ComponentTemplateRegionNames.LoadAllowedNames();
        _allowedNamesHint = ComponentTemplateRegionNames.FormatAllowedNamesHint(_allowedRegionNames);

        Regions.CollectionChanged += OnRegionsCollectionChanged;
    }

    private void OnRegionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (TemplateRegionItem item in e.OldItems)
                item.PropertyChanged -= OnRegionItemPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (TemplateRegionItem item in e.NewItems)
            {
                AttachRegionItem(item);
                item.PropertyChanged += OnRegionItemPropertyChanged;
            }
        }

        NotifyRegionValidationChanged();
    }

    private void AttachRegionItem(TemplateRegionItem item)
    {
        item.NameValidator = name =>
            ComponentTemplateRegionNames.IsAllowedName(name, _allowedRegionNames);
        item.RefreshNameValidation();
    }

    private void OnRegionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TemplateRegionItem.Name))
            NotifyRegionValidationChanged();
    }

    private void NotifyRegionValidationChanged()
    {
        OnPropertyChanged(nameof(RegionCount));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(RegionProgressText));
    }

    private void NotifyBoardChanged()
    {
        OnPropertyChanged(nameof(BoardWidth));
        OnPropertyChanged(nameof(BoardHeight));
        OnPropertyChanged(nameof(CanRotateBoard));
    }

    // â”€â”€â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Náº¡p áº£nh vÃ  danh sÃ¡ch vÃ¹ng cá»§a má»™t máº«u cÃ³ sáºµn Ä‘á»ƒ chá»‰nh sá»­a.
    /// </summary>
    public void LoadExistingTemplate(Mat boardImage, IEnumerable<TemplateRegion> existingRegions)
    {
        _boardImage?.Dispose();
        _boardImage = boardImage.Clone();

        var bitmap = BitmapSourceConverter.ToBitmapSource(_boardImage);
        bitmap.Freeze();
        _boardBitmap = bitmap;
        BoardImageReady?.Invoke(bitmap);

        Regions.Clear();
        int idx = 0;
        foreach (var r in existingRegions)
        {
            var item = TemplateRegionItem.FromModel(r, RegionPalette[idx % RegionPalette.Length]);
            item.Stt = idx + 1;
            Regions.Add(item);
            idx++;
        }

        StatusText = CanSave
            ? $"Cháº¿ Ä‘á»™ chá»‰nh sá»­a â€” {Regions.Count} vÃ¹ng, cÃ³ thá»ƒ lÆ°u."
            : $"Cháº¿ Ä‘á»™ chá»‰nh sá»­a â€” {Regions.Count} vÃ¹ng (Ä‘áº·t tÃªn theo danh sÃ¡ch cáº¥u hÃ¬nh Ä‘á»ƒ lÆ°u).";
        NotifyRegionValidationChanged();
        NotifyBoardChanged();
    }

    /// <summary>
    /// Tráº£ vá» danh sÃ¡ch vÃ¹ng hiá»‡n táº¡i dÆ°á»›i dáº¡ng model (dÃ¹ng khi cáº­p nháº­t máº«u tá»« bÃªn ngoÃ i).
    /// </summary>
    public List<TemplateRegion> GetCurrentRegions()
        => Regions.Select(r => r.ToModel()).ToList();

    /// <summary>Náº¡p áº£nh chá»¥p tá»« camera, cáº¯t bo máº¡ch, táº£i vÃ¹ng Ä‘Ã£ lÆ°u.</summary>
    public async Task LoadFrameAsync(Mat sourceFrame)
    {
        StatusText = "Äang cáº¯t bo máº¡ch...";
        using var source = sourceFrame.Clone();

        var segmented = await Task.Run(() => _segmentationService.Segment(source));

        _boardImage?.Dispose();
        _boardImage = segmented ?? source.Clone();

        var bitmap = BitmapSourceConverter.ToBitmapSource(_boardImage);
        bitmap.Freeze();
        _boardBitmap = bitmap;
        BoardImageReady?.Invoke(bitmap);

        Regions.Clear();

        StatusText =
            $"KÃ©o tháº£ trÃªn áº£nh Ä‘á»ƒ Ä‘Ã¡nh dáº¥u vÃ¹ng linh kiá»‡n. TÃªn há»£p lá»‡: {_allowedNamesHint}.";
        NotifyRegionValidationChanged();
        NotifyBoardChanged();
    }

    /// <summary>Xoay áº£nh bo máº¡ch 180Â° vÃ  cáº­p nháº­t tá»a Ä‘á»™ vÃ¹ng tÆ°Æ¡ng á»©ng.</summary>
    public void RotateBoard180()
    {
        if (_boardImage is null || _boardImage.Empty())
            return;

        var rotated = new Mat();
        Cv2.Rotate(_boardImage, rotated, RotateFlags.Rotate180);
        _boardImage.Dispose();
        _boardImage = rotated;

        foreach (var region in Regions)
        {
            region.RelX = 1.0 - region.RelX - region.RelWidth;
            region.RelY = 1.0 - region.RelY - region.RelHeight;
        }

        var bitmap = BitmapSourceConverter.ToBitmapSource(_boardImage);
        bitmap.Freeze();
        _boardBitmap = bitmap;
        BoardImageReady?.Invoke(bitmap);

        StatusText = RegionValidationStatusSuffix("ÄÃ£ xoay áº£nh 180Â°.");
    }

    /// <summary>
    /// ThÃªm má»™t vÃ¹ng má»›i (tá»a Ä‘á»™ tÆ°Æ¡ng Ä‘á»‘i so vá»›i áº£nh bo máº¡ch).
    /// </summary>
    public void AddRegion(double relX, double relY, double relW, double relH)
    {
        var item = new TemplateRegionItem
        {
            Stt = Regions.Count + 1,
            Name = $"VÃ¹ng {Regions.Count + 1}",
            RelX = relX,
            RelY = relY,
            RelWidth = relW,
            RelHeight = relH,
            RegionColor = GetNextColor()
        };
        Regions.Add(item);
        StatusText = RegionValidationStatusSuffix($"ÄÃ£ thÃªm \"{item.Name}\".");
    }

    /// <summary>XÃ³a vÃ¹ng Ä‘Æ°á»£c chá»n.</summary>
    public void RemoveRegion(TemplateRegionItem item)
    {
        Regions.Remove(item);
        // Cáº­p nháº­t láº¡i sá»‘ thá»© tá»±
        for (int i = 0; i < Regions.Count; i++)
            Regions[i].Stt = i + 1;
        StatusText = RegionValidationStatusSuffix($"ÄÃ£ xÃ³a \"{item.Name}\".");
    }

    private string RegionValidationStatusSuffix(string action)
    {
        if (CanSave)
            return $"{action} CÃ³ thá»ƒ lÆ°u ({Regions.Count} vÃ¹ng).";
        if (Regions.Count == 0)
            return $"{action} ChÆ°a cÃ³ vÃ¹ng nÃ o.";
        return $"{action} ({Regions.Count} vÃ¹ng â€” kiá»ƒm tra tÃªn theo cáº¥u hÃ¬nh).";
    }

    /// <summary>Kiá»ƒm tra tÃªn vÃ¹ng trÆ°á»›c khi lÆ°u.</summary>
    public bool TrySaveRegions(out string? errorMessage)
    {
        if (_boardImage is null)
        {
            errorMessage = "ChÆ°a cÃ³ áº£nh bo máº¡ch.";
            return false;
        }

        if (!TryValidateRegionNames(Regions.Select(r => r.Name), out errorMessage))
            return false;

        SaveRegionsCore();
        errorMessage = null;
        return true;
    }

    private void SaveRegionsCore()
    {
        if (_boardImage is null)
            return;

        var templateName = $"{DateTime.Now:dd/MM/yyyy HH:mm}";
        var imagePath = _libraryService.SaveBoardImage(templateName, _boardImage);
        var regionsPath = _libraryService.GetRegionsFilePathForBoardImage(imagePath);
        var regionModels = Regions.Select(r => r.ToModel()).ToList();
        _libraryService.SaveRegions(regionsPath, templateName, regionModels);

        var existing = _libraryService.LoadAll().ToList();
        existing.Add(new TemplateEntry
        {
            Name = templateName,
            BoardImagePath = imagePath,
            RegionsFilePath = regionsPath,
            Regions = regionModels
        });
        _libraryService.SaveAll(existing);

        var folder = _libraryService.GetLibraryFolder();
        StatusText =
            $"ÄÃ£ lÆ°u áº£nh máº«u ({Regions.Count} vÃ¹ng) vÃ o thÆ° viá»‡n \"{folder}\".";
    }

    /// <summary>Kiá»ƒm tra tÃªn vÃ¹ng theo cáº¥u hÃ¬nh (dÃ¹ng khi sá»­a tá»« thÆ° viá»‡n).</summary>
    public bool ValidateRegions(IReadOnlyCollection<TemplateRegion> regions, out string? errorMessage)
    {
        if (regions.Count == 0)
        {
            errorMessage = "Cáº§n Ã­t nháº¥t má»™t vÃ¹ng linh kiá»‡n trÃªn áº£nh máº«u.";
            return false;
        }

        return TryValidateRegionNames(regions.Select(r => r.Name), out errorMessage);
    }

    private bool TryValidateRegionNames(
        IEnumerable<string> regionNames,
        out string? errorMessage)
    {
        if (!ComponentTemplateRegionNames.TryGetInvalidNames(
                regionNames, _allowedRegionNames, out var invalid))
        {
            errorMessage =
                $"TÃªn vÃ¹ng khÃ´ng há»£p lá»‡: {string.Join(", ", invalid)}. " +
                $"Chá»‰ dÃ¹ng: {_allowedNamesHint}.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        if (_disposed) return;
        _boardImage?.Dispose();
        _disposed = true;
    }
}

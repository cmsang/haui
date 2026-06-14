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
/// Đại diện một vùng mẫu trong danh sách — hỗ trợ sửa tên trực tiếp.
/// </summary>
public class TemplateRegionItem : INotifyPropertyChanged
{
    private static readonly SolidColorBrush ValidNameBrush = Brushes.Black;
    private static readonly SolidColorBrush InvalidNameBrush = Brushes.Red;

    private string _name = string.Empty;
    private int _stt;

    /// <summary>Gán từ <see cref="CreateTemplateViewModel"/> để tô màu tên hợp lệ / không hợp lệ.</summary>
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

    /// <summary>Màu hiển thị của vùng này trên canvas và trong grid.</summary>
    public Color RegionColor { get; set; } = Colors.LimeGreen;

    /// <summary>Brush từ RegionColor để bind trong XAML.</summary>
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
/// ViewModel cho CreateTemplateWindow — xử lý logic cắt bo mạch,
/// quản lý danh sách vùng, lưu và tải vùng mẫu.
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

    // ──── Events ──────────────────────────────────────────────────────────────

    /// <summary>Phát khi ảnh bo mạch sẵn sàng để hiển thị.</summary>
    public event Action<BitmapSource>? BoardImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public ObservableCollection<TemplateRegionItem> Regions { get; } = [];

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    /// <summary>Kích thước ảnh bo mạch (để View tính tỉ lệ vùng chọn).</summary>
    public int BoardWidth => _boardImage?.Width ?? 0;
    public int BoardHeight => _boardImage?.Height ?? 0;

    public int RegionCount => Regions.Count;

    /// <summary>Chỉ cho lưu khi có ảnh bo mạch, ít nhất một vùng và mọi tên nằm trong danh sách cấu hình.</summary>
    public bool CanSave =>
        _boardImage is not null
        && Regions.Count > 0
        && ComponentTemplateRegionNames.TryGetInvalidNames(
            Regions.Select(r => r.Name), _allowedRegionNames, out _);

    /// <summary>Cho phép xoay khi đã có ảnh bo mạch.</summary>
    public bool CanRotateBoard => _boardImage is not null && !_boardImage.Empty();

    /// <summary>Hiển thị tiến độ đánh dấu vùng trên UI.</summary>
    public string RegionProgressText =>
        CanSave
            ? $"Vùng linh kiện: {Regions.Count} — có thể lưu."
            : $"Vùng linh kiện: {Regions.Count} — tên hợp lệ: {_allowedNamesHint}";

    // ──── Bảng 50 màu phân biệt ───────────────────────────────────────────────

    internal static readonly Color[] RegionPalette =
    [
        Color.FromRgb(255,  80,  80),  //  1 đỏ
        Color.FromRgb( 80, 160, 255),  //  2 xanh dương
        Color.FromRgb(255, 215,  40),  //  3 vàng
        Color.FromRgb( 60, 210, 110),  //  4 xanh lá
        Color.FromRgb(210,  80, 255),  //  5 tím
        Color.FromRgb(255, 140,  40),  //  6 cam
        Color.FromRgb( 40, 215, 215),  //  7 cyan
        Color.FromRgb(255, 100, 180),  //  8 hồng
        Color.FromRgb(160, 230,  60),  //  9 xanh lá nõn
        Color.FromRgb(255, 180, 100),  // 10 cam nhạt
        Color.FromRgb(100, 100, 255),  // 11 xanh má»±c
        Color.FromRgb(255,  50, 150),  // 12 hồng đậm
        Color.FromRgb( 50, 200, 170),  // 13 tím xanh
        Color.FromRgb(200, 160,  40),  // 14 vàng đồng
        Color.FromRgb(255, 120, 120),  // 15 đỏ hồng
        Color.FromRgb( 40, 180, 255),  // 16 xanh trời
        Color.FromRgb(180, 255,  80),  // 17 vàng xanh
        Color.FromRgb(255,  80, 200),  // 18 tím hồng
        Color.FromRgb( 80, 255, 160),  // 19 xanh bạc hà
        Color.FromRgb(255, 200,  60),  // 20 vàng sáng
        Color.FromRgb(200,  80,  80),  // 21 đỏ đậm
        Color.FromRgb( 60, 120, 220),  // 22 xanh navy nhạt
        Color.FromRgb(220, 200,  50),  // 23 vàng ô liu
        Color.FromRgb( 80, 220,  60),  // 24 xanh cỏ
        Color.FromRgb(180,  60, 220),  // 25 tím hồng
        Color.FromRgb(255, 160,  60),  // 26 cam vàng
        Color.FromRgb( 60, 220, 220),  // 27 ngọc lam
        Color.FromRgb(220, 100, 160),  // 28 hồng đỏ
        Color.FromRgb(140, 220,  80),  // 29 xanh lá sáng
        Color.FromRgb(255, 140, 180),  // 30 hồng nhạt
        Color.FromRgb( 80, 200,  80),  // 31 xanh lá vừa
        Color.FromRgb(200, 120, 255),  // 32 tím nhạt
        Color.FromRgb(255, 220, 100),  // 33 vàng nhạt
        Color.FromRgb( 80, 140, 200),  // 34 xanh xám
        Color.FromRgb(240,  80,  40),  // 35 đỏ cam
        Color.FromRgb(100, 240, 200),  // 36 xanh lá biển
        Color.FromRgb(240, 160, 240),  // 37 hồng tím nhạt
        Color.FromRgb(200, 240,  80),  // 38 vàng xanh nhạt
        Color.FromRgb(255,  80, 100),  // 39 đỏ hồng đậm
        Color.FromRgb( 60, 200, 240),  // 40 xanh sáng
        Color.FromRgb(255, 180, 200),  // 41 hồng phấn
        Color.FromRgb(160, 255, 120),  // 42 xanh lá nõn nhạt
        Color.FromRgb(255, 120,  60),  // 43 cam đậm
        Color.FromRgb(120, 120, 240),  // 44 đỏ tím
        Color.FromRgb( 80, 240, 140),  // 45 bạc hà sáng
        Color.FromRgb(240, 200,  80),  // 46 vàng kim
        Color.FromRgb(200,  60, 100),  // 47 đỏ rượu
        Color.FromRgb( 60, 180, 160),  // 48 xanh rêu
        Color.FromRgb(240, 120, 200),  // 49 hồng neon
        Color.FromRgb(180, 240, 180),  // 50 xanh lá pastel
    ];

    /// <summary>
    /// Chọn màu tiếp theo chưa được dùng bởi bất kỳ vùng nào đang có trong danh sách.
    /// </summary>
    internal Color GetNextColor()
    {
        var usedColors = Regions.Select(r => r.RegionColor).ToHashSet();
        foreach (var color in RegionPalette)
            if (!usedColors.Contains(color))
                return color;

        // Tất cả 50 màu đã dùng hết — quay vòng theo index
        return RegionPalette[Regions.Count % RegionPalette.Length];
    }

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

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

    // ──── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Nạp ảnh và danh sách vùng của một mẫu có sẵn để chỉnh sửa.
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
            ? $"Chế độ chỉnh sửa — {Regions.Count} vùng, có thể lưu."
            : $"Chế độ chỉnh sửa — {Regions.Count} vùng (đặt tên theo danh sách cấu hình để lưu).";
        NotifyRegionValidationChanged();
        NotifyBoardChanged();
    }

    /// <summary>
    /// Trả về danh sách vùng hiện tại dưới dạng model (dùng khi cập nhật mẫu từ bên ngoài).
    /// </summary>
    public List<TemplateRegion> GetCurrentRegions()
        => Regions.Select(r => r.ToModel()).ToList();

    /// <summary>Nạp ảnh chụp từ camera, cắt bo mạch, tải vùng đã lưu.</summary>
    public async Task LoadFrameAsync(Mat sourceFrame)
    {
        StatusText = "Đang cắt bo mạch...";
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
            $"Kéo thả trên ảnh để đánh dấu vùng linh kiện. Tên hợp lệ: {_allowedNamesHint}.";
        NotifyRegionValidationChanged();
        NotifyBoardChanged();
    }

    /// <summary>Xoay ảnh bo mạch 180° và cập nhật tọa độ vùng tương ứng.</summary>
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

        StatusText = RegionValidationStatusSuffix("Đã xoay ảnh 180°.");
    }

    /// <summary>
    /// Thêm một vùng mới (tọa độ tương đối so với ảnh bo mạch).
    /// </summary>
    public void AddRegion(double relX, double relY, double relW, double relH)
    {
        var item = new TemplateRegionItem
        {
            Stt = Regions.Count + 1,
            Name = $"Vùng {Regions.Count + 1}",
            RelX = relX,
            RelY = relY,
            RelWidth = relW,
            RelHeight = relH,
            RegionColor = GetNextColor()
        };
        Regions.Add(item);
        StatusText = RegionValidationStatusSuffix($"Đã thêm \"{item.Name}\".");
    }

    /// <summary>Xóa vùng được chọn.</summary>
    public void RemoveRegion(TemplateRegionItem item)
    {
        Regions.Remove(item);
        // Cập nhật lại số thứ tự
        for (int i = 0; i < Regions.Count; i++)
            Regions[i].Stt = i + 1;
        StatusText = RegionValidationStatusSuffix($"Đã xóa \"{item.Name}\".");
    }

    private string RegionValidationStatusSuffix(string action)
    {
        if (CanSave)
            return $"{action} Có thể lưu ({Regions.Count} vùng).";
        if (Regions.Count == 0)
            return $"{action} Chưa có vùng nào.";
        return $"{action} ({Regions.Count} vùng — kiểm tra tên theo cấu hình).";
    }

    /// <summary>Kiểm tra tên vùng trước khi lưu.</summary>
    public bool TrySaveRegions(out string? errorMessage)
    {
        if (_boardImage is null)
        {
            errorMessage = "Chưa có ảnh bo mạch.";
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
            $"Đã lưu ảnh mẫu ({Regions.Count} vùng) vào thư viện \"{folder}\".";
    }

    /// <summary>Kiểm tra tên vùng theo cấu hình (dùng khi sửa từ thư viện).</summary>
    public bool ValidateRegions(IReadOnlyCollection<TemplateRegion> regions, out string? errorMessage)
    {
        if (regions.Count == 0)
        {
            errorMessage = "Cần ít nhất một vùng linh kiện trên ảnh mẫu.";
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
                $"Tên vùng không hợp lệ: {string.Join(", ", invalid)}. " +
                $"Chỉ dùng: {_allowedNamesHint}.";
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

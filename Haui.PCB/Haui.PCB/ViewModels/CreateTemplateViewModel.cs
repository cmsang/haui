using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Haui.PCB.Processing.Configuration;
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

    private bool _isOrientationMarker;

    /// <summary>Marks this region as the board-orientation reference (only one allowed per template).</summary>
    public bool IsOrientationMarker
    {
        get => _isOrientationMarker;
        set
        {
            if (_isOrientationMarker == value) return;
            _isOrientationMarker = value;
            OnPropertyChanged();
        }
    }

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
        RelHeight = RelHeight,
        IsOrientationMarker = IsOrientationMarker
    };

    public static TemplateRegionItem FromModel(TemplateRegion m, Color color) => new()
    {
        Name = m.Name,
        RelX = m.RelX,
        RelY = m.RelY,
        RelWidth = m.RelWidth,
        RelHeight = m.RelHeight,
        IsOrientationMarker = m.IsOrientationMarker,
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
    private readonly IReadOnlyList<string> _allowedRegionNamesOrdered;
    private readonly string _allowedNamesHint;
    private bool _disposed;
    private bool _isWhiteCircuitMode;
    private string _libraryFolderPath = string.Empty;
    private string _modeDisplayText = "Linh kiện";
    private TemplateEntry? _editingEntry;
    private string _orientationComponentName = string.Empty;
    private TemplateRegionItem? _orientationRegion;
    private bool _isSettingOrientationPosition;

    internal static readonly Color OrientationRegionColor = Color.FromRgb(0, 200, 255);

    // ──── Events ──────────────────────────────────────────────────────────────

    /// <summary>Phát khi ảnh bo mạch sẵn sàng để hiển thị.</summary>
    public event Action<BitmapSource>? BoardImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public ObservableCollection<TemplateRegionItem> Regions { get; } = [];

    /// <summary>Danh sách tên vùng được phép — bind ComboBox chọn tên.</summary>
    public IReadOnlyList<string> AllowedRegionNames => _allowedRegionNamesOrdered;

    public bool IsWhiteCircuitMode
    {
        get => _isWhiteCircuitMode;
        private set
        {
            if (_isWhiteCircuitMode == value) return;
            _isWhiteCircuitMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ModeBannerText));
        }
    }

    public string ModeDisplayText
    {
        get => _modeDisplayText;
        private set { _modeDisplayText = value; OnPropertyChanged(); }
    }

    public string LibraryFolderPath
    {
        get => _libraryFolderPath;
        private set { _libraryFolderPath = value; OnPropertyChanged(); }
    }

    public string ModeBannerText =>
        $"Chế độ train: {ModeDisplayText} — Thư viện: {LibraryFolderPath}";

    /// <summary>Configured orientation component name — checkbox enabled when region name matches.</summary>
    public string OrientationComponentName
    {
        get => _orientationComponentName;
        private set { _orientationComponentName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSetOrientationPosition)); OnPropertyChanged(nameof(OrientationPositionHint)); }
    }

    /// <summary>When true, dragging on the board sets the orientation marker region.</summary>
    public bool IsSettingOrientationPosition
    {
        get => _isSettingOrientationPosition;
        set
        {
            if (_isSettingOrientationPosition == value) return;
            _isSettingOrientationPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OrientationPositionHint));
        }
    }

    public bool CanSetOrientationPosition =>
        !string.IsNullOrEmpty(OrientationComponentName);

    public bool HasOrientationRegion => _orientationRegion is not null;

    public TemplateRegionItem? OrientationRegion => _orientationRegion;

    public string OrientationPositionHint
    {
        get
        {
            if (string.IsNullOrEmpty(OrientationComponentName))
                return "Cấu hình tên thành phần xác định chiều trong tab Cài đặt.";

            var placed = HasOrientationRegion ? "Đã đặt vị trí" : "Chưa đặt vị trí";
            var mode = IsSettingOrientationPosition
                ? " — kéo trên ảnh để đặt vùng"
                : string.Empty;
            return $"{OrientationComponentName}: {placed}{mode}";
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    /// <summary>Kích thước ảnh bo mạch (để View tính tỉ lệ vùng chọn).</summary>
    public int BoardWidth => _boardImage?.Width ?? 0;
    public int BoardHeight => _boardImage?.Height ?? 0;

    public int RegionCount => Regions.Count;

    public bool IsEditing => _editingEntry is not null;

    /// <summary>Chỉ cho lưu khi có ảnh bo mạch, ít nhất một vùng, tên ∈ cấu hình và không trùng.</summary>
    public bool CanSave =>
        _boardImage is not null
        && Regions.Count > 0
        && ComponentTemplateRegionNames.TryGetInvalidNames(
            Regions.Select(r => r.Name), _allowedRegionNames, out _)
        && ComponentTemplateRegionNames.TryGetDuplicateNames(
            Regions.Select(r => r.Name), out _);

    /// <summary>Cho phép xoay khi đã có ảnh bo mạch.</summary>
    public bool CanRotateBoard => _boardImage is not null && !_boardImage.Empty();

    /// <summary>Hiển thị tiến độ đánh dấu vùng trên UI.</summary>
    public string RegionProgressText =>
        CanSave
            ? $"Vùng linh kiện: {Regions.Count} — có thể lưu."
            : $"Vùng linh kiện: {Regions.Count} — chọn tên từ danh sách cấu hình, không trùng.";

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

    /// <summary>First allowed name not yet used by another region.</summary>
    private string GetNextUnusedRegionName()
    {
        var used = Regions
            .Select(r => r.Name.Trim())
            .Where(n => n.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var name in _allowedRegionNamesOrdered)
        {
            if (!used.Contains(name))
                return name;
        }

        return string.Empty;
    }

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public CreateTemplateViewModel(
        IPcbSegmentationService segmentationService,
        ITemplateLibraryService libraryService)
    {
        _segmentationService = segmentationService;
        _libraryService = libraryService;
        _allowedRegionNames = ComponentTemplateRegionNames.LoadAllowedNames();
        _allowedRegionNamesOrdered = ComponentTemplateRegionNames.LoadAllowedNamesInOrder();
        _allowedNamesHint = ComponentTemplateRegionNames.FormatAllowedNamesHint(_allowedRegionNames);

        Regions.CollectionChanged += OnRegionsCollectionChanged;
        RefreshModeFromConfig();
    }

    public void RefreshModeFromConfig()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        IsWhiteCircuitMode = TemplateLibraryPaths.IsWhiteCircuitMode(settings);
        ModeDisplayText = IsWhiteCircuitMode ? "Mạch trắng" : "Linh kiện";
        LibraryFolderPath = _libraryService.GetLibraryFolder();
        OrientationComponentName = settings.OrientationComponentName.Trim();
        OnPropertyChanged(nameof(ModeBannerText));
        RefreshAllRegionNameValidation();
    }

    private void NotifyOrientationRegionChanged()
    {
        OnPropertyChanged(nameof(OrientationRegion));
        OnPropertyChanged(nameof(HasOrientationRegion));
        OnPropertyChanged(nameof(OrientationPositionHint));
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
        item.NameValidator = IsRegionNameValid;
        item.RefreshNameValidation();
    }

    private bool IsRegionNameValid(string name)
    {
        if (!ComponentTemplateRegionNames.IsAllowedName(name, _allowedRegionNames))
            return false;

        var trimmed = name.Trim();
        return Regions.Count(r =>
            string.Equals(r.Name.Trim(), trimmed, StringComparison.Ordinal)) <= 1;
    }

    private void RefreshAllRegionNameValidation()
    {
        foreach (var item in Regions)
            item.RefreshNameValidation();
    }

    private void OnRegionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TemplateRegionItem.Name))
            NotifyRegionValidationChanged();
    }

    private void NotifyRegionValidationChanged()
    {
        RefreshAllRegionNameValidation();
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
    public void LoadExistingTemplate(Mat boardImage, TemplateEntry editingEntry)
    {
        _editingEntry = editingEntry;

        _boardImage?.Dispose();
        _boardImage = boardImage.Clone();

        var bitmap = BitmapSourceConverter.ToBitmapSource(_boardImage);
        bitmap.Freeze();
        _boardBitmap = bitmap;
        BoardImageReady?.Invoke(bitmap);

        Regions.Clear();
        _orientationRegion = null;
        int idx = 0;
        foreach (var r in editingEntry.Regions)
        {
            if (r.IsOrientationMarker)
            {
                _orientationRegion = TemplateRegionItem.FromModel(r, OrientationRegionColor);
                _orientationRegion.IsOrientationMarker = true;
                continue;
            }

            var item = TemplateRegionItem.FromModel(r, RegionPalette[idx % RegionPalette.Length]);
            item.Stt = idx + 1;
            Regions.Add(item);
            idx++;
        }

        NotifyOrientationRegionChanged();

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
        => BuildAllRegionModels();

    private List<TemplateRegion> BuildAllRegionModels()
    {
        var models = Regions.Select(r => r.ToModel()).ToList();
        if (_orientationRegion is not null)
            models.Add(_orientationRegion.ToModel());
        return models;
    }

    /// <summary>Nạp ảnh chụp từ camera, cắt bo mạch, tải vùng đã lưu.</summary>
    public async Task LoadFrameAsync(Mat sourceFrame)
    {
        _editingEntry = null;
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
        _orientationRegion = null;
        NotifyOrientationRegionChanged();

        StatusText =
            "Kéo thả trên ảnh để đánh dấu vùng linh kiện. Chọn tên vùng từ danh sách cấu hình.";
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

        if (_orientationRegion is not null)
        {
            _orientationRegion.RelX = 1.0 - _orientationRegion.RelX - _orientationRegion.RelWidth;
            _orientationRegion.RelY = 1.0 - _orientationRegion.RelY - _orientationRegion.RelHeight;
        }

        NotifyOrientationRegionChanged();

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
        var defaultName = GetNextUnusedRegionName();
        var item = new TemplateRegionItem
        {
            Stt = Regions.Count + 1,
            Name = defaultName,
            RelX = relX,
            RelY = relY,
            RelWidth = relW,
            RelHeight = relH,
            RegionColor = GetNextColor()
        };
        Regions.Add(item);
        var nameLabel = string.IsNullOrEmpty(defaultName) ? "(chưa chọn tên)" : $"\"{defaultName}\"";
        StatusText = RegionValidationStatusSuffix($"Đã thêm vùng {nameLabel}.");
    }

    /// <summary>Creates or updates the orientation marker region from a canvas drag.</summary>
    public void AddOrUpdateOrientationRegion(double relX, double relY, double relW, double relH)
    {
        if (string.IsNullOrEmpty(OrientationComponentName))
            return;

        if (_orientationRegion is null)
        {
            _orientationRegion = new TemplateRegionItem
            {
                Name = OrientationComponentName,
                IsOrientationMarker = true,
                RegionColor = OrientationRegionColor
            };
        }

        _orientationRegion.RelX = relX;
        _orientationRegion.RelY = relY;
        _orientationRegion.RelWidth = relW;
        _orientationRegion.RelHeight = relH;
        _orientationRegion.Name = OrientationComponentName;
        _orientationRegion.IsOrientationMarker = true;

        NotifyOrientationRegionChanged();
        StatusText = RegionValidationStatusSuffix(
            $"Đã đặt vị trí thành phần xác định chiều \"{OrientationComponentName}\".");
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
        return $"{action} ({Regions.Count} vùng — chọn tên từ danh sách cấu hình, không trùng).";
    }

    /// <summary>Kiểm tra tên vùng trước khi lưu.</summary>
    public bool TrySaveRegions(out string? errorMessage)
    {
        if (_boardImage is null)
        {
            errorMessage = "Chưa có ảnh bo mạch.";
            return false;
        }

        if (!TryValidateForSave(out errorMessage))
            return false;

        SaveRegionsCore();
        errorMessage = null;
        return true;
    }

    private void SaveRegionsCore()
    {
        if (_boardImage is null)
            return;

        var regionModels = BuildAllRegionModels();

        // Editing existing template: overwrite its board image + regions JSON.
        if (_editingEntry is not null)
        {
            var templateName = _editingEntry.Name;
            var imagePath = _editingEntry.BoardImagePath;
            var regionsPath = !string.IsNullOrWhiteSpace(_editingEntry.RegionsFilePath)
                ? _editingEntry.RegionsFilePath
                : (!string.IsNullOrWhiteSpace(imagePath)
                    ? _libraryService.GetRegionsFilePathForBoardImage(imagePath)
                    : string.Empty);

            if (string.IsNullOrWhiteSpace(imagePath) || string.IsNullOrWhiteSpace(regionsPath))
            {
                StatusText = "Không thể lưu mẫu: thiếu đường dẫn file.";
                return;
            }

            Cv2.ImWrite(imagePath, _boardImage);
            _libraryService.SaveRegions(regionsPath, templateName, regionModels);

            _editingEntry.Regions = regionModels;

            var folder = _libraryService.GetLibraryFolder();
            StatusText =
                $"Đã lưu mẫu \"{templateName}\" ({Regions.Count} vùng) vào thư viện \"{folder}\".";
            return;
        }

        // Create new template: save new board image + regions JSON, then persist.
        var newTemplateName = $"{DateTime.Now:dd/MM/yyyy HH:mm}";
        var newImagePath = _libraryService.SaveBoardImage(newTemplateName, _boardImage);
        var newRegionsPath = _libraryService.GetRegionsFilePathForBoardImage(newImagePath);
        _libraryService.SaveRegions(newRegionsPath, newTemplateName, regionModels);

        var existing = _libraryService.LoadAll().ToList();
        existing.Add(new TemplateEntry
        {
            Name = newTemplateName,
            BoardImagePath = newImagePath,
            RegionsFilePath = newRegionsPath,
            Regions = regionModels
        });
        _libraryService.SaveAll(existing);

        var createFolder = _libraryService.GetLibraryFolder();
        StatusText =
            $"Đã lưu ảnh mẫu ({Regions.Count} vùng) vào thư viện \"{createFolder}\".";
    }

    /// <summary>Kiểm tra tên vùng theo cấu hình (dùng khi sửa từ thư viện).</summary>
    public bool ValidateRegions(IReadOnlyCollection<TemplateRegion> regions, out string? errorMessage)
    {
        var componentRegions = regions.Where(r => !r.IsOrientationMarker).ToList();
        var orientationRegions = regions.Where(r => r.IsOrientationMarker).ToList();

        if (componentRegions.Count == 0)
        {
            errorMessage = "Cần ít nhất một vùng linh kiện trên ảnh mẫu.";
            return false;
        }

        if (!TryValidateComponentRegionNames(
                componentRegions.Select(r => r.Name), out errorMessage))
            return false;

        if (!TryValidateOrientationRegions(orientationRegions, out errorMessage))
            return false;

        return true;
    }

    private bool TryValidateForSave(out string? errorMessage)
    {
        if (!TryValidateComponentRegionNames(Regions.Select(r => r.Name), out errorMessage))
            return false;

        if (!TryValidateOrientationRegions(
                _orientationRegion is null ? [] : [_orientationRegion.ToModel()],
                out errorMessage))
            return false;

        errorMessage = null;
        return true;
    }

    private bool TryValidateComponentRegionNames(
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

        if (!ComponentTemplateRegionNames.TryGetDuplicateNames(regionNames, out var duplicates))
        {
            errorMessage =
                $"Tên vùng trùng lặp: {string.Join(", ", duplicates)}. " +
                "Mỗi tên chỉ được dùng một lần.";
            return false;
        }

        var orientationName = OrientationComponentName;
        if (!string.IsNullOrEmpty(orientationName)
            && Regions.Any(r => string.Equals(r.Name.Trim(), orientationName, StringComparison.Ordinal)))
        {
            errorMessage =
                $"Tên \"{orientationName}\" đã dùng cho vị trí xác định chiều — " +
                "không thêm vùng linh kiện trùng tên.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    private bool TryValidateOrientationRegions(
        IReadOnlyList<TemplateRegion> orientationRegions,
        out string? errorMessage)
    {
        if (orientationRegions.Count > 1)
        {
            errorMessage = "Chỉ được đánh dấu một vùng xác định chiều mạch.";
            return false;
        }

        var orientationName = OrientationComponentName;
        if (string.IsNullOrEmpty(orientationName))
        {
            errorMessage = null;
            return true;
        }

        if (orientationRegions.Count == 0)
        {
            errorMessage = null;
            return true;
        }

        if (!string.Equals(orientationRegions[0].Name.Trim(), orientationName, StringComparison.Ordinal))
        {
            errorMessage =
                $"Vùng xác định chiều phải có tên \"{orientationName}\" theo cấu hình.";
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

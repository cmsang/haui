using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Haui.PCB.Models;
using Haui.PCB.Processing;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Đại diện một vùng mẫu trong danh sách — hỗ trợ sửa tên trực tiếp.
/// </summary>
public class TemplateRegionItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _stt;

    public int Stt
    {
        get => _stt;
        set { _stt = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }

    /// <summary>Màu hiển thị của vùng này trên canvas và trong grid.</summary>
    public Color RegionColor { get; set; } = Colors.LimeGreen;

    /// <summary>Brush từ RegionColor để bind trong XAML.</summary>
    public SolidColorBrush RegionBrush => new(RegionColor);

    public event PropertyChangedEventHandler? PropertyChanged;

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
    private bool _useCustomDataFolder;
    private string _dataFolder = ComponentTemplateSettings.DefaultLibraryFolder;
    private readonly int _requiredRegionCount;
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

    /// <summary>Bật lưu vào <see cref="DataFolder"/> thay vì mặc định.</summary>
    public bool UseCustomDataFolder
    {
        get => _useCustomDataFolder;
        set
        {
            if (_useCustomDataFolder == value) return;
            _useCustomDataFolder = value;
            if (!value)
                DataFolder = ComponentTemplateSettings.DefaultLibraryFolder;
            OnPropertyChanged();
            PersistStorageConfiguration();
        }
    }

    /// <summary>Thư mục hiển thị (mặc định <c>templates</c> hoặc đường dẫn tùy chọn).</summary>
    public string DataFolder
    {
        get => _dataFolder;
        private set { _dataFolder = value; OnPropertyChanged(); }
    }

    /// <summary>Kích thước ảnh bo mạch (để View tính tỉ lệ vùng chọn).</summary>
    public int BoardWidth => _boardImage?.Width ?? 0;
    public int BoardHeight => _boardImage?.Height ?? 0;

    /// <summary>Số vùng linh kiện bắt buộc (từ <c>component_template_settings.json</c>).</summary>
    public int RequiredRegionCount => _requiredRegionCount;

    public int RegionCount => Regions.Count;

    /// <summary>Chỉ cho lưu khi đã có ảnh bo mạch và đủ số vùng theo cấu hình.</summary>
    public bool CanSave => _boardImage is not null && Regions.Count == _requiredRegionCount;

    /// <summary>Cho phép xoay khi đã có ảnh bo mạch.</summary>
    public bool CanRotateBoard => _boardImage is not null && !_boardImage.Empty();

    /// <summary>Hiển thị tiến độ đánh dấu vùng trên UI.</summary>
    public string RegionProgressText =>
        $"Vùng linh kiện: {Regions.Count}/{_requiredRegionCount}";

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
        Color.FromRgb(100, 100, 255),  // 11 xanh mực
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
        _requiredRegionCount = ComponentTemplateSettingsStore.Load().RequiredRegionCount;
        if (_requiredRegionCount < 1)
            _requiredRegionCount = ComponentTemplateSettings.DefaultRequiredRegionCount;

        Regions.CollectionChanged += (_, _) => NotifyRegionCountChanged();
        LoadStorageConfiguration();
    }

    private void NotifyRegionCountChanged()
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

    /// <summary>Chọn thư mục lưu tùy chỉnh (gọi từ View sau hộp thoại chọn thư mục).</summary>
    public void SetCustomDataFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath)) return;
        DataFolder = folderPath.Trim();
        UseCustomDataFolder = true;
    }

    private void LoadStorageConfiguration()
    {
        var (useCustom, folder) = _libraryService.GetStorageConfiguration();
        _useCustomDataFolder = useCustom;
        _dataFolder = folder;

        OnPropertyChanged(nameof(UseCustomDataFolder));
        OnPropertyChanged(nameof(DataFolder));
    }

    private void PersistStorageConfiguration()
        => _libraryService.ConfigureStorage(UseCustomDataFolder, DataFolder);

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

        StatusText = Regions.Count == _requiredRegionCount
            ? $"Chế độ chỉnh sửa — đủ {_requiredRegionCount} vùng."
            : $"Chế độ chỉnh sửa — {Regions.Count}/{_requiredRegionCount} vùng (cần đủ {_requiredRegionCount} để lưu).";
        NotifyRegionCountChanged();
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
            $"Kéo thả trên ảnh để đánh dấu {_requiredRegionCount} vùng linh kiện (0/{_requiredRegionCount}).";
        NotifyRegionCountChanged();
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

        StatusText = RegionCountStatusSuffix("Đã xoay ảnh 180°.");
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
        StatusText = RegionCountStatusSuffix($"Đã thêm \"{item.Name}\".");
    }

    /// <summary>Xóa vùng được chọn.</summary>
    public void RemoveRegion(TemplateRegionItem item)
    {
        Regions.Remove(item);
        // Cập nhật lại số thứ tự
        for (int i = 0; i < Regions.Count; i++)
            Regions[i].Stt = i + 1;
        StatusText = RegionCountStatusSuffix($"Đã xóa \"{item.Name}\".");
    }

    private string RegionCountStatusSuffix(string action)
    {
        if (Regions.Count == _requiredRegionCount)
            return $"{action} Đủ {_requiredRegionCount} vùng — có thể lưu.";
        return $"{action} ({Regions.Count}/{_requiredRegionCount} vùng).";
    }

    /// <summary>Kiểm tra đủ vùng trước khi lưu.</summary>
    public bool TrySaveRegions(out string? errorMessage)
    {
        if (_boardImage is null)
        {
            errorMessage = "Chưa có ảnh bo mạch.";
            return false;
        }

        if (Regions.Count != _requiredRegionCount)
        {
            errorMessage =
                $"Cần đánh dấu đủ {_requiredRegionCount} vùng linh kiện trên ảnh mẫu (hiện có {Regions.Count}).";
            return false;
        }

        SaveRegionsCore();
        errorMessage = null;
        return true;
    }

    private void SaveRegionsCore()
    {
        PersistStorageConfiguration();

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
            $"Đã lưu ảnh mẫu ({_requiredRegionCount} vùng) vào thư viện \"{folder}\".";
    }

    /// <summary>Kiểm tra danh sách vùng đủ số lượng theo cấu hình (dùng khi sửa từ thư viện).</summary>
    public bool ValidateRegionCount(IReadOnlyCollection<TemplateRegion> regions, out string? errorMessage)
    {
        if (regions.Count != _requiredRegionCount)
        {
            errorMessage =
                $"Cần đánh dấu đủ {_requiredRegionCount} vùng linh kiện (hiện có {regions.Count}).";
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

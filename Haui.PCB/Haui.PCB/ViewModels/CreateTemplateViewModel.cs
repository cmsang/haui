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
    private readonly ITemplateRegionService _regionService;
    private readonly IPcbSegmentationService _segmentationService;
    private readonly ITemplateLibraryService? _libraryService;
    private Mat? _boardImage;
    private BitmapSource? _boardBitmap;
    private string _statusText = string.Empty;
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
        ITemplateRegionService regionService,
        IPcbSegmentationService segmentationService,
        ITemplateLibraryService? libraryService = null)
    {
        _regionService = regionService;
        _segmentationService = segmentationService;
        _libraryService = libraryService;
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

        StatusText = $"Chế độ chỉnh sửa — {Regions.Count} vùng đã tải.";
    }

    /// <summary>
    /// Trả về danh sách vùng hiện tại dưới dạng model (dùng khi cập nhật mẫu từ bên ngoài).
    /// </summary>
    public List<TemplateRegion> GetCurrentRegions()
        => Regions.Select(r => r.ToModel()).ToList();

    /// <summary>
    /// Nạp ảnh chụp từ camera, cắt bo mạch, tải vùng đã lưu.
    /// </summary>
    public void LoadFrame(Mat sourceFrame)
    {
        // Cắt bo mạch từ ảnh chụp
        var segmented = _segmentationService.Segment(sourceFrame);
        _boardImage?.Dispose();
        _boardImage = segmented ?? sourceFrame.Clone();

        var bitmap = BitmapSourceConverter.ToBitmapSource(_boardImage);
        bitmap.Freeze();
        _boardBitmap = bitmap;
        BoardImageReady?.Invoke(bitmap);

        // Tải các vùng đã lưu
        Regions.Clear();
        int idx = 0;
        foreach (var r in _regionService.Load())
        {
            var item = TemplateRegionItem.FromModel(r, RegionPalette[idx % RegionPalette.Length]);
            item.Stt = idx + 1;
            Regions.Add(item);
            idx++;
        }

        StatusText = Regions.Count > 0
            ? $"Đã tải {Regions.Count} vùng mẫu."
            : "Kéo thả trên ảnh để tạo vùng mới.";
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
        StatusText = $"Đã thêm \"{item.Name}\".";
    }

    /// <summary>Xóa vùng được chọn.</summary>
    public void RemoveRegion(TemplateRegionItem item)
    {
        Regions.Remove(item);
        // Cập nhật lại số thứ tự
        for (int i = 0; i < Regions.Count; i++)
            Regions[i].Stt = i + 1;
        StatusText = $"Đã xóa \"{item.Name}\".";
    }

    /// <summary>Lưu tất cả vùng và ảnh bo mạch mẫu xuống file.</summary>
    public void SaveRegions()
    {
        _regionService.Save(Regions.Select(r => r.ToModel()));

        // Lưu ảnh bo mạch mẫu để dùng cho việc so sánh sau này
        if (_boardImage is not null)
            _regionService.SaveBoardImage(_boardImage);

        // Thêm vào thư viện mẫu nếu có service
        if (_libraryService is not null && _boardImage is not null)
        {
            var templateName = $"{DateTime.Now:dd/MM/yyyy HH:mm}";
            var imagePath = _libraryService.SaveBoardImage(templateName, _boardImage);

            var existing = _libraryService.LoadAll().ToList();
            existing.Add(new Models.TemplateEntry
            {
                Name = templateName,
                BoardImagePath = imagePath,
                Regions = Regions.Select(r => r.ToModel()).ToList()
            });
            _libraryService.SaveAll(existing);
        }

        StatusText = $"Đã lưu {Regions.Count} vùng mẫu.";
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

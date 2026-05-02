using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }

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

    public static TemplateRegionItem FromModel(TemplateRegion m) => new()
    {
        Name = m.Name,
        RelX = m.RelX,
        RelY = m.RelY,
        RelWidth = m.RelWidth,
        RelHeight = m.RelHeight
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

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public CreateTemplateViewModel(
        ITemplateRegionService regionService,
        IPcbSegmentationService segmentationService)
    {
        _regionService = regionService;
        _segmentationService = segmentationService;
    }

    // ──── Public API ──────────────────────────────────────────────────────────

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
        foreach (var r in _regionService.Load())
            Regions.Add(TemplateRegionItem.FromModel(r));

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
            Name = $"Vùng {Regions.Count + 1}",
            RelX = relX,
            RelY = relY,
            RelWidth = relW,
            RelHeight = relH
        };
        Regions.Add(item);
        StatusText = $"Đã thêm \"{item.Name}\".";
    }

    /// <summary>Xóa vùng được chọn.</summary>
    public void RemoveRegion(TemplateRegionItem item)
    {
        Regions.Remove(item);
        StatusText = $"Đã xóa \"{item.Name}\".";
    }

    /// <summary>Lưu tất cả vùng và ảnh bo mạch mẫu xuống file.</summary>
    public void SaveRegions()
    {
        _regionService.Save(Regions.Select(r => r.ToModel()));

        // Lưu ảnh bo mạch mẫu để dùng cho việc so sánh sau này
        if (_boardImage is not null)
            _regionService.SaveBoardImage(_boardImage);

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

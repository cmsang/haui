using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Đại diện một mục ảnh mẫu trong danh sách — hiển thị trên grid đầu tiên.
/// </summary>
public class TemplateEntryItem
{
    public int Stt { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RegionCount { get; set; }

    /// <summary>Dữ liệu gốc của mẫu.</summary>
    public TemplateEntry Source { get; set; } = new();
}

/// <summary>
/// Đại diện một vùng trong mẫu đang được xem — hiển thị trên grid thứ hai.
/// </summary>
public class TemplateRegionViewItem
{
    public int Stt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;

    public double RelX { get; set; }
    public double RelY { get; set; }
    public double RelWidth { get; set; }
    public double RelHeight { get; set; }

    /// <summary>Màu hiển thị vùng trên ảnh mẫu.</summary>
    public System.Windows.Media.Color RegionColor { get; set; } = System.Windows.Media.Colors.LimeGreen;

    /// <summary>Brush để bind XAML.</summary>
    public System.Windows.Media.SolidColorBrush RegionBrush => new(RegionColor);
}

/// <summary>
/// ViewModel cho TemplateViewerWindow — xử lý logic hiển thị thư viện ảnh mẫu.
/// </summary>
public class TemplateViewerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ITemplateLibraryService _libraryService;
    private readonly int _requiredRegionCount;
    private BitmapSource? _previewImage;
    private string _statusText = "Chọn một mẫu để xem chi tiết.";
    private Mat? _currentMat;
    private bool _disposed;
    private int _boardWidth;
    private int _boardHeight;
    private bool _isWhiteCircuitMode;
    private string _libraryFolderPath = string.Empty;
    private string _modeDisplayText = "Linh kiện";

    public event Action<BitmapSource?>? PreviewImageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public ObservableCollection<TemplateEntryItem> Templates { get; } = [];
    public ObservableCollection<TemplateRegionViewItem> Regions { get; } = [];

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public BitmapSource? PreviewImage
    {
        get => _previewImage;
        private set { _previewImage = value; OnPropertyChanged(); }
    }

    /// <summary>Kích thước ảnh bo mạch mẫu để View tính toán vùng hiển thị.</summary>
    public int BoardWidth => _boardWidth;
    public int BoardHeight => _boardHeight;

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

    // ──── Internal state ─────────────────────────────────────────────────────

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public int RequiredRegionCount => _requiredRegionCount;

    public TemplateViewerViewModel(ITemplateLibraryService libraryService)
    {
        _libraryService = libraryService;
        _requiredRegionCount = ComponentTemplateRegionNames.RequiredRegionCount;
        RefreshModeFromConfig();
    }

    public void RefreshModeFromConfig()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        IsWhiteCircuitMode = TemplateLibraryPaths.IsWhiteCircuitMode(settings);
        ModeDisplayText = IsWhiteCircuitMode ? "Mạch trắng" : "Linh kiện";
        LibraryFolderPath = _libraryService.GetLibraryFolder();
        OnPropertyChanged(nameof(ModeBannerText));
    }

    // ──── Public API ─────────────────────────────────────────────────────────

    /// <summary>Nạp danh sách mẫu từ thư viện.</summary>
    public void LoadTemplates()
    {
        RefreshModeFromConfig();
        Templates.Clear();
        Regions.Clear();
        PreviewImage = null;

        var entries = _libraryService.LoadAll().ToList();
        int stt = 1;
        foreach (var entry in entries)
        {
            Templates.Add(new TemplateEntryItem
            {
                Stt = stt++,
                Name = entry.Name,
                RegionCount = entry.Regions.Count,
                Source = entry
            });
        }
        StatusText = Templates.Count > 0
            ? $"Thư viện có {Templates.Count} ảnh mẫu."
            : "Chưa có ảnh mẫu nào. Hãy tạo mẫu trước.";
    }

    /// <summary>Lấy ảnh bo mạch của mẫu đang được chọn để đưa vào form chỉnh sửa.</summary>
    public Mat? GetSelectedBoardImage(TemplateEntryItem item)
        => _libraryService.LoadBoardImage(item.Source.BoardImagePath);

    /// <summary>Xóa mẫu khỏi danh sách và xóa file trên đĩa ngay.</summary>
    public void DeleteTemplate(TemplateEntryItem item)
    {
        _libraryService.DeleteTemplate(item.Source);
        Templates.Remove(item);

        // Cập nhật lại số thứ tự
        for (int i = 0; i < Templates.Count; i++)
            Templates[i].Stt = i + 1;

        Regions.Clear();
        PreviewImage = null;
        PreviewImageChanged?.Invoke(null);
        StatusText = $"Đã xóa mẫu \"{item.Name}\".";
    }

    /// <summary>Chọn một mẫu để xem ảnh và danh sách vùng.</summary>
    public void SelectTemplate(TemplateEntryItem item)
    {
        // Tải danh sách vùng
        Regions.Clear();
        int stt = 1;
        int colorIdx = 0;
        foreach (var region in item.Source.Regions)
        {
            var color = CreateTemplateViewModel.RegionPalette[
                colorIdx % CreateTemplateViewModel.RegionPalette.Length];
            colorIdx++;
            Regions.Add(new TemplateRegionViewItem
            {
                Stt = stt++,
                Name = region.Name,
                Position = $"({region.RelX:P1}, {region.RelY:P1})",
                Size = $"{region.RelWidth:P1} × {region.RelHeight:P1}",
                RelX = region.RelX,
                RelY = region.RelY,
                RelWidth = region.RelWidth,
                RelHeight = region.RelHeight,
                RegionColor = color
            });
        }

        // Tải ảnh bo mạch mẫu
        _currentMat?.Dispose();
        _currentMat = _libraryService.LoadBoardImage(item.Source.BoardImagePath);

        if (_currentMat is not null)
        {
            _boardWidth = _currentMat.Width;
            _boardHeight = _currentMat.Height;
            var bitmap = BitmapSourceConverter.ToBitmapSource(_currentMat);
            bitmap.Freeze();
            PreviewImage = bitmap;
            PreviewImageChanged?.Invoke(bitmap);
            StatusText = item.RegionCount == _requiredRegionCount
                ? $"Mẫu \"{item.Name}\" — đủ {_requiredRegionCount} vùng."
                : $"Mẫu \"{item.Name}\" — {item.RegionCount}/{_requiredRegionCount} vùng (chưa đủ để lưu thư viện).";
        }
        else
        {
            _boardWidth = 0;
            _boardHeight = 0;
            PreviewImage = null;
            PreviewImageChanged?.Invoke(null);
            StatusText = $"Mẫu \"{item.Name}\" — không tìm thấy ảnh.";
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        if (_disposed) return;
        _currentMat?.Dispose();
        _disposed = true;
    }
}

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

    /// <summary>Cờ đánh dấu có thay đổi chưa được lưu.</summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set { _hasUnsavedChanges = value; OnPropertyChanged(); }
    }
    private bool _hasUnsavedChanges;

    /// <summary>Kích thước ảnh bo mạch mẫu để View tính toán vùng hiển thị.</summary>
    public int BoardWidth => _boardWidth;
    public int BoardHeight => _boardHeight;

    // ──── Internal state ─────────────────────────────────────────────────────

    // Danh sách mẫu đang làm việc (có thể đã bị sửa/xóa nhưng chưa lưu)
    private List<TemplateEntry> _workingEntries = [];

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public int RequiredRegionCount => _requiredRegionCount;

    public TemplateViewerViewModel(ITemplateLibraryService libraryService)
    {
        _libraryService = libraryService;
        _requiredRegionCount = ComponentTemplateSettingsStore.Load().RequiredRegionCount;
        if (_requiredRegionCount < 1)
            _requiredRegionCount = ComponentTemplateSettings.DefaultRequiredRegionCount;
    }

    // ──── Public API ─────────────────────────────────────────────────────────

    /// <summary>Nạp danh sách mẫu từ thư viện.</summary>
    public void LoadTemplates()
    {
        Templates.Clear();
        Regions.Clear();
        PreviewImage = null;

        _workingEntries = _libraryService.LoadAll().ToList();
        int stt = 1;
        foreach (var entry in _workingEntries)
        {
            Templates.Add(new TemplateEntryItem
            {
                Stt = stt++,
                Name = entry.Name,
                RegionCount = entry.Regions.Count,
                Source = entry
            });
        }

        HasUnsavedChanges = false;
        StatusText = Templates.Count > 0
            ? $"Thư viện có {Templates.Count} ảnh mẫu."
            : "Chưa có ảnh mẫu nào. Hãy tạo mẫu trước.";
    }

    /// <summary>Lấy ảnh bo mạch của mẫu đang được chọn để đưa vào form chỉnh sửa.</summary>
    public Mat? GetSelectedBoardImage(TemplateEntryItem item)
        => _libraryService.LoadBoardImage(item.Source.BoardImagePath);

    /// <summary>
    /// Cập nhật danh sách vùng cho mẫu sau khi sửa trên form tạo mẫu.
    /// Chưa lưu file — cần gọi SaveLibrary để lưu thật sự.
    /// </summary>
    public void UpdateTemplateRegions(TemplateEntryItem item, List<TemplateRegion> newRegions)
    {
        item.Source.Regions = newRegions;
        item.RegionCount = newRegions.Count;

        // Cập nhật lại item trong Templates list để UI phản ánh
        var idx = Templates.IndexOf(item);
        if (idx >= 0)
        {
            Templates[idx] = item;
        }

        // Refresh regions grid nếu đây là item đang được chọn
        SelectTemplate(item);

        HasUnsavedChanges = true;
        StatusText = newRegions.Count == _requiredRegionCount
            ? $"Đã cập nhật mẫu \"{item.Name}\" — đủ {_requiredRegionCount} vùng. Nhấn Lưu để ghi file."
            : $"Mẫu \"{item.Name}\" có {newRegions.Count}/{_requiredRegionCount} vùng — cần đủ {_requiredRegionCount} trước khi lưu thư viện.";
    }

    /// <summary>Xóa mẫu khỏi danh sách. Chưa lưu file — cần gọi SaveLibrary.</summary>
    public void DeleteTemplate(TemplateEntryItem item)
    {
        _workingEntries.Remove(item.Source);
        Templates.Remove(item);

        // Cập nhật lại số thứ tự
        for (int i = 0; i < Templates.Count; i++)
            Templates[i].Stt = i + 1;

        Regions.Clear();
        PreviewImage = null;
        PreviewImageChanged?.Invoke(null);

        HasUnsavedChanges = true;
        StatusText = $"Đã xóa mẫu \"{item.Name}\". Nhấn Lưu để ghi file.";
    }

    /// <summary>Ghi từng file *_regions.json (không dùng index.json).</summary>
    public bool TrySaveLibrary(out string? errorMessage)
    {
        var invalid = Templates
            .Where(t => t.RegionCount != _requiredRegionCount)
            .Select(t => t.Name)
            .ToList();

        if (invalid.Count > 0)
        {
            errorMessage =
                $"Mỗi ảnh mẫu phải có đủ {_requiredRegionCount} vùng linh kiện. " +
                $"Các mẫu chưa đạt: {string.Join(", ", invalid)}.";
            return false;
        }

        _workingEntries = Templates.Select(t => t.Source).ToList();
        _libraryService.SaveAll(_workingEntries);
        HasUnsavedChanges = false;
        StatusText = $"Đã lưu thư viện — {Templates.Count} ảnh mẫu (mỗi mẫu {_requiredRegionCount} vùng).";
        errorMessage = null;
        return true;
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

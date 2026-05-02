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
}

/// <summary>
/// ViewModel cho TemplateViewerWindow — xử lý logic hiển thị thư viện ảnh mẫu.
/// </summary>
public class TemplateViewerViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ITemplateLibraryService _libraryService;
    private BitmapSource? _previewImage;
    private string _statusText = "Chọn một mẫu để xem chi tiết.";
    private Mat? _currentMat;
    private bool _disposed;

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

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public TemplateViewerViewModel(ITemplateLibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    // ──── Public API ─────────────────────────────────────────────────────────

    /// <summary>Nạp danh sách mẫu từ thư viện.</summary>
    public void LoadTemplates()
    {
        Templates.Clear();
        Regions.Clear();
        PreviewImage = null;

        var entries = _libraryService.LoadAll();
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

    /// <summary>Chọn một mẫu để xem ảnh và danh sách vùng.</summary>
    public void SelectTemplate(TemplateEntryItem item)
    {
        // Tải danh sách vùng
        Regions.Clear();
        int stt = 1;
        foreach (var region in item.Source.Regions)
        {
            Regions.Add(new TemplateRegionViewItem
            {
                Stt = stt++,
                Name = region.Name,
                Position = $"({region.RelX:P1}, {region.RelY:P1})",
                Size = $"{region.RelWidth:P1} × {region.RelHeight:P1}"
            });
        }

        // Tải ảnh bo mạch mẫu
        _currentMat?.Dispose();
        _currentMat = _libraryService.LoadBoardImage(item.Source.BoardImagePath);

        if (_currentMat is not null)
        {
            var bitmap = BitmapSourceConverter.ToBitmapSource(_currentMat);
            bitmap.Freeze();
            PreviewImage = bitmap;
            PreviewImageChanged?.Invoke(bitmap);
            StatusText = $"Mẫu \"{item.Name}\" — {item.RegionCount} vùng.";
        }
        else
        {
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

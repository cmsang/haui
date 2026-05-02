using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;
using Haui.PCB.Models;
using Haui.PCB.Processing;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho TestPipelineWindow — chứa toàn bộ logic xử lý ảnh và phân vùng PCB.
/// Tách biệt hoàn toàn khỏi UI, tuân theo SOLID: SRP, DIP.
/// </summary>
public class TestPipelineViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentation;
    private readonly ITemplateRegionService _regionService;
    private readonly IRegionComparisonService _comparisonService;

    private Mat? _sourceMat;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _disposed;

    // ──── Sự kiện ────────────────────────────────────────────────────────────

    /// <summary>Phát khi ảnh đã xử lý (bo mạch đã cắt) sẵn sàng — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? ProcessedImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool HasSource => _sourceMat is not null && !_sourceMat.Empty();

    /// <summary>Các vùng đạt ngưỡng tương đồng >= 80% (giống nhau).</summary>
    public ObservableCollection<RegionComparisonResult> MatchedRegions { get; } = [];

    /// <summary>Các vùng có độ tương đồng dưới 80% (khác nhau).</summary>
    public ObservableCollection<RegionComparisonResult> DifferentRegions { get; } = [];

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public TestPipelineViewModel(
        IPcbSegmentationService segmentation,
        ITemplateRegionService regionService,
        IRegionComparisonService comparisonService)
    {
        _segmentation = segmentation;
        _regionService = regionService;
        _comparisonService = comparisonService;
    }

    // ──── Actions ─────────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (ví dụ từ camera chụp) rồi tự động chạy pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));

        // Tự động chạy pipeline ngay sau khi nhận ảnh
        _ = RunSegmentationAsync();
    }

    /// <summary>Nạp ảnh từ đường dẫn file.</summary>
    public void LoadImageFromFile(string filePath)
    {
        _sourceMat?.Dispose();
        _sourceMat = Cv2.ImRead(filePath, ImreadModes.Color);

        if (_sourceMat.Empty())
        {
            StatusText = "Không thể đọc ảnh.";
            OnPropertyChanged(nameof(HasSource));
            return;
        }

        StatusText = $"Đã chọn: {System.IO.Path.GetFileName(filePath)}";
        OnPropertyChanged(nameof(HasSource));
    }

    /// <summary>
    /// Chạy pipeline: cắt bo mạch → hiển thị → so sánh vùng với mẫu.
    /// </summary>
    public async Task RunSegmentationAsync()
    {
        if (!HasSource)
        {
            StatusText = "Vui lòng chọn ảnh trước.";
            return;
        }

        IsBusy = true;
        StatusText = "Đang xử lý...";
        MatchedRegions.Clear();
        DifferentRegions.Clear();

        using var source = _sourceMat!.Clone();
        Mat? newBoard = null;

        try
        {
            newBoard = await Task.Run(() => _segmentation.Segment(source));

            if (newBoard is null)
            {
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
                return;
            }

            // Hiển thị ảnh bo mạch đã cắt
            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(newBoard);
            bitmap.Freeze();
            ProcessedImageReady?.Invoke(bitmap);

            // So sánh với ảnh mẫu nếu có
            await CompareWithTemplateAsync(newBoard);
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi: {ex.Message}";
            ProcessedImageReady?.Invoke(null);
        }
        finally
        {
            newBoard?.Dispose();
            IsBusy = false;
        }
    }

    /// <summary>
    /// Tải ảnh mẫu và danh sách vùng, sau đó so sánh với bo mạch mới.
    /// </summary>
    private async Task CompareWithTemplateAsync(Mat newBoard)
    {
        var regions = _regionService.Load();
        if (regions.Count == 0)
        {
            StatusText = $"Bo mạch đã cắt. Chưa có vùng mẫu để so sánh.";
            return;
        }

        using var templateBoard = await Task.Run(() => _regionService.LoadBoardImage());
        if (templateBoard is null)
        {
            StatusText = "Bo mạch đã cắt. Chưa có ảnh mẫu để so sánh.";
            return;
        }

        using var newBoardClone = newBoard.Clone();
        var results = await Task.Run(() =>
            _comparisonService.Compare(templateBoard, newBoardClone, regions));

        foreach (var r in results)
        {
            if (r.IsMatch)
                MatchedRegions.Add(r);
            else
                DifferentRegions.Add(r);
        }

        StatusText = $"Hoàn thành. Giống: {MatchedRegions.Count} | Khác: {DifferentRegions.Count} / {results.Count} vùng.";
    }

    // ──── INotifyPropertyChanged ──────────────────────────────────────────────

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ──── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _sourceMat?.Dispose();
        _disposed = true;
    }
}

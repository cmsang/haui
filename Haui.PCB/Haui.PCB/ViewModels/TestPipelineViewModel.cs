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

    /// <summary>Phát khi ảnh đã vẽ các vùng so sánh sẵn sàng — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? AnnotatedImageReady;

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

        // Vẽ các vùng so sánh lên ảnh bo mạch (xanh = giống, đỏ = khác)
        var annotated = DrawAnnotations(newBoard, results);
        if (annotated is not null)
            AnnotatedImageReady?.Invoke(annotated);

        StatusText = $"Hoàn thành. Giống: {MatchedRegions.Count} | Khác: {DifferentRegions.Count} / {results.Count} vùng.";
    }

    /// <summary>
    /// Vẽ hình chữ nhật lên ảnh bo mạch: xanh lá = giống, đỏ = khác.
    /// Trả về BitmapSource đã Freeze, hoặc null nếu lỗi.
    /// </summary>
    private static System.Windows.Media.Imaging.BitmapSource? DrawAnnotations(
        Mat board, IReadOnlyList<RegionComparisonResult> results)
    {
        try
        {
            using var canvas = board.Clone();
            var green = new Scalar(0, 200, 0);
            var red = new Scalar(0, 0, 220);
            const int thickness = 2;
            const double fontScale = 0.45;

            foreach (var r in results)
            {
                var color = r.IsMatch ? green : red;
                Cv2.Rectangle(canvas, r.BoardRect, color, thickness);

                // Vẽ nhãn tên vùng phía trên hình chữ nhật
                var labelPos = new Point(r.BoardRect.X + 2, r.BoardRect.Y - 4);
                if (labelPos.Y < 10) labelPos.Y = r.BoardRect.Y + 12;
                Cv2.PutText(canvas, r.Name, labelPos,
                    HersheyFonts.HersheySimplex, fontScale, color, 1, LineTypes.AntiAlias);
            }

            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(canvas);
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
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

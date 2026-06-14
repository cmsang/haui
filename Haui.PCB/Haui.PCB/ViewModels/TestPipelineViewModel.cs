using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho TestPipelineWindow — chứa toàn bộ logic xử lý ảnh và phân vùng PCB.
/// Tách biệt hoàn toàn khỏi UI, tuân theo SOLID: SRP, DIP.
/// </summary>
public class TestPipelineViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentation;
    private readonly ICompositeTemplateMatchService _compositeMatchService;

    private Mat? _sourceMat;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _disposed;
    private double _matchThresholdPercent = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;
    private bool? _isFullMatch;

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

    /// <summary>True when every configured region is recognized; false on failure; null before first run.</summary>
    public bool? IsFullMatch
    {
        get => _isFullMatch;
        private set { _isFullMatch = value; OnPropertyChanged(); }
    }

    /// <summary>Ngưỡng % từ <c>setting.json</c> (cập nhật mỗi lần so).</summary>
    public double MatchThresholdPercent => _matchThresholdPercent;

    public string DifferentRegionsHeader =>
        $"⚠ Vùng khác nhau (< {FormatThresholdPercent(_matchThresholdPercent)})";

    public string MatchedRegionsHeader =>
        $"✔ Vùng giống nhau (≥ {FormatThresholdPercent(_matchThresholdPercent)})";

    /// <summary>Các vùng đạt ngưỡng cấu hình (giống nhau).</summary>
    public ObservableCollection<RegionComparisonResult> MatchedRegions { get; } = [];

    /// <summary>Các vùng dưới ngưỡng cấu hình (khác nhau).</summary>
    public ObservableCollection<RegionComparisonResult> DifferentRegions { get; } = [];

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public TestPipelineViewModel(
        IPcbSegmentationService segmentation,
        ICompositeTemplateMatchService compositeMatchService)
    {
        _segmentation = segmentation;
        _compositeMatchService = compositeMatchService;
        RefreshMatchThresholdFromConfig();
    }

    // ──── Actions ─────────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (ví dụ từ camera chụp) rồi tự động chạy pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;

        _ = RunSegmentationAsync();
    }

    /// <summary>Load a captured frame and await the full inspection pipeline.</summary>
    public async Task InspectAsync(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;
        await RunSegmentationAsync();
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
    /// Chạy pipeline: cắt bo mạch → hiển thị → so sánh vùng với thư viện mẫu.
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
                IsFullMatch = false;
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
                AnnotatedImageReady?.Invoke(null);
                return;
            }

            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(newBoard);
            bitmap.Freeze();
            ProcessedImageReady?.Invoke(bitmap);

            await CompareWithTemplatesAsync(newBoard);
        }
        catch (Exception ex)
        {
            IsFullMatch = false;
            StatusText = $"Lỗi: {ex.Message}";
            ProcessedImageReady?.Invoke(null);
            AnnotatedImageReady?.Invoke(null);
        }
        finally
        {
            newBoard?.Dispose();
            IsBusy = false;
        }
    }

    /// <summary>
    /// So từng tên trong AllowedRegionNames: lấy ứng viên đầu tiên đạt MinMatchSimilarityPercent trong nhóm.
    /// Nếu chưa đạt đủ, xoay bo mạch 180° và so lại.
    /// </summary>
    private async Task CompareWithTemplatesAsync(Mat newBoard)
    {
        var match = await Task.Run(() =>
        {
            using var clone = newBoard.Clone();
            return _compositeMatchService.Match(clone);
        });

        if (match is null)
        {
            RefreshMatchThresholdFromConfig();
            IsFullMatch = false;
            StatusText = "Bo mạch đã cắt. Chưa có mẫu nào trong thư viện (hoặc thiếu vùng/ảnh).";
            PublishAnnotatedImage(newBoard, []);
            return;
        }

        RefreshMatchThreshold(match.MatchThresholdPercent);

        Mat boardForDisplay = newBoard;
        Mat? rotatedBoard = null;
        var usedRotation = false;

        if (!match.IsFullMatch)
        {
            rotatedBoard = new Mat();
            Cv2.Rotate(newBoard, rotatedBoard, RotateFlags.Rotate180);

            var rotatedMatch = await Task.Run(() => _compositeMatchService.Match(rotatedBoard));

            if (rotatedMatch is not null
                && (rotatedMatch.IsFullMatch
                    || rotatedMatch.AverageSimilarity > match.AverageSimilarity))
            {
                match = rotatedMatch;
                boardForDisplay = rotatedBoard;
                usedRotation = true;
            }
            else
            {
                rotatedBoard.Dispose();
                rotatedBoard = null;
            }
        }

        if (usedRotation)
        {
            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(boardForDisplay);
            bitmap.Freeze();
            ProcessedImageReady?.Invoke(bitmap);
        }

        ApplyRegionResultsToGrids(match.RegionResults);
        PublishAnnotatedImage(boardForDisplay, match.RegionResults);
        IsFullMatch = match.IsFullMatch;

        var thresholdText = FormatThresholdPercent(match.MatchThresholdPercent);
        var rotationNote = usedRotation ? " (đã xoay ảnh 180°)" : "";
        StatusText = match.IsFullMatch
            ? $"Đạt — {match.MatchedCount}/{match.TotalCount} vùng giống (≥ {thresholdText} mỗi tên).{rotationNote}"
            : $"Chưa đạt — TB {match.AverageSimilarity:F1}%, {match.MatchedCount}/{match.TotalCount} vùng giống (ngưỡng {thresholdText}).{rotationNote}";

        rotatedBoard?.Dispose();
    }

    private void RefreshMatchThresholdFromConfig()
        => RefreshMatchThreshold(AppSettingsStore.LoadMatchThresholdPercent());

    private void RefreshMatchThreshold(double percent)
    {
        if (Math.Abs(_matchThresholdPercent - percent) < 0.001)
            return;

        _matchThresholdPercent = percent;
        OnPropertyChanged(nameof(MatchThresholdPercent));
        OnPropertyChanged(nameof(DifferentRegionsHeader));
        OnPropertyChanged(nameof(MatchedRegionsHeader));
    }

    private static string FormatThresholdPercent(double percent)
        => percent % 1 == 0 ? $"{percent:F0}%" : $"{percent:F1}%";

    private void ApplyRegionResultsToGrids(IReadOnlyList<RegionComparisonResult> results)
    {
        MatchedRegions.Clear();
        DifferentRegions.Clear();

        foreach (var r in results)
        {
            if (r.IsMatch)
                MatchedRegions.Add(r);
            else
                DifferentRegions.Add(r);
        }
    }

    /// <summary>
    /// Vẽ hình chữ nhật lên ảnh bo mạch: xanh lá = giống, đỏ = khác.
    /// </summary>
    public void PublishAnnotatedImage(Mat board, IReadOnlyList<RegionComparisonResult> results)
    {
        var annotated = DrawAnnotations(board, results);
        if (annotated is not null)
            AnnotatedImageReady?.Invoke(annotated);
    }

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

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void Dispose()
    {
        if (_disposed) return;
        _sourceMat?.Dispose();
        _disposed = true;
    }

}

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
    private readonly ITemplateLibraryService _libraryService;
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
        ITemplateLibraryService libraryService,
        IRegionComparisonService comparisonService)
    {
        _segmentation = segmentation;
        _libraryService = libraryService;
        _comparisonService = comparisonService;
    }

    // ──── Actions ─────────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (ví dụ từ camera chụp) rồi tự động chạy pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));

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
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
                return;
            }

            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(newBoard);
            bitmap.Freeze();
            ProcessedImageReady?.Invoke(bitmap);

            await CompareWithTemplatesAsync(newBoard);
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
    /// Duyệt thư viện: gặp mẫu đạt đủ vùng thì dừng; không thì chọn mẫu có TB% cao nhất.
    /// </summary>
    private async Task CompareWithTemplatesAsync(Mat newBoard)
    {
        using var newBoardClone = newBoard.Clone();

        var match = await Task.Run(() => FindTemplateMatch(newBoardClone));

        if (match is null)
        {
            StatusText = "Bo mạch đã cắt. Chưa có mẫu nào trong thư viện (hoặc thiếu vùng/ảnh).";
            return;
        }

        ApplyRegionResultsToGrids(match.RegionResults);
        PublishAnnotatedImage(newBoard, match.RegionResults);

        StatusText = match.IsFullMatch
            ? $"Đạt mẫu \"{match.TemplateName}\" — {match.MatchedCount}/{match.TotalCount} vùng giống."
            : $"Không đạt mẫu nào. Gần nhất: \"{match.TemplateName}\" (TB {match.AverageSimilarity:F1}%, {match.MatchedCount}/{match.TotalCount} giống).";
    }

    private TemplateMatchResult? FindTemplateMatch(Mat newBoard)
    {
        TemplateMatchResult? bestByAverage = null;
        var libraryEntries = _libraryService.LoadAll();

        foreach (var entry in libraryEntries)
        {
            if (entry.Regions.Count == 0)
                continue;

            using var templateBoard = _libraryService.LoadBoardImage(entry.BoardImagePath);
            if (templateBoard is null)
                continue;

            var results = _comparisonService.Compare(templateBoard, newBoard, entry.Regions);
            var match = BuildMatchResult(entry.Name, results);

            if (match.IsFullMatch)
                return match;

            if (bestByAverage is null || match.AverageSimilarity > bestByAverage.AverageSimilarity)
                bestByAverage = match;
        }

        return bestByAverage;
    }

    private static TemplateMatchResult BuildMatchResult(
        string templateName,
        IReadOnlyList<RegionComparisonResult> results)
    {
        var numbered = results
            .Select((r, i) => new RegionComparisonResult
            {
                Stt = i + 1,
                Name = r.Name,
                Similarity = r.Similarity,
                BoardRect = r.BoardRect
            })
            .ToList();

        int matched = numbered.Count(r => r.IsMatch);
        double avg = numbered.Count > 0
            ? numbered.Average(r => r.Similarity)
            : 0;

        return new TemplateMatchResult
        {
            TemplateName = templateName,
            MatchedCount = matched,
            DifferentCount = numbered.Count - matched,
            AverageSimilarity = avg,
            RegionResults = numbered
        };
    }

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

    private sealed class TemplateMatchResult
    {
        public string TemplateName { get; init; } = string.Empty;
        public int MatchedCount { get; init; }
        public int DifferentCount { get; init; }
        public int TotalCount => MatchedCount + DifferentCount;
        public bool IsFullMatch => TotalCount > 0 && DifferentCount == 0;
        public double AverageSimilarity { get; init; }
        public IReadOnlyList<RegionComparisonResult> RegionResults { get; init; } = [];
    }
}

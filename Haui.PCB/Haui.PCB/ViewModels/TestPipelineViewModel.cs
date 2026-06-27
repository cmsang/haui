using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Haui.PCB.ViewModels.Pipeline;
using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel for PCB inspection — segmentation, YOLO missing-component detection, annotated result.
/// </summary>
public class TestPipelineViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentation;
    private readonly IComponentInspectionService _inspectionService;

    private Mat? _sourceMat;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _disposed;
    private bool? _isFullMatch;
    private string _totalInspectionElapsedText = "—";
    private bool _hasInspectionTiming;
    private bool _hasInspectionResult;
    private BitmapSource? _resultAnnotatedImage;

    public event Action<BitmapSource?>? ProcessedImageReady;
    public event Action<BitmapSource?>? AnnotatedImageReady;
    public event Action<bool>? InspectionCompleted;
    public event PropertyChangedEventHandler? PropertyChanged;

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

    public bool? IsFullMatch
    {
        get => _isFullMatch;
        private set
        {
            _isFullMatch = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PassFailLabel));
            OnPropertyChanged(nameof(InspectionSummaryText));
        }
    }

    public string InspectionSummaryText => IsFullMatch switch
    {
        true => "Đủ linh kiện",
        false => MissingComponentCount > 0
            ? $"Thiếu {MissingComponentCount} linh kiện"
            : "Chưa đạt",
        _ => "—"
    };

    public string DifferentRegionsHeader => "Danh sách linh kiện thiếu";

    public ObservableCollection<MissingComponentRowItem> DifferentRegions { get; } = [];

    public ObservableCollection<InspectionResultRowItem> InspectionResults { get; } = [];

    public bool HasInspectionResultList => InspectionResults.Count > 0;

    public BitmapSource? ResultAnnotatedImage
    {
        get => _resultAnnotatedImage;
        private set { _resultAnnotatedImage = value; OnPropertyChanged(); }
    }

    public string PassFailLabel => IsFullMatch switch
    {
        true => "PASS",
        false => "FAIL",
        _ => "—"
    };

    public ObservableCollection<PipelineStep> Steps { get; } = [];

    public bool HasPipelineSteps => Steps.Count > 0;

    public int MissingComponentCount => DifferentRegions.Count;

    public bool HasComponentResults => _hasInspectionResult;

    public string TotalInspectionElapsedText
    {
        get => _totalInspectionElapsedText;
        private set { _totalInspectionElapsedText = value; OnPropertyChanged(); }
    }

    public bool HasInspectionTiming
    {
        get => _hasInspectionTiming;
        private set { _hasInspectionTiming = value; OnPropertyChanged(); }
    }

    public TestPipelineViewModel(
        IPcbSegmentationService segmentation,
        IComponentInspectionService inspectionService)
    {
        _segmentation = segmentation;
        _inspectionService = inspectionService;
    }

    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;
        _hasInspectionResult = false;
        OnPropertyChanged(nameof(HasComponentResults));

        _ = RunSegmentationAsync();
    }

    public async Task InspectAsync(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;
        _hasInspectionResult = false;
        OnPropertyChanged(nameof(HasComponentResults));
        await RunSegmentationAsync();
    }

    public async Task InspectFromFileAsync(string filePath)
    {
        _sourceMat?.Dispose();
        _sourceMat = await Task.Run(() => Cv2.ImRead(filePath, ImreadModes.Color));

        if (_sourceMat.Empty())
        {
            StatusText = "Không thể đọc ảnh.";
            OnPropertyChanged(nameof(HasSource));
            return;
        }

        StatusText = $"Đã chọn: {Path.GetFileName(filePath)}";
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;
        _hasInspectionResult = false;
        OnPropertyChanged(nameof(HasComponentResults));
        await RunSegmentationAsync();
    }

    public async Task RunSegmentationAsync()
    {
        if (!HasSource)
        {
            StatusText = "Vui lòng chọn ảnh trước.";
            return;
        }

        IsBusy = true;
        StatusText = "Đang xử lý...";
        ResetInspectionTiming();
        DifferentRegions.Clear();
        NotifyComponentCounts();
        Steps.Clear();
        OnPropertyChanged(nameof(HasPipelineSteps));

        using var source = _sourceMat!.Clone();
        Mat? newBoard = null;
        var totalSw = Stopwatch.StartNew();

        try
        {
            var runResult = await Task.Run(() => RunSegmentationPipeline(source));
            ReplaceSteps(runResult.Steps);
            newBoard = runResult.Warped;

            if (newBoard is null)
            {
                IsFullMatch = false;
                _hasInspectionResult = false;
                OnPropertyChanged(nameof(HasComponentResults));
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
                AnnotatedImageReady?.Invoke(null);
                RaiseInspectionCompleted(false);
                return;
            }

            var bitmap = await ToFrozenBitmapAsync(newBoard);
            ProcessedImageReady?.Invoke(bitmap);

            await InspectComponentsAsync(newBoard);
        }
        catch (Exception ex)
        {
            IsFullMatch = false;
            _hasInspectionResult = false;
            OnPropertyChanged(nameof(HasComponentResults));
            StatusText = $"Lỗi: {ex.Message}";
            ProcessedImageReady?.Invoke(null);
            AnnotatedImageReady?.Invoke(null);
            RaiseInspectionCompleted(false);
        }
        finally
        {
            totalSw.Stop();
            SetTotalInspectionElapsed(totalSw.Elapsed);
            newBoard?.Dispose();
            IsBusy = false;
        }
    }

    private async Task InspectComponentsAsync(Mat newBoard)
    {
        ComponentInspectionResult result;
        TimeSpan inspectElapsed;

        try
        {
            (result, inspectElapsed) = await Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                using var clone = newBoard.Clone();
                var inspection = _inspectionService.Inspect(clone);
                sw.Stop();
                return (inspection, sw.Elapsed);
            });
        }
        catch (Exception ex)
        {
            IsFullMatch = false;
            _hasInspectionResult = false;
            OnPropertyChanged(nameof(HasComponentResults));
            InspectionResults.Clear();
            OnPropertyChanged(nameof(HasInspectionResultList));
            StatusText = $"Lỗi nhận diện linh kiện: {ex.Message}";
            var fallback = await ToFrozenBitmapAsync(newBoard);
            ResultAnnotatedImage = fallback;
            AnnotatedImageReady?.Invoke(fallback);
            RaiseInspectionCompleted(false);
            return;
        }

        var annotated = await DrawMissingAnnotationsAsync(newBoard, result.Missing);
        if (annotated is null)
            annotated = await ToFrozenBitmapAsync(newBoard);

        ResultAnnotatedImage = annotated;
        AnnotatedImageReady?.Invoke(annotated);
        AppendRecognitionStep(annotated, result, inspectElapsed);

        ApplyInspectionResult(result);
        IsFullMatch = result.IsComplete;
        _hasInspectionResult = true;
        OnPropertyChanged(nameof(HasComponentResults));

        StatusText = result.IsComplete
            ? "Đủ linh kiện — không phát hiện vị trí thiếu."
            : BuildMissingStatusText(result);

        RaiseInspectionCompleted(result.IsComplete);
    }

    private static string BuildMissingStatusText(ComponentInspectionResult result)
    {
        var names = result.Missing
            .Select(m => m.Label)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var nameList = names.Count > 0
            ? string.Join(", ", names)
            : "—";

        return $"Thiếu {result.MissingCount} vị trí linh kiện: {nameList}.";
    }

    private void AppendRecognitionStep(
        BitmapSource annotated,
        ComponentInspectionResult result,
        TimeSpan elapsed)
    {
        var description = result.IsComplete
            ? "Không phát hiện linh kiện thiếu trên bo mạch đã cắt"
            : $"Phát hiện {result.MissingCount} vị trí thiếu linh kiện";

        Steps.Add(PipelineStep.WithTiming("Nhận diện linh kiện", annotated, description, elapsed));
        OnPropertyChanged(nameof(HasPipelineSteps));
    }

    private void SetTotalInspectionElapsed(TimeSpan elapsed)
    {
        TotalInspectionElapsedText = PipelineStep.FormatElapsed(elapsed);
        HasInspectionTiming = true;
    }

    private void ResetInspectionTiming()
    {
        TotalInspectionElapsedText = "—";
        HasInspectionTiming = false;
    }

    private void RaiseInspectionCompleted(bool isPass)
        => InspectionCompleted?.Invoke(isPass);

    private SegmentationRunResult RunSegmentationPipeline(Mat source)
    {
        using var pipeline = _segmentation.RunPipeline(source);
        var steps = PipelineStepMapper.MapSteps(pipeline, source);
        Mat? warped = pipeline.Warped is not null && !pipeline.Warped.Empty()
            ? pipeline.Warped.Clone()
            : null;
        return new SegmentationRunResult(steps, warped);
    }

    private void ReplaceSteps(IReadOnlyList<PipelineStep> steps)
    {
        Steps.Clear();
        foreach (var step in steps)
            Steps.Add(step);
        OnPropertyChanged(nameof(HasPipelineSteps));
    }

    private sealed record SegmentationRunResult(IReadOnlyList<PipelineStep> Steps, Mat? Warped);

    private void ApplyInspectionResult(ComponentInspectionResult result)
    {
        DifferentRegions.Clear();
        InspectionResults.Clear();

        var stt = 1;
        foreach (var missing in result.Missing)
        {
            DifferentRegions.Add(MissingComponentRowItem.From(missing, stt));
            InspectionResults.Add(InspectionResultRowItem.From(missing, stt));
            stt++;
        }

        OnPropertyChanged(nameof(HasInspectionResultList));
        NotifyComponentCounts();
    }

    public void ClearInspectionResults()
    {
        DifferentRegions.Clear();
        InspectionResults.Clear();
        IsFullMatch = null;
        ResultAnnotatedImage = null;
        _hasInspectionResult = false;
        ResetInspectionTiming();
        OnPropertyChanged(nameof(HasInspectionResultList));
        OnPropertyChanged(nameof(HasComponentResults));
        NotifyComponentCounts();
    }

    private void NotifyComponentCounts()
    {
        OnPropertyChanged(nameof(MissingComponentCount));
        OnPropertyChanged(nameof(InspectionSummaryText));
    }

    private static Task<BitmapSource?> DrawMissingAnnotationsAsync(
        Mat board, IReadOnlyList<MissingComponent> missing)
        => Task.Run(() => DrawMissingAnnotations(board, missing));

    private static Task<BitmapSource> ToFrozenBitmapAsync(Mat mat)
        => Task.Run(() =>
        {
            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(mat);
            bitmap.Freeze();
            return bitmap;
        });

    private static BitmapSource? DrawMissingAnnotations(
        Mat board, IReadOnlyList<MissingComponent> missing)
    {
        try
        {
            using var canvas = board.Channels() == 1 ? EnsureBgr(board) : board.Clone();
            const int thickness = 8;
            const double fontScale = 0.45;

            foreach (var item in missing)
            {
                if (item.Box.Width <= 0 || item.Box.Height <= 0)
                    continue;

                var color = ComponentColorPalette.GetColor(item.Label);
                Cv2.Rectangle(canvas, item.Box, color, thickness);

                var labelPos = new Point(item.Box.X + 2, item.Box.Y - 4);
                if (labelPos.Y < 10) labelPos.Y = item.Box.Y + 12;
                Cv2.PutText(canvas, item.Label, labelPos,
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

    private static Mat EnsureBgr(Mat source)
    {
        var bgr = new Mat();
        Cv2.CvtColor(source, bgr, ColorConversionCodes.GRAY2BGR);
        return bgr;
    }

    public void ClearPipelineSteps()
    {
        Steps.Clear();
        OnPropertyChanged(nameof(HasPipelineSteps));
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

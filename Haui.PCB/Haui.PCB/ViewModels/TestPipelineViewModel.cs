using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Haui.PCB.Processing.Configuration;
using Haui.PCB.ViewModels.Pipeline;
using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel for PCB inspection — segmentation, template match, annotated result.
/// Tách biệt hoàn toàn khỏi UI, tuân theo SOLID: SRP, DIP.
/// </summary>
public class TestPipelineViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentation;
    private readonly ICompositeTemplateMatchService _compositeMatchService;
    private readonly IBoardOrientationDetectionService _orientationService;

    private Mat? _sourceMat;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _disposed;
    private double _matchThresholdPercent = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;
    private bool? _isFullMatch;
    private string _totalInspectionElapsedText = "—";
    private bool _hasInspectionTiming;
    private bool _isWhiteCircuitInspectionMode;
    private string _whiteCircuitSummaryText = string.Empty;
    private string _inspectionModeLabel = "Linh kiện";
    private BitmapSource? _resultAnnotatedImage;

    // ──── Sự kiện ────────────────────────────────────────────────────────────

    /// <summary>Phát khi ảnh đã xử lý (bo mạch đã cắt) sẵn sàng — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? ProcessedImageReady;

    /// <summary>Phát khi ảnh đã vẽ các vùng so sánh sẵn sàng — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? AnnotatedImageReady;

    /// <summary>Phát khi nhận dạng xong — true = PASS, false = FAIL (kích hoạt phân loại robot).</summary>
    public event Action<bool>? InspectionCompleted;

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
        private set
        {
            _isFullMatch = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PassFailLabel));
        }
    }

    /// <summary>Ngưỡng % từ <c>setting.json</c> (cập nhật mỗi lần so).</summary>
    public double MatchThresholdPercent => _matchThresholdPercent;

    public string DifferentRegionsHeader => "⚠ Vùng thiếu linh kiện";

    public string MatchedRegionsHeader => "✔ Vùng có linh kiện";

    /// <summary>Regions where a component is present.</summary>
    public ObservableCollection<RegionComparisonResult> MatchedRegions { get; } = [];

    /// <summary>Regions where a component is missing (Dashboard grid rows).</summary>
    public ObservableCollection<MissingComponentRowItem> DifferentRegions { get; } = [];

    /// <summary>Full result list for developer inspection result window.</summary>
    public ObservableCollection<InspectionResultRowItem> InspectionResults { get; } = [];

    public bool HasInspectionResultList => InspectionResults.Count > 0;

    /// <summary>Last annotated board image from the most recent inspection run.</summary>
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

    /// <summary>Debug gallery — populated from the same RunPipeline call as PASS/FAIL.</summary>
    public ObservableCollection<PipelineStep> Steps { get; } = [];

    public bool HasPipelineSteps => Steps.Count > 0;

    public int SearchedComponentCount => MatchedRegions.Count + DifferentRegions.Count;

    public int MatchedComponentCount => MatchedRegions.Count;

    public int MissingComponentCount => DifferentRegions.Count;

    public bool HasComponentResults => SearchedComponentCount > 0;

    /// <summary>True when the active config uses white-circuit inspection.</summary>
    public bool IsWhiteCircuitInspectionMode
    {
        get => _isWhiteCircuitInspectionMode;
        private set
        {
            if (_isWhiteCircuitInspectionMode == value) return;
            _isWhiteCircuitInspectionMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsComponentInspectionMode));
            OnPropertyChanged(nameof(HasComponentResults));
        }
    }

    public bool IsComponentInspectionMode => !IsWhiteCircuitInspectionMode;

    public string InspectionModeLabel
    {
        get => _inspectionModeLabel;
        private set { _inspectionModeLabel = value; OnPropertyChanged(); }
    }

    public string WhiteCircuitSummaryText
    {
        get => _whiteCircuitSummaryText;
        private set
        {
            _whiteCircuitSummaryText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasWhiteCircuitSummary));
        }
    }

    public bool HasWhiteCircuitSummary =>
        IsWhiteCircuitInspectionMode && !string.IsNullOrWhiteSpace(WhiteCircuitSummaryText);

    /// <summary>Formatted full-pipeline elapsed time (segmentation + component match).</summary>
    public string TotalInspectionElapsedText
    {
        get => _totalInspectionElapsedText;
        private set { _totalInspectionElapsedText = value; OnPropertyChanged(); }
    }

    /// <summary>True after an inspection run completes and total elapsed time is available.</summary>
    public bool HasInspectionTiming
    {
        get => _hasInspectionTiming;
        private set { _hasInspectionTiming = value; OnPropertyChanged(); }
    }

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public TestPipelineViewModel(
        IPcbSegmentationService segmentation,
        ICompositeTemplateMatchService compositeMatchService,
        IBoardOrientationDetectionService? orientationService = null)
    {
        _segmentation = segmentation;
        _compositeMatchService = compositeMatchService;
        _orientationService = orientationService ?? new BoardOrientationDetectionService(new TemplateLibraryService());
        RefreshMatchThresholdFromConfig();
        RefreshInspectionModeFromConfig();
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

    /// <summary>Load an image from disk and await the full inspection pipeline.</summary>
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

        StatusText = $"Đã chọn: {System.IO.Path.GetFileName(filePath)}";
        OnPropertyChanged(nameof(HasSource));
        IsFullMatch = null;
        await RunSegmentationAsync();
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
        ResetInspectionTiming();
        MatchedRegions.Clear();
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
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
                AnnotatedImageReady?.Invoke(null);
                RaiseInspectionCompleted(false);
                return;
            }

            var bitmap = await ToFrozenBitmapAsync(newBoard);
            ProcessedImageReady?.Invoke(bitmap);

            var boardForMatch = newBoard;
            Mat? orientedBoard = null;
            TemplateEntry? orientationTemplate = null;

            try
            {
                var (orientation, orientationElapsed) = await Task.Run(() =>
                {
                    var sw = Stopwatch.StartNew();
                    var result = _orientationService.Detect(newBoard);
                    sw.Stop();
                    return (result, sw.Elapsed);
                });

                if (!orientation.Success)
                {
                    IsFullMatch = false;
                    StatusText = orientation.ErrorMessage ?? "Không tìm được chiều mạch.";
                    AnnotatedImageReady?.Invoke(bitmap);
                    RaiseInspectionCompleted(false);
                    return;
                }

                if (orientation.RequiresRotation180)
                {
                    orientedBoard = new Mat();
                    Cv2.Rotate(newBoard, orientedBoard, RotateFlags.Rotate180);
                    boardForMatch = orientedBoard;

                    var orientedBitmap = await ToFrozenBitmapAsync(orientedBoard);
                    ProcessedImageReady?.Invoke(orientedBitmap);
                    bitmap = orientedBitmap;
                }

                orientationTemplate = orientation.MatchedTemplate;

                if (orientation.MatchedTemplate is not null)
                {
                    var orientationStep = await BuildOrientationStepAsync(
                        boardForMatch,
                        orientation.MatchedTemplate,
                        orientation.MatchScore,
                        orientation.RequiresRotation180,
                        orientationElapsed);
                    if (orientationStep is not null)
                    {
                        Steps.Add(orientationStep);
                        OnPropertyChanged(nameof(HasPipelineSteps));
                    }
                }

                await CompareWithTemplatesAsync(boardForMatch, ResolveMatchRestrictTemplate(orientationTemplate));
            }
            finally
            {
                orientedBoard?.Dispose();
            }
        }
        catch (Exception ex)
        {
            IsFullMatch = false;
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

    private async Task<PipelineStep?> BuildOrientationStepAsync(
        Mat orientedBoard,
        TemplateEntry matchedTemplate,
        double matchScore,
        bool wasRotated,
        TimeSpan elapsed)
    {
        try
        {
            var settings = AppSettingsStore.LoadComponentTemplates();
            var orientationName = settings.OrientationComponentName.Trim();
            var region = matchedTemplate.Regions.FirstOrDefault(r =>
                r.IsOrientationMarker
                && string.Equals(r.Name.Trim(), orientationName, StringComparison.Ordinal));

            using var annotated = orientedBoard.Channels() == 1 ? EnsureBgr(orientedBoard) : orientedBoard.Clone();

            if (region is not null)
            {
                int bx = Math.Clamp((int)(region.RelX * annotated.Width), 0, annotated.Width - 1);
                int by = Math.Clamp((int)(region.RelY * annotated.Height), 0, annotated.Height - 1);
                int bw = Math.Clamp((int)(region.RelWidth * annotated.Width), 1, annotated.Width - bx);
                int bh = Math.Clamp((int)(region.RelHeight * annotated.Height), 1, annotated.Height - by);
                Cv2.Rectangle(annotated, new Rect(bx, by, bw, bh), new Scalar(0, 200, 255), 6);
            }

            var bitmap = await ToFrozenBitmapAsync(annotated);
            var scorePercent = Math.Round(matchScore * 100.0, 1);
            var rotationText = wasRotated ? " — đã xoay 180°" : string.Empty;
            var description =
                $"Mẫu \"{matchedTemplate.Name}\" — {orientationName}: {scorePercent}%{rotationText}";

            return PipelineStep.WithTiming(
                "Xác định chiều mạch",
                bitmap,
                description,
                elapsed);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Component mode: lock matching to the orientation template (board variant).
    /// White-circuit mode: search all partial templates per region — orientation only fixes 180° rotation.
    /// </summary>
    private TemplateEntry? ResolveMatchRestrictTemplate(TemplateEntry? orientationTemplate)
    {
        RefreshInspectionModeFromConfig();
        return IsWhiteCircuitInspectionMode ? null : orientationTemplate;
    }

    /// <summary>
    /// So từng tên trong AllowedRegionNames: lấy ứng viên đầu tiên đạt MinMatchSimilarityPercent trong nhóm.
    /// </summary>
    private async Task CompareWithTemplatesAsync(Mat newBoard, TemplateEntry? restrictToTemplate = null)
    {
        RefreshInspectionModeFromConfig();

        var (match, matchElapsed) = await Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            using var clone = newBoard.Clone();
            var result = _compositeMatchService.Match(clone, restrictToTemplate);
            sw.Stop();
            return (result, sw.Elapsed);
        });

        var regionResults = match?.RegionResults ?? [];
        var invertColors = match?.UseInvertedPassLogic ?? IsWhiteCircuitInspectionMode;
        var annotated = await DrawAnnotationsAsync(newBoard, regionResults, invertColors);
        if (annotated is null)
            annotated = await ToFrozenBitmapAsync(newBoard);

        ResultAnnotatedImage = annotated;
        AnnotatedImageReady?.Invoke(annotated);
        AppendRecognitionStep(annotated, match, matchElapsed);

        if (match is null)
        {
            RefreshMatchThresholdFromConfig();
            IsFullMatch = false;
            WhiteCircuitSummaryText = string.Empty;
            InspectionResults.Clear();
            OnPropertyChanged(nameof(HasInspectionResultList));
            StatusText = IsWhiteCircuitInspectionMode
                ? "Bo mạch đã cắt. Chưa có mẫu mạch trắng nào trong thư viện."
                : "Bo mạch đã cắt. Chưa có mẫu nào trong thư viện (hoặc thiếu vùng/ảnh).";
            RaiseInspectionCompleted(false);
            return;
        }

        RefreshMatchThreshold(match.MatchThresholdPercent);

        ApplyRegionResultsToGrids(match);
        IsFullMatch = match.IsFullMatch;

        var thresholdText = FormatThresholdPercent(match.MatchThresholdPercent);
        WhiteCircuitSummaryText = string.Empty;
        var present = match.PresentComponentCount;
        var total = match.TotalCount;
        var absent = match.AbsentComponentCount;

        StatusText = match.IsFullMatch
            ? $"Đạt — {present}/{total} vùng có linh kiện."
            : $"Chưa đạt — thiếu {absent}/{total} vùng linh kiện (ngưỡng {thresholdText}).";

        RaiseInspectionCompleted(match.IsFullMatch);
    }

    private void AppendRecognitionStep(
        System.Windows.Media.Imaging.BitmapSource annotated,
        CompositeTemplateMatchResult? match,
        TimeSpan matchElapsed)
    {
        string stepName = "Nhận diện linh kiện";
        string description = match is not null
            ? $"Nhận diện theo vùng — {match.PresentComponentCount}/{match.TotalCount} vùng có linh kiện"
            : "Nhận diện linh kiện theo vùng trên ảnh bo mạch đã cắt";

        Steps.Add(PipelineStep.WithTiming(stepName, annotated, description, matchElapsed));
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

    private void RefreshMatchThresholdFromConfig()
        => RefreshMatchThreshold(AppSettingsStore.LoadMatchThresholdPercent());

    public void RefreshInspectionModeFromConfig()
    {
        var settings = AppSettingsStore.LoadComponentTemplates();
        IsWhiteCircuitInspectionMode = TemplateLibraryPaths.IsWhiteCircuitMode(settings);
        InspectionModeLabel = IsWhiteCircuitInspectionMode ? "Mạch trắng" : "Linh kiện";
    }

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

    private void ApplyRegionResultsToGrids(CompositeTemplateMatchResult match)
    {
        MatchedRegions.Clear();
        DifferentRegions.Clear();
        InspectionResults.Clear();

        foreach (var r in match.RegionResults.OrderBy(x => x.Stt))
        {
            InspectionResults.Add(InspectionResultRowItem.From(r, match.UseInvertedPassLogic));

            if (r.HasComponent(match.UseInvertedPassLogic))
                MatchedRegions.Add(r);
            else
                DifferentRegions.Add(MissingComponentRowItem.From(r, match.UseInvertedPassLogic));
        }

        OnPropertyChanged(nameof(HasInspectionResultList));
        NotifyComponentCounts();
    }

    public void ClearInspectionResults()
    {
        MatchedRegions.Clear();
        DifferentRegions.Clear();
        InspectionResults.Clear();
        IsFullMatch = null;
        WhiteCircuitSummaryText = string.Empty;
        ResultAnnotatedImage = null;
        ResetInspectionTiming();
        OnPropertyChanged(nameof(HasInspectionResultList));
        NotifyComponentCounts();
    }

    private void NotifyComponentCounts()
    {
        OnPropertyChanged(nameof(SearchedComponentCount));
        OnPropertyChanged(nameof(MatchedComponentCount));
        OnPropertyChanged(nameof(MissingComponentCount));
        OnPropertyChanged(nameof(HasComponentResults));
    }

    /// <summary>
    /// Draws region boxes on the board: green = component present, red = missing.
    /// </summary>
    public async Task PublishAnnotatedImageAsync(Mat board, IReadOnlyList<RegionComparisonResult> results)
    {
        var invert = TemplateLibraryPaths.IsWhiteCircuitMode(AppSettingsStore.LoadComponentTemplates());
        var annotated = await DrawAnnotationsAsync(board, results, invert);
        if (annotated is not null)
            AnnotatedImageReady?.Invoke(annotated);
    }

    private static Task<System.Windows.Media.Imaging.BitmapSource?> DrawAnnotationsAsync(
        Mat board, IReadOnlyList<RegionComparisonResult> results, bool invertPassColors)
        => Task.Run(() => DrawAnnotations(board, results, invertPassColors));

    private static Task<System.Windows.Media.Imaging.BitmapSource> ToFrozenBitmapAsync(Mat mat)
        => Task.Run(() =>
        {
            var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(mat);
            bitmap.Freeze();
            return bitmap;
        });

    private static System.Windows.Media.Imaging.BitmapSource? DrawAnnotations(
        Mat board, IReadOnlyList<RegionComparisonResult> results, bool invertPassColors)
    {
        try
        {
            using var canvas = board.Channels() == 1 ? EnsureBgr(board) : board.Clone();
            var green = new Scalar(0, 200, 0);
            var red = new Scalar(0, 0, 220);
            const int thickness = 8;
            const double fontScale = 0.45;

            foreach (var r in results)
            {
                if (!r.HasBoardRect)
                    continue;

                var hasComponent = r.HasComponent(invertPassColors);
                var color = hasComponent ? green : red;
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

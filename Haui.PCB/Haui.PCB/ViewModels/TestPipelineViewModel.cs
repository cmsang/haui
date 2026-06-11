using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho TestPipelineWindow â€” chá»©a toÃ n bá»™ logic xá»­ lÃ½ áº£nh vÃ  phÃ¢n vÃ¹ng PCB.
/// TÃ¡ch biá»‡t hoÃ n toÃ n khá»i UI, tuÃ¢n theo SOLID: SRP, DIP.
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

    // â”€â”€â”€â”€ Sá»± kiá»‡n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>PhÃ¡t khi áº£nh Ä‘Ã£ xá»­ lÃ½ (bo máº¡ch Ä‘Ã£ cáº¯t) sáºµn sÃ ng â€” BitmapSource Ä‘Ã£ Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? ProcessedImageReady;

    /// <summary>PhÃ¡t khi áº£nh Ä‘Ã£ váº½ cÃ¡c vÃ¹ng so sÃ¡nh sáºµn sÃ ng â€” BitmapSource Ä‘Ã£ Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? AnnotatedImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // â”€â”€â”€â”€ Properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    /// <summary>NgÆ°á»¡ng % tá»« <c>setting.json</c> (cáº­p nháº­t má»—i láº§n so).</summary>
    public double MatchThresholdPercent => _matchThresholdPercent;

    public string DifferentRegionsHeader =>
        $"âš  VÃ¹ng khÃ¡c nhau (< {FormatThresholdPercent(_matchThresholdPercent)})";

    public string MatchedRegionsHeader =>
        $"âœ” VÃ¹ng giá»‘ng nhau (â‰¥ {FormatThresholdPercent(_matchThresholdPercent)})";

    /// <summary>CÃ¡c vÃ¹ng Ä‘áº¡t ngÆ°á»¡ng cáº¥u hÃ¬nh (giá»‘ng nhau).</summary>
    public ObservableCollection<RegionComparisonResult> MatchedRegions { get; } = [];

    /// <summary>CÃ¡c vÃ¹ng dÆ°á»›i ngÆ°á»¡ng cáº¥u hÃ¬nh (khÃ¡c nhau).</summary>
    public ObservableCollection<RegionComparisonResult> DifferentRegions { get; } = [];

    // â”€â”€â”€â”€ Khá»Ÿi táº¡o â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public TestPipelineViewModel(
        IPcbSegmentationService segmentation,
        ICompositeTemplateMatchService compositeMatchService)
    {
        _segmentation = segmentation;
        _compositeMatchService = compositeMatchService;
        RefreshMatchThresholdFromConfig();
    }

    // â”€â”€â”€â”€ Actions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Náº¡p áº£nh tá»« bÃªn ngoÃ i (vÃ­ dá»¥ tá»« camera chá»¥p) rá»“i tá»± Ä‘á»™ng cháº¡y pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        OnPropertyChanged(nameof(HasSource));

        _ = RunSegmentationAsync();
    }

    /// <summary>Náº¡p áº£nh tá»« Ä‘Æ°á»ng dáº«n file.</summary>
    public void LoadImageFromFile(string filePath)
    {
        _sourceMat?.Dispose();
        _sourceMat = Cv2.ImRead(filePath, ImreadModes.Color);

        if (_sourceMat.Empty())
        {
            StatusText = "KhÃ´ng thá»ƒ Ä‘á»c áº£nh.";
            OnPropertyChanged(nameof(HasSource));
            return;
        }

        StatusText = $"ÄÃ£ chá»n: {System.IO.Path.GetFileName(filePath)}";
        OnPropertyChanged(nameof(HasSource));
    }

    /// <summary>
    /// Cháº¡y pipeline: cáº¯t bo máº¡ch â†’ hiá»ƒn thá»‹ â†’ so sÃ¡nh vÃ¹ng vá»›i thÆ° viá»‡n máº«u.
    /// </summary>
    public async Task RunSegmentationAsync()
    {
        if (!HasSource)
        {
            StatusText = "Vui lÃ²ng chá»n áº£nh trÆ°á»›c.";
            return;
        }

        IsBusy = true;
        StatusText = "Äang xá»­ lÃ½...";
        MatchedRegions.Clear();
        DifferentRegions.Clear();

        using var source = _sourceMat!.Clone();
        Mat? newBoard = null;

        try
        {
            newBoard = await Task.Run(() => _segmentation.Segment(source));

            if (newBoard is null)
            {
                StatusText = "KhÃ´ng phÃ¡t hiá»‡n Ä‘Æ°á»£c bo máº¡ch. Thá»­ Ä‘iá»u chá»‰nh áº£nh.";
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
            StatusText = $"Lá»—i: {ex.Message}";
            ProcessedImageReady?.Invoke(null);
        }
        finally
        {
            newBoard?.Dispose();
            IsBusy = false;
        }
    }

    /// <summary>
    /// So tá»«ng tÃªn trong AllowedRegionNames: láº¥y á»©ng viÃªn Ä‘áº§u tiÃªn Ä‘áº¡t MinMatchSimilarityPercent trong nhÃ³m.
    /// Náº¿u chÆ°a Ä‘áº¡t Ä‘á»§, xoay bo máº¡ch 180Â° vÃ  so láº¡i.
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
            StatusText = "Bo máº¡ch Ä‘Ã£ cáº¯t. ChÆ°a cÃ³ máº«u nÃ o trong thÆ° viá»‡n (hoáº·c thiáº¿u vÃ¹ng/áº£nh).";
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

        var thresholdText = FormatThresholdPercent(match.MatchThresholdPercent);
        var rotationNote = usedRotation ? " (Ä‘Ã£ xoay áº£nh 180Â°)" : "";
        StatusText = match.IsFullMatch
            ? $"Äáº¡t â€” {match.MatchedCount}/{match.TotalCount} vÃ¹ng giá»‘ng (â‰¥ {thresholdText} má»—i tÃªn).{rotationNote}"
            : $"ChÆ°a Ä‘áº¡t â€” TB {match.AverageSimilarity:F1}%, {match.MatchedCount}/{match.TotalCount} vÃ¹ng giá»‘ng (ngÆ°á»¡ng {thresholdText}).{rotationNote}";

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
    /// Váº½ hÃ¬nh chá»¯ nháº­t lÃªn áº£nh bo máº¡ch: xanh lÃ¡ = giá»‘ng, Ä‘á» = khÃ¡c.
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

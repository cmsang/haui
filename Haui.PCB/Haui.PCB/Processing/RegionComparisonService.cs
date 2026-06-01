using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// So sánh từng vùng giữa ảnh mẫu và ảnh bo mạch mới bằng histogram correlation.
/// Tiền xử lý: resize → LAB-L → CLAHE → bilateral (cùng tham số cho mẫu và ảnh mới).
/// </summary>
public class RegionComparisonService : IRegionComparisonService
{
    private const int ComparisonPatchSize = 128;
    private const double ClaheClipLimit = 2.0;
    private static readonly Size ClaheTileGridSize = new(8, 8);
    private const int BilateralDiameter = 5;
    private const double BilateralSigmaColor = 50;
    private const double BilateralSigmaSpace = 50;

    private static readonly Size ComparisonPatchDimensions = new(ComparisonPatchSize, ComparisonPatchSize);
    private static readonly CLAHE SharedClahe = Cv2.CreateCLAHE(ClaheClipLimit, ClaheTileGridSize);

    public IReadOnlyList<RegionComparisonResult> Compare(
        Mat templateBoard,
        Mat newBoard,
        IReadOnlyList<TemplateRegion> regions)
    {
        var results = new List<RegionComparisonResult>(regions.Count);
        var matchThreshold = ComponentTemplateSettingsStore.LoadMatchThresholdPercent();

        foreach (var region in regions)
        {
            double similarity = CompareRegion(templateBoard, newBoard, region);

            // Tính tọa độ tuyệt đối của vùng trên ảnh bo mạch mới
            int bx = Math.Clamp((int)(region.RelX * newBoard.Width), 0, newBoard.Width - 1);
            int by = Math.Clamp((int)(region.RelY * newBoard.Height), 0, newBoard.Height - 1);
            int bw = Math.Clamp((int)(region.RelWidth * newBoard.Width), 1, newBoard.Width - bx);
            int bh = Math.Clamp((int)(region.RelHeight * newBoard.Height), 1, newBoard.Height - by);

            results.Add(new RegionComparisonResult
            {
                Name = region.Name,
                Similarity = Math.Round(similarity * 100.0, 1),
                BoardRect = new Rect(bx, by, bw, bh),
                MatchThresholdPercent = matchThreshold
            });
        }

        return results;
    }

    /// <summary>
    /// Cắt vùng từ cả hai ảnh rồi so sánh histogram. Trả về 0..1.
    /// </summary>
    private static double CompareRegion(Mat templateBoard, Mat newBoard, TemplateRegion region)
    {
        try
        {
            using var tCrop = CropRegion(templateBoard, region);
            using var nCrop = CropRegion(newBoard, region);

            if (tCrop is null || nCrop is null) return 0;

            using var tPrepared = PreparePatchForHistogram(tCrop);
            using var nPrepared = PreparePatchForHistogram(nCrop);

            using var tHist = new Mat();
            using var nHist = new Mat();

            int[] channels = [0];
            int[] histSize = [256];
            Rangef[] ranges = [new Rangef(0, 256)];

            Cv2.CalcHist([tPrepared], channels, null, tHist, 1, histSize, ranges);
            Cv2.CalcHist([nPrepared], channels, null, nHist, 1, histSize, ranges);

            Cv2.Normalize(tHist, tHist, 0, 1, NormTypes.MinMax);
            Cv2.Normalize(nHist, nHist, 0, 1, NormTypes.MinMax);

            double correlation = Cv2.CompareHist(tHist, nHist, HistCompMethods.Correl);

            // correlation trong [-1, 1]; clamp về [0, 1]
            return Math.Clamp(correlation, 0.0, 1.0);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Resize (Area khi thu nhỏ), kênh L của LAB, CLAHE, bilateral nhẹ — dùng chung cho mẫu và ảnh test.
    /// </summary>
    private static Mat PreparePatchForHistogram(Mat crop)
    {
        using var resized = new Mat();
        var interpolation = SelectResizeInterpolation(crop.Width, crop.Height);
        Cv2.Resize(crop, resized, ComparisonPatchDimensions, 0, 0, interpolation);

        using var lChannel = ExtractLabLChannel(resized);
        using var claheOut = new Mat();
        SharedClahe.Apply(lChannel, claheOut);

        var filtered = new Mat();
        Cv2.BilateralFilter(
            claheOut,
            filtered,
            BilateralDiameter,
            BilateralSigmaColor,
            BilateralSigmaSpace);

        return filtered;
    }

    private static Mat ExtractLabLChannel(Mat image)
    {
        using var bgr = EnsureBgr(image);
        using var lab = new Mat();
        Cv2.CvtColor(bgr, lab, ColorConversionCodes.BGR2Lab);

        var lChannel = new Mat();
        Cv2.ExtractChannel(lab, lChannel, 0);
        return lChannel;
    }

    private static Mat EnsureBgr(Mat image)
    {
        if (image.Channels() == 3)
            return image.Clone();

        var bgr = new Mat();
        Cv2.CvtColor(image, bgr, ColorConversionCodes.GRAY2BGR);
        return bgr;
    }

    private static InterpolationFlags SelectResizeInterpolation(int srcWidth, int srcHeight)
    {
        if (srcWidth > ComparisonPatchSize || srcHeight > ComparisonPatchSize)
            return InterpolationFlags.Area;

        return InterpolationFlags.Linear;
    }

    /// <summary>
    /// Cắt vùng từ ảnh theo tọa độ tương đối. Trả về null nếu vùng không hợp lệ.
    /// </summary>
    private static Mat? CropRegion(Mat board, TemplateRegion region)
    {
        int x = (int)(region.RelX * board.Width);
        int y = (int)(region.RelY * board.Height);
        int w = (int)(region.RelWidth * board.Width);
        int h = (int)(region.RelHeight * board.Height);

        // Đảm bảo không vượt biên
        x = Math.Clamp(x, 0, board.Width - 1);
        y = Math.Clamp(y, 0, board.Height - 1);
        w = Math.Clamp(w, 1, board.Width - x);
        h = Math.Clamp(h, 1, board.Height - y);

        if (w < 1 || h < 1) return null;

        var roi = new OpenCvSharp.Rect(x, y, w, h);
        return board[roi].Clone();
    }
}

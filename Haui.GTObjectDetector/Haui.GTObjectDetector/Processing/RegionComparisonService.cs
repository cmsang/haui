using Haui.GTObjectDetector.Models;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// So sánh từng vùng giữa ảnh mẫu và ảnh bo mạch mới bằng histogram correlation.
/// </summary>
public class RegionComparisonService : IRegionComparisonService
{
    public IReadOnlyList<RegionComparisonResult> Compare(
        Mat templateBoard,
        Mat newBoard,
        IReadOnlyList<TemplateRegion> regions)
    {
        var results = new List<RegionComparisonResult>(regions.Count);

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
                BoardRect = new Rect(bx, by, bw, bh)
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

            // Chuẩn hóa kích thước về giống nhau
            using var tResized = new Mat();
            using var nResized = new Mat();
            Cv2.Resize(tCrop, tResized, new OpenCvSharp.Size(64, 64));
            Cv2.Resize(nCrop, nResized, new OpenCvSharp.Size(64, 64));

            // Chuyển về grayscale trước khi so sánh để loại bỏ ảnh hưởng ánh sáng màu
            using var tGray = new Mat();
            using var nGray = new Mat();
            Cv2.CvtColor(tResized, tGray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(nResized, nGray, ColorConversionCodes.BGR2GRAY);

            using var tHist = new Mat();
            using var nHist = new Mat();

            int[] channels = [0];
            int[] histSize = [256];
            Rangef[] ranges = [new Rangef(0, 256)];

            Cv2.CalcHist([tGray], channels, null, tHist, 1, histSize, ranges);
            Cv2.CalcHist([nGray], channels, null, nHist, 1, histSize, ranges);

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

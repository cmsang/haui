using Haui.PCB.Models.Segmentation;
using Haui.PCB.Processing.Configuration;
using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>
/// Compares the orientation marker crop on the test board against each template that defines one.
/// Uses <see cref="TemplateMatchSimilarityService"/> (CCoeffNormed).
/// </summary>
public sealed class BoardOrientationDetectionService : IBoardOrientationDetectionService
{
    private readonly ITemplateLibraryService _libraryService;

    public BoardOrientationDetectionService(ITemplateLibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public BoardOrientationDetectionResult Detect(Mat newBoard)
    {
        if (newBoard.Empty())
            return BoardOrientationDetectionResult.Fail("Ảnh bo mạch rỗng.");

        var settings = AppSettingsStore.LoadComponentTemplates();

        var orientationName = settings.OrientationComponentName.Trim();
        if (string.IsNullOrEmpty(orientationName))
            return BoardOrientationDetectionResult.Skipped();

        var minScore = Math.Clamp(settings.MinOrientationMatchScore, 0, 1);
        var candidates = OrientationMarkerCache.GetCandidates(_libraryService, orientationName);
        if (candidates.Count == 0)
        {
            return BoardOrientationDetectionResult.Fail(
                $"Không có mẫu nào đánh dấu vị trí thành phần xác định chiều \"{orientationName}\".");
        }

        var firstPass = FindBestMatch(newBoard, candidates, minScore);
        if (firstPass is not null)
            return BoardOrientationDetectionResult.Ok(firstPass.Entry, firstPass.Score, requiresRotation180: false);

        using var rotated = new Mat();
        Cv2.Rotate(newBoard, rotated, RotateFlags.Rotate180);

        var secondPass = FindBestMatch(rotated, candidates, minScore);
        if (secondPass is not null)
            return BoardOrientationDetectionResult.Ok(secondPass.Entry, secondPass.Score, requiresRotation180: true);

        return BoardOrientationDetectionResult.Fail("Không tìm được chiều mạch.");
    }

    private static OrientationMatch? FindBestMatch(
        Mat newBoard,
        IReadOnlyList<CachedOrientationCandidate> candidates,
        double minScore)
    {
        OrientationMatch? best = null;
        using var grayBoard = ToGrayscaleBoard(newBoard);

        foreach (var candidate in candidates)
        {
            using var boardCrop = OrientationRegionCropper.Crop(grayBoard, candidate.Region);
            if (boardCrop is null || boardCrop.Empty() || candidate.GrayscaleCrop.Empty())
                continue;

            var score = TemplateMatchSimilarityService.CompareGrayscale(
                candidate.GrayscaleCrop,
                boardCrop);

            if (score < minScore)
                continue;

            if (best is null
                || score > best.Score
                || (Math.Abs(score - best.Score) <= 1e-9
                    && candidate.ComponentRegionCount > best.ComponentRegionCount))
            {
                best = new OrientationMatch(candidate.Entry, score, candidate.ComponentRegionCount);
            }
        }

        return best;
    }

    private static Mat ToGrayscaleBoard(Mat board)
    {
        if (board.Channels() == 1)
            return board.Clone();

        var gray = new Mat();
        Cv2.CvtColor(board, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private sealed record OrientationMatch(TemplateEntry Entry, double Score, int ComponentRegionCount);
}

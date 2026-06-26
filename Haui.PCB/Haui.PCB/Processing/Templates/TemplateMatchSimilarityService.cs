using OpenCvSharp;

namespace Haui.PCB.Processing.Templates;

/// <summary>Compares image patches using CCoeffNormed template matching.</summary>
public static class TemplateMatchSimilarityService
{
    private const int MinTemplateSize = 8;

    public static double Compare(Mat templateA, Mat templateB)
    {
        if (templateA.Empty() || templateB.Empty())
            return 0;

        using var grayA = ToGrayscale(templateA);
        using var grayB = ToGrayscale(templateB);
        return CompareGrayscale(grayA, grayB);
    }

    /// <summary>Compares two grayscale patches without channel conversion.</summary>
    public static double CompareGrayscale(Mat grayA, Mat grayB)
    {
        if (grayA.Width < MinTemplateSize || grayA.Height < MinTemplateSize
            || grayB.Width < MinTemplateSize || grayB.Height < MinTemplateSize)
            return 0;

        if (grayA.Size() == grayB.Size())
            return ScoreMatch(grayA, grayB);

        var (search, template) = LargerIsSearch(grayA, grayB);
        return ScoreMatch(search, template);
    }

    private static Mat ToGrayscale(Mat source)
    {
        if (source.Channels() == 1)
            return source.Clone();

        var gray = new Mat();
        Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    private static (Mat Search, Mat Template) LargerIsSearch(Mat a, Mat b)
    {
        int areaA = a.Width * a.Height;
        int areaB = b.Width * b.Height;
        return areaA >= areaB ? (a, b) : (b, a);
    }

    private static double ScoreMatch(Mat search, Mat template)
    {
        if (search.Width < template.Width || search.Height < template.Height)
            return 0;

        using var result = new Mat();
        Cv2.MatchTemplate(search, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
        return Math.Clamp(maxVal, 0, 1);
    }
}

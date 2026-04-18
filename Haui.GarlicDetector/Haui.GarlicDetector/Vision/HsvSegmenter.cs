using Haui.GarlicDetector.Models;
using OpenCvSharp;

namespace Haui.GarlicDetector.Vision;

/// <summary>
/// Thuật toán phân vùng màu sắc theo không gian màu HSV để phát hiện tỏi.
/// Quy trình: lọc ngưỡng HSV → hình thái học (mở + đóng) → tìm contour.
/// Nhận frame đã được tiền xử lý (HSV) từ <see cref="IImagePreprocessor"/>.
/// </summary>
public sealed class HsvSegmenter : IGarlicSegmentor
{
    // ─── Ngưỡng HSV ──────────────────────────────────────────────────────────

    /// <summary>Ngưỡng Hue tối thiểu (0–179).</summary>
    public int HMin { get; set; } = 0;

    /// <summary>
    /// Ngưỡng Hue tối đa (0–179).
    /// Tỏi trắng/kem có Saturation rất thấp → kênh Hue không đáng tin cậy.
    /// Đặt 179 để bỏ qua ràng buộc Hue, dựa vào S và V để lọc màu trắng.
    /// </summary>
    public int HMax { get; set; } = 179;

    /// <summary>Ngưỡng Saturation tối thiểu (0–255).</summary>
    public int SMin { get; set; } = 0;

    /// <summary>
    /// Ngưỡng Saturation tối đa (0–255).
    /// Giữ thấp (≤ 60) để chỉ bắt màu trắng/kem có sắc yếu, loại nền màu sắc mạnh.
    /// </summary>
    public int SMax { get; set; } = 60;

    /// <summary>
    /// Ngưỡng Value tối thiểu (0–255).
    /// Tỏi trắng sáng → V cao; đặt 170 để loại bóng tối và tỏi hỏng tối màu.
    /// </summary>
    public int VMin { get; set; } = 170;

    /// <summary>Ngưỡng Value tối đa (0–255).</summary>
    public int VMax { get; set; } = 255;

    /// <summary>Diện tích tối thiểu (pixel²) để vùng được coi là tỏi hợp lệ.</summary>
    public double MinArea { get; set; } = 800;

    // ─── Phân vùng ───────────────────────────────────────────────────────────

    /// <summary>
    /// Phân vùng frame HSV thành danh sách vùng tỏi, mỗi vùng kèm diện tích contour thực tế
    /// và độ tròn kết hợp (<see cref="GarlicSegmentResult.Circularity"/>).
    /// <para>
    /// Độ tròn được tính bằng trung bình có trọng số đều của hai chỉ số độc lập:
    /// <list type="bullet">
    ///   <item>
    ///     <term>Isoperimetric circularity</term>
    ///     <description>
    ///       <c>4π·A / P²</c> — đo mức độ mượt và tròn của <b>biên contour</b>.
    ///       Giá trị = 1 khi hình tròn hoàn hảo; giảm khi biên lởm chởm hoặc có góc cạnh.
    ///       Dùng <see cref="ContourApproximationModes.ApproxNone"/> để giữ toàn bộ điểm
    ///       biên, tránh chu vi bị rút ngắn khi dùng <c>ApproxSimple</c>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>MinEnclosingCircle ratio</term>
    ///     <description>
    ///       <c>A_contour / (π·r²)</c> — đo mức độ <b>lấp đầy vòng tròn bao ngoài nhỏ nhất</b>.
    ///       Không phụ thuộc vào chu vi nên ổn định khi biên bị nhiễu pixel nhỏ.
    ///       Bị kéo thấp khi hình có dạng oval dài hoặc lõm sâu.
    ///     </description>
    ///   </item>
    /// </list>
    /// Kết hợp: <c>circularity = (isoCircularity + enclosingRatio) / 2</c>.<br/>
    /// Mỗi chỉ số bù đắp điểm yếu của chỉ số kia — tránh đánh giá tốt oan
    /// khi hình oval mượt (iso cao nhưng enclosing thấp) hoặc hình tròn biên nhiễu
    /// (enclosing cao nhưng iso thấp).
    /// </para>
    /// </summary>
    /// <param name="preprocessedFrame">Frame HSV đã được tiền xử lý bởi <see cref="IImagePreprocessor"/>.</param>
    /// <returns>
    /// Danh sách <see cref="GarlicSegmentResult"/> — mỗi phần tử chứa bounding rect,
    /// diện tích contour (px²) và circularity ∈ [0, 1].
    /// </returns>
    public List<GarlicSegmentResult> Segment(Mat preprocessedFrame)
    {
        // Bước 1: Lọc các pixel nằm trong ngưỡng HSV chỉ định
        using var mask = new Mat();
        Cv2.InRange(
            preprocessedFrame,
            new Scalar(HMin, SMin, VMin),
            new Scalar(HMax, SMax, VMax),
            mask);

        // Bước 2: Hình thái học
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(7, 7));
        using var opened = new Mat();
        Cv2.MorphologyEx(mask,   opened, MorphTypes.Open,  kernel, iterations: 2);
        using var closed = new Mat();
        Cv2.MorphologyEx(opened, closed, MorphTypes.Close, kernel, iterations: 3);

        // Bước 3: Tìm contour — dùng ApproxNone để giữ đủ điểm biên cho ArcLength chính xác
        Cv2.FindContours(
            closed,
            out var contours,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxNone);

        // Bước 4: Lọc theo diện tích và tính circularity kết hợp
        var results = new List<GarlicSegmentResult>();
        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area < MinArea) continue;

            // ── Chỉ số 1: Isoperimetric circularity = 4π·A / P² ──────────────
            // Đo mức độ mượt và tròn của BIÊN contour.
            // ApproxNone giữ toàn bộ điểm pixel trên biên → P không bị rút ngắn
            // như khi dùng ApproxSimple (chỉ giữ đầu/cuối đoạn thẳng).
            // Điểm yếu: oval dài có biên mượt vẫn cho iso cao (~0.78) dù không tròn.
            double perimeter      = Cv2.ArcLength(contour, closed: true);
            double isoCircularity = perimeter > 0
                ? Math.Clamp(4.0 * Math.PI * area / (perimeter * perimeter), 0.0, 1.0)
                : 0.0;

            // ── Chỉ số 2: MinEnclosingCircle ratio = A_contour / (π·r²) ─────────
            // Đo mức lấp đầy vòng tròn bao ngoài nhỏ nhất của contour.
            // Không dùng chu vi → ổn định khi biên bị nhiễu pixel nhỏ.
            // Điểm yếu: hình tròn có lỗ nhỏ hoặc lõm sâu vẫn bị kéo thấp.
            Cv2.MinEnclosingCircle(contour, out _, out float radius);
            double circleArea     = Math.PI * radius * radius;
            double enclosingRatio = circleArea > 0
                ? Math.Clamp(area / circleArea, 0.0, 1.0)
                : 0.0;

            // ── Kết hợp trọng số đều (0.5 / 0.5) ───────────────────────────────
            // Hai chỉ số bù đắp điểm yếu cho nhau:
            //   • Oval mượt  → iso cao (~0.88), enclosing thấp (~0.55) → kết hợp ~0.71 ✓
            //   • Tròn nhiễu → iso thấp (~0.55), enclosing cao (~0.85) → kết hợp ~0.70 ✓
            //   • Tỏi lành   → cả hai cao (~0.85, ~0.80)               → kết hợp ~0.82 ✓
            //   • Tỏi hỏng   → cả hai thấp (~0.45, ~0.50)             → kết hợp ~0.48 ✓
            // Trọng số có thể điều chỉnh sau khi có dữ liệu thực tế để calibrate.
            double circularity = (isoCircularity + enclosingRatio) / 2.0;

            results.Add(new GarlicSegmentResult(
                BoundingRect: Cv2.BoundingRect(contour),
                Area:         area,
                Circularity:  circularity));
        }

        return results;
    }
}

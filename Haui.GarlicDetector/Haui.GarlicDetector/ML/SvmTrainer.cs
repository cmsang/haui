using OpenCvSharp;
using OpenCvSharp.ML;

namespace Haui.GarlicDetector.ML;

/// <summary>Tham số cấu hình cho quá trình huấn luyện SVM.</summary>
public sealed record SvmConfig(double C, double Gamma, int ImageSize);

/// <summary>
/// Huấn luyện SVM từ thư mục ảnh đã gán nhãn và lưu model ra file XML.
/// <para>
/// SVM phân 2 lớp:
/// <list type="bullet">
///   <item><b>Label 0 — Tỏi bình thường</b>: gộp cả <c>toi_to_*</c> lẫn <c>toi_nho_*</c></item>
///   <item><b>Label 1 — Tỏi hỏng</b>: file <c>toi_hong_*</c></item>
/// </list>
/// </para>
/// </summary>
public sealed class SvmTrainer
{
    private readonly IFeatureExtractor _extractor;

    public SvmTrainer(IFeatureExtractor extractor)
    {
        _extractor = extractor;
    }

    /// <summary>
    /// Đọc toàn bộ ảnh trong <paramref name="dataFolder"/>, trích đặc trưng,
    /// huấn luyện SVM RBF rồi lưu model tại <paramref name="outputPath"/>.
    /// </summary>
    /// <returns>Thông báo kết quả (số mẫu, thời gian…).</returns>
    public async Task<string> TrainAsync(
        string              dataFolder,
        string              outputPath,
        SvmConfig           config,
        IProgress<string>?  progress = null)
    {
        return await Task.Run(() =>
        {
            progress?.Report("Đang quét thư mục dữ liệu...");

            var imageFiles = Directory
                .EnumerateFiles(dataFolder, "*.png", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(dataFolder, "*.jpg", SearchOption.AllDirectories))
                .Concat(Directory.EnumerateFiles(dataFolder, "*.bmp", SearchOption.AllDirectories))
                .ToList();

            if (imageFiles.Count == 0)
                throw new InvalidOperationException("Không tìm thấy ảnh nào trong thư mục dữ liệu.");

            progress?.Report($"Tìm thấy {imageFiles.Count} ảnh. Đang trích xuất đặc trưng...");

            var featureList = new List<float[]>();
            var labelList   = new List<int>();
            int skipped     = 0;

            // Dùng GarlicFeatureExtractor mới với imageSize từ config
            var extractor = new GarlicFeatureExtractor(config.ImageSize);

            foreach (var file in imageFiles)
            {
                int label = InferLabel(file);
                if (label < 0) { skipped++; continue; }

                using var mat = Cv2.ImRead(file, ImreadModes.Color);
                if (mat.Empty()) { skipped++; continue; }

                try
                {
                    var feat = extractor.Extract(mat);
                    featureList.Add(feat);
                    labelList.Add(label);
                }
                catch
                {
                    skipped++;
                }
            }

            int total = featureList.Count;
            if (total == 0)
                throw new InvalidOperationException("Không trích xuất được đặc trưng từ bất kỳ ảnh nào.");

            progress?.Report($"Trích xuất xong {total} mẫu (bỏ qua {skipped}). Đang huấn luyện SVM...");

            // ─── Tạo training matrix ─────────────────────────────────────────
            int dim = featureList[0].Length;
            using var trainData = new Mat(total, dim, MatType.CV_32F);
            using var labels    = new Mat(total, 1,   MatType.CV_32S);

            for (int i = 0; i < total; i++)
            {
                for (int j = 0; j < dim; j++)
                    trainData.At<float>(i, j) = featureList[i][j];

                labels.At<int>(i, 0) = labelList[i];
            }

            // ─── Đếm số mẫu theo nhãn ────────────────────────────────────────
            int nBinhThuong = labelList.Count(l => l == 0);
            int nHong       = labelList.Count(l => l == 1);
            progress?.Report($"  Bình thường: {nBinhThuong} | Tỏi hỏng: {nHong}");

            // ─── Huấn luyện SVM ──────────────────────────────────────────────
            var sw = System.Diagnostics.Stopwatch.StartNew();

            using var svm = SVM.Create();
            svm.Type         = SVM.Types.CSvc;
            svm.KernelType   = SVM.KernelTypes.Rbf;
            svm.C            = config.C;
            svm.Gamma        = config.Gamma;
            svm.TermCriteria = new TermCriteria(CriteriaTypes.MaxIter | CriteriaTypes.Eps, 1000, 1e-6);

            // Huấn luyện trực tiếp từ Mat (không cần TrainData.Create)
            bool ok = svm.Train(trainData, SampleTypes.RowSample, labels);

            sw.Stop();

            if (!ok)
                throw new InvalidOperationException("SVM.Train() trả về false — kiểm tra dữ liệu huấn luyện.");

            // ─── Lưu model ───────────────────────────────────────────────────
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            svm.Save(outputPath);

            progress?.Report($"✅ Hoàn thành! Thời gian: {sw.Elapsed.TotalSeconds:F1}s");
            return $"Đã huấn luyện {total} mẫu (bình thường: {nBinhThuong}, tỏi hỏng: {nHong}) " +
                   $"trong {sw.Elapsed.TotalSeconds:F1}s. Model lưu tại:\n{outputPath}";
        });
    }

    // ─── Suy luận nhãn từ tên file ───────────────────────────────────────────
    // Label 0 = tỏi bình thường (gộp toi_to + toi_nho)
    // Label 1 = tỏi hỏng

    private static int InferLabel(string filePath)
    {
        var name = Path.GetFileName(filePath).ToLowerInvariant();
        if (name.StartsWith("toi_to_"))   return 0; // bình thường
        if (name.StartsWith("toi_nho_"))  return 0; // bình thường
        if (name.StartsWith("toi_hong_")) return 1; // hỏng

        // Hỗ trợ thêm tổ chức theo thư mục con
        var dir = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "").ToLowerInvariant();
        if (dir is "toi_to"  or "toi_nho" or "binh_thuong" or "0") return 0;
        if (dir is "toi_hong" or "hong"   or "1")                  return 1;

        return -1; // Không xác định được nhãn
    }
}

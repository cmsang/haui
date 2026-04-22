using OpenCvSharp;
using OpenCvSharp.ML;

namespace Haui.GarlicDetector.ML;

/// <summary>Tham số cấu hình cho quá trình huấn luyện SVM.</summary>
public sealed record SvmConfig(double C, double Gamma, int ImageSize);

/// <summary>
/// Triển khai <see cref="IGarlicClassifier"/>: bao gồm toàn bộ vòng đời SVM
/// (Train → Save → Load → Predict) trong một class duy nhất.
/// </summary>
public sealed class SvmClassifier : IGarlicClassifier, IDisposable
{
    private readonly IFeatureExtractor _extractor;
    private SVM? _svm;

    public SvmClassifier(IFeatureExtractor extractor)
    {
        _extractor = extractor;
    }

    // ─── IsLoaded ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool IsLoaded => _svm is not null;

    // ─── Load ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Không tìm thấy file model SVM: {path}");

        _svm?.Dispose();
        _svm = SVM.Load(path);
    }

    // ─── Train ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> TrainAsync(
        string             dataFolder,
        string             outputPath,
        SvmConfig          config,
        IProgress<string>? progress = null)
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

            foreach (var file in imageFiles)
            {
                int label = InferLabel(file);
                if (label < 0) { skipped++; continue; }

                using var mat = Cv2.ImRead(file, ImreadModes.Color);
                if (mat.Empty()) { skipped++; continue; }

                try
                {
                    var feat = _extractor.Extract(mat);
                    featureList.Add(feat);
                    labelList.Add(label);
                }
                catch { skipped++; }
            }

            int total = featureList.Count;
            if (total == 0)
                throw new InvalidOperationException("Không trích xuất được đặc trưng từ bất kỳ ảnh nào.");

            progress?.Report($"Trích xuất xong {total} mẫu (bỏ qua {skipped}). Đang huấn luyện SVM...");

            // ─── Tạo training matrix ──────────────────────────────────────
            int dim = featureList[0].Length;
            using var trainData = new Mat(total, dim, MatType.CV_32F);
            using var labels    = new Mat(total, 1,   MatType.CV_32S);

            for (int i = 0; i < total; i++)
            {
                for (int j = 0; j < dim; j++)
                    trainData.At<float>(i, j) = featureList[i][j];

                labels.At<int>(i, 0) = labelList[i];
            }

            int nBinhThuong = labelList.Count(l => l == 0);
            int nHong       = labelList.Count(l => l == 1);
            progress?.Report($"  Bình thường: {nBinhThuong} | Tỏi hỏng: {nHong}");

            // ─── Huấn luyện SVM ───────────────────────────────────────────
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _svm?.Dispose();
            _svm = SVM.Create();
            _svm.Type         = SVM.Types.CSvc;
            _svm.KernelType   = SVM.KernelTypes.Rbf;
            _svm.C            = config.C;
            _svm.Gamma        = config.Gamma;
            _svm.TermCriteria = new TermCriteria(CriteriaTypes.MaxIter | CriteriaTypes.Eps, 1000, 1e-6);

            bool ok = _svm.Train(trainData, SampleTypes.RowSample, labels);
            sw.Stop();

            if (!ok)
                throw new InvalidOperationException("SVM.Train() trả về false — kiểm tra dữ liệu huấn luyện.");

            // ─── Lưu model ────────────────────────────────────────────────
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            _svm.Save(outputPath);

            progress?.Report($"✅ Hoàn thành! Thời gian: {sw.Elapsed.TotalSeconds:F1}s");
            return $"Đã huấn luyện {total} mẫu (bình thường: {nBinhThuong}, tỏi hỏng: {nHong}) " +
                   $"trong {sw.Elapsed.TotalSeconds:F1}s. Model lưu tại:\n{outputPath}";
        });
    }

    // ─── Predict ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public int Predict(float[] features)
    {
        if (_svm is null)
            throw new InvalidOperationException("Model SVM chưa được nạp. Gọi Load() hoặc TrainAsync() trước.");

        // Tạo Mat 1 hàng × N cột (CV_32F) từ vector đặc trưng
        using var sample = new Mat(1, features.Length, MatType.CV_32F);
        for (int i = 0; i < features.Length; i++)
            sample.At<float>(0, i) = features[i];

        return (int)_svm.Predict(sample);
    }

    // ─── Suy luận nhãn từ tên file ────────────────────────────────────────
    // Label 0 = tỏi bình thường (gộp toi_to + toi_nho)
    // Label 1 = tỏi hỏng

    private static int InferLabel(string filePath)
    {
        var name = Path.GetFileName(filePath).ToLowerInvariant();
        if (name.StartsWith("toi_to_"))   return 0;
        if (name.StartsWith("toi_nho_"))  return 0;
        if (name.StartsWith("toi_hong_")) return 1;

        var dir = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "").ToLowerInvariant();
        if (dir is "toi_to" or "toi_nho" or "binh_thuong" or "0") return 0;
        if (dir is "toi_hong" or "hong"  or "1")                  return 1;

        return -1;
    }

    // ─── Dispose ──────────────────────────────────────────────────────────

    public void Dispose()
    {
        _svm?.Dispose();
        _svm = null;
    }
}

namespace Haui.GarlicDetector.ML;

/// <summary>
/// Interface thống nhất cho SVM: Train + Load + Predict.
/// Tuân thủ ISP — chỉ bao gồm các hành vi liên quan đến vòng đời model SVM.
/// </summary>
public interface IGarlicClassifier : IDisposable
{
    /// <summary>Model đã được nạp và sẵn sàng dự đoán.</summary>
    bool IsLoaded { get; }

    /// <summary>Nạp model SVM từ file XML đã lưu.</summary>
    void Load(string path);

    /// <summary>
    /// Huấn luyện SVM từ thư mục ảnh đã gán nhãn và lưu model ra <paramref name="outputPath"/>.
    /// </summary>
    /// <returns>Thông báo kết quả (số mẫu, thời gian…).</returns>
    Task<string> TrainAsync(
        string             dataFolder,
        string             outputPath,
        SvmConfig          config,
        IProgress<string>? progress = null);

    /// <summary>
    /// Dự đoán nhãn SVM Stage 1.
    /// </summary>
    /// <param name="features">Vector đặc trưng từ <see cref="IFeatureExtractor"/>.</param>
    /// <returns>
    /// <c>0</c> = tỏi bình thường (tiếp tục Stage 2);<br/>
    /// <c>1</c> = tỏi hỏng (kết thúc).
    /// </returns>
    int Predict(float[] features);
}

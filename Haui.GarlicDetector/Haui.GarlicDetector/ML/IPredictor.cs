namespace Haui.GarlicDetector.ML;

/// <summary>
/// Dự đoán nhãn SVM Stage 1 (bình thường = 0 / hỏng = 1) từ vector đặc trưng.
/// </summary>
public interface IPredictor
{
    /// <summary>Model đã được nạp và sẵn sàng dự đoán.</summary>
    bool IsLoaded { get; }

    /// <summary>Nạp model SVM từ file XML đã lưu.</summary>
    /// <param name="path">Đường dẫn đến file <c>.xml</c>.</param>
    void Load(string path);

    /// <summary>
    /// Dự đoán nhãn SVM Stage 1.
    /// </summary>
    /// <param name="features">Vector đặc trưng do <see cref="IFeatureExtractor"/> tạo ra.</param>
    /// <returns>
    /// <c>0</c> = tỏi bình thường (tiếp tục sang Stage 2 phân kích thước);<br/>
    /// <c>1</c> = tỏi hỏng (kết thúc ngay).
    /// </returns>
    int Predict(float[] features);
}

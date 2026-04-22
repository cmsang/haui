using OpenCvSharp;
using OpenCvSharp.ML;

namespace Haui.GarlicDetector.ML;

/// <summary>
/// Bọc <see cref="SVM"/> của OpenCvSharp để nạp model đã train và dự đoán.
/// Chỉ thực hiện <b>Load + Predict</b> — không train lại.
/// <para>
/// SVM nhận vector đặc trưng từ <see cref="GarlicFeatureExtractor"/> và trả về:<br/>
/// <c>0</c> = tỏi bình thường, <c>1</c> = tỏi hỏng.
/// </para>
/// </summary>
public sealed class SvmClassifier : IPredictor, IDisposable
{
    private SVM? _svm;

    /// <inheritdoc/>
    public bool IsLoaded => _svm != null;

    /// <inheritdoc/>
    public void Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Không tìm thấy file model SVM: {path}");

        _svm?.Dispose();
        _svm = SVM.Load(path);
    }

    /// <inheritdoc/>
    public int Predict(float[] features)
    {
        if (_svm == null)
            throw new InvalidOperationException("Model SVM chưa được nạp. Gọi Load() trước.");

        // Tạo Mat 1 hàng × N cột (CV_32F) từ vector đặc trưng
        using var sample = new Mat(1, features.Length, MatType.CV_32F);
        for (int i = 0; i < features.Length; i++)
            sample.At<float>(0, i) = features[i];

        float result = _svm.Predict(sample);
        return (int)result;
    }

    public void Dispose()
    {
        _svm?.Dispose();
        _svm = null;
    }
}

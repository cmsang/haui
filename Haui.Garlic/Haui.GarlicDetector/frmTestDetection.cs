using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.ML;
using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Services;
using Haui.GarlicDetector.Vision;

namespace Haui.GarlicDetector;

public partial class frmTestDetection : Form
{
    private Bitmap? _originalImage;

    // Pipeline tạm dùng cho nhận diện ảnh tĩnh
    private readonly HsvSegmenter _segmenter;
    private readonly HsvGarlicPreprocessor _preprocessor;
    private readonly GarlicPipeline _pipeline;

    // Ảnh nét nhất từ camera (truyền từ frmMain), tự động chạy nhận diện khi form hiển thị
    private readonly Bitmap? _autoDetectImage;

    public frmTestDetection() : this(null) { }

    /// <summary>
    /// Khởi tạo form với ảnh được chụp sẵn từ camera.
    /// Khi <paramref name="capturedImage"/> khác null, form sẽ tự động hiển thị ảnh và
    /// chạy nhận diện ngay khi form xuất hiện.
    /// </summary>
    public frmTestDetection(Bitmap? capturedImage)
    {
        InitializeComponent();

        _autoDetectImage = capturedImage != null ? (Bitmap)capturedImage.Clone() : null;

        _segmenter = new HsvSegmenter();
        _preprocessor = new HsvGarlicPreprocessor();

        // Nạp SVM classifier nếu có model
        IGarlicClassifier? classifier = null;
        var modelPath = AppSettings.Instance.SvmModelPath;
        if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
        {
            try
            {
                var svm = new SvmClassifier(new GarlicFeatureExtractor(AppSettings.Instance.SvmTrainImageSize));
                svm.Load(modelPath);
                classifier = svm;
            }
            catch { /* bỏ qua lỗi nạp model */ }
        }

        _pipeline = new GarlicPipeline(
            new CameraService(),
            _preprocessor,
            _segmenter,
            classifier,
            new GarlicFeatureExtractor(AppSettings.Instance.SvmTrainImageSize));
    }

    // ─── Tự động nhận diện khi form mở với ảnh từ camera ────────────────────

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_autoDetectImage == null) return;

        // Hiển thị ảnh nét nhất lên picOriginal
        _originalImage?.Dispose();
        _originalImage = (Bitmap)_autoDetectImage.Clone();
        picOriginal.Image?.Dispose();
        picOriginal.Image = (Bitmap)_originalImage.Clone();

        lblInfo.Text = "Ảnh từ camera đã tải. Đang nhận diện...";

        // Kích hoạt nhận diện ngay lập tức
        btnDetect_Click(this, EventArgs.Empty);
    }

    // ─── Chọn ảnh ────────────────────────────────────────────────────────────

    private void btnLoadImage_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Chọn ảnh tỏi",
            Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|All files|*.*",
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            _originalImage?.Dispose();
            _originalImage = new Bitmap(dlg.FileName);

            picOriginal.Image?.Dispose();
            picOriginal.Image = (Bitmap)_originalImage.Clone();

            // Xóa kết quả cũ
            picResult.Image?.Dispose();
            picResult.Image = null;
            lblInfo.Text = "Ảnh đã tải. Nhấn \"Nhận diện\" để phân loại.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─── Nhận diện ───────────────────────────────────────────────────────────

    private async void btnDetect_Click(object sender, EventArgs e)
    {
        if (_originalImage == null)
        {
            MessageBox.Show("Vui lòng chọn ảnh trước.", "Chưa có ảnh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        btnDetect.Enabled = false;
        btnLoadImage.Enabled = false;
        lblInfo.Text = "Đang nhận diện...";

        try
        {
            var source = (Bitmap)_originalImage.Clone();

            // Chạy phân vùng + phân loại trên thread-pool
            var regions = await Task.Run(() => _pipeline.SegmentFrame(source, null));

            // Vẽ annotation lên bản sao ảnh gốc
            var resultBitmap = (Bitmap)_originalImage.Clone();
            GarlicPipeline.DrawRegions(resultBitmap, regions);

            picResult.Image?.Dispose();
            picResult.Image = resultBitmap;
            source.Dispose();

            // Thống kê kết quả
            int toTo  = regions.Count(r => r.FinalLabel == GarlicLabel.ToTo);
            int toNho = regions.Count(r => r.FinalLabel == GarlicLabel.ToNho);
            int toHong = regions.Count(r => r.FinalLabel == GarlicLabel.ToHong);
            int unknown = regions.Count(r => r.FinalLabel == null);

            lblInfo.Text = regions.Count == 0
                ? "Không phát hiện tỏi nào."
                : $"Phát hiện {regions.Count} củ — Tỏi to: {toTo}  |  Tỏi nhỏ: {toNho}  |  Tỏi hỏng: {toHong}" +
                  (unknown > 0 ? $"  |  Chưa phân loại: {unknown}" : string.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi nhận diện: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblInfo.Text = "Nhận diện thất bại.";
        }
        finally
        {
            btnDetect.Enabled = true;
            btnLoadImage.Enabled = true;
        }
    }

    // ─── Dọn dẹp ─────────────────────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _originalImage?.Dispose();
        _autoDetectImage?.Dispose();
        _pipeline.Dispose();
    }
}

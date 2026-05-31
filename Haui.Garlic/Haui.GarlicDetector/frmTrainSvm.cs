using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.ML;
using Haui.GarlicDetector.Vision;

namespace Haui.GarlicDetector;

/// <summary>
/// Form huấn luyện SVM cho bộ phân loại tỏi.
/// Đọc thông số từ <see cref="AppSettings"/>, cho phép train và lưu model.
/// </summary>
public partial class frmTrainSvm : Form
{
    private CancellationTokenSource? _cts;

    public frmTrainSvm()
    {
        InitializeComponent();
    }

    // ─── Load form ───────────────────────────────────────────────────────────

    private void frmTrainSvm_Load(object sender, EventArgs e)
    {
        var s = AppSettings.Instance;

        txtTrainFolder.Text  = s.TrainDataFolder ?? string.Empty;
        txtOutputFolder.Text = s.SvmModelPath is not null
            ? Path.GetDirectoryName(s.SvmModelPath) ?? string.Empty
            : string.Empty;

        numC.Value         = (decimal)Math.Clamp(s.SvmC,         0.001, 10000);
        numGamma.Value     = (decimal)Math.Clamp(s.SvmGamma,     0.001, 100);
        numImageSize.Value = (decimal)Math.Clamp(s.SvmTrainImageSize, 16, 256);

        AppendLog("Đã tải thông số từ settings.json.");
        AppendLog($"Model hiện tại: {(string.IsNullOrWhiteSpace(s.SvmModelPath) ? "(chưa có)" : s.SvmModelPath)}");
    }

    // ─── Chọn thư mục dữ liệu huấn luyện ────────────────────────────────────

    private void btnBrowseData_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description            = "Chọn thư mục chứa ảnh đã gán nhãn",
            UseDescriptionForTitle = true,
        };

        if (!string.IsNullOrWhiteSpace(txtTrainFolder.Text) && Directory.Exists(txtTrainFolder.Text))
            dlg.InitialDirectory = txtTrainFolder.Text;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtTrainFolder.Text = dlg.SelectedPath;
        SaveSettings();
    }

    // ─── Chọn thư mục lưu model ──────────────────────────────────────────────

    private void btnBrowseOutput_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description            = "Chọn thư mục lưu file model SVM (.xml)",
            UseDescriptionForTitle = true,
        };

        if (!string.IsNullOrWhiteSpace(txtOutputFolder.Text) && Directory.Exists(txtOutputFolder.Text))
            dlg.InitialDirectory = txtOutputFolder.Text;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtOutputFolder.Text = dlg.SelectedPath;
        SaveSettings();
    }

    // ─── Huấn luyện ──────────────────────────────────────────────────────────

    private async void btnTrain_Click(object sender, EventArgs e)
    {
        var dataFolder   = txtTrainFolder.Text.Trim();
        var outputFolder = txtOutputFolder.Text.Trim();

        if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
        {
            MessageBox.Show("Vui lòng chọn thư mục dữ liệu huấn luyện hợp lệ.",
                "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            MessageBox.Show("Vui lòng chọn thư mục lưu model.",
                "Thiếu thư mục lưu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Lưu thông số trước khi train
        SaveSettings();

        var config = new SvmConfig(
            C:         (double)numC.Value,
            Gamma:     (double)numGamma.Value,
            ImageSize: (int)numImageSize.Value);

        var outputPath = Path.Combine(outputFolder, "garlic_svm.xml");

        SetTrainingMode(true);
        rtxLog.Clear();
        AppendLog($"Bắt đầu huấn luyện SVM...");
        AppendLog($"  Dữ liệu  : {dataFolder}");
        AppendLog($"  Lưu tại  : {outputPath}");
        AppendLog($"  C={config.C}  Gamma={config.Gamma}  ImageSize={config.ImageSize}px");
        AppendLog(new string('─', 55));

        _cts = new CancellationTokenSource();

        try
        {
            var progress = new Progress<string>(msg => AppendLog(msg));

                // Xây dựng preprocessor + segmentor từ AppSettings (giống pipeline chính)
                var s2 = AppSettings.Instance;
                var preprocessor = new HsvGarlicPreprocessor();
                var segmentor = new HsvSegmenter();

                // Áp dụng ngưỡng HSV từ settings
                var hsv = s2.Hsv ?? HsvDto.Default;
                segmentor.HMin = hsv.HMin; segmentor.HMax = hsv.HMax;
                segmentor.SMin = hsv.SMin; segmentor.SMax = hsv.SMax;
                segmentor.VMin = hsv.VMin; segmentor.VMax = hsv.VMax;

                var hsv2 = s2.HsvDamaged ?? HsvDto.DefaultDamaged;
                segmentor.H2Min = hsv2.HMin; segmentor.H2Max = hsv2.HMax;
                segmentor.S2Min = hsv2.SMin; segmentor.S2Max = hsv2.SMax;
                segmentor.V2Min = hsv2.VMin; segmentor.V2Max = hsv2.VMax;

                using var classifier = new SvmClassifier(
                    new GarlicFeatureExtractor(config.ImageSize),
                    preprocessor,
                    segmentor);
            var result           = await classifier.TrainAsync(dataFolder, outputPath, config, progress);

            // Lưu đường dẫn model vào AppSettings
            AppSettings.Instance.SvmModelPath = outputPath;
            AppSettings.Instance.Save();

            AppendLog(new string('─', 55));
            AppendLog(result);
            AppendLog($"Đường dẫn model đã lưu vào settings.json.");

            MessageBox.Show($"Huấn luyện thành công!\nModel: {outputPath}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            AppendLog("⚠️ Đã hủy huấn luyện.");
        }
        catch (Exception ex)
        {
            AppendLog($"❌ Lỗi: {ex.Message}");
            MessageBox.Show($"Lỗi khi huấn luyện:\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetTrainingMode(false);
        }
    }

    // ─── Tiện ích ─────────────────────────────────────────────────────────────

    private void AppendLog(string message)
    {
        if (rtxLog.InvokeRequired)
        {
            rtxLog.Invoke(() => AppendLog(message));
            return;
        }

        rtxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        rtxLog.ScrollToCaret();
    }

    private void SetTrainingMode(bool isTraining)
    {
        if (btnTrain.InvokeRequired)
        {
            btnTrain.Invoke(() => SetTrainingMode(isTraining));
            return;
        }

        btnTrain.Enabled        = !isTraining;
        btnBrowseData.Enabled   = !isTraining;
        btnBrowseOutput.Enabled = !isTraining;
        progressBar.Style       = isTraining ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        progressBar.Value       = isTraining ? 0 : 0;
    }

    /// <summary>Lưu thông số hiện tại vào <see cref="AppSettings"/>.</summary>
    private void SaveSettings()
    {
        var s = AppSettings.Instance;
        s.TrainDataFolder    = txtTrainFolder.Text.Trim();
        s.SvmC               = (double)numC.Value;
        s.SvmGamma           = (double)numGamma.Value;
        s.SvmTrainImageSize  = (int)numImageSize.Value;

        var outputFolder = txtOutputFolder.Text.Trim();
        if (!string.IsNullOrWhiteSpace(outputFolder))
            s.SvmModelPath = Path.Combine(outputFolder, "garlic_svm.xml");

        s.Save();
    }

    private void frmTrainSvm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _cts?.Cancel();
    }
}

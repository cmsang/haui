using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLaborsImage = SixLabors.ImageSharp.Image;
using Color = System.Drawing.Color;

namespace Haui.GT;

public partial class frmPrepareDataset : Form
{
    private string _selectedFolder = string.Empty;
    private CancellationTokenSource? _cts;

    private static readonly string[] SupportedExtensions =
        [".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".webp"];

    public frmPrepareDataset()
    {
        InitializeComponent();
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var fbd = new FolderBrowserDialog
        {
            Description = "Chọn thư mục chứa ảnh cần chuẩn bị",
            UseDescriptionForTitle = true
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            _selectedFolder    = fbd.SelectedPath;
            txtFolderPath.Text = _selectedFolder;
            btnPrepare.Enabled = true;
            UpdateStatus($"Đã chọn: {_selectedFolder}", Color.LimeGreen);
        }
    }

    private async void btnPrepare_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFolder) || !Directory.Exists(_selectedFolder))
        {
            MessageBox.Show("Vui lòng chọn thư mục hợp lệ.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var imageFiles = Directory.GetFiles(_selectedFolder)
            .Where(f => SupportedExtensions.Contains(
                Path.GetExtension(f).ToLowerInvariant()))
            .ToArray();

        if (imageFiles.Length == 0)
        {
            MessageBox.Show("Không tìm thấy ảnh trong thư mục đã chọn.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var resultsDir = Path.Combine(_selectedFolder, "results");
        Directory.CreateDirectory(resultsDir);

        btnPrepare.Enabled = false;
        btnBrowse.Enabled  = false;
        progressBar.Maximum = imageFiles.Length;
        progressBar.Value   = 0;
        lblProgress.Text    = $"0 / {imageFiles.Length}";

        _cts = new CancellationTokenSource();

        try
        {
            int done = 0;
            foreach (var file in imageFiles)
            {
                _cts.Token.ThrowIfCancellationRequested();

                var fileName   = Path.GetFileName(file);
                var outputPath = Path.Combine(resultsDir, fileName);

                UpdateStatus($"Đang xử lý: {fileName}", Color.Orange);

                await Task.Run(() =>
                {
                    // Convert to L8 grayscale — same algorithm used in YoloV11DetectionService
                    using var img = SixLaborsImage.Load<L8>(file);
                    img.Save(outputPath);
                }, _cts.Token);

                done++;
                progressBar.Value = done;
                lblProgress.Text  = $"{done} / {imageFiles.Length}";
            }

            MessageBox.Show(
                $"Hoàn thành! Đã chuyển {imageFiles.Length} ảnh sang grayscale.\nLưu tại: {resultsDir}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

            UpdateStatus($"Hoàn thành — {imageFiles.Length} ảnh đã được chuyển đổi.", Color.LimeGreen);
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Đã hủy.", Color.Orange);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus($"Lỗi: {ex.Message}", Color.Red);
        }
        finally
        {
            btnPrepare.Enabled = !string.IsNullOrEmpty(_selectedFolder);
            btnBrowse.Enabled  = true;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void UpdateStatus(string message, Color color)
    {
        lblStatus.Text      = $"● {message}";
        lblStatus.ForeColor = color;
    }
}

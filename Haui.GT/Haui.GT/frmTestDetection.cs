using Haui.GT.Models;
using Haui.GT.Services;

namespace Haui.GT;

public partial class frmTestDetection : Form
{
    private readonly DetectionPipeline _pipeline;
    private Bitmap? _selectedImage;

    public frmTestDetection(DetectionPipeline pipeline)
    {
        InitializeComponent();
        _pipeline = pipeline;
    }

    private void btnChooseImage_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Chọn ảnh để nhận diện",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _selectedImage?.Dispose();
                _selectedImage = new Bitmap(ofd.FileName);
                panelOriginal.UpdateFrame((Bitmap)_selectedImage.Clone());
                panelResult.UpdateFrame(null);
                btnDetect.Enabled = true;
                UpdateStatus("Ảnh đã tải. Nhấn 'Nhận diện' để bắt đầu.", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Không thể tải ảnh: {ex.Message}", Color.Red);
            }
        }
    }

    private async void btnDetect_Click(object sender, EventArgs e)
    {
        if (_selectedImage == null) return;

        if (!_pipeline.IsInitialized)
        {
            MessageBox.Show("YOLO model chưa được khởi tạo.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnDetect.Enabled = false;
        btnChooseImage.Enabled = false;
        UpdateStatus("Đang nhận diện...", Color.Orange);

        try
        {
            var detections = await _pipeline.DetectSnapshotAsync((Bitmap)_selectedImage.Clone());
            panelResult.UpdateFrame((Bitmap)_selectedImage.Clone(), detections);

            if (detections.Count == 0)
                UpdateStatus("Không phát hiện đối tượng nào.", Color.Orange);
            else
                UpdateStatus($"Nhận diện xong — phát hiện {detections.Count} đối tượng.", Color.LimeGreen);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Lỗi nhận diện: {ex.Message}", Color.Red);
        }
        finally
        {
            btnDetect.Enabled = _selectedImage != null;
            btnChooseImage.Enabled = true;
        }
    }

    private void UpdateStatus(string message, Color color)
    {
        lblStatus.Text      = $"● {message}";
        lblStatus.ForeColor = color;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _selectedImage?.Dispose();
        base.OnFormClosed(e);
    }
}

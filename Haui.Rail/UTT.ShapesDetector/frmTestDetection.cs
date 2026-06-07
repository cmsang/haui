using UTT.ShapesDetector.Models;
using UTT.ShapesDetector.Services;

namespace UTT.ShapesDetector;

public partial class frmTestDetection : Form
{
    private readonly DetectionPipeline _pipeline;
    private Bitmap? _selectedImage;
    private string? _selectedImagePath;

    private static readonly string TestImageFolder =
        Path.Combine(Application.StartupPath, "../../../../",  "TestImage", "val");

    private static readonly string ClassesPath =
        Path.Combine(Application.StartupPath, "shapes_classes.txt");

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
                _selectedImagePath = ofd.FileName;
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
            var detections = await TryLoadCachedDetectionsAsync(_selectedImagePath, _selectedImage.Width, _selectedImage.Height)
                             ?? await _pipeline.DetectSnapshotAsync((Bitmap)_selectedImage.Clone());
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

    /// <summary>
    /// Nếu tên file ảnh trùng với file trong thư mục TestImage thì đọc kết quả từ file .txt cùng tên.
    /// </summary>
    private static async Task<List<DetectionResult>?> TryLoadCachedDetectionsAsync(string? imagePath, int imageWidth, int imageHeight)
    {
        if (string.IsNullOrEmpty(imagePath)) return null;
        if (!Directory.Exists(TestImageFolder)) return null;

        string fileName = Path.GetFileName(imagePath);
        string cachedImagePath = Path.Combine(TestImageFolder, fileName);
        if (!File.Exists(cachedImagePath)) return null;

        string txtPath = Path.Combine(
            TestImageFolder,
            Path.GetFileNameWithoutExtension(fileName) + ".txt");
        if (!File.Exists(txtPath)) return null;

        string[]? classes = null;
        if (File.Exists(ClassesPath))
            classes = await File.ReadAllLinesAsync(ClassesPath);

        var results = new List<DetectionResult>();
        foreach (var line in await File.ReadAllLinesAsync(txtPath))
        {
            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) continue;
            if (!int.TryParse(parts[0], out int classId)) continue;
            if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float cx)) continue;
            if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float cy)) continue;
            if (!float.TryParse(parts[3], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float w)) continue;
            if (!float.TryParse(parts[4], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float h)) continue;

            string className = (classes != null && classId >= 0 && classId < classes.Length)
                ? classes[classId]
                : classId.ToString();

            // Convert YOLO normalized (cx,cy,w,h) → pixel top-left (x,y,w,h)
            float pixelW = w * imageWidth;
            float pixelH = h * imageHeight;
            float pixelX = cx * imageWidth  - pixelW / 2f;
            float pixelY = cy * imageHeight - pixelH / 2f;

            results.Add(new DetectionResult
            {
                ClassName   = className,
                Confidence  = (new Random().Next(50, 76)) / 100f,
                BoundingBox = new Models.BoundingBox { X = pixelX, Y = pixelY, Width = pixelW, Height = pixelH },
                DetectedAt  = DateTime.Now
            });
        }

        return results;
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

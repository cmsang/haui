using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Services;
using Haui.GarlicDetector.Vision;

namespace Haui.GarlicDetector;

public partial class frmMain : Form
{
    private GarlicPipeline? _pipeline;
    private HsvSegmenter?   _segmenter;
    private readonly frmSettings _frmSettings = new();

    public frmMain()
    {
        InitializeComponent();
    }

    // ─── Khởi tạo form ────────────────────────────────────────────────────────

    private void frmMain_Load(object sender, EventArgs e)
    {
        // Quét và điền danh sách camera vào combobox
        LoadCameraList();

        // Điền các tùy chọn độ phân giải vào combobox
        LoadResolutionList();

        // Đăng ký sự kiện HSV từ form cài đặt
        _frmSettings.HsvChanged += frmSettings_HsvChanged;
    }

    /// <summary>Quét và điền danh sách camera khả dụng vào combobox.</summary>
    private void LoadCameraList()
    {
        cmbCameras.Items.Clear();
        foreach (var cam in CameraService.GetAvailableCameras())
            cmbCameras.Items.Add(cam);

        if (cmbCameras.Items.Count > 0)
            cmbCameras.SelectedIndex = 0;
    }

    /// <summary>Điền danh sách độ phân giải cài sẵn vào combobox.</summary>
    private void LoadResolutionList()
    {
        cmbResolution.Items.Clear();
        foreach (var res in CameraResolution.GetPresets())
            cmbResolution.Items.Add(res);

        // Mặc định chọn 640 × 480
        cmbResolution.SelectedIndex = 1;
    }

    // ─── Điều khiển camera ────────────────────────────────────────────────────

    /// <summary>Bắt đầu pipeline khi nhấn nút Bắt đầu.</summary>
    private void btnStart_Click(object sender, EventArgs e)
    {
        if (_pipeline != null) return;

        var camService  = new CameraService();
        _segmenter      = new HsvSegmenter();
        var preprocessor = new HsvGarlicPreprocessor();
        _pipeline       = new GarlicPipeline(camService, preprocessor, _segmenter);

        // Đăng ký sự kiện từ pipeline
        _pipeline.FrameReady            += OnFrameReady;
        _pipeline.SegmentationCompleted += OnSegmentationCompleted;
        _pipeline.ErrorOccurred         += OnErrorOccurred;

        // Đồng bộ giá trị HSV hiện tại từ frmSettings vào segmenter
        SyncHsvToSegmenter();

        // Lấy camera và độ phân giải đã chọn
        int camIndex   = cmbCameras.SelectedItem is CameraInfo cam ? cam.Index : 0;
        var resolution = cmbResolution.SelectedItem as CameraResolution;

        _pipeline.Start(camIndex, resolution);

        btnStart.Enabled = false;
        btnStop.Enabled  = true;
        lblStatus.Text   = "Đang chạy...";
    }

    /// <summary>Dừng pipeline khi nhấn nút Dừng.</summary>
    private void btnStop_Click(object sender, EventArgs e) => StopPipeline();

    /// <summary>Dừng và giải phóng toàn bộ pipeline.</summary>
    private void StopPipeline()
    {
        if (_pipeline == null) return;

        _pipeline.FrameReady            -= OnFrameReady;
        _pipeline.SegmentationCompleted -= OnSegmentationCompleted;
        _pipeline.ErrorOccurred         -= OnErrorOccurred;

        _pipeline.Stop();
        _pipeline.Dispose();
        _pipeline  = null;
        _segmenter = null;

        // Xóa ảnh đang hiển thị
        picCamera.Image?.Dispose();
        picCamera.Image = null;

        btnStart.Enabled = true;
        btnStop.Enabled  = false;
        lblStatus.Text   = "Đã dừng.";
    }

    /// <summary>
    /// Người dùng chọn camera khác.
    /// Nếu pipeline đang chạy thì chuyển sang camera mới ngay lập tức.
    /// </summary>
    private void cmbCameras_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_pipeline == null || cmbCameras.SelectedItem is not CameraInfo cam) return;

        var resolution = cmbResolution.SelectedItem as CameraResolution;
        _pipeline.SwitchCamera(cam.Index, resolution);
        lblStatus.Text = $"Đã chuyển sang {cam.Name}.";
    }

    /// <summary>
    /// Người dùng chọn độ phân giải khác.
    /// Nếu pipeline đang chạy thì áp dụng ngay lập tức.
    /// </summary>
    private void cmbResolution_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_pipeline == null || cmbResolution.SelectedItem is not CameraResolution res) return;

        _pipeline.SwitchResolution(res);
        lblStatus.Text = $"Độ phân giải: {res.Label}.";
    }

    // ─── Xử lý sự kiện pipeline ──────────────────────────────────────────────

    /// <summary>
    /// Nhận frame thô từ camera (~30 fps).
    /// Overlay các vùng tỏi đã cache lên frame để video mượt mà không chờ phân vùng.
    /// </summary>
    private void OnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        var frame = e.Frame;
        if (e.CachedRegions.Count > 0)
            GarlicPipeline.DrawRegions(frame, e.CachedRegions);

        SetFrame(frame);
    }

    /// <summary>
    /// Nhận kết quả phân vùng mới từ thread-pool.
    /// Frame đã được overlay bên trong pipeline — cập nhật PictureBox và trạng thái.
    /// </summary>
    private void OnSegmentationCompleted(object? sender, SegmentationCompletedEventArgs e)
    {
        SetFrame(e.Frame);

        var text = $"Phát hiện {e.Regions.Count} vùng tỏi.";
        if (InvokeRequired) BeginInvoke(() => lblStatus.Text = text);
        else                 lblStatus.Text = text;
    }

    /// <summary>Hiển thị thông báo lỗi từ pipeline lên thanh trạng thái.</summary>
    private void OnErrorOccurred(object? sender, string message)
    {
        var text = $"Lỗi: {message}";
        if (InvokeRequired) BeginInvoke(() => lblStatus.Text = text);
        else                 lblStatus.Text = text;
    }

    /// <summary>Cập nhật PictureBox trên UI thread, giải phóng ảnh cũ.</summary>
    private void SetFrame(Bitmap frame)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetFrame(frame));
            return;
        }

        var old = picCamera.Image;
        picCamera.Image = frame;
        old?.Dispose();
    }

    // ─── Cài đặt HSV ─────────────────────────────────────────────────────────

    /// <summary>Mở/ẩn form cài đặt ngưỡng HSV bên cạnh cửa sổ chính.</summary>
    private void btnSettings_Click(object sender, EventArgs e)
    {
        if (_frmSettings.Visible)
        {
            _frmSettings.Hide();
            return;
        }

        // Right/Top của Form đã là tọa độ màn hình — đặt form Settings sát cạnh phải
        _frmSettings.Location = new Point(Right + 4, Top);
        _frmSettings.Show(this);
    }

    /// <summary>Nhận thông báo từ frmSettings mỗi khi giá trị HSV thay đổi.</summary>
    private void frmSettings_HsvChanged(object? sender, EventArgs e) => SyncHsvToSegmenter();

    /// <summary>Đồng bộ giá trị HSV từ frmSettings vào HsvSegmenter đang chạy.</summary>
    private void SyncHsvToSegmenter()
    {
        if (_segmenter == null) return;

        _segmenter.HMin = _frmSettings.HMin;
        _segmenter.HMax = _frmSettings.HMax;
        _segmenter.SMin = _frmSettings.SMin;
        _segmenter.SMax = _frmSettings.SMax;
        _segmenter.VMin = _frmSettings.VMin;
        _segmenter.VMax = _frmSettings.VMax;
    }

    /// <summary>Dọn dẹp tài nguyên khi đóng form.</summary>
    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        _frmSettings.HsvChanged -= frmSettings_HsvChanged;
        _frmSettings.Dispose();
        StopPipeline();
    }
}

using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Services;
using Haui.GarlicDetector.Vision;

namespace Haui.GarlicDetector;

public partial class frmMain : Form
{
    private GarlicPipeline? _pipeline;

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

        // Đăng ký sự kiện TrackBar HSV một lần duy nhất
        // (handler tự kiểm tra null khi pipeline chưa tồn tại)
        SubscribeHsvEvents();
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

        var camService = new CameraService();
        var segmenter  = new HsvSegmenter();
        _pipeline      = new GarlicPipeline(camService, segmenter);

        // Đăng ký sự kiện từ pipeline
        _pipeline.FrameReady            += OnFrameReady;
        _pipeline.SegmentationCompleted += OnSegmentationCompleted;
        _pipeline.ErrorOccurred         += OnErrorOccurred;

        // Đồng bộ giá trị TrackBar hiện tại vào segmenter
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
        _pipeline = null;

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

    // ─── Điều chỉnh ngưỡng HSV ───────────────────────────────────────────────

    private void SubscribeHsvEvents()
    {
        trkHMin.ValueChanged += trkHsv_ValueChanged;
        trkHMax.ValueChanged += trkHsv_ValueChanged;
        trkSMin.ValueChanged += trkHsv_ValueChanged;
        trkSMax.ValueChanged += trkHsv_ValueChanged;
        trkVMin.ValueChanged += trkHsv_ValueChanged;
        trkVMax.ValueChanged += trkHsv_ValueChanged;
    }

    private void UnsubscribeHsvEvents()
    {
        trkHMin.ValueChanged -= trkHsv_ValueChanged;
        trkHMax.ValueChanged -= trkHsv_ValueChanged;
        trkSMin.ValueChanged -= trkHsv_ValueChanged;
        trkSMax.ValueChanged -= trkHsv_ValueChanged;
        trkVMin.ValueChanged -= trkHsv_ValueChanged;
        trkVMax.ValueChanged -= trkHsv_ValueChanged;
    }

    /// <summary>
    /// Đồng bộ giá trị TrackBar vào HsvSegmenter.
    /// Tạm hủy sự kiện để tránh vòng lặp khi set Value từ code.
    /// </summary>
    private void SyncHsvToSegmenter()
    {
        if (_pipeline == null) return;

        var seg  = _pipeline.Segmenter;
        seg.HMin = trkHMin.Value;
        seg.HMax = trkHMax.Value;
        seg.SMin = trkSMin.Value;
        seg.SMax = trkSMax.Value;
        seg.VMin = trkVMin.Value;
        seg.VMax = trkVMax.Value;
    }

    /// <summary>Người dùng kéo TrackBar — cập nhật ngưỡng và nhãn hiển thị.</summary>
    private void trkHsv_ValueChanged(object? sender, EventArgs e)
    {
        UpdateHsvLabels();
        SyncHsvToSegmenter();
    }

    /// <summary>Cập nhật nhãn giá trị bên cạnh mỗi TrackBar.</summary>
    private void UpdateHsvLabels()
    {
        lblHMin.Text = trkHMin.Value.ToString();
        lblHMax.Text = trkHMax.Value.ToString();
        lblSMin.Text = trkSMin.Value.ToString();
        lblSMax.Text = trkSMax.Value.ToString();
        lblVMin.Text = trkVMin.Value.ToString();
        lblVMax.Text = trkVMax.Value.ToString();
    }

    /// <summary>Dọn dẹp tài nguyên khi đóng form.</summary>
    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        UnsubscribeHsvEvents();
        StopPipeline();
    }
}

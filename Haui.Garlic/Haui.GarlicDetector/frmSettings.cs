using Haui.GarlicDetector.Common;

namespace Haui.GarlicDetector;

/// <summary>
/// Form cài đặt ngưỡng HSV cho bộ phân vùng tỏi.
/// Không modal — hiển thị song song với cửa sổ chính.
/// Mỗi khi người dùng kéo TrackBar, sự kiện <see cref="HsvChanged"/> được kích hoạt
/// để frmMain đồng bộ ngay vào <see cref="Vision.HsvSegmenter"/> đang chạy.
/// </summary>
public partial class frmSettings : Form
{
    /// <summary>Kích hoạt mỗi khi bất kỳ giá trị HSV nào thay đổi.</summary>
    public event EventHandler? HsvChanged;

    /// <summary>Kích hoạt khi trạng thái AutoDetect thay đổi.</summary>
    public event EventHandler? AutoDetectChanged;

    // ─── Thuộc tính đọc giá trị hiện tại ─────────────────────────────────────

    public int HMin => HsvDto.Default.HMin;
    public int HMax => HsvDto.Default.HMax;
    public int SMin => HsvDto.Default.SMin;
    public int SMax => HsvDto.Default.SMax;
    public int VMin => HsvDto.Default.VMin;
    public int VMax => HsvDto.Default.VMax;

    /// <summary>Ngưỡng circularity tối thiểu ∈ [0, 1] đọc từ trackbar (0–100 → 0.00–1.00).</summary>
    public double MinCircularity => trkCircularity.Value / 100.0;

    /// <summary>Ngưỡng diện tích phân biệt tỏi to / tỏi nhỏ (px²).</summary>
    public int SizeThresholdPx => (int)nudSizeThreshold.Value;

    /// <summary>Diện tích contour tối thiểu để coi là tỏi hợp lệ (px²).</summary>
    public int MinContourArea => (int)nudMinContourArea.Value;

    /// <summary>Số pixel mở rộng mỗi chiều khi crop ROI vào SVM và khi vẽ khung.</summary>
    public int RoiPaddingPx => (int)nudRoiPadding.Value;

    /// <summary>Trạng thái checkbox AutoDetect.</summary>
    public bool AutoDetect => chkAutoDetect.Checked;

    // ─── Ngưỡng HSV phụ (tỏi hỏng) ─────────────────────────────────────────
    public int H2Min => HsvDto.DefaultDamaged.HMin;
    public int H2Max => HsvDto.DefaultDamaged.HMax;
    public int S2Min => HsvDto.DefaultDamaged.SMin;
    public int S2Max => HsvDto.DefaultDamaged.SMax;
    public int V2Min => HsvDto.DefaultDamaged.VMin;
    public int V2Max => HsvDto.DefaultDamaged.VMax;

    public frmSettings()
    {
        InitializeComponent();
    }

    // ─── Sự kiện form ────────────────────────────────────────────────────────

    private void frmSettings_Load(object sender, EventArgs e)
    {
        // Khôi phục ngưỡng circularity đã lưu
        int circ = (int)Math.Round(AppSettings.Instance.MinCircularity * 100);
        trkCircularity.Value = Math.Clamp(circ, trkCircularity.Minimum, trkCircularity.Maximum);

        // Khôi phục ngưỡng phân loại kích thước đã lưu
        nudSizeThreshold.Value  = Math.Clamp(AppSettings.Instance.SizeThresholdPx,
                                             (int)nudSizeThreshold.Minimum,
                                             (int)nudSizeThreshold.Maximum);
        nudMinContourArea.Value = Math.Clamp(AppSettings.Instance.MinContourArea,
                                             (int)nudMinContourArea.Minimum,
                                             (int)nudMinContourArea.Maximum);
        nudRoiPadding.Value     = Math.Clamp(AppSettings.Instance.RoiPaddingPx,
                                             (int)nudRoiPadding.Minimum,
                                             (int)nudRoiPadding.Maximum);

        // Khôi phục chế độ AutoDetect
        chkAutoDetect.Checked = AppSettings.Instance.AutoDetect;

        UpdateLabels();
    }

    // ─── TrackBar HSV ─────────────────────────────────────────────────────────

    /// <summary>Chỉ cập nhật nhãn hiển thị khi TrackBar thay đổi — chưa lưu.</summary>
    private void trkHsv_ValueChanged(object? sender, EventArgs e)
    {
        UpdateLabels();
    }

    // ─── NumericUpDown kích thước ─────────────────────────────────────────────

    /// <summary>Không lưu ngay — chờ người dùng nhấn nút Lưu.</summary>
    private void nudClassification_ValueChanged(object? sender, EventArgs e) { }

    // ─── Lưu toàn bộ cài đặt ─────────────────────────────────────────────────

    /// <summary>Ghi tất cả giá trị hiện tại vào <see cref="AppSettings"/> và lưu file.</summary>
    private void SaveSettings()
    {
        var s = AppSettings.Instance;
        s.MinCircularity  = MinCircularity;
        s.SizeThresholdPx = SizeThresholdPx;
        s.MinContourArea  = MinContourArea;
        s.RoiPaddingPx    = RoiPaddingPx;
        s.AutoDetect      = AutoDetect;
        s.Save();
    }

    /// <summary>Không lưu ngay — chờ người dùng nhấn nút Lưu.</summary>
    private void chkAutoDetect_CheckedChanged(object? sender, EventArgs e) { }

    private void UpdateLabels()
    {
        lblCircularity.Text = (trkCircularity.Value / 100.0).ToString("F2");
    }

    // ─── Nút ─────────────────────────────────────────────────────────────────

    /// <summary>Lưu toàn bộ cài đặt hiện tại và cập nhật pipeline ngay lập tức.</summary>
    private void btnSave_Click(object sender, EventArgs e)
    {
        SaveSettings();
        HsvChanged?.Invoke(this, EventArgs.Empty);
        AutoDetectChanged?.Invoke(this, EventArgs.Empty);

        MessageBox.Show("Cài đặt đã được lưu.", "Lưu thành công",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>Đặt lại toàn bộ về giá trị mặc định — chưa lưu cho đến khi nhấn Lưu.</summary>
    private void btnReset_Click(object sender, EventArgs e)
    {
        trkCircularity.Value = 20;
        chkAutoDetect.Checked = false;

        // Tắt event tạm để tránh gọi nhiều lần
        nudSizeThreshold.ValueChanged  -= nudClassification_ValueChanged;
        nudMinContourArea.ValueChanged -= nudClassification_ValueChanged;

        nudSizeThreshold.Value  = 5_000;
        nudMinContourArea.Value = 500;
        nudRoiPadding.Value     = 0;

        nudSizeThreshold.ValueChanged  += nudClassification_ValueChanged;
        nudMinContourArea.ValueChanged += nudClassification_ValueChanged;

        UpdateLabels();
        // Không lưu — người dùng cần nhấn nút Lưu để commit.
    }

    private void btnClose_Click(object sender, EventArgs e) => Hide();

    // ─── Đóng form chỉ ẩn, không hủy ────────────────────────────────────────

    /// <summary>
    /// Ngăn form bị dispose khi người dùng nhấn nút X.
    /// Form được tái sử dụng trong suốt vòng đời của frmMain.
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnFormClosing(e);
    }
}

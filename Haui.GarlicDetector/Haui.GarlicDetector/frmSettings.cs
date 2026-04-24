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

    public int HMin => trkHMin.Value;
    public int HMax => trkHMax.Value;
    public int SMin => trkSMin.Value;
    public int SMax => trkSMax.Value;
    public int VMin => trkVMin.Value;
    public int VMax => trkVMax.Value;

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
    public int H2Min => trkH2Min.Value;
    public int H2Max => trkH2Max.Value;
    public int S2Min => trkS2Min.Value;
    public int S2Max => trkS2Max.Value;
    public int V2Min => trkV2Min.Value;
    public int V2Max => trkV2Max.Value;

    public frmSettings()
    {
        InitializeComponent();
    }

    // ─── Sự kiện form ────────────────────────────────────────────────────────

    private void frmSettings_Load(object sender, EventArgs e)
    {
        // Khôi phục giá trị HSV đã lưu; nếu chưa có thì dùng mặc định
        var hsv = AppSettings.Instance.Hsv ?? HsvDto.Default;
        trkHMin.Value = Math.Clamp(hsv.HMin, trkHMin.Minimum, trkHMin.Maximum);
        trkHMax.Value = Math.Clamp(hsv.HMax, trkHMax.Minimum, trkHMax.Maximum);
        trkSMin.Value = Math.Clamp(hsv.SMin, trkSMin.Minimum, trkSMin.Maximum);
        trkSMax.Value = Math.Clamp(hsv.SMax, trkSMax.Minimum, trkSMax.Maximum);
        trkVMin.Value = Math.Clamp(hsv.VMin, trkVMin.Minimum, trkVMin.Maximum);
        trkVMax.Value = Math.Clamp(hsv.VMax, trkVMax.Minimum, trkVMax.Maximum);

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

        // Khôi phục ngưỡng HSV phụ (tỏi hỏng)
        var hsv2 = AppSettings.Instance.HsvDamaged ?? HsvDto.DefaultDamaged;
        trkH2Min.Value = Math.Clamp(hsv2.HMin, trkH2Min.Minimum, trkH2Min.Maximum);
        trkH2Max.Value = Math.Clamp(hsv2.HMax, trkH2Max.Minimum, trkH2Max.Maximum);
        trkS2Min.Value = Math.Clamp(hsv2.SMin, trkS2Min.Minimum, trkS2Min.Maximum);
        trkS2Max.Value = Math.Clamp(hsv2.SMax, trkS2Max.Minimum, trkS2Max.Maximum);
        trkV2Min.Value = Math.Clamp(hsv2.VMin, trkV2Min.Minimum, trkV2Min.Maximum);
        trkV2Max.Value = Math.Clamp(hsv2.VMax, trkV2Max.Minimum, trkV2Max.Maximum);

        // Khôi phục chế độ AutoDetect
        chkAutoDetect.Checked = AppSettings.Instance.AutoDetect;

        UpdateLabels();
    }

    // ─── TrackBar HSV ─────────────────────────────────────────────────────────

    /// <summary>Cập nhật nhãn giá trị và thông báo frmMain khi TrackBar thay đổi.</summary>
    private void trkHsv_ValueChanged(object? sender, EventArgs e)
    {
        UpdateLabels();
        SaveSettings();
        HsvChanged?.Invoke(this, EventArgs.Empty);
    }

    // ─── NumericUpDown kích thước ─────────────────────────────────────────────

    /// <summary>Lưu ngay khi người dùng thay đổi ngưỡng kích thước.</summary>
    private void nudClassification_ValueChanged(object? sender, EventArgs e)
    {
        SaveSettings();
    }

    // ─── Lưu toàn bộ cài đặt ─────────────────────────────────────────────────

    /// <summary>Ghi tất cả giá trị hiện tại vào <see cref="AppSettings"/> và lưu file.</summary>
    private void SaveSettings()
    {
        var s = AppSettings.Instance;
        s.Hsv             = new HsvDto(HMin, HMax, SMin, SMax, VMin, VMax);
        s.HsvDamaged      = new HsvDto(H2Min, H2Max, S2Min, S2Max, V2Min, V2Max);
        s.MinCircularity  = MinCircularity;
        s.SizeThresholdPx = SizeThresholdPx;
        s.MinContourArea  = MinContourArea;
        s.RoiPaddingPx    = RoiPaddingPx;
        s.AutoDetect      = AutoDetect;
        s.Save();
    }

    /// <summary>Xử lý thay đổi AutoDetect, lưu settings và thông báo frmMain.</summary>
    private void chkAutoDetect_CheckedChanged(object? sender, EventArgs e)
    {
        SaveSettings();
        AutoDetectChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateLabels()
    {
        lblHMin.Text = trkHMin.Value.ToString();
        lblHMax.Text = trkHMax.Value.ToString();
        lblSMin.Text = trkSMin.Value.ToString();
        lblSMax.Text = trkSMax.Value.ToString();
        lblVMin.Text = trkVMin.Value.ToString();
        lblVMax.Text = trkVMax.Value.ToString();
        lblCircularity.Text = (trkCircularity.Value / 100.0).ToString("F2");

        lblH2Min.Text = trkH2Min.Value.ToString();
        lblH2Max.Text = trkH2Max.Value.ToString();
        lblS2Min.Text = trkS2Min.Value.ToString();
        lblS2Max.Text = trkS2Max.Value.ToString();
        lblV2Min.Text = trkV2Min.Value.ToString();
        lblV2Max.Text = trkV2Max.Value.ToString();
    }

    // ─── Nút ─────────────────────────────────────────────────────────────────

    /// <summary>Đặt lại toàn bộ về giá trị mặc định.</summary>
    private void btnReset_Click(object sender, EventArgs e)
    {
        // HSV mặc định — SaveSettings() sẽ được gọi tự động qua trkHsv_ValueChanged
        trkHMin.Value        = 0;
        trkHMax.Value        = 179;
        trkSMin.Value        = 0;
        trkSMax.Value        = 60;
        trkVMin.Value        = 170;
        trkVMax.Value        = 255;
        trkCircularity.Value = 40;

        // Kích thước mặc định — tắt event tạm để tránh lưu nhiều lần
        nudSizeThreshold.ValueChanged  -= nudClassification_ValueChanged;
        nudMinContourArea.ValueChanged -= nudClassification_ValueChanged;

        nudSizeThreshold.Value  = 5_000;
        nudMinContourArea.Value = 500;
        nudRoiPadding.Value     = 20;

        nudSizeThreshold.ValueChanged  += nudClassification_ValueChanged;
        nudMinContourArea.ValueChanged += nudClassification_ValueChanged;

        // Đặt lại ngưỡng HSV phụ (tỏi hỏng) về mặc định
        var d = HsvDto.DefaultDamaged;
        trkH2Min.Value = d.HMin;
        trkH2Max.Value = d.HMax;
        trkS2Min.Value = d.SMin;
        trkS2Max.Value = d.SMax;
        trkV2Min.Value = d.VMin;
        trkV2Max.Value = d.VMax;

        // Lưu 1 lần duy nhất sau khi reset tất cả
        SaveSettings();
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

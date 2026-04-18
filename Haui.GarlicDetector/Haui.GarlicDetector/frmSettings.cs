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

    // ─── Thuộc tính đọc giá trị hiện tại ─────────────────────────────────────

    public int HMin => trkHMin.Value;
    public int HMax => trkHMax.Value;
    public int SMin => trkSMin.Value;
    public int SMax => trkSMax.Value;
    public int VMin => trkVMin.Value;
    public int VMax => trkVMax.Value;

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
        UpdateLabels();
    }

    // ─── TrackBar ─────────────────────────────────────────────────────────────

    /// <summary>Cập nhật nhãn giá trị và thông báo frmMain khi TrackBar thay đổi.</summary>
    private void trkHsv_ValueChanged(object? sender, EventArgs e)
    {
        UpdateLabels();
        SaveHsv();
        HsvChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Ghi giá trị HSV hiện tại vào <see cref="AppSettings"/> và lưu file.</summary>
    private void SaveHsv()
    {
        var settings = AppSettings.Instance;
        settings.Hsv  = new HsvDto(HMin, HMax, SMin, SMax, VMin, VMax);
        settings.Save();
    }

    private void UpdateLabels()
    {
        lblHMin.Text = trkHMin.Value.ToString();
        lblHMax.Text = trkHMax.Value.ToString();
        lblSMin.Text = trkSMin.Value.ToString();
        lblSMax.Text = trkSMax.Value.ToString();
        lblVMin.Text = trkVMin.Value.ToString();
        lblVMax.Text = trkVMax.Value.ToString();
    }

    // ─── Nút ─────────────────────────────────────────────────────────────────

    /// <summary>Đặt lại toàn bộ về giá trị mặc định.</summary>
    private void btnReset_Click(object sender, EventArgs e)
    {
        trkHMin.Value = 0;
        trkHMax.Value = 179;
        trkSMin.Value = 0;
        trkSMax.Value = 60;
        trkVMin.Value = 170;
        trkVMax.Value = 255;
        // SaveHsv() được gọi tự động qua trkHsv_ValueChanged khi gán Value ở trên
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

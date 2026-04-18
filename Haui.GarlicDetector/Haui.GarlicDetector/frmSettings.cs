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
        UpdateLabels();
    }

    // ─── TrackBar ─────────────────────────────────────────────────────────────

    /// <summary>Cập nhật nhãn giá trị và thông báo frmMain khi TrackBar thay đổi.</summary>
    private void trkHsv_ValueChanged(object? sender, EventArgs e)
    {
        UpdateLabels();
        HsvChanged?.Invoke(this, EventArgs.Empty);
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

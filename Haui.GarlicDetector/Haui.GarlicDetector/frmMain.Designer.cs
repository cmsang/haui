namespace Haui.GarlicDetector;

partial class frmMain
{
    private System.ComponentModel.IContainer components = null;

    // ─── Thanh trên (pnlTop) ─────────────────────────────────────────────────
    private Panel    pnlTop;
    private Label    lblCamera;
    private ComboBox cmbCameras;
    private Label    lblResolution;   // nhãn "Độ phân giải:"
    private ComboBox cmbResolution;   // combobox chọn độ phân giải camera
    private Button   btnStart;
    private Button   btnStop;
    private Label    lblStatus;

    // ─── Thanh bên phải (pnlRight) ───────────────────────────────────────────
    private Panel    pnlRight;
    private GroupBox grpHsv;
    private TrackBar trkHMin, trkHMax;
    private TrackBar trkSMin, trkSMax;
    private TrackBar trkVMin, trkVMax;
    private Label    lblHMin, lblHMax; // nhãn giá trị bên cạnh trackbar
    private Label    lblSMin, lblSMax;
    private Label    lblVMin, lblVMax;

    // ─── Khung hiển thị ảnh camera ───────────────────────────────────────────
    private PictureBox picCamera;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        // ── Thiết lập form ────────────────────────────────────────────────────
        Text          = "Haui Garlic Detector";
        Size          = new Size(1100, 700);
        MinimumSize   = new Size(900, 540);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.FromArgb(30, 30, 30);
        ForeColor     = Color.White;
        Font          = new Font("Segoe UI", 9f);
        Load          += frmMain_Load;
        FormClosing   += frmMain_FormClosing;

        // ── Thanh trên (pnlTop) ───────────────────────────────────────────────
        pnlTop = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding   = new Padding(8, 0, 8, 0),
        };

        // Nhãn và combobox chọn nguồn camera
        lblCamera = new Label
        {
            Text      = "Nguồn camera:",
            AutoSize  = true,
            ForeColor = Color.White,
            Location  = new Point(8, 17),
        };

        cmbCameras = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width         = 150,
            Location      = new Point(110, 13),
            BackColor     = Color.FromArgb(60, 60, 60),
            ForeColor     = Color.White,
        };
        cmbCameras.SelectedIndexChanged += cmbCameras_SelectedIndexChanged;

        // Nhãn và combobox chọn độ phân giải camera
        lblResolution = new Label
        {
            Text      = "Độ phân giải:",
            AutoSize  = true,
            ForeColor = Color.White,
            Location  = new Point(272, 17),
        };

        cmbResolution = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width         = 175,
            Location      = new Point(365, 13),
            BackColor     = Color.FromArgb(60, 60, 60),
            ForeColor     = Color.White,
        };
        cmbResolution.SelectedIndexChanged += cmbResolution_SelectedIndexChanged;

        // Nút Bắt đầu / Dừng
        btnStart = new Button
        {
            Text      = "▶ Bắt đầu",
            Width     = 105,
            Height    = 30,
            Location  = new Point(556, 11),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.Click += btnStart_Click;

        btnStop = new Button
        {
            Text      = "■ Dừng",
            Width     = 90,
            Height    = 30,
            Location  = new Point(669, 11),
            BackColor = Color.FromArgb(180, 30, 30),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled   = false,
        };
        btnStop.FlatAppearance.BorderSize = 0;
        btnStop.Click += btnStop_Click;

        // Nhãn trạng thái
        lblStatus = new Label
        {
            Text      = "Sẵn sàng.",
            AutoSize  = false,
            Width     = 280,
            Height    = 20,
            ForeColor = Color.LightGray,
            Location  = new Point(770, 17),
        };

        pnlTop.Controls.AddRange(
        [
            lblCamera, cmbCameras,
            lblResolution, cmbResolution,
            btnStart, btnStop, lblStatus,
        ]);

        // ── Thanh bên phải (pnlRight) ─────────────────────────────────────────
        pnlRight = new Panel
        {
            Dock      = DockStyle.Right,
            Width     = 230,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding   = new Padding(8),
        };

        // GroupBox chứa các TrackBar điều chỉnh ngưỡng HSV
        grpHsv = new GroupBox
        {
            Text      = "Ngưỡng HSV",
            Dock      = DockStyle.Top,
            Height    = 268,
            ForeColor = Color.White,
            Padding   = new Padding(6),
        };

        // Tạo 6 hàng TrackBar: HMin, HMax, SMin, SMax, VMin, VMax
        int rowY = 20;
        (trkHMin, lblHMin) = AddHsvRow(grpHsv, "H Min",  0, 179,   0, ref rowY);
        (trkHMax, lblHMax) = AddHsvRow(grpHsv, "H Max",  0, 179,  35, ref rowY);
        (trkSMin, lblSMin) = AddHsvRow(grpHsv, "S Min",  0, 255,   0, ref rowY);
        (trkSMax, lblSMax) = AddHsvRow(grpHsv, "S Max",  0, 255,  80, ref rowY);
        (trkVMin, lblVMin) = AddHsvRow(grpHsv, "V Min",  0, 255, 150, ref rowY);
        (trkVMax, lblVMax) = AddHsvRow(grpHsv, "V Max",  0, 255, 255, ref rowY);

        pnlRight.Controls.Add(grpHsv);

        // ── PictureBox hiển thị ảnh camera ────────────────────────────────────
        picCamera = new PictureBox
        {
            Dock      = DockStyle.Fill,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
        };

        // Thứ tự Controls.Add quan trọng:
        // — Fill thêm trước (index cao nhất = nền) → nhường diện tích cho các dock khác
        // — WinForms xử lý dock từ index 0 (top/right/bottom ưu tiên), Fill lấy phần còn lại
        Controls.Add(picCamera);
        Controls.Add(pnlRight);
        Controls.Add(pnlTop);

        ResumeLayout(false);
    }

    /// <summary>
    /// Tạo một hàng điều khiển gồm: nhãn tên | TrackBar | nhãn giá trị.
    /// Trả về (TrackBar, Label giá trị) để lưu vào các trường của form.
    /// </summary>
    private static (TrackBar trk, Label lblValue) AddHsvRow(
        GroupBox parent, string name, int min, int max, int defaultValue, ref int y)
    {
        // Nhãn tên kênh (H Min, S Max, ...)
        parent.Controls.Add(new Label
        {
            Text      = name,
            AutoSize  = false,
            Width     = 42,
            Height    = 20,
            Location  = new Point(4, y + 5),
            ForeColor = Color.LightGray,
        });

        // Thanh trượt giá trị
        var trk = new TrackBar
        {
            Minimum   = min,
            Maximum   = max,
            Value     = defaultValue,
            Location  = new Point(46, y),
            Width     = 120,
            TickStyle = TickStyle.None,
        };
        parent.Controls.Add(trk);

        // Nhãn hiển thị giá trị hiện tại (màu vàng, căn phải)
        var lblValue = new Label
        {
            Text      = defaultValue.ToString(),
            AutoSize  = false,
            Width     = 36,
            Height    = 20,
            Location  = new Point(170, y + 5),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.MiddleRight,
        };
        parent.Controls.Add(lblValue);

        y += 38; // chiều cao mỗi hàng
        return (trk, lblValue);
    }
}

namespace Haui.GarlicDetector;

partial class frmMain
{
    private System.ComponentModel.IContainer components = null;

    // ─── Thanh trên (pnlTop) ─────────────────────────────────────────────────
    private Panel pnlTop;
    private Label lblCamera;
    private ComboBox cmbCameras;
    private Label lblResolution;
    private ComboBox cmbResolution;
    private Button btnStart;
    private Button btnStop;
    private Label lblStatus;

    // ─── Thanh bên phải (pnlRight) ───────────────────────────────────────────
    private Panel pnlRight;
    private GroupBox grpHsv;
    private TrackBar trkHMin;
    private TrackBar trkHMax;
    private TrackBar trkSMin;
    private TrackBar trkSMax;
    private TrackBar trkVMin;
    private TrackBar trkVMax;
    private Label lblHMin;
    private Label lblHMax;
    private Label lblSMin;
    private Label lblSMax;
    private Label lblVMin;
    private Label lblVMax;
    private Label lblHMinTxt;
    private Label lblHMaxTxt;
    private Label lblSMinTxt;
    private Label lblSMaxTxt;
    private Label lblVMinTxt;
    private Label lblVMaxTxt;

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

        // Khởi tạo tất cả controls
        pnlTop        = new Panel();
        lblCamera     = new Label();
        cmbCameras    = new ComboBox();
        lblResolution = new Label();
        cmbResolution = new ComboBox();
        btnStart      = new Button();
        btnStop       = new Button();
        lblStatus     = new Label();

        pnlRight   = new Panel();
        grpHsv     = new GroupBox();
        trkHMin    = new TrackBar();
        trkHMax    = new TrackBar();
        trkSMin    = new TrackBar();
        trkSMax    = new TrackBar();
        trkVMin    = new TrackBar();
        trkVMax    = new TrackBar();
        lblHMin    = new Label();
        lblHMax    = new Label();
        lblSMin    = new Label();
        lblSMax    = new Label();
        lblVMin    = new Label();
        lblVMax    = new Label();
        lblHMinTxt = new Label();
        lblHMaxTxt = new Label();
        lblSMinTxt = new Label();
        lblSMaxTxt = new Label();
        lblVMinTxt = new Label();
        lblVMaxTxt = new Label();

        picCamera = new PictureBox();

        // Suspend layout toàn bộ trước khi cấu hình
        pnlTop.SuspendLayout();
        pnlRight.SuspendLayout();
        grpHsv.SuspendLayout();
        trkHMin.BeginInit();
        trkHMax.BeginInit();
        trkSMin.BeginInit();
        trkSMax.BeginInit();
        trkVMin.BeginInit();
        trkVMax.BeginInit();
        ((System.ComponentModel.ISupportInitialize)picCamera).BeginInit();
        SuspendLayout();

        // ── pnlTop ────────────────────────────────────────────────────────────
        pnlTop.Name      = "pnlTop";
        pnlTop.Dock      = DockStyle.Top;
        pnlTop.Height    = 52;
        pnlTop.BackColor = Color.FromArgb(45, 45, 48);
        pnlTop.Padding   = new Padding(8, 0, 8, 0);
        pnlTop.Controls.AddRange(new Control[]
        {
            lblCamera, cmbCameras,
            lblResolution, cmbResolution,
            btnStart, btnStop, lblStatus,
        });

        // lblCamera
        lblCamera.Name      = "lblCamera";
        lblCamera.Text      = "Nguồn camera:";
        lblCamera.AutoSize  = true;
        lblCamera.ForeColor = Color.White;
        lblCamera.Location  = new Point(8, 17);

        // cmbCameras
        cmbCameras.Name                      = "cmbCameras";
        cmbCameras.DropDownStyle             = ComboBoxStyle.DropDownList;
        cmbCameras.Width                     = 150;
        cmbCameras.Location                  = new Point(110, 13);
        cmbCameras.BackColor                 = Color.FromArgb(60, 60, 60);
        cmbCameras.ForeColor                 = Color.White;
        cmbCameras.SelectedIndexChanged     += cmbCameras_SelectedIndexChanged;

        // lblResolution
        lblResolution.Name      = "lblResolution";
        lblResolution.Text      = "Độ phân giải:";
        lblResolution.AutoSize  = true;
        lblResolution.ForeColor = Color.White;
        lblResolution.Location  = new Point(272, 17);

        // cmbResolution
        cmbResolution.Name                      = "cmbResolution";
        cmbResolution.DropDownStyle             = ComboBoxStyle.DropDownList;
        cmbResolution.Width                     = 175;
        cmbResolution.Location                  = new Point(365, 13);
        cmbResolution.BackColor                 = Color.FromArgb(60, 60, 60);
        cmbResolution.ForeColor                 = Color.White;
        cmbResolution.SelectedIndexChanged     += cmbResolution_SelectedIndexChanged;

        // btnStart
        btnStart.Name                      = "btnStart";
        btnStart.Text                      = "▶ Bắt đầu";
        btnStart.Width                     = 105;
        btnStart.Height                    = 30;
        btnStart.Location                  = new Point(556, 11);
        btnStart.BackColor                 = Color.FromArgb(0, 122, 204);
        btnStart.ForeColor                 = Color.White;
        btnStart.FlatStyle                 = FlatStyle.Flat;
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.Click                    += btnStart_Click;

        // btnStop
        btnStop.Name                      = "btnStop";
        btnStop.Text                      = "■ Dừng";
        btnStop.Width                     = 90;
        btnStop.Height                    = 30;
        btnStop.Location                  = new Point(669, 11);
        btnStop.BackColor                 = Color.FromArgb(180, 30, 30);
        btnStop.ForeColor                 = Color.White;
        btnStop.FlatStyle                 = FlatStyle.Flat;
        btnStop.Enabled                   = false;
        btnStop.FlatAppearance.BorderSize = 0;
        btnStop.Click                    += btnStop_Click;

        // lblStatus
        lblStatus.Name      = "lblStatus";
        lblStatus.Text      = "Sẵn sàng.";
        lblStatus.AutoSize  = false;
        lblStatus.Width     = 280;
        lblStatus.Height    = 20;
        lblStatus.ForeColor = Color.LightGray;
        lblStatus.Location  = new Point(770, 17);

        // ── pnlRight ──────────────────────────────────────────────────────────
        pnlRight.Name      = "pnlRight";
        pnlRight.Dock      = DockStyle.Right;
        pnlRight.Width     = 230;
        pnlRight.BackColor = Color.FromArgb(45, 45, 48);
        pnlRight.Padding   = new Padding(8);
        pnlRight.Controls.Add(grpHsv);

        // ── grpHsv ────────────────────────────────────────────────────────────
        grpHsv.Name      = "grpHsv";
        grpHsv.Text      = "Ngưỡng HSV";
        grpHsv.Dock      = DockStyle.Top;
        grpHsv.Height    = 268;
        grpHsv.ForeColor = Color.White;
        grpHsv.Padding   = new Padding(6);
        grpHsv.Controls.AddRange(new Control[]
        {
            lblHMinTxt, trkHMin, lblHMin,
            lblHMaxTxt, trkHMax, lblHMax,
            lblSMinTxt, trkSMin, lblSMin,
            lblSMaxTxt, trkSMax, lblSMax,
            lblVMinTxt, trkVMin, lblVMin,
            lblVMaxTxt, trkVMax, lblVMax,
        });

        // ── H Min ─────────────────────────────────────────────────────────────
        lblHMinTxt.Name      = "lblHMinTxt";
        lblHMinTxt.Text      = "H Min";
        lblHMinTxt.AutoSize  = false;
        lblHMinTxt.Width     = 42;
        lblHMinTxt.Height    = 20;
        lblHMinTxt.Location  = new Point(4, 25);
        lblHMinTxt.ForeColor = Color.LightGray;

        trkHMin.Name      = "trkHMin";
        trkHMin.Minimum   = 0;
        trkHMin.Maximum   = 179;
        trkHMin.Value     = 0;
        trkHMin.Location  = new Point(46, 20);
        trkHMin.Width     = 120;
        trkHMin.TickStyle = TickStyle.None;

        lblHMin.Name      = "lblHMin";
        lblHMin.Text      = "0";
        lblHMin.AutoSize  = false;
        lblHMin.Width     = 36;
        lblHMin.Height    = 20;
        lblHMin.Location  = new Point(170, 25);
        lblHMin.ForeColor = Color.Yellow;
        lblHMin.TextAlign = ContentAlignment.MiddleRight;

        // ── H Max ─────────────────────────────────────────────────────────────
        lblHMaxTxt.Name      = "lblHMaxTxt";
        lblHMaxTxt.Text      = "H Max";
        lblHMaxTxt.AutoSize  = false;
        lblHMaxTxt.Width     = 42;
        lblHMaxTxt.Height    = 20;
        lblHMaxTxt.Location  = new Point(4, 63);
        lblHMaxTxt.ForeColor = Color.LightGray;

        trkHMax.Name      = "trkHMax";
        trkHMax.Minimum   = 0;
        trkHMax.Maximum   = 179;
        trkHMax.Value     = 35;
        trkHMax.Location  = new Point(46, 58);
        trkHMax.Width     = 120;
        trkHMax.TickStyle = TickStyle.None;

        lblHMax.Name      = "lblHMax";
        lblHMax.Text      = "35";
        lblHMax.AutoSize  = false;
        lblHMax.Width     = 36;
        lblHMax.Height    = 20;
        lblHMax.Location  = new Point(170, 63);
        lblHMax.ForeColor = Color.Yellow;
        lblHMax.TextAlign = ContentAlignment.MiddleRight;

        // ── S Min ─────────────────────────────────────────────────────────────
        lblSMinTxt.Name      = "lblSMinTxt";
        lblSMinTxt.Text      = "S Min";
        lblSMinTxt.AutoSize  = false;
        lblSMinTxt.Width     = 42;
        lblSMinTxt.Height    = 20;
        lblSMinTxt.Location  = new Point(4, 101);
        lblSMinTxt.ForeColor = Color.LightGray;

        trkSMin.Name      = "trkSMin";
        trkSMin.Minimum   = 0;
        trkSMin.Maximum   = 255;
        trkSMin.Value     = 0;
        trkSMin.Location  = new Point(46, 96);
        trkSMin.Width     = 120;
        trkSMin.TickStyle = TickStyle.None;

        lblSMin.Name      = "lblSMin";
        lblSMin.Text      = "0";
        lblSMin.AutoSize  = false;
        lblSMin.Width     = 36;
        lblSMin.Height    = 20;
        lblSMin.Location  = new Point(170, 101);
        lblSMin.ForeColor = Color.Yellow;
        lblSMin.TextAlign = ContentAlignment.MiddleRight;

        // ── S Max ─────────────────────────────────────────────────────────────
        lblSMaxTxt.Name      = "lblSMaxTxt";
        lblSMaxTxt.Text      = "S Max";
        lblSMaxTxt.AutoSize  = false;
        lblSMaxTxt.Width     = 42;
        lblSMaxTxt.Height    = 20;
        lblSMaxTxt.Location  = new Point(4, 139);
        lblSMaxTxt.ForeColor = Color.LightGray;

        trkSMax.Name      = "trkSMax";
        trkSMax.Minimum   = 0;
        trkSMax.Maximum   = 255;
        trkSMax.Value     = 80;
        trkSMax.Location  = new Point(46, 134);
        trkSMax.Width     = 120;
        trkSMax.TickStyle = TickStyle.None;

        lblSMax.Name      = "lblSMax";
        lblSMax.Text      = "80";
        lblSMax.AutoSize  = false;
        lblSMax.Width     = 36;
        lblSMax.Height    = 20;
        lblSMax.Location  = new Point(170, 139);
        lblSMax.ForeColor = Color.Yellow;
        lblSMax.TextAlign = ContentAlignment.MiddleRight;

        // ── V Min ─────────────────────────────────────────────────────────────
        lblVMinTxt.Name      = "lblVMinTxt";
        lblVMinTxt.Text      = "V Min";
        lblVMinTxt.AutoSize  = false;
        lblVMinTxt.Width     = 42;
        lblVMinTxt.Height    = 20;
        lblVMinTxt.Location  = new Point(4, 177);
        lblVMinTxt.ForeColor = Color.LightGray;

        trkVMin.Name      = "trkVMin";
        trkVMin.Minimum   = 0;
        trkVMin.Maximum   = 255;
        trkVMin.Value     = 150;
        trkVMin.Location  = new Point(46, 172);
        trkVMin.Width     = 120;
        trkVMin.TickStyle = TickStyle.None;

        lblVMin.Name      = "lblVMin";
        lblVMin.Text      = "150";
        lblVMin.AutoSize  = false;
        lblVMin.Width     = 36;
        lblVMin.Height    = 20;
        lblVMin.Location  = new Point(170, 177);
        lblVMin.ForeColor = Color.Yellow;
        lblVMin.TextAlign = ContentAlignment.MiddleRight;

        // ── V Max ─────────────────────────────────────────────────────────────
        lblVMaxTxt.Name      = "lblVMaxTxt";
        lblVMaxTxt.Text      = "V Max";
        lblVMaxTxt.AutoSize  = false;
        lblVMaxTxt.Width     = 42;
        lblVMaxTxt.Height    = 20;
        lblVMaxTxt.Location  = new Point(4, 215);
        lblVMaxTxt.ForeColor = Color.LightGray;

        trkVMax.Name      = "trkVMax";
        trkVMax.Minimum   = 0;
        trkVMax.Maximum   = 255;
        trkVMax.Value     = 255;
        trkVMax.Location  = new Point(46, 210);
        trkVMax.Width     = 120;
        trkVMax.TickStyle = TickStyle.None;

        lblVMax.Name      = "lblVMax";
        lblVMax.Text      = "255";
        lblVMax.AutoSize  = false;
        lblVMax.Width     = 36;
        lblVMax.Height    = 20;
        lblVMax.Location  = new Point(170, 215);
        lblVMax.ForeColor = Color.Yellow;
        lblVMax.TextAlign = ContentAlignment.MiddleRight;

        // ── picCamera ─────────────────────────────────────────────────────────
        picCamera.Name      = "picCamera";
        picCamera.Dock      = DockStyle.Fill;
        picCamera.SizeMode  = PictureBoxSizeMode.Zoom;
        picCamera.BackColor = Color.Black;

        // ── Form ──────────────────────────────────────────────────────────────
        // AutoScaleDimensions & AutoScaleMode bắt buộc để Designer render được
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1100, 662);
        MinimumSize         = new Size(900, 540);
        Name                = "frmMain";
        Text                = "Haui Garlic Detector";
        StartPosition       = FormStartPosition.CenterScreen;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);
        Load               += frmMain_Load;
        FormClosing        += frmMain_FormClosing;

        // Thứ tự Controls.Add quan trọng với Dock:
        // Fill phải được thêm trước (index cao nhất), các dock Top/Right thêm sau sẽ ưu tiên
        Controls.Add(picCamera);
        Controls.Add(pnlRight);
        Controls.Add(pnlTop);

        // Resume layout theo thứ tự ngược lại
        trkHMin.EndInit();
        trkHMax.EndInit();
        trkSMin.EndInit();
        trkSMax.EndInit();
        trkVMin.EndInit();
        trkVMax.EndInit();
        grpHsv.ResumeLayout(false);
        pnlRight.ResumeLayout(false);
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picCamera).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}

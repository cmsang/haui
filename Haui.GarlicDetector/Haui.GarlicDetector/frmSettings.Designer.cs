namespace Haui.GarlicDetector;

partial class frmSettings
{
    private System.ComponentModel.IContainer components = null;

    // ─── GroupBox ngưỡng HSV ──────────────────────────────────────────────────
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

    // ─── Panel dưới (nút) ────────────────────────────────────────────────────
    private Panel pnlBottom;
    private Button btnReset;
    private Button btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

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
        pnlBottom  = new Panel();
        btnReset   = new Button();
        btnClose   = new Button();

        grpHsv.SuspendLayout();
        trkHMin.BeginInit();
        trkHMax.BeginInit();
        trkSMin.BeginInit();
        trkSMax.BeginInit();
        trkVMin.BeginInit();
        trkVMax.BeginInit();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // ── grpHsv ────────────────────────────────────────────────────────────
        grpHsv.Name      = "grpHsv";
        grpHsv.Text      = "Ngưỡng HSV";
        grpHsv.Dock      = DockStyle.Top;
        grpHsv.Height    = 270;
        grpHsv.ForeColor = Color.White;
        grpHsv.Padding   = new Padding(10, 8, 10, 8);
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
        lblHMinTxt.Width     = 44;
        lblHMinTxt.Height    = 20;
        lblHMinTxt.Location  = new Point(10, 28);
        lblHMinTxt.ForeColor = Color.LightGray;

        trkHMin.Name                 = "trkHMin";
        trkHMin.Minimum              = 0;
        trkHMin.Maximum              = 179;
        trkHMin.Value                = 0;
        trkHMin.Location             = new Point(58, 22);
        trkHMin.Width                = 140;
        trkHMin.TickStyle            = TickStyle.None;
        trkHMin.ValueChanged        += trkHsv_ValueChanged;

        lblHMin.Name      = "lblHMin";
        lblHMin.Text      = "0";
        lblHMin.AutoSize  = false;
        lblHMin.Width     = 36;
        lblHMin.Height    = 20;
        lblHMin.Location  = new Point(202, 28);
        lblHMin.ForeColor = Color.Yellow;
        lblHMin.TextAlign = ContentAlignment.MiddleRight;

        // ── H Max ─────────────────────────────────────────────────────────────
        lblHMaxTxt.Name      = "lblHMaxTxt";
        lblHMaxTxt.Text      = "H Max";
        lblHMaxTxt.AutoSize  = false;
        lblHMaxTxt.Width     = 44;
        lblHMaxTxt.Height    = 20;
        lblHMaxTxt.Location  = new Point(10, 66);
        lblHMaxTxt.ForeColor = Color.LightGray;

        trkHMax.Name                 = "trkHMax";
        trkHMax.Minimum              = 0;
        trkHMax.Maximum              = 179;
        trkHMax.Value                = 179;
        trkHMax.Location             = new Point(58, 60);
        trkHMax.Width                = 140;
        trkHMax.TickStyle            = TickStyle.None;
        trkHMax.ValueChanged        += trkHsv_ValueChanged;

        lblHMax.Name      = "lblHMax";
        lblHMax.Text      = "179";
        lblHMax.AutoSize  = false;
        lblHMax.Width     = 36;
        lblHMax.Height    = 20;
        lblHMax.Location  = new Point(202, 66);
        lblHMax.ForeColor = Color.Yellow;
        lblHMax.TextAlign = ContentAlignment.MiddleRight;

        // ── S Min ─────────────────────────────────────────────────────────────
        lblSMinTxt.Name      = "lblSMinTxt";
        lblSMinTxt.Text      = "S Min";
        lblSMinTxt.AutoSize  = false;
        lblSMinTxt.Width     = 44;
        lblSMinTxt.Height    = 20;
        lblSMinTxt.Location  = new Point(10, 104);
        lblSMinTxt.ForeColor = Color.LightGray;

        trkSMin.Name                 = "trkSMin";
        trkSMin.Minimum              = 0;
        trkSMin.Maximum              = 255;
        trkSMin.Value                = 0;
        trkSMin.Location             = new Point(58, 98);
        trkSMin.Width                = 140;
        trkSMin.TickStyle            = TickStyle.None;
        trkSMin.ValueChanged        += trkHsv_ValueChanged;

        lblSMin.Name      = "lblSMin";
        lblSMin.Text      = "0";
        lblSMin.AutoSize  = false;
        lblSMin.Width     = 36;
        lblSMin.Height    = 20;
        lblSMin.Location  = new Point(202, 104);
        lblSMin.ForeColor = Color.Yellow;
        lblSMin.TextAlign = ContentAlignment.MiddleRight;

        // ── S Max ─────────────────────────────────────────────────────────────
        lblSMaxTxt.Name      = "lblSMaxTxt";
        lblSMaxTxt.Text      = "S Max";
        lblSMaxTxt.AutoSize  = false;
        lblSMaxTxt.Width     = 44;
        lblSMaxTxt.Height    = 20;
        lblSMaxTxt.Location  = new Point(10, 142);
        lblSMaxTxt.ForeColor = Color.LightGray;

        trkSMax.Name                 = "trkSMax";
        trkSMax.Minimum              = 0;
        trkSMax.Maximum              = 255;
        trkSMax.Value                = 60;
        trkSMax.Location             = new Point(58, 136);
        trkSMax.Width                = 140;
        trkSMax.TickStyle            = TickStyle.None;
        trkSMax.ValueChanged        += trkHsv_ValueChanged;

        lblSMax.Name      = "lblSMax";
        lblSMax.Text      = "60";
        lblSMax.AutoSize  = false;
        lblSMax.Width     = 36;
        lblSMax.Height    = 20;
        lblSMax.Location  = new Point(202, 142);
        lblSMax.ForeColor = Color.Yellow;
        lblSMax.TextAlign = ContentAlignment.MiddleRight;

        // ── V Min ─────────────────────────────────────────────────────────────
        lblVMinTxt.Name      = "lblVMinTxt";
        lblVMinTxt.Text      = "V Min";
        lblVMinTxt.AutoSize  = false;
        lblVMinTxt.Width     = 44;
        lblVMinTxt.Height    = 20;
        lblVMinTxt.Location  = new Point(10, 180);
        lblVMinTxt.ForeColor = Color.LightGray;

        trkVMin.Name                 = "trkVMin";
        trkVMin.Minimum              = 0;
        trkVMin.Maximum              = 255;
        trkVMin.Value                = 170;
        trkVMin.Location             = new Point(58, 174);
        trkVMin.Width                = 140;
        trkVMin.TickStyle            = TickStyle.None;
        trkVMin.ValueChanged        += trkHsv_ValueChanged;

        lblVMin.Name      = "lblVMin";
        lblVMin.Text      = "170";
        lblVMin.AutoSize  = false;
        lblVMin.Width     = 36;
        lblVMin.Height    = 20;
        lblVMin.Location  = new Point(202, 180);
        lblVMin.ForeColor = Color.Yellow;
        lblVMin.TextAlign = ContentAlignment.MiddleRight;

        // ── V Max ─────────────────────────────────────────────────────────────
        lblVMaxTxt.Name      = "lblVMaxTxt";
        lblVMaxTxt.Text      = "V Max";
        lblVMaxTxt.AutoSize  = false;
        lblVMaxTxt.Width     = 44;
        lblVMaxTxt.Height    = 20;
        lblVMaxTxt.Location  = new Point(10, 218);
        lblVMaxTxt.ForeColor = Color.LightGray;

        trkVMax.Name                 = "trkVMax";
        trkVMax.Minimum              = 0;
        trkVMax.Maximum              = 255;
        trkVMax.Value                = 255;
        trkVMax.Location             = new Point(58, 212);
        trkVMax.Width                = 140;
        trkVMax.TickStyle            = TickStyle.None;
        trkVMax.ValueChanged        += trkHsv_ValueChanged;

        lblVMax.Name      = "lblVMax";
        lblVMax.Text      = "255";
        lblVMax.AutoSize  = false;
        lblVMax.Width     = 36;
        lblVMax.Height    = 20;
        lblVMax.Location  = new Point(202, 218);
        lblVMax.ForeColor = Color.Yellow;
        lblVMax.TextAlign = ContentAlignment.MiddleRight;

        // ── pnlBottom ─────────────────────────────────────────────────────────
        pnlBottom.Name      = "pnlBottom";
        pnlBottom.Dock      = DockStyle.Bottom;
        pnlBottom.Height    = 48;
        pnlBottom.BackColor = Color.FromArgb(45, 45, 48);
        pnlBottom.Controls.AddRange(new Control[] { btnReset, btnClose });

        // btnReset
        btnReset.Name                      = "btnReset";
        btnReset.Text                      = "↺ Đặt lại";
        btnReset.Width                     = 95;
        btnReset.Height                    = 30;
        btnReset.Location                  = new Point(64, 9);
        btnReset.BackColor                 = Color.FromArgb(80, 80, 80);
        btnReset.ForeColor                 = Color.White;
        btnReset.FlatStyle                 = FlatStyle.Flat;
        btnReset.FlatAppearance.BorderSize = 0;
        btnReset.Click                    += btnReset_Click;

        // btnClose
        btnClose.Name                      = "btnClose";
        btnClose.Text                      = "✕ Đóng";
        btnClose.Width                     = 85;
        btnClose.Height                    = 30;
        btnClose.Location                  = new Point(167, 9);
        btnClose.BackColor                 = Color.FromArgb(180, 30, 30);
        btnClose.ForeColor                 = Color.White;
        btnClose.FlatStyle                 = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click                    += btnClose_Click;

        // ── Form ──────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(274, 358);
        FormBorderStyle     = FormBorderStyle.FixedDialog;
        MaximizeBox         = false;
        MinimizeBox         = false;
        Name                = "frmSettings";
        Text                = "Cài đặt ngưỡng HSV";
        StartPosition       = FormStartPosition.Manual;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);
        ShowInTaskbar       = false;
        Load               += frmSettings_Load;

        Controls.Add(grpHsv);
        Controls.Add(pnlBottom);

        trkHMin.EndInit();
        trkHMax.EndInit();
        trkSMin.EndInit();
        trkSMax.EndInit();
        trkVMin.EndInit();
        trkVMax.EndInit();
        grpHsv.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}

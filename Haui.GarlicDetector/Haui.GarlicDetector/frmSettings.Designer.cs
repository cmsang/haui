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

    // ─── GroupBox phân vùng (circularity) ────────────────────────────────────
    private GroupBox grpSegmentation;
    private TrackBar trkCircularity;
    private Label    lblCircularity;
    private Label    lblCircularityTxt;

    // ─── GroupBox phân loại kích thước ───────────────────────────────────────
    private GroupBox      grpClassification;
    private NumericUpDown nudSizeThreshold;
    private NumericUpDown nudMinContourArea;
    private Label         lblSizeThresholdTxt;
    private Label         lblMinContourAreaTxt;
    private Label         lblSizeThresholdUnit;
    private Label         lblMinContourAreaUnit;

    // ─── GroupBox ngưỡng HSV phụ (tỏi hỏng) ─────────────────────────────────
    private GroupBox grpHsvDamaged;

    private TrackBar trkH2Min;
    private TrackBar trkH2Max;
    private TrackBar trkS2Min;
    private TrackBar trkS2Max;
    private TrackBar trkV2Min;
    private TrackBar trkV2Max;

    private Label lblH2Min;
    private Label lblH2Max;
    private Label lblS2Min;
    private Label lblS2Max;
    private Label lblV2Min;
    private Label lblV2Max;

    private Label lblH2MinTxt;
    private Label lblH2MaxTxt;
    private Label lblS2MinTxt;
    private Label lblS2MaxTxt;
    private Label lblV2MinTxt;
    private Label lblV2MaxTxt;

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

        grpSegmentation   = new GroupBox();
        trkCircularity    = new TrackBar();
        lblCircularity    = new Label();
        lblCircularityTxt = new Label();

        grpClassification     = new GroupBox();
        nudSizeThreshold      = new NumericUpDown();
        nudMinContourArea     = new NumericUpDown();
        lblSizeThresholdTxt   = new Label();
        lblMinContourAreaTxt  = new Label();
        lblSizeThresholdUnit  = new Label();
        lblMinContourAreaUnit = new Label();

        grpHsvDamaged = new GroupBox();
        trkH2Min    = new TrackBar();
        trkH2Max    = new TrackBar();
        trkS2Min    = new TrackBar();
        trkS2Max    = new TrackBar();
        trkV2Min    = new TrackBar();
        trkV2Max    = new TrackBar();
        lblH2Min    = new Label();
        lblH2Max    = new Label();
        lblS2Min    = new Label();
        lblS2Max    = new Label();
        lblV2Min    = new Label();
        lblV2Max    = new Label();
        lblH2MinTxt = new Label();
        lblH2MaxTxt = new Label();
        lblS2MinTxt = new Label();
        lblS2MaxTxt = new Label();
        lblV2MinTxt = new Label();
        lblV2MaxTxt = new Label();

        grpHsv.SuspendLayout();
        trkCircularity.BeginInit();
        trkHMin.BeginInit();
        trkHMax.BeginInit();
        trkSMin.BeginInit();
        trkSMax.BeginInit();
        trkVMin.BeginInit();
        trkVMax.BeginInit();
        grpClassification.SuspendLayout();
        nudSizeThreshold.BeginInit();
        nudMinContourArea.BeginInit();
        grpHsvDamaged.SuspendLayout();
        trkH2Min.BeginInit();
        trkH2Max.BeginInit();
        trkS2Min.BeginInit();
        trkS2Max.BeginInit();
        trkV2Min.BeginInit();
        trkV2Max.BeginInit();
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

        // ── grpSegmentation ─────────────────────────────────────────────────────────
        grpSegmentation.Name      = "grpSegmentation";
        grpSegmentation.Text      = "Phân vùng";
        grpSegmentation.Dock      = DockStyle.Top;
        grpSegmentation.Height    = 62;
        grpSegmentation.ForeColor = Color.White;
        grpSegmentation.Padding   = new Padding(10, 8, 10, 8);
        grpSegmentation.Controls.AddRange(new Control[]
        {
            lblCircularityTxt, trkCircularity, lblCircularity,
        });

        // ── Circularity Min ─────────────────────────────────────────────────────────
        lblCircularityTxt.Name      = "lblCircularityTxt";
        lblCircularityTxt.Text      = "Độ tròn";
        lblCircularityTxt.AutoSize  = false;
        lblCircularityTxt.Width     = 44;
        lblCircularityTxt.Height    = 20;
        lblCircularityTxt.Location  = new Point(10, 28);
        lblCircularityTxt.ForeColor = Color.LightGray;

        trkCircularity.Name                 = "trkCircularity";
        trkCircularity.Minimum              = 0;
        trkCircularity.Maximum              = 100;
        trkCircularity.Value                = 60;
        trkCircularity.Location             = new Point(58, 22);
        trkCircularity.Width                = 140;
        trkCircularity.TickStyle            = TickStyle.None;
        trkCircularity.ValueChanged        += trkHsv_ValueChanged;

        lblCircularity.Name      = "lblCircularity";
        lblCircularity.Text      = "0.60";
        lblCircularity.AutoSize  = false;
        lblCircularity.Width     = 36;
        lblCircularity.Height    = 20;
        lblCircularity.Location  = new Point(202, 28);
        lblCircularity.ForeColor = Color.Yellow;
        lblCircularity.TextAlign = ContentAlignment.MiddleRight;

        // ── grpClassification ─────────────────────────────────────────────────
        grpClassification.Name      = "grpClassification";
        grpClassification.Text      = "Phân loại kích thước";
        grpClassification.Dock      = DockStyle.Top;
        grpClassification.Height    = 100;
        grpClassification.ForeColor = Color.White;
        grpClassification.Padding   = new Padding(10, 8, 10, 8);
        grpClassification.Controls.AddRange(new Control[]
        {
            lblSizeThresholdTxt,  nudSizeThreshold,  lblSizeThresholdUnit,
            lblMinContourAreaTxt, nudMinContourArea,  lblMinContourAreaUnit,
        });

        // ── Size Threshold (tỏi to ≥) ─────────────────────────────────────────
        lblSizeThresholdTxt.Name      = "lblSizeThresholdTxt";
        lblSizeThresholdTxt.Text      = "Tỏi to ≥";
        lblSizeThresholdTxt.AutoSize  = false;
        lblSizeThresholdTxt.Width     = 52;
        lblSizeThresholdTxt.Height    = 22;
        lblSizeThresholdTxt.Location  = new Point(10, 28);
        lblSizeThresholdTxt.ForeColor = Color.LightGray;

        nudSizeThreshold.Name          = "nudSizeThreshold";
        nudSizeThreshold.Minimum       = 100;
        nudSizeThreshold.Maximum       = 200_000;
        nudSizeThreshold.Increment     = 500;
        nudSizeThreshold.Value         = 5_000;
        nudSizeThreshold.Location      = new Point(66, 26);
        nudSizeThreshold.Width         = 100;
        nudSizeThreshold.BackColor     = Color.FromArgb(45, 45, 48);
        nudSizeThreshold.ForeColor     = Color.Yellow;
        nudSizeThreshold.ValueChanged += nudClassification_ValueChanged;

        lblSizeThresholdUnit.Name      = "lblSizeThresholdUnit";
        lblSizeThresholdUnit.Text      = "px²";
        lblSizeThresholdUnit.AutoSize  = false;
        lblSizeThresholdUnit.Width     = 28;
        lblSizeThresholdUnit.Height    = 22;
        lblSizeThresholdUnit.Location  = new Point(170, 28);
        lblSizeThresholdUnit.ForeColor = Color.LightGray;

        // ── Min Contour Area (diện tích nhận tối thiểu) ───────────────────────
        lblMinContourAreaTxt.Name      = "lblMinContourAreaTxt";
        lblMinContourAreaTxt.Text      = "Nhỏ nhất ≥";
        lblMinContourAreaTxt.AutoSize  = false;
        lblMinContourAreaTxt.Width     = 52;
        lblMinContourAreaTxt.Height    = 22;
        lblMinContourAreaTxt.Location  = new Point(10, 62);
        lblMinContourAreaTxt.ForeColor = Color.LightGray;

        nudMinContourArea.Name          = "nudMinContourArea";
        nudMinContourArea.Minimum       = 50;
        nudMinContourArea.Maximum       = 50_000;
        nudMinContourArea.Increment     = 100;
        nudMinContourArea.Value         = 500;
        nudMinContourArea.Location      = new Point(66, 60);
        nudMinContourArea.Width         = 100;
        nudMinContourArea.BackColor     = Color.FromArgb(45, 45, 48);
        nudMinContourArea.ForeColor     = Color.Yellow;
        nudMinContourArea.ValueChanged += nudClassification_ValueChanged;

        lblMinContourAreaUnit.Name      = "lblMinContourAreaUnit";
        lblMinContourAreaUnit.Text      = "px²";
        lblMinContourAreaUnit.AutoSize  = false;
        lblMinContourAreaUnit.Width     = 28;
        lblMinContourAreaUnit.Height    = 22;
        lblMinContourAreaUnit.Location  = new Point(170, 62);
        lblMinContourAreaUnit.ForeColor = Color.LightGray;

        // ── grpHsvDamaged ─────────────────────────────────────────────────────
        grpHsvDamaged.Name      = "grpHsvDamaged";
        grpHsvDamaged.Text      = "Ngưỡng HSV tỏi hỏng (nâu/tối)";
        grpHsvDamaged.Dock      = DockStyle.Top;
        grpHsvDamaged.Height    = 270;
        grpHsvDamaged.ForeColor = Color.Salmon;
        grpHsvDamaged.Padding   = new Padding(10, 8, 10, 8);
        grpHsvDamaged.Controls.AddRange(new Control[]
        {
            lblH2MinTxt, trkH2Min, lblH2Min,
            lblH2MaxTxt, trkH2Max, lblH2Max,
            lblS2MinTxt, trkS2Min, lblS2Min,
            lblS2MaxTxt, trkS2Max, lblS2Max,
            lblV2MinTxt, trkV2Min, lblV2Min,
            lblV2MaxTxt, trkV2Max, lblV2Max,
        });

        // H2Min
        lblH2MinTxt.Name = "lblH2MinTxt"; lblH2MinTxt.Text = "H min"; lblH2MinTxt.AutoSize = false; lblH2MinTxt.Width = 40; lblH2MinTxt.Height = 15; lblH2MinTxt.Location = new Point(10,  22); lblH2MinTxt.ForeColor = Color.LightGray;
        trkH2Min.Name = "trkH2Min"; trkH2Min.Minimum = 0; trkH2Min.Maximum = 179; trkH2Min.Value = 5;   trkH2Min.TickFrequency = 18; trkH2Min.Location = new Point(54,  16); trkH2Min.Width = 172; trkH2Min.AutoSize = false; trkH2Min.Height = 28; trkH2Min.ValueChanged += trkHsv_ValueChanged;
        lblH2Min.Name = "lblH2Min"; lblH2Min.AutoSize = false; lblH2Min.Width = 28; lblH2Min.Height = 15; lblH2Min.Location = new Point(230, 22); lblH2Min.ForeColor = Color.Yellow;

        // H2Max
        lblH2MaxTxt.Name = "lblH2MaxTxt"; lblH2MaxTxt.Text = "H max"; lblH2MaxTxt.AutoSize = false; lblH2MaxTxt.Width = 40; lblH2MaxTxt.Height = 15; lblH2MaxTxt.Location = new Point(10,  58); lblH2MaxTxt.ForeColor = Color.LightGray;
        trkH2Max.Name = "trkH2Max"; trkH2Max.Minimum = 0; trkH2Max.Maximum = 179; trkH2Max.Value = 25;  trkH2Max.TickFrequency = 18; trkH2Max.Location = new Point(54,  52); trkH2Max.Width = 172; trkH2Max.AutoSize = false; trkH2Max.Height = 28; trkH2Max.ValueChanged += trkHsv_ValueChanged;
        lblH2Max.Name = "lblH2Max"; lblH2Max.AutoSize = false; lblH2Max.Width = 28; lblH2Max.Height = 15; lblH2Max.Location = new Point(230, 58); lblH2Max.ForeColor = Color.Yellow;

        // S2Min
        lblS2MinTxt.Name = "lblS2MinTxt"; lblS2MinTxt.Text = "S min"; lblS2MinTxt.AutoSize = false; lblS2MinTxt.Width = 40; lblS2MinTxt.Height = 15; lblS2MinTxt.Location = new Point(10,  94); lblS2MinTxt.ForeColor = Color.LightGray;
        trkS2Min.Name = "trkS2Min"; trkS2Min.Minimum = 0; trkS2Min.Maximum = 255; trkS2Min.Value = 40;  trkS2Min.TickFrequency = 25; trkS2Min.Location = new Point(54,  88); trkS2Min.Width = 172; trkS2Min.AutoSize = false; trkS2Min.Height = 28; trkS2Min.ValueChanged += trkHsv_ValueChanged;
        lblS2Min.Name = "lblS2Min"; lblS2Min.AutoSize = false; lblS2Min.Width = 28; lblS2Min.Height = 15; lblS2Min.Location = new Point(230, 94); lblS2Min.ForeColor = Color.Yellow;

        // S2Max
        lblS2MaxTxt.Name = "lblS2MaxTxt"; lblS2MaxTxt.Text = "S max"; lblS2MaxTxt.AutoSize = false; lblS2MaxTxt.Width = 40; lblS2MaxTxt.Height = 15; lblS2MaxTxt.Location = new Point(10, 130); lblS2MaxTxt.ForeColor = Color.LightGray;
        trkS2Max.Name = "trkS2Max"; trkS2Max.Minimum = 0; trkS2Max.Maximum = 255; trkS2Max.Value = 255; trkS2Max.TickFrequency = 25; trkS2Max.Location = new Point(54, 124); trkS2Max.Width = 172; trkS2Max.AutoSize = false; trkS2Max.Height = 28; trkS2Max.ValueChanged += trkHsv_ValueChanged;
        lblS2Max.Name = "lblS2Max"; lblS2Max.AutoSize = false; lblS2Max.Width = 28; lblS2Max.Height = 15; lblS2Max.Location = new Point(230, 130); lblS2Max.ForeColor = Color.Yellow;

        // V2Min
        lblV2MinTxt.Name = "lblV2MinTxt"; lblV2MinTxt.Text = "V min"; lblV2MinTxt.AutoSize = false; lblV2MinTxt.Width = 40; lblV2MinTxt.Height = 15; lblV2MinTxt.Location = new Point(10, 166); lblV2MinTxt.ForeColor = Color.LightGray;
        trkV2Min.Name = "trkV2Min"; trkV2Min.Minimum = 0; trkV2Min.Maximum = 255; trkV2Min.Value = 50;  trkV2Min.TickFrequency = 25; trkV2Min.Location = new Point(54, 160); trkV2Min.Width = 172; trkV2Min.AutoSize = false; trkV2Min.Height = 28; trkV2Min.ValueChanged += trkHsv_ValueChanged;
        lblV2Min.Name = "lblV2Min"; lblV2Min.AutoSize = false; lblV2Min.Width = 28; lblV2Min.Height = 15; lblV2Min.Location = new Point(230, 166); lblV2Min.ForeColor = Color.Yellow;

        // V2Max
        lblV2MaxTxt.Name = "lblV2MaxTxt"; lblV2MaxTxt.Text = "V max"; lblV2MaxTxt.AutoSize = false; lblV2MaxTxt.Width = 40; lblV2MaxTxt.Height = 15; lblV2MaxTxt.Location = new Point(10, 202); lblV2MaxTxt.ForeColor = Color.LightGray;
        trkV2Max.Name = "trkV2Max"; trkV2Max.Minimum = 0; trkV2Max.Maximum = 255; trkV2Max.Value = 175; trkV2Max.TickFrequency = 25; trkV2Max.Location = new Point(54, 196); trkV2Max.Width = 172; trkV2Max.AutoSize = false; trkV2Max.Height = 28; trkV2Max.ValueChanged += trkHsv_ValueChanged;
        lblV2Max.Name = "lblV2Max"; lblV2Max.AutoSize = false; lblV2Max.Width = 28; lblV2Max.Height = 15; lblV2Max.Location = new Point(230, 202); lblV2Max.ForeColor = Color.Yellow;

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
        // Chiều cao = pnlBottom(48) + grpHsvDamaged(270) + grpClassification(100) + grpSegmentation(80) + grpHsv(270)
        ClientSize          = new Size(274, 790);
        FormBorderStyle     = FormBorderStyle.FixedDialog;
        MaximizeBox         = false;
        MinimizeBox         = false;
        Name                = "frmSettings";
        Text                = "Cài đặt";
        StartPosition       = FormStartPosition.Manual;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);
        ShowInTaskbar       = false;
        Load               += frmSettings_Load;

        // Dock=Top: control Add sau sẽ hiển thị phía trên — thứ tự từ trên xuống:
        // grpHsvDamaged → grpClassification → grpSegmentation → grpHsv
        Controls.Add(grpHsv);
        Controls.Add(grpSegmentation);
        Controls.Add(grpClassification);
        Controls.Add(grpHsvDamaged);
        Controls.Add(pnlBottom);

        trkHMin.EndInit();
        trkHMax.EndInit();
        trkSMin.EndInit();
        trkSMax.EndInit();
        trkVMin.EndInit();
        trkVMax.EndInit();
        trkCircularity.EndInit();
        nudSizeThreshold.EndInit();
        nudMinContourArea.EndInit();
        trkH2Min.EndInit();
        trkH2Max.EndInit();
        trkS2Min.EndInit();
        trkS2Max.EndInit();
        trkV2Min.EndInit();
        trkV2Max.EndInit();
        grpHsv.ResumeLayout(false);
        grpSegmentation.ResumeLayout(false);
        grpClassification.ResumeLayout(false);
        grpHsvDamaged.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}

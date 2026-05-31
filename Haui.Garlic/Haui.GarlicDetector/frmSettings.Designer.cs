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
    private Button btnSave;
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
    private NumericUpDown nudRoiPadding;
    private Label         lblSizeThresholdTxt;
    private Label         lblMinContourAreaTxt;
    private Label         lblRoiPaddingTxt;
    private Label         lblSizeThresholdUnit;
    private Label         lblMinContourAreaUnit;
    private Label         lblRoiPaddingUnit;

    // ─── GroupBox ngưỡng HSV phụ (tỏi hỏng) ─────────────────────────────────
    private GroupBox grpHsvDamaged;

    private TrackBar trkH2Min;    private TrackBar trkH2Max;
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

    // ─── GroupBox chế độ nhận diện ───────────────────────────────────────────
    private GroupBox grpDetection;
    private CheckBox chkAutoDetect;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        grpHsv = new GroupBox();
        lblHMinTxt = new Label();
        trkHMin = new TrackBar();
        lblHMin = new Label();
        lblHMaxTxt = new Label();
        trkHMax = new TrackBar();
        lblHMax = new Label();
        lblSMinTxt = new Label();
        trkSMin = new TrackBar();
        lblSMin = new Label();
        lblSMaxTxt = new Label();
        trkSMax = new TrackBar();
        lblSMax = new Label();
        lblVMinTxt = new Label();
        trkVMin = new TrackBar();
        lblVMin = new Label();
        lblVMaxTxt = new Label();
        trkVMax = new TrackBar();
        lblVMax = new Label();
        pnlBottom = new Panel();
        btnReset = new Button();
        btnSave = new Button();
        btnClose = new Button();
        grpSegmentation = new GroupBox();
        lblCircularityTxt = new Label();
        trkCircularity = new TrackBar();
        lblCircularity = new Label();
        grpClassification = new GroupBox();
        lblSizeThresholdTxt = new Label();
        nudSizeThreshold = new NumericUpDown();
        lblSizeThresholdUnit = new Label();
        lblMinContourAreaTxt = new Label();
        nudMinContourArea = new NumericUpDown();
        lblMinContourAreaUnit = new Label();
        lblRoiPaddingTxt = new Label();
        nudRoiPadding = new NumericUpDown();
        lblRoiPaddingUnit = new Label();
        grpHsvDamaged = new GroupBox();
        lblH2MinTxt = new Label();
        trkH2Min = new TrackBar();
        lblH2Min = new Label();
        lblH2MaxTxt = new Label();
        trkH2Max = new TrackBar();
        lblH2Max = new Label();
        lblS2MinTxt = new Label();
        trkS2Min = new TrackBar();
        lblS2Min = new Label();
        lblS2MaxTxt = new Label();
        trkS2Max = new TrackBar();
        lblS2Max = new Label();
        lblV2MinTxt = new Label();
        trkV2Min = new TrackBar();
        lblV2Min = new Label();
        lblV2MaxTxt = new Label();
        trkV2Max = new TrackBar();
        lblV2Max = new Label();
        grpDetection = new GroupBox();
        chkAutoDetect = new CheckBox();
        grpHsv.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trkHMin).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkHMax).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkSMin).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkSMax).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkVMin).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkVMax).BeginInit();
        pnlBottom.SuspendLayout();
        grpSegmentation.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trkCircularity).BeginInit();
        grpClassification.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudSizeThreshold).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMinContourArea).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudRoiPadding).BeginInit();
        grpHsvDamaged.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)trkH2Min).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkH2Max).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkS2Min).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkS2Max).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkV2Min).BeginInit();
        ((System.ComponentModel.ISupportInitialize)trkV2Max).BeginInit();
        grpDetection.SuspendLayout();
        SuspendLayout();
        // 
        // grpHsv
        // 
        grpHsv.Controls.Add(lblHMinTxt);
        grpHsv.Controls.Add(trkHMin);
        grpHsv.Controls.Add(lblHMin);
        grpHsv.Controls.Add(lblHMaxTxt);
        grpHsv.Controls.Add(trkHMax);
        grpHsv.Controls.Add(lblHMax);
        grpHsv.Controls.Add(lblSMinTxt);
        grpHsv.Controls.Add(trkSMin);
        grpHsv.Controls.Add(lblSMin);
        grpHsv.Controls.Add(lblSMaxTxt);
        grpHsv.Controls.Add(trkSMax);
        grpHsv.Controls.Add(lblSMax);
        grpHsv.Controls.Add(lblVMinTxt);
        grpHsv.Controls.Add(trkVMin);
        grpHsv.Controls.Add(lblVMin);
        grpHsv.Controls.Add(lblVMaxTxt);
        grpHsv.Controls.Add(trkVMax);
        grpHsv.Controls.Add(lblVMax);
        grpHsv.Dock = DockStyle.Top;
        grpHsv.ForeColor = Color.White;
        grpHsv.Location = new Point(0, 0);
        grpHsv.Name = "grpHsv";
        grpHsv.Padding = new Padding(10, 8, 10, 8);
        grpHsv.Size = new Size(200, 270);
        grpHsv.TabIndex = 0;
        grpHsv.TabStop = false;
        grpHsv.Text = "Ngưỡng HSV";
        // 
        // lblHMinTxt
        // 
        lblHMinTxt.ForeColor = Color.LightGray;
        lblHMinTxt.Location = new Point(10, 28);
        lblHMinTxt.Name = "lblHMinTxt";
        lblHMinTxt.Size = new Size(44, 20);
        lblHMinTxt.TabIndex = 0;
        lblHMinTxt.Text = "H Min";
        // 
        // trkHMin
        // 
        trkHMin.Location = new Point(58, 22);
        trkHMin.Maximum = 179;
        trkHMin.Name = "trkHMin";
        trkHMin.Size = new Size(140, 45);
        trkHMin.TabIndex = 1;
        trkHMin.TickStyle = TickStyle.None;
        trkHMin.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblHMin
        // 
        lblHMin.ForeColor = Color.Yellow;
        lblHMin.Location = new Point(202, 28);
        lblHMin.Name = "lblHMin";
        lblHMin.Size = new Size(36, 20);
        lblHMin.TabIndex = 2;
        lblHMin.Text = "0";
        lblHMin.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblHMaxTxt
        // 
        lblHMaxTxt.ForeColor = Color.LightGray;
        lblHMaxTxt.Location = new Point(10, 66);
        lblHMaxTxt.Name = "lblHMaxTxt";
        lblHMaxTxt.Size = new Size(44, 20);
        lblHMaxTxt.TabIndex = 3;
        lblHMaxTxt.Text = "H Max";
        // 
        // trkHMax
        // 
        trkHMax.Location = new Point(58, 60);
        trkHMax.Maximum = 179;
        trkHMax.Name = "trkHMax";
        trkHMax.Size = new Size(140, 45);
        trkHMax.TabIndex = 4;
        trkHMax.TickStyle = TickStyle.None;
        trkHMax.Value = 179;
        trkHMax.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblHMax
        // 
        lblHMax.ForeColor = Color.Yellow;
        lblHMax.Location = new Point(202, 66);
        lblHMax.Name = "lblHMax";
        lblHMax.Size = new Size(36, 20);
        lblHMax.TabIndex = 5;
        lblHMax.Text = "179";
        lblHMax.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblSMinTxt
        // 
        lblSMinTxt.ForeColor = Color.LightGray;
        lblSMinTxt.Location = new Point(10, 104);
        lblSMinTxt.Name = "lblSMinTxt";
        lblSMinTxt.Size = new Size(44, 20);
        lblSMinTxt.TabIndex = 6;
        lblSMinTxt.Text = "S Min";
        // 
        // trkSMin
        // 
        trkSMin.Location = new Point(58, 98);
        trkSMin.Maximum = 255;
        trkSMin.Name = "trkSMin";
        trkSMin.Size = new Size(140, 45);
        trkSMin.TabIndex = 7;
        trkSMin.TickStyle = TickStyle.None;
        trkSMin.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblSMin
        // 
        lblSMin.ForeColor = Color.Yellow;
        lblSMin.Location = new Point(202, 104);
        lblSMin.Name = "lblSMin";
        lblSMin.Size = new Size(36, 20);
        lblSMin.TabIndex = 8;
        lblSMin.Text = "0";
        lblSMin.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblSMaxTxt
        // 
        lblSMaxTxt.ForeColor = Color.LightGray;
        lblSMaxTxt.Location = new Point(10, 142);
        lblSMaxTxt.Name = "lblSMaxTxt";
        lblSMaxTxt.Size = new Size(44, 20);
        lblSMaxTxt.TabIndex = 9;
        lblSMaxTxt.Text = "S Max";
        // 
        // trkSMax
        // 
        trkSMax.Location = new Point(58, 136);
        trkSMax.Maximum = 255;
        trkSMax.Name = "trkSMax";
        trkSMax.Size = new Size(140, 45);
        trkSMax.TabIndex = 10;
        trkSMax.TickStyle = TickStyle.None;
        trkSMax.Value = 60;
        trkSMax.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblSMax
        // 
        lblSMax.ForeColor = Color.Yellow;
        lblSMax.Location = new Point(202, 142);
        lblSMax.Name = "lblSMax";
        lblSMax.Size = new Size(36, 20);
        lblSMax.TabIndex = 11;
        lblSMax.Text = "60";
        lblSMax.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblVMinTxt
        // 
        lblVMinTxt.ForeColor = Color.LightGray;
        lblVMinTxt.Location = new Point(10, 180);
        lblVMinTxt.Name = "lblVMinTxt";
        lblVMinTxt.Size = new Size(44, 20);
        lblVMinTxt.TabIndex = 12;
        lblVMinTxt.Text = "V Min";
        // 
        // trkVMin
        // 
        trkVMin.Location = new Point(58, 174);
        trkVMin.Maximum = 255;
        trkVMin.Name = "trkVMin";
        trkVMin.Size = new Size(140, 45);
        trkVMin.TabIndex = 13;
        trkVMin.TickStyle = TickStyle.None;
        trkVMin.Value = 170;
        trkVMin.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblVMin
        // 
        lblVMin.ForeColor = Color.Yellow;
        lblVMin.Location = new Point(202, 180);
        lblVMin.Name = "lblVMin";
        lblVMin.Size = new Size(36, 20);
        lblVMin.TabIndex = 14;
        lblVMin.Text = "170";
        lblVMin.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblVMaxTxt
        // 
        lblVMaxTxt.ForeColor = Color.LightGray;
        lblVMaxTxt.Location = new Point(10, 218);
        lblVMaxTxt.Name = "lblVMaxTxt";
        lblVMaxTxt.Size = new Size(44, 20);
        lblVMaxTxt.TabIndex = 15;
        lblVMaxTxt.Text = "V Max";
        // 
        // trkVMax
        // 
        trkVMax.Location = new Point(58, 212);
        trkVMax.Maximum = 255;
        trkVMax.Name = "trkVMax";
        trkVMax.Size = new Size(140, 45);
        trkVMax.TabIndex = 16;
        trkVMax.TickStyle = TickStyle.None;
        trkVMax.Value = 255;
        trkVMax.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblVMax
        // 
        lblVMax.ForeColor = Color.Yellow;
        lblVMax.Location = new Point(202, 218);
        lblVMax.Name = "lblVMax";
        lblVMax.Size = new Size(36, 20);
        lblVMax.TabIndex = 17;
        lblVMax.Text = "255";
        lblVMax.TextAlign = ContentAlignment.MiddleRight;
        // 
        // pnlBottom
        // 
        pnlBottom.BackColor = Color.FromArgb(45, 45, 48);
        pnlBottom.Controls.Add(btnReset);
        pnlBottom.Controls.Add(btnSave);
        pnlBottom.Controls.Add(btnClose);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 302);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Size = new Size(274, 48);
        pnlBottom.TabIndex = 3;
        // 
        // btnReset
        // 
        btnReset.BackColor = Color.FromArgb(80, 80, 80);
        btnReset.FlatAppearance.BorderSize = 0;
        btnReset.FlatStyle = FlatStyle.Flat;
        btnReset.ForeColor = Color.White;
        btnReset.Location = new Point(5, 9);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(79, 30);
        btnReset.TabIndex = 0;
        btnReset.Text = "↺ Đặt lại";
        btnReset.UseVisualStyleBackColor = false;
        btnReset.Click += btnReset_Click;
        // 
        // btnSave
        // 
        btnSave.BackColor = Color.FromArgb(30, 130, 30);
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(89, 9);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(88, 30);
        btnSave.TabIndex = 1;
        btnSave.Text = "💾 Lưu";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += btnSave_Click;
        // 
        // btnClose
        // 
        btnClose.BackColor = Color.FromArgb(180, 30, 30);
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.ForeColor = Color.White;
        btnClose.Location = new Point(182, 9);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(87, 30);
        btnClose.TabIndex = 2;
        btnClose.Text = "✕ Đóng";
        btnClose.UseVisualStyleBackColor = false;
        btnClose.Click += btnClose_Click;
        // 
        // grpSegmentation
        // 
        grpSegmentation.Controls.Add(lblCircularityTxt);
        grpSegmentation.Controls.Add(trkCircularity);
        grpSegmentation.Controls.Add(lblCircularity);
        grpSegmentation.Dock = DockStyle.Top;
        grpSegmentation.ForeColor = Color.White;
        grpSegmentation.Location = new Point(0, 192);
        grpSegmentation.Name = "grpSegmentation";
        grpSegmentation.Padding = new Padding(10, 8, 10, 8);
        grpSegmentation.Size = new Size(274, 62);
        grpSegmentation.TabIndex = 0;
        grpSegmentation.TabStop = false;
        grpSegmentation.Text = "Phân vùng";
        // 
        // lblCircularityTxt
        // 
        lblCircularityTxt.ForeColor = Color.LightGray;
        lblCircularityTxt.Location = new Point(10, 28);
        lblCircularityTxt.Name = "lblCircularityTxt";
        lblCircularityTxt.Size = new Size(44, 20);
        lblCircularityTxt.TabIndex = 0;
        lblCircularityTxt.Text = "Độ tròn";
        // 
        // trkCircularity
        // 
        trkCircularity.Location = new Point(76, 22);
        trkCircularity.Maximum = 100;
        trkCircularity.Name = "trkCircularity";
        trkCircularity.Size = new Size(140, 45);
        trkCircularity.TabIndex = 1;
        trkCircularity.TickStyle = TickStyle.None;
        trkCircularity.Value = 60;
        trkCircularity.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblCircularity
        // 
        lblCircularity.ForeColor = Color.Yellow;
        lblCircularity.Location = new Point(220, 28);
        lblCircularity.Name = "lblCircularity";
        lblCircularity.Size = new Size(36, 20);
        lblCircularity.TabIndex = 2;
        lblCircularity.Text = "0.60";
        lblCircularity.TextAlign = ContentAlignment.MiddleRight;
        // 
        // grpClassification
        // 
        grpClassification.Controls.Add(lblSizeThresholdTxt);
        grpClassification.Controls.Add(nudSizeThreshold);
        grpClassification.Controls.Add(lblSizeThresholdUnit);
        grpClassification.Controls.Add(lblMinContourAreaTxt);
        grpClassification.Controls.Add(nudMinContourArea);
        grpClassification.Controls.Add(lblMinContourAreaUnit);
        grpClassification.Controls.Add(lblRoiPaddingTxt);
        grpClassification.Controls.Add(nudRoiPadding);
        grpClassification.Controls.Add(lblRoiPaddingUnit);
        grpClassification.Dock = DockStyle.Top;
        grpClassification.ForeColor = Color.White;
        grpClassification.Location = new Point(0, 56);
        grpClassification.Name = "grpClassification";
        grpClassification.Padding = new Padding(10, 8, 10, 8);
        grpClassification.Size = new Size(274, 136);
        grpClassification.TabIndex = 1;
        grpClassification.TabStop = false;
        grpClassification.Text = "Phân loại kích thước";
        // 
        // lblSizeThresholdTxt
        // 
        lblSizeThresholdTxt.ForeColor = Color.LightGray;
        lblSizeThresholdTxt.Location = new Point(10, 28);
        lblSizeThresholdTxt.Name = "lblSizeThresholdTxt";
        lblSizeThresholdTxt.Size = new Size(52, 22);
        lblSizeThresholdTxt.TabIndex = 0;
        lblSizeThresholdTxt.Text = "Tỏi to ≥";
        // 
        // nudSizeThreshold
        // 
        nudSizeThreshold.BackColor = Color.FromArgb(45, 45, 48);
        nudSizeThreshold.ForeColor = Color.Yellow;
        nudSizeThreshold.Increment = new decimal(new int[] { 500, 0, 0, 0 });
        nudSizeThreshold.Location = new Point(84, 26);
        nudSizeThreshold.Maximum = new decimal(new int[] { 200000, 0, 0, 0 });
        nudSizeThreshold.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
        nudSizeThreshold.Name = "nudSizeThreshold";
        nudSizeThreshold.Size = new Size(100, 23);
        nudSizeThreshold.TabIndex = 1;
        nudSizeThreshold.Value = new decimal(new int[] { 5000, 0, 0, 0 });
        nudSizeThreshold.ValueChanged += nudClassification_ValueChanged;
        // 
        // lblSizeThresholdUnit
        // 
        lblSizeThresholdUnit.ForeColor = Color.LightGray;
        lblSizeThresholdUnit.Location = new Point(188, 28);
        lblSizeThresholdUnit.Name = "lblSizeThresholdUnit";
        lblSizeThresholdUnit.Size = new Size(28, 22);
        lblSizeThresholdUnit.TabIndex = 2;
        lblSizeThresholdUnit.Text = "px²";
        // 
        // lblMinContourAreaTxt
        // 
        lblMinContourAreaTxt.ForeColor = Color.LightGray;
        lblMinContourAreaTxt.Location = new Point(10, 62);
        lblMinContourAreaTxt.Name = "lblMinContourAreaTxt";
        lblMinContourAreaTxt.Size = new Size(52, 22);
        lblMinContourAreaTxt.TabIndex = 3;
        lblMinContourAreaTxt.Text = "Nhỏ nhất ≥";
        // 
        // nudMinContourArea
        // 
        nudMinContourArea.BackColor = Color.FromArgb(45, 45, 48);
        nudMinContourArea.ForeColor = Color.Yellow;
        nudMinContourArea.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        nudMinContourArea.Location = new Point(84, 60);
        nudMinContourArea.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
        nudMinContourArea.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
        nudMinContourArea.Name = "nudMinContourArea";
        nudMinContourArea.Size = new Size(100, 23);
        nudMinContourArea.TabIndex = 4;
        nudMinContourArea.Value = new decimal(new int[] { 500, 0, 0, 0 });
        nudMinContourArea.ValueChanged += nudClassification_ValueChanged;
        // 
        // lblMinContourAreaUnit
        // 
        lblMinContourAreaUnit.ForeColor = Color.LightGray;
        lblMinContourAreaUnit.Location = new Point(188, 62);
        lblMinContourAreaUnit.Name = "lblMinContourAreaUnit";
        lblMinContourAreaUnit.Size = new Size(28, 22);
        lblMinContourAreaUnit.TabIndex = 5;
        lblMinContourAreaUnit.Text = "px²";
        // 
        // lblRoiPaddingTxt
        // 
        lblRoiPaddingTxt.ForeColor = Color.LightGray;
        lblRoiPaddingTxt.Location = new Point(10, 98);
        lblRoiPaddingTxt.Name = "lblRoiPaddingTxt";
        lblRoiPaddingTxt.Size = new Size(69, 22);
        lblRoiPaddingTxt.TabIndex = 6;
        lblRoiPaddingTxt.Text = "Padding";
        // 
        // nudRoiPadding
        // 
        nudRoiPadding.BackColor = Color.FromArgb(45, 45, 48);
        nudRoiPadding.ForeColor = Color.Yellow;
        nudRoiPadding.Increment = new decimal(new int[] { 5, 0, 0, 0 });
        nudRoiPadding.Location = new Point(84, 96);
        nudRoiPadding.Name = "nudRoiPadding";
        nudRoiPadding.Size = new Size(100, 23);
        nudRoiPadding.TabIndex = 7;
        nudRoiPadding.ValueChanged += nudClassification_ValueChanged;
        // 
        // lblRoiPaddingUnit
        // 
        lblRoiPaddingUnit.ForeColor = Color.LightGray;
        lblRoiPaddingUnit.Location = new Point(188, 98);
        lblRoiPaddingUnit.Name = "lblRoiPaddingUnit";
        lblRoiPaddingUnit.Size = new Size(28, 22);
        lblRoiPaddingUnit.TabIndex = 8;
        lblRoiPaddingUnit.Text = "px";
        // 
        // grpHsvDamaged
        // 
        grpHsvDamaged.Controls.Add(lblH2MinTxt);
        grpHsvDamaged.Controls.Add(trkH2Min);
        grpHsvDamaged.Controls.Add(lblH2Min);
        grpHsvDamaged.Controls.Add(lblH2MaxTxt);
        grpHsvDamaged.Controls.Add(trkH2Max);
        grpHsvDamaged.Controls.Add(lblH2Max);
        grpHsvDamaged.Controls.Add(lblS2MinTxt);
        grpHsvDamaged.Controls.Add(trkS2Min);
        grpHsvDamaged.Controls.Add(lblS2Min);
        grpHsvDamaged.Controls.Add(lblS2MaxTxt);
        grpHsvDamaged.Controls.Add(trkS2Max);
        grpHsvDamaged.Controls.Add(lblS2Max);
        grpHsvDamaged.Controls.Add(lblV2MinTxt);
        grpHsvDamaged.Controls.Add(trkV2Min);
        grpHsvDamaged.Controls.Add(lblV2Min);
        grpHsvDamaged.Controls.Add(lblV2MaxTxt);
        grpHsvDamaged.Controls.Add(trkV2Max);
        grpHsvDamaged.Controls.Add(lblV2Max);
        grpHsvDamaged.Dock = DockStyle.Top;
        grpHsvDamaged.ForeColor = Color.Salmon;
        grpHsvDamaged.Location = new Point(0, 0);
        grpHsvDamaged.Name = "grpHsvDamaged";
        grpHsvDamaged.Padding = new Padding(10, 8, 10, 8);
        grpHsvDamaged.Size = new Size(200, 270);
        grpHsvDamaged.TabIndex = 0;
        grpHsvDamaged.TabStop = false;
        grpHsvDamaged.Text = "Ngưỡng HSV tỏi hỏng (nâu/tối)";
        // 
        // lblH2MinTxt
        // 
        lblH2MinTxt.ForeColor = Color.LightGray;
        lblH2MinTxt.Location = new Point(10, 22);
        lblH2MinTxt.Name = "lblH2MinTxt";
        lblH2MinTxt.Size = new Size(40, 15);
        lblH2MinTxt.TabIndex = 0;
        lblH2MinTxt.Text = "H min";
        // 
        // trkH2Min
        // 
        trkH2Min.AutoSize = false;
        trkH2Min.Location = new Point(54, 16);
        trkH2Min.Maximum = 179;
        trkH2Min.Name = "trkH2Min";
        trkH2Min.Size = new Size(172, 28);
        trkH2Min.TabIndex = 1;
        trkH2Min.TickFrequency = 18;
        trkH2Min.Value = 5;
        trkH2Min.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblH2Min
        // 
        lblH2Min.ForeColor = Color.Yellow;
        lblH2Min.Location = new Point(230, 22);
        lblH2Min.Name = "lblH2Min";
        lblH2Min.Size = new Size(28, 15);
        lblH2Min.TabIndex = 2;
        // 
        // lblH2MaxTxt
        // 
        lblH2MaxTxt.ForeColor = Color.LightGray;
        lblH2MaxTxt.Location = new Point(10, 58);
        lblH2MaxTxt.Name = "lblH2MaxTxt";
        lblH2MaxTxt.Size = new Size(40, 15);
        lblH2MaxTxt.TabIndex = 3;
        lblH2MaxTxt.Text = "H max";
        // 
        // trkH2Max
        // 
        trkH2Max.AutoSize = false;
        trkH2Max.Location = new Point(54, 52);
        trkH2Max.Maximum = 179;
        trkH2Max.Name = "trkH2Max";
        trkH2Max.Size = new Size(172, 28);
        trkH2Max.TabIndex = 4;
        trkH2Max.TickFrequency = 18;
        trkH2Max.Value = 25;
        trkH2Max.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblH2Max
        // 
        lblH2Max.ForeColor = Color.Yellow;
        lblH2Max.Location = new Point(230, 58);
        lblH2Max.Name = "lblH2Max";
        lblH2Max.Size = new Size(28, 15);
        lblH2Max.TabIndex = 5;
        // 
        // lblS2MinTxt
        // 
        lblS2MinTxt.ForeColor = Color.LightGray;
        lblS2MinTxt.Location = new Point(10, 94);
        lblS2MinTxt.Name = "lblS2MinTxt";
        lblS2MinTxt.Size = new Size(40, 15);
        lblS2MinTxt.TabIndex = 6;
        lblS2MinTxt.Text = "S min";
        // 
        // trkS2Min
        // 
        trkS2Min.AutoSize = false;
        trkS2Min.Location = new Point(54, 88);
        trkS2Min.Maximum = 255;
        trkS2Min.Name = "trkS2Min";
        trkS2Min.Size = new Size(172, 28);
        trkS2Min.TabIndex = 7;
        trkS2Min.TickFrequency = 25;
        trkS2Min.Value = 40;
        trkS2Min.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblS2Min
        // 
        lblS2Min.ForeColor = Color.Yellow;
        lblS2Min.Location = new Point(230, 94);
        lblS2Min.Name = "lblS2Min";
        lblS2Min.Size = new Size(28, 15);
        lblS2Min.TabIndex = 8;
        // 
        // lblS2MaxTxt
        // 
        lblS2MaxTxt.ForeColor = Color.LightGray;
        lblS2MaxTxt.Location = new Point(10, 130);
        lblS2MaxTxt.Name = "lblS2MaxTxt";
        lblS2MaxTxt.Size = new Size(40, 15);
        lblS2MaxTxt.TabIndex = 9;
        lblS2MaxTxt.Text = "S max";
        // 
        // trkS2Max
        // 
        trkS2Max.AutoSize = false;
        trkS2Max.Location = new Point(54, 124);
        trkS2Max.Maximum = 255;
        trkS2Max.Name = "trkS2Max";
        trkS2Max.Size = new Size(172, 28);
        trkS2Max.TabIndex = 10;
        trkS2Max.TickFrequency = 25;
        trkS2Max.Value = 255;
        trkS2Max.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblS2Max
        // 
        lblS2Max.ForeColor = Color.Yellow;
        lblS2Max.Location = new Point(230, 130);
        lblS2Max.Name = "lblS2Max";
        lblS2Max.Size = new Size(28, 15);
        lblS2Max.TabIndex = 11;
        // 
        // lblV2MinTxt
        // 
        lblV2MinTxt.ForeColor = Color.LightGray;
        lblV2MinTxt.Location = new Point(10, 166);
        lblV2MinTxt.Name = "lblV2MinTxt";
        lblV2MinTxt.Size = new Size(40, 15);
        lblV2MinTxt.TabIndex = 12;
        lblV2MinTxt.Text = "V min";
        // 
        // trkV2Min
        // 
        trkV2Min.AutoSize = false;
        trkV2Min.Location = new Point(54, 160);
        trkV2Min.Maximum = 255;
        trkV2Min.Name = "trkV2Min";
        trkV2Min.Size = new Size(172, 28);
        trkV2Min.TabIndex = 13;
        trkV2Min.TickFrequency = 25;
        trkV2Min.Value = 50;
        trkV2Min.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblV2Min
        // 
        lblV2Min.ForeColor = Color.Yellow;
        lblV2Min.Location = new Point(230, 166);
        lblV2Min.Name = "lblV2Min";
        lblV2Min.Size = new Size(28, 15);
        lblV2Min.TabIndex = 14;
        // 
        // lblV2MaxTxt
        // 
        lblV2MaxTxt.ForeColor = Color.LightGray;
        lblV2MaxTxt.Location = new Point(10, 202);
        lblV2MaxTxt.Name = "lblV2MaxTxt";
        lblV2MaxTxt.Size = new Size(40, 15);
        lblV2MaxTxt.TabIndex = 15;
        lblV2MaxTxt.Text = "V max";
        // 
        // trkV2Max
        // 
        trkV2Max.AutoSize = false;
        trkV2Max.Location = new Point(54, 196);
        trkV2Max.Maximum = 255;
        trkV2Max.Name = "trkV2Max";
        trkV2Max.Size = new Size(172, 28);
        trkV2Max.TabIndex = 16;
        trkV2Max.TickFrequency = 25;
        trkV2Max.Value = 175;
        trkV2Max.ValueChanged += trkHsv_ValueChanged;
        // 
        // lblV2Max
        // 
        lblV2Max.ForeColor = Color.Yellow;
        lblV2Max.Location = new Point(230, 202);
        lblV2Max.Name = "lblV2Max";
        lblV2Max.Size = new Size(28, 15);
        lblV2Max.TabIndex = 17;
        // 
        // grpDetection
        // 
        grpDetection.Controls.Add(chkAutoDetect);
        grpDetection.Dock = DockStyle.Top;
        grpDetection.ForeColor = Color.White;
        grpDetection.Location = new Point(0, 0);
        grpDetection.Name = "grpDetection";
        grpDetection.Padding = new Padding(10, 8, 10, 8);
        grpDetection.Size = new Size(274, 56);
        grpDetection.TabIndex = 2;
        grpDetection.TabStop = false;
        grpDetection.Text = "Chế độ nhận diện";
        // 
        // chkAutoDetect
        // 
        chkAutoDetect.AutoSize = true;
        chkAutoDetect.ForeColor = Color.White;
        chkAutoDetect.Location = new Point(12, 24);
        chkAutoDetect.Name = "chkAutoDetect";
        chkAutoDetect.Size = new Size(126, 19);
        chkAutoDetect.TabIndex = 0;
        chkAutoDetect.Text = "Tự động nhận diện";
        chkAutoDetect.CheckedChanged += chkAutoDetect_CheckedChanged;
        // 
        // frmSettings
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(274, 350);
        Controls.Add(grpSegmentation);
        Controls.Add(grpClassification);
        Controls.Add(grpDetection);
        Controls.Add(pnlBottom);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "frmSettings";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Cài đặt";
        Load += frmSettings_Load;
        grpHsv.ResumeLayout(false);
        grpHsv.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)trkHMin).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkHMax).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkSMin).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkSMax).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkVMin).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkVMax).EndInit();
        pnlBottom.ResumeLayout(false);
        grpSegmentation.ResumeLayout(false);
        grpSegmentation.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)trkCircularity).EndInit();
        grpClassification.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)nudSizeThreshold).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMinContourArea).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudRoiPadding).EndInit();
        grpHsvDamaged.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)trkH2Min).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkH2Max).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkS2Min).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkS2Max).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkV2Min).EndInit();
        ((System.ComponentModel.ISupportInitialize)trkV2Max).EndInit();
        grpDetection.ResumeLayout(false);
        grpDetection.PerformLayout();
        ResumeLayout(false);
    }
}

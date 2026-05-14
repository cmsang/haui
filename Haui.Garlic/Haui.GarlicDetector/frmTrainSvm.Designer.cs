namespace Haui.GarlicDetector;

partial class frmTrainSvm
{
    private System.ComponentModel.IContainer components = null;

    // ─── Controls ────────────────────────────────────────────────────────────
    private GroupBox   grpParams;
    private GroupBox   grpLog;
    private Label      lblTrainFolder;
    private TextBox    txtTrainFolder;
    private Button     btnBrowseData;
    private Label      lblOutputFolder;
    private TextBox    txtOutputFolder;
    private Button     btnBrowseOutput;
    private Label      lblC;
    private NumericUpDown numC;
    private Label      lblGamma;
    private NumericUpDown numGamma;
    private Label      lblImageSize;
    private NumericUpDown numImageSize;
    private Label      lblImageSizeUnit;
    private RichTextBox rtxLog;
    private Button     btnTrain;
    private ProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        grpParams       = new GroupBox();
        grpLog          = new GroupBox();
        lblTrainFolder  = new Label();
        txtTrainFolder  = new TextBox();
        btnBrowseData   = new Button();
        lblOutputFolder = new Label();
        txtOutputFolder = new TextBox();
        btnBrowseOutput = new Button();
        lblC            = new Label();
        numC            = new NumericUpDown();
        lblGamma        = new Label();
        numGamma        = new NumericUpDown();
        lblImageSize    = new Label();
        numImageSize    = new NumericUpDown();
        lblImageSizeUnit = new Label();
        rtxLog          = new RichTextBox();
        btnTrain        = new Button();
        progressBar     = new ProgressBar();

        grpParams.SuspendLayout();
        grpLog.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numC).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numGamma).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numImageSize).BeginInit();
        SuspendLayout();

        // ── grpParams ────────────────────────────────────────────────────────
        grpParams.Text     = "Thông số huấn luyện";
        grpParams.Location = new Point(12, 12);
        grpParams.Size     = new Size(580, 190);
        grpParams.Font     = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        // ── Row 1: Thư mục dữ liệu ───────────────────────────────────────────
        lblTrainFolder.Text      = "Thư mục dữ liệu:";
        lblTrainFolder.Font      = new Font("Segoe UI", 9.5f);
        lblTrainFolder.Location  = new Point(15, 32);
        lblTrainFolder.Size      = new Size(130, 22);
        lblTrainFolder.TextAlign = ContentAlignment.MiddleLeft;

        txtTrainFolder.Location  = new Point(150, 32);
        txtTrainFolder.Size      = new Size(330, 24);
        txtTrainFolder.Font      = new Font("Segoe UI", 9.5f);
        txtTrainFolder.ReadOnly  = true;
        txtTrainFolder.BackColor = SystemColors.Window;

        btnBrowseData.Text     = "...";
        btnBrowseData.Location = new Point(488, 31);
        btnBrowseData.Size     = new Size(75, 26);
        btnBrowseData.Font     = new Font("Segoe UI", 9.5f);
        btnBrowseData.Click   += btnBrowseData_Click;

        // ── Row 2: Thư mục lưu model ─────────────────────────────────────────
        lblOutputFolder.Text      = "Thư mục lưu model:";
        lblOutputFolder.Font      = new Font("Segoe UI", 9.5f);
        lblOutputFolder.Location  = new Point(15, 66);
        lblOutputFolder.Size      = new Size(130, 22);
        lblOutputFolder.TextAlign = ContentAlignment.MiddleLeft;

        txtOutputFolder.Location  = new Point(150, 66);
        txtOutputFolder.Size      = new Size(330, 24);
        txtOutputFolder.Font      = new Font("Segoe UI", 9.5f);
        txtOutputFolder.ReadOnly  = true;
        txtOutputFolder.BackColor = SystemColors.Window;

        btnBrowseOutput.Text     = "...";
        btnBrowseOutput.Location = new Point(488, 65);
        btnBrowseOutput.Size     = new Size(75, 26);
        btnBrowseOutput.Font     = new Font("Segoe UI", 9.5f);
        btnBrowseOutput.Click   += btnBrowseOutput_Click;

        // ── Row 3: C + Gamma ─────────────────────────────────────────────────
        lblC.Text      = "Tham số C:";
        lblC.Font      = new Font("Segoe UI", 9.5f);
        lblC.Location  = new Point(15, 104);
        lblC.Size      = new Size(90, 22);
        lblC.TextAlign = ContentAlignment.MiddleLeft;

        numC.Location             = new Point(110, 103);
        numC.Size                 = new Size(100, 24);
        numC.Font                 = new Font("Segoe UI", 9.5f);
        numC.DecimalPlaces        = 3;
        numC.Minimum              = 0.001m;
        numC.Maximum              = 10000m;
        numC.Increment            = 1m;
        numC.Value                = 10m;

        lblGamma.Text      = "Gamma:";
        lblGamma.Font      = new Font("Segoe UI", 9.5f);
        lblGamma.Location  = new Point(230, 104);
        lblGamma.Size      = new Size(70, 22);
        lblGamma.TextAlign = ContentAlignment.MiddleLeft;

        numGamma.Location      = new Point(305, 103);
        numGamma.Size          = new Size(100, 24);
        numGamma.Font          = new Font("Segoe UI", 9.5f);
        numGamma.DecimalPlaces = 4;
        numGamma.Minimum       = 0.0001m;
        numGamma.Maximum       = 100m;
        numGamma.Increment     = 0.1m;
        numGamma.Value         = 0.5m;

        // ── Row 4: Image Size ─────────────────────────────────────────────────
        lblImageSize.Text      = "Kích thước ảnh:";
        lblImageSize.Font      = new Font("Segoe UI", 9.5f);
        lblImageSize.Location  = new Point(15, 142);
        lblImageSize.Size      = new Size(120, 22);
        lblImageSize.TextAlign = ContentAlignment.MiddleLeft;

        numImageSize.Location      = new Point(140, 141);
        numImageSize.Size          = new Size(80, 24);
        numImageSize.Font          = new Font("Segoe UI", 9.5f);
        numImageSize.DecimalPlaces = 0;
        numImageSize.Minimum       = 16;
        numImageSize.Maximum       = 256;
        numImageSize.Increment     = 8;
        numImageSize.Value         = 64;

        lblImageSizeUnit.Text      = "px (áp dụng khi resize ROI)";
        lblImageSizeUnit.Font      = new Font("Segoe UI", 9f, FontStyle.Italic);
        lblImageSizeUnit.ForeColor = Color.Gray;
        lblImageSizeUnit.Location  = new Point(228, 143);
        lblImageSizeUnit.Size      = new Size(240, 22);
        lblImageSizeUnit.TextAlign = ContentAlignment.MiddleLeft;

        grpParams.Controls.AddRange(new Control[]
        {
            lblTrainFolder,  txtTrainFolder,  btnBrowseData,
            lblOutputFolder, txtOutputFolder, btnBrowseOutput,
            lblC, numC, lblGamma, numGamma,
            lblImageSize, numImageSize, lblImageSizeUnit,
        });

        // ── grpLog ───────────────────────────────────────────────────────────
        grpLog.Text     = "Log huấn luyện";
        grpLog.Location = new Point(12, 212);
        grpLog.Size     = new Size(580, 240);
        grpLog.Font     = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        rtxLog.Location   = new Point(10, 24);
        rtxLog.Size       = new Size(558, 206);
        rtxLog.Font       = new Font("Consolas", 9f);
        rtxLog.ReadOnly   = true;
        rtxLog.BackColor  = Color.FromArgb(20, 20, 20);
        rtxLog.ForeColor  = Color.LimeGreen;
        rtxLog.ScrollBars = RichTextBoxScrollBars.Vertical;
        rtxLog.BorderStyle = BorderStyle.None;

        grpLog.Controls.Add(rtxLog);

        // ── progressBar ──────────────────────────────────────────────────────
        progressBar.Location    = new Point(12, 462);
        progressBar.Size        = new Size(580, 18);
        progressBar.Style       = ProgressBarStyle.Continuous;
        progressBar.MarqueeAnimationSpeed = 30;

        // ── btnTrain ─────────────────────────────────────────────────────────
        btnTrain.Text      = "🚀  Huấn luyện & Lưu Model";
        btnTrain.Location  = new Point(12, 488);
        btnTrain.Size      = new Size(580, 40);
        btnTrain.Font      = new Font("Segoe UI", 11f, FontStyle.Bold);
        btnTrain.BackColor = Color.FromArgb(0, 122, 204);
        btnTrain.ForeColor = Color.White;
        btnTrain.FlatStyle = FlatStyle.Flat;
        btnTrain.FlatAppearance.BorderSize = 0;
        btnTrain.Cursor    = Cursors.Hand;
        btnTrain.Click    += btnTrain_Click;

        // ── Form ─────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7f, 15f);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(608, 542);
        FormBorderStyle     = FormBorderStyle.FixedSingle;
        MaximizeBox         = false;
        StartPosition       = FormStartPosition.CenterParent;
        Text                = "Huấn luyện SVM — Phân loại tỏi";
        Icon                = SystemIcons.Application;
        Font                = new Font("Segoe UI", 9.5f);

        Controls.AddRange(new Control[] { grpParams, grpLog, progressBar, btnTrain });

        Load         += frmTrainSvm_Load;
        FormClosing  += frmTrainSvm_FormClosing;

        grpParams.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numC).EndInit();
        ((System.ComponentModel.ISupportInitialize)numGamma).EndInit();
        ((System.ComponentModel.ISupportInitialize)numImageSize).EndInit();
        ResumeLayout(false);
    }
}

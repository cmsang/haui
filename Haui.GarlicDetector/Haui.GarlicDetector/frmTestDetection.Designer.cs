namespace Haui.GarlicDetector;

partial class frmTestDetection
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlBottom;
    private Button btnLoadImage;
    private Button btnDetect;
    private Label lblInfo;

    private TableLayoutPanel tblImages;
    private Label lblOriginalHeader;
    private Label lblResultHeader;
    private PictureBox picOriginal;
    private PictureBox picResult;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        pnlBottom    = new Panel();
        btnLoadImage = new Button();
        btnDetect    = new Button();
        lblInfo      = new Label();

        tblImages        = new TableLayoutPanel();
        lblOriginalHeader = new Label();
        lblResultHeader   = new Label();
        picOriginal      = new PictureBox();
        picResult        = new PictureBox();

        pnlBottom.SuspendLayout();
        tblImages.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picOriginal).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picResult).BeginInit();
        SuspendLayout();

        // ── pnlBottom ─────────────────────────────────────────────────────────
        pnlBottom.Name      = "pnlBottom";
        pnlBottom.Dock      = DockStyle.Bottom;
        pnlBottom.Height    = 56;
        pnlBottom.BackColor = Color.FromArgb(45, 45, 48);
        pnlBottom.Padding   = new Padding(12, 0, 12, 0);
        pnlBottom.Controls.AddRange(new Control[] { btnLoadImage, btnDetect, lblInfo });

        // btnLoadImage
        btnLoadImage.Name                      = "btnLoadImage";
        btnLoadImage.Text                      = "📂 Chọn ảnh";
        btnLoadImage.Width                     = 120;
        btnLoadImage.Height                    = 34;
        btnLoadImage.Location                  = new Point(12, 11);
        btnLoadImage.BackColor                 = Color.FromArgb(0, 122, 204);
        btnLoadImage.ForeColor                 = Color.White;
        btnLoadImage.FlatStyle                 = FlatStyle.Flat;
        btnLoadImage.FlatAppearance.BorderSize = 0;
        btnLoadImage.Click                    += btnLoadImage_Click;

        // btnDetect
        btnDetect.Name                      = "btnDetect";
        btnDetect.Text                      = "🔍 Nhận diện";
        btnDetect.Width                     = 120;
        btnDetect.Height                    = 34;
        btnDetect.Location                  = new Point(144, 11);
        btnDetect.BackColor                 = Color.FromArgb(40, 140, 40);
        btnDetect.ForeColor                 = Color.White;
        btnDetect.FlatStyle                 = FlatStyle.Flat;
        btnDetect.FlatAppearance.BorderSize = 0;
        btnDetect.Click                    += btnDetect_Click;

        // lblInfo
        lblInfo.Name      = "lblInfo";
        lblInfo.Text      = "Chọn ảnh để bắt đầu.";
        lblInfo.AutoSize  = false;
        lblInfo.Width     = 500;
        lblInfo.Height    = 20;
        lblInfo.ForeColor = Color.LightGray;
        lblInfo.Location  = new Point(280, 18);

        // ── tblImages ─────────────────────────────────────────────────────────
        tblImages.Name        = "tblImages";
        tblImages.Dock        = DockStyle.Fill;
        tblImages.ColumnCount = 2;
        tblImages.RowCount    = 2;
        tblImages.BackColor   = Color.FromArgb(30, 30, 30);
        tblImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblImages.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        tblImages.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        tblImages.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tblImages.Controls.Add(lblOriginalHeader, 0, 0);
        tblImages.Controls.Add(lblResultHeader,   1, 0);
        tblImages.Controls.Add(picOriginal,       0, 1);
        tblImages.Controls.Add(picResult,         1, 1);

        // lblOriginalHeader
        lblOriginalHeader.Name      = "lblOriginalHeader";
        lblOriginalHeader.Text      = "Ảnh gốc";
        lblOriginalHeader.Dock      = DockStyle.Fill;
        lblOriginalHeader.TextAlign = ContentAlignment.MiddleCenter;
        lblOriginalHeader.ForeColor = Color.White;
        lblOriginalHeader.BackColor = Color.FromArgb(50, 50, 50);
        lblOriginalHeader.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);

        // lblResultHeader
        lblResultHeader.Name      = "lblResultHeader";
        lblResultHeader.Text      = "Kết quả nhận diện";
        lblResultHeader.Dock      = DockStyle.Fill;
        lblResultHeader.TextAlign = ContentAlignment.MiddleCenter;
        lblResultHeader.ForeColor = Color.White;
        lblResultHeader.BackColor = Color.FromArgb(50, 50, 50);
        lblResultHeader.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);

        // picOriginal
        picOriginal.Name      = "picOriginal";
        picOriginal.Dock      = DockStyle.Fill;
        picOriginal.SizeMode  = PictureBoxSizeMode.Zoom;
        picOriginal.BackColor = Color.Black;

        // picResult
        picResult.Name      = "picResult";
        picResult.Dock      = DockStyle.Fill;
        picResult.SizeMode  = PictureBoxSizeMode.Zoom;
        picResult.BackColor = Color.Black;

        // ── Form ──────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1100, 620);
        MinimumSize         = new Size(800, 480);
        Name                = "frmTestDetection";
        Text                = "Test Nhận Diện Tỏi";
        StartPosition       = FormStartPosition.CenterParent;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);

        Controls.Add(tblImages);
        Controls.Add(pnlBottom);

        pnlBottom.ResumeLayout(false);
        tblImages.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picOriginal).EndInit();
        ((System.ComponentModel.ISupportInitialize)picResult).EndInit();
        ResumeLayout(false);
    }
}

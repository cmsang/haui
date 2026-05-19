namespace Haui.GarlicDetector;

partial class frmLabeling
{
    private System.ComponentModel.IContainer components = null;

    // ─── Thanh trên ──────────────────────────────────────────────────────────
    private Panel    pnlTop;
    private Button   btnOpenImage;
    private Button   btnClearAll;
    private Label    lblCurrentLabel;
    private ComboBox cmbLabel;

    // ─── Panel phải ──────────────────────────────────────────────────────────
    private Panel   pnlRight;
    private Label   lblRegions;
    private ListBox lstRegions;
    private Button  btnDeleteRegion;
    private Label   lblFolder;
    private TextBox txtFolder;
    private Button  btnBrowseFolder;
    private Button  btnSave;

    // ─── Khung ảnh ───────────────────────────────────────────────────────────
    private PictureBox picImage;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        pnlTop          = new Panel();
        btnOpenImage    = new Button();
        btnClearAll     = new Button();
        lblCurrentLabel = new Label();
        cmbLabel        = new ComboBox();

        pnlRight        = new Panel();
        lblRegions      = new Label();
        lstRegions      = new ListBox();
        btnDeleteRegion = new Button();
        lblFolder       = new Label();
        txtFolder       = new TextBox();
        btnBrowseFolder = new Button();
        btnSave         = new Button();

        picImage = new PictureBox();

        pnlTop.SuspendLayout();
        pnlRight.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
        SuspendLayout();

        // ── pnlTop ────────────────────────────────────────────────────────────
        pnlTop.Name      = "pnlTop";
        pnlTop.Dock      = DockStyle.Top;
        pnlTop.Height    = 52;
        pnlTop.BackColor = Color.FromArgb(45, 45, 48);
        pnlTop.Padding   = new Padding(8, 0, 8, 0);
        pnlTop.Controls.AddRange(new Control[]
        {
            btnOpenImage, btnClearAll, lblCurrentLabel, cmbLabel,
        });

        // btnOpenImage
        btnOpenImage.Name                      = "btnOpenImage";
        btnOpenImage.Text                      = "📷 Chụp / Mở ảnh";
        btnOpenImage.Width                     = 150;
        btnOpenImage.Height                    = 30;
        btnOpenImage.Location                  = new Point(8, 11);
        btnOpenImage.BackColor                 = Color.FromArgb(0, 122, 204);
        btnOpenImage.ForeColor                 = Color.White;
        btnOpenImage.FlatStyle                 = FlatStyle.Flat;
        btnOpenImage.FlatAppearance.BorderSize = 0;
        btnOpenImage.Click                    += btnOpenImage_Click;

        // btnClearAll
        btnClearAll.Name                      = "btnClearAll";
        btnClearAll.Text                      = "✕ Xóa tất cả";
        btnClearAll.Width                     = 110;
        btnClearAll.Height                    = 30;
        btnClearAll.Location                  = new Point(168, 11);
        btnClearAll.BackColor                 = Color.FromArgb(180, 30, 30);
        btnClearAll.ForeColor                 = Color.White;
        btnClearAll.FlatStyle                 = FlatStyle.Flat;
        btnClearAll.FlatAppearance.BorderSize = 0;
        btnClearAll.Click                    += btnClearAll_Click;

        // lblCurrentLabel
        lblCurrentLabel.Name      = "lblCurrentLabel";
        lblCurrentLabel.Text      = "Nhãn:";
        lblCurrentLabel.AutoSize  = true;
        lblCurrentLabel.ForeColor = Color.White;
        lblCurrentLabel.Location  = new Point(296, 17);

        // cmbLabel
        cmbLabel.Name          = "cmbLabel";
        cmbLabel.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLabel.Width         = 150;
        cmbLabel.Location      = new Point(340, 13);
        cmbLabel.BackColor     = Color.FromArgb(60, 60, 60);
        cmbLabel.ForeColor     = Color.White;
        cmbLabel.Items.AddRange(new object[] { "Tỏi to (0)", "Tỏi nhỏ (1)", "Tỏi hỏng (2)" });
        cmbLabel.SelectedIndex = 0;

        // ── pnlRight ──────────────────────────────────────────────────────────
        pnlRight.Name      = "pnlRight";
        pnlRight.Dock      = DockStyle.Right;
        pnlRight.Width     = 230;
        pnlRight.BackColor = Color.FromArgb(45, 45, 48);
        pnlRight.Padding   = new Padding(8);
        pnlRight.Controls.AddRange(new Control[]
        {
            lblRegions, lstRegions, btnDeleteRegion,
            lblFolder, txtFolder, btnBrowseFolder, btnSave,
        });

        // lblRegions
        lblRegions.Name      = "lblRegions";
        lblRegions.Text      = "Danh sách vùng đã chọn:";
        lblRegions.AutoSize  = true;
        lblRegions.ForeColor = Color.White;
        lblRegions.Location  = new Point(8, 8);

        // lstRegions
        lstRegions.Name                  = "lstRegions";
        lstRegions.Location              = new Point(8, 30);
        lstRegions.Width                 = 210;
        lstRegions.Height                = 320;
        lstRegions.BackColor             = Color.FromArgb(30, 30, 30);
        lstRegions.ForeColor             = Color.White;
        lstRegions.BorderStyle           = BorderStyle.FixedSingle;
        lstRegions.SelectedIndexChanged += lstRegions_SelectedIndexChanged;

        // btnDeleteRegion
        btnDeleteRegion.Name                      = "btnDeleteRegion";
        btnDeleteRegion.Text                      = "✕ Xóa vùng đã chọn";
        btnDeleteRegion.Width                     = 210;
        btnDeleteRegion.Height                    = 28;
        btnDeleteRegion.Location                  = new Point(8, 358);
        btnDeleteRegion.BackColor                 = Color.FromArgb(100, 30, 30);
        btnDeleteRegion.ForeColor                 = Color.White;
        btnDeleteRegion.FlatStyle                 = FlatStyle.Flat;
        btnDeleteRegion.FlatAppearance.BorderSize = 0;
        btnDeleteRegion.Click                    += btnDeleteRegion_Click;

        // lblFolder
        lblFolder.Name      = "lblFolder";
        lblFolder.Text      = "Thư mục lưu:";
        lblFolder.AutoSize  = true;
        lblFolder.ForeColor = Color.White;
        lblFolder.Location  = new Point(8, 402);

        // txtFolder
        txtFolder.Name        = "txtFolder";
        txtFolder.Location    = new Point(8, 422);
        txtFolder.Width       = 210;
        txtFolder.BackColor   = Color.FromArgb(60, 60, 60);
        txtFolder.ForeColor   = Color.White;
        txtFolder.BorderStyle = BorderStyle.FixedSingle;
        txtFolder.ReadOnly    = true;

        // btnBrowseFolder
        btnBrowseFolder.Name                      = "btnBrowseFolder";
        btnBrowseFolder.Text                      = "📁 Chọn thư mục";
        btnBrowseFolder.Width                     = 210;
        btnBrowseFolder.Height                    = 28;
        btnBrowseFolder.Location                  = new Point(8, 452);
        btnBrowseFolder.BackColor                 = Color.FromArgb(60, 60, 60);
        btnBrowseFolder.ForeColor                 = Color.White;
        btnBrowseFolder.FlatStyle                 = FlatStyle.Flat;
        btnBrowseFolder.FlatAppearance.BorderSize = 0;
        btnBrowseFolder.Click                    += btnBrowseFolder_Click;

        // btnSave
        btnSave.Name                      = "btnSave";
        btnSave.Text                      = "💾 Lưu tất cả vùng";
        btnSave.Width                     = 210;
        btnSave.Height                    = 36;
        btnSave.Location                  = new Point(8, 494);
        btnSave.BackColor                 = Color.FromArgb(0, 130, 60);
        btnSave.ForeColor                 = Color.White;
        btnSave.FlatStyle                 = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click                    += btnSave_Click;

        // ── picImage ──────────────────────────────────────────────────────────
        picImage.Name        = "picImage";
        picImage.Dock        = DockStyle.Fill;
        picImage.SizeMode    = PictureBoxSizeMode.Zoom;
        picImage.BackColor   = Color.FromArgb(20, 20, 20);
        picImage.Cursor      = Cursors.Cross;
        picImage.MouseDown  += picImage_MouseDown;
        picImage.MouseMove  += picImage_MouseMove;
        picImage.MouseUp    += picImage_MouseUp;
        picImage.Paint      += picImage_Paint;

        // ── Form ──────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1100, 700);
        MinimumSize         = new Size(800, 560);
        Name                = "frmLabeling";
        Text                = "Gán nhãn tỏi";
        StartPosition       = FormStartPosition.CenterScreen;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);

        // Thứ tự: Fill trước, Right và Top sau
        Controls.Add(picImage);
        Controls.Add(pnlRight);
        Controls.Add(pnlTop);

        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlRight.ResumeLayout(false);
        pnlRight.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}

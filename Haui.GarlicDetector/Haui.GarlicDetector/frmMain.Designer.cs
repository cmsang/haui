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
    private Button btnSelectRegion;
    private Button btnLabeling;
    private Button btnSettings;
    private Button btnTrainSvm;
    private Button btnTest;
    private Label lblStatus;

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
        btnSelectRegion = new Button();
        btnLabeling     = new Button();
        btnSettings     = new Button();
        btnTrainSvm     = new Button();
        btnTest         = new Button();
        lblStatus     = new Label();

        picCamera = new PictureBox();

        // Suspend layout toàn bộ trước khi cấu hình
        pnlTop.SuspendLayout();
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
            btnStart, btnStop, btnSelectRegion, btnLabeling, btnSettings, btnTrainSvm, btnTest, lblStatus,
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

        // btnSelectRegion
        btnSelectRegion.Name                      = "btnSelectRegion";
        btnSelectRegion.Text                      = "⬚ Chọn vùng";
        btnSelectRegion.Width                     = 115;
        btnSelectRegion.Height                    = 30;
        btnSelectRegion.Location                  = new Point(771, 11);
        btnSelectRegion.BackColor                 = Color.FromArgb(40, 110, 40);
        btnSelectRegion.ForeColor                 = Color.White;
        btnSelectRegion.FlatStyle                 = FlatStyle.Flat;
        btnSelectRegion.FlatAppearance.BorderSize = 0;
        btnSelectRegion.Click                    += btnSelectRegion_Click;

        // btnLabeling
        btnLabeling.Name                      = "btnLabeling";
        btnLabeling.Text                      = "🏷 Gán nhãn";
        btnLabeling.Width                     = 110;
        btnLabeling.Height                    = 30;
        btnLabeling.Location                  = new Point(898, 11);
        btnLabeling.BackColor                 = Color.FromArgb(120, 80, 0);
        btnLabeling.ForeColor                 = Color.White;
        btnLabeling.FlatStyle                 = FlatStyle.Flat;
        btnLabeling.FlatAppearance.BorderSize = 0;
        btnLabeling.Click                    += btnLabeling_Click;

        // btnSettings
        btnSettings.Name                      = "btnSettings";
        btnSettings.Text                      = "⚙ Cài đặt";
        btnSettings.Width                     = 100;
        btnSettings.Height                    = 30;
        btnSettings.Location                  = new Point(1020, 11);
        btnSettings.BackColor                 = Color.FromArgb(60, 60, 60);
        btnSettings.ForeColor                 = Color.White;
        btnSettings.FlatStyle                 = FlatStyle.Flat;
        btnSettings.FlatAppearance.BorderSize = 0;
        btnSettings.Click                    += btnSettings_Click;

        // btnTrainSvm
        btnTrainSvm.Name                      = "btnTrainSvm";
        btnTrainSvm.Text                      = "🤖 Train SVM";
        btnTrainSvm.Width                     = 115;
        btnTrainSvm.Height                    = 30;
        btnTrainSvm.Location                  = new Point(1132, 11);
        btnTrainSvm.BackColor                 = Color.FromArgb(80, 40, 120);
        btnTrainSvm.ForeColor                 = Color.White;
        btnTrainSvm.FlatStyle                 = FlatStyle.Flat;
        btnTrainSvm.FlatAppearance.BorderSize = 0;
        btnTrainSvm.Click                    += btnTrainSvm_Click;

        // btnTest
        btnTest.Name                      = "btnTest";
        btnTest.Text                      = "🧪 Test";
        btnTest.Width                     = 90;
        btnTest.Height                    = 30;
        btnTest.Location                  = new Point(1259, 11);
        btnTest.BackColor                 = Color.FromArgb(0, 140, 140);
        btnTest.ForeColor                 = Color.White;
        btnTest.FlatStyle                 = FlatStyle.Flat;
        btnTest.FlatAppearance.BorderSize = 0;
        btnTest.Click                    += btnTest_Click;

        // lblStatus
        lblStatus.Name      = "lblStatus";
        lblStatus.Text      = "Sẵn sàng.";
        lblStatus.AutoSize  = false;
        lblStatus.Width     = 250;
        lblStatus.Height    = 20;
        lblStatus.ForeColor = Color.LightGray;
        lblStatus.Location  = new Point(1362, 17);

        // ── picCamera ─────────────────────────────────────────────────────────
        picCamera.Name      = "picCamera";
        picCamera.Dock      = DockStyle.Fill;
        picCamera.SizeMode  = PictureBoxSizeMode.Zoom;
        picCamera.BackColor = Color.Black;

        // ── Form ──────────────────────────────────────────────────────────────
        // AutoScaleDimensions & AutoScaleMode bắt buộc để Designer render được
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1280, 662);
        MinimumSize         = new Size(1000, 540);
        Name                = "frmMain";
        Text                = "Haui Garlic Detector";
        StartPosition       = FormStartPosition.CenterScreen;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);
        Load               += frmMain_Load;
        FormClosing        += frmMain_FormClosing;

        // Thứ tự Controls.Add quan trọng với Dock:
        // Fill phải được thêm trước (index cao nhất), các dock Top thêm sau sẽ ưu tiên
        Controls.Add(picCamera);
        Controls.Add(pnlTop);

        // Resume layout theo thứ tự ngược lại
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picCamera).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}

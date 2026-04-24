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

    // ─── Panel kết quả nhận diện (bên phải) ──────────────────────────────────
    private Panel pnlRight;
    private Label lblResultTitle;
    private DataGridView dgvResults;
    private Button btnClearResults;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        dataGridViewCellStyle1.BackColor = SystemColors.Control;
        dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
        dataGridViewCellStyle1.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
        pnlTop = new Panel();
        lblCamera = new Label();
        cmbCameras = new ComboBox();
        lblResolution = new Label();
        cmbResolution = new ComboBox();
        btnStart = new Button();
        btnStop = new Button();
        btnSelectRegion = new Button();
        btnLabeling = new Button();
        btnSettings = new Button();
        btnTrainSvm = new Button();
        btnTest = new Button();
        lblStatus = new Label();
        picCamera = new PictureBox();
        pnlRight = new Panel();
        panel2 = new Panel();
        dgvResults = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        panel1 = new Panel();
        lblResultTitle = new Label();
        btnClearResults = new Button();
        pnlTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picCamera).BeginInit();
        pnlRight.SuspendLayout();
        panel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
        panel1.SuspendLayout();
        SuspendLayout();
        // 
        // pnlTop
        // 
        pnlTop.BackColor = Color.FromArgb(45, 45, 48);
        pnlTop.Controls.Add(lblCamera);
        pnlTop.Controls.Add(cmbCameras);
        pnlTop.Controls.Add(lblResolution);
        pnlTop.Controls.Add(cmbResolution);
        pnlTop.Controls.Add(btnStart);
        pnlTop.Controls.Add(btnStop);
        pnlTop.Controls.Add(btnSelectRegion);
        pnlTop.Controls.Add(btnLabeling);
        pnlTop.Controls.Add(btnSettings);
        pnlTop.Controls.Add(btnTrainSvm);
        pnlTop.Controls.Add(btnTest);
        pnlTop.Controls.Add(lblStatus);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Margin = new Padding(3, 4, 3, 4);
        pnlTop.Name = "pnlTop";
        pnlTop.Padding = new Padding(9, 0, 9, 0);
        pnlTop.Size = new Size(1806, 69);
        pnlTop.TabIndex = 2;
        // 
        // lblCamera
        // 
        lblCamera.AutoSize = true;
        lblCamera.ForeColor = Color.White;
        lblCamera.Location = new Point(9, 23);
        lblCamera.Name = "lblCamera";
        lblCamera.Size = new Size(110, 20);
        lblCamera.TabIndex = 0;
        lblCamera.Text = "Nguồn camera:";
        // 
        // cmbCameras
        // 
        cmbCameras.BackColor = Color.FromArgb(60, 60, 60);
        cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCameras.ForeColor = Color.White;
        cmbCameras.Location = new Point(126, 17);
        cmbCameras.Margin = new Padding(3, 4, 3, 4);
        cmbCameras.Name = "cmbCameras";
        cmbCameras.Size = new Size(171, 28);
        cmbCameras.TabIndex = 1;
        cmbCameras.SelectedIndexChanged += cmbCameras_SelectedIndexChanged;
        // 
        // lblResolution
        // 
        lblResolution.AutoSize = true;
        lblResolution.ForeColor = Color.White;
        lblResolution.Location = new Point(311, 23);
        lblResolution.Name = "lblResolution";
        lblResolution.Size = new Size(98, 20);
        lblResolution.TabIndex = 2;
        lblResolution.Text = "Độ phân giải:";
        // 
        // cmbResolution
        // 
        cmbResolution.BackColor = Color.FromArgb(60, 60, 60);
        cmbResolution.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbResolution.ForeColor = Color.White;
        cmbResolution.Location = new Point(417, 17);
        cmbResolution.Margin = new Padding(3, 4, 3, 4);
        cmbResolution.Name = "cmbResolution";
        cmbResolution.Size = new Size(199, 28);
        cmbResolution.TabIndex = 3;
        cmbResolution.SelectedIndexChanged += cmbResolution_SelectedIndexChanged;
        // 
        // btnStart
        // 
        btnStart.BackColor = Color.FromArgb(0, 122, 204);
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.ForeColor = Color.White;
        btnStart.Location = new Point(635, 15);
        btnStart.Margin = new Padding(3, 4, 3, 4);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(120, 40);
        btnStart.TabIndex = 4;
        btnStart.Text = "▶ Bắt đầu";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.BackColor = Color.FromArgb(180, 30, 30);
        btnStop.Enabled = false;
        btnStop.FlatAppearance.BorderSize = 0;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.ForeColor = Color.White;
        btnStop.Location = new Point(765, 15);
        btnStop.Margin = new Padding(3, 4, 3, 4);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(103, 40);
        btnStop.TabIndex = 5;
        btnStop.Text = "■ Dừng";
        btnStop.UseVisualStyleBackColor = false;
        btnStop.Click += btnStop_Click;
        // 
        // btnSelectRegion
        // 
        btnSelectRegion.BackColor = Color.FromArgb(40, 110, 40);
        btnSelectRegion.FlatAppearance.BorderSize = 0;
        btnSelectRegion.FlatStyle = FlatStyle.Flat;
        btnSelectRegion.ForeColor = Color.White;
        btnSelectRegion.Location = new Point(881, 15);
        btnSelectRegion.Margin = new Padding(3, 4, 3, 4);
        btnSelectRegion.Name = "btnSelectRegion";
        btnSelectRegion.Size = new Size(131, 40);
        btnSelectRegion.TabIndex = 6;
        btnSelectRegion.Text = "⬚ Chọn vùng";
        btnSelectRegion.UseVisualStyleBackColor = false;
        btnSelectRegion.Click += btnSelectRegion_Click;
        // 
        // btnLabeling
        // 
        btnLabeling.BackColor = Color.FromArgb(120, 80, 0);
        btnLabeling.FlatAppearance.BorderSize = 0;
        btnLabeling.FlatStyle = FlatStyle.Flat;
        btnLabeling.ForeColor = Color.White;
        btnLabeling.Location = new Point(1026, 15);
        btnLabeling.Margin = new Padding(3, 4, 3, 4);
        btnLabeling.Name = "btnLabeling";
        btnLabeling.Size = new Size(126, 40);
        btnLabeling.TabIndex = 7;
        btnLabeling.Text = "🏷 Gán nhãn";
        btnLabeling.UseVisualStyleBackColor = false;
        btnLabeling.Click += btnLabeling_Click;
        // 
        // btnSettings
        // 
        btnSettings.BackColor = Color.FromArgb(60, 60, 60);
        btnSettings.FlatAppearance.BorderSize = 0;
        btnSettings.FlatStyle = FlatStyle.Flat;
        btnSettings.ForeColor = Color.White;
        btnSettings.Location = new Point(1166, 15);
        btnSettings.Margin = new Padding(3, 4, 3, 4);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(114, 40);
        btnSettings.TabIndex = 8;
        btnSettings.Text = "⚙ Cài đặt";
        btnSettings.UseVisualStyleBackColor = false;
        btnSettings.Click += btnSettings_Click;
        // 
        // btnTrainSvm
        // 
        btnTrainSvm.BackColor = Color.FromArgb(80, 40, 120);
        btnTrainSvm.FlatAppearance.BorderSize = 0;
        btnTrainSvm.FlatStyle = FlatStyle.Flat;
        btnTrainSvm.ForeColor = Color.White;
        btnTrainSvm.Location = new Point(1294, 15);
        btnTrainSvm.Margin = new Padding(3, 4, 3, 4);
        btnTrainSvm.Name = "btnTrainSvm";
        btnTrainSvm.Size = new Size(131, 40);
        btnTrainSvm.TabIndex = 9;
        btnTrainSvm.Text = "🤖 Train SVM";
        btnTrainSvm.UseVisualStyleBackColor = false;
        btnTrainSvm.Click += btnTrainSvm_Click;
        // 
        // btnTest
        // 
        btnTest.BackColor = Color.FromArgb(0, 140, 140);
        btnTest.FlatAppearance.BorderSize = 0;
        btnTest.FlatStyle = FlatStyle.Flat;
        btnTest.ForeColor = Color.White;
        btnTest.Location = new Point(1439, 15);
        btnTest.Margin = new Padding(3, 4, 3, 4);
        btnTest.Name = "btnTest";
        btnTest.Size = new Size(103, 40);
        btnTest.TabIndex = 10;
        btnTest.Text = "\U0001f9ea Test";
        btnTest.UseVisualStyleBackColor = false;
        btnTest.Click += btnTest_Click;
        // 
        // lblStatus
        // 
        lblStatus.ForeColor = Color.LightGray;
        lblStatus.Location = new Point(1557, 23);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(286, 27);
        lblStatus.TabIndex = 11;
        lblStatus.Text = "Sẵn sàng.";
        // 
        // picCamera
        // 
        picCamera.BackColor = Color.Black;
        picCamera.Dock = DockStyle.Fill;
        picCamera.Location = new Point(0, 69);
        picCamera.Margin = new Padding(3, 4, 3, 4);
        picCamera.Name = "picCamera";
        picCamera.Size = new Size(1463, 891);
        picCamera.SizeMode = PictureBoxSizeMode.Zoom;
        picCamera.TabIndex = 0;
        picCamera.TabStop = false;
        // 
        // pnlRight
        // 
        pnlRight.BackColor = SystemColors.Control;
        pnlRight.Controls.Add(panel2);
        pnlRight.Controls.Add(panel1);
        pnlRight.Controls.Add(btnClearResults);
        pnlRight.Dock = DockStyle.Right;
        pnlRight.Location = new Point(1463, 69);
        pnlRight.Margin = new Padding(3, 4, 3, 4);
        pnlRight.Name = "pnlRight";
        pnlRight.Padding = new Padding(7, 8, 7, 8);
        pnlRight.Size = new Size(343, 891);
        pnlRight.TabIndex = 1;
        // 
        // panel2
        // 
        panel2.BackColor = SystemColors.Window;
        panel2.Controls.Add(dgvResults);
        panel2.Dock = DockStyle.Fill;
        panel2.Location = new Point(7, 52);
        panel2.Name = "panel2";
        panel2.Size = new Size(329, 791);
        panel2.TabIndex = 3;
        // 
        // dgvResults
        // 
        dgvResults.AllowUserToAddRows = false;
        dgvResults.AllowUserToDeleteRows = false;
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvResults.BackgroundColor = SystemColors.Window;
        dgvResults.ForeColor = SystemColors.WindowText;
        dgvResults.GridColor = SystemColors.ControlLight;
        dgvResults.BorderStyle = BorderStyle.None;
        dgvResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dgvResults.ColumnHeadersHeight = 28;
        dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvResults.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = SystemColors.Window,
            ForeColor          = SystemColors.WindowText,
            SelectionBackColor = SystemColors.Highlight,
            SelectionForeColor = SystemColors.HighlightText,
        };
        dgvResults.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = SystemColors.ButtonFace,
            ForeColor          = SystemColors.WindowText,
            SelectionBackColor = SystemColors.Highlight,
            SelectionForeColor = SystemColors.HighlightText,
        };
        dgvResults.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
        dgvResults.Dock = DockStyle.Fill;
        dgvResults.Location = new Point(0, 0);
        dgvResults.Margin = new Padding(3, 4, 3, 4);
        dgvResults.Name = "dgvResults";
        dgvResults.ReadOnly = true;
        dgvResults.RowHeadersVisible = false;
        dgvResults.RowHeadersWidth = 51;
        dgvResults.RowTemplate.Height = 24;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.Size = new Size(329, 791);
        dgvResults.TabIndex = 2;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.HeaderText = "Thời gian";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "Nhãn";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewTextBoxColumn3.HeaderText = "Kích thước";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        dataGridViewTextBoxColumn3.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.HeaderText = "Vị trí(x, y)";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.ReadOnly = true;
        // 
        // panel1
        // 
        panel1.BackColor = SystemColors.Control;
        panel1.Controls.Add(lblResultTitle);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(7, 8);
        panel1.Name = "panel1";
        panel1.Size = new Size(329, 44);
        panel1.TabIndex = 2;
        // 
        // lblResultTitle
        // 
        lblResultTitle.Dock = DockStyle.Fill;
        lblResultTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblResultTitle.ForeColor = SystemColors.ControlText;
        lblResultTitle.Location = new Point(0, 0);
        lblResultTitle.Name = "lblResultTitle";
        lblResultTitle.Size = new Size(329, 44);
        lblResultTitle.TabIndex = 0;
        lblResultTitle.Text = "Kết quả nhận diện";
        lblResultTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnClearResults
        // 
        btnClearResults.BackColor = SystemColors.ControlDark;
        btnClearResults.Dock = DockStyle.Bottom;
        btnClearResults.FlatAppearance.BorderSize = 0;
        btnClearResults.FlatStyle = FlatStyle.Flat;
        btnClearResults.ForeColor = SystemColors.ControlLightLight;
        btnClearResults.Location = new Point(7, 843);
        btnClearResults.Margin = new Padding(3, 4, 3, 4);
        btnClearResults.Name = "btnClearResults";
        btnClearResults.Size = new Size(329, 40);
        btnClearResults.TabIndex = 1;
        btnClearResults.Text = "🗑 Xóa danh sách";
        btnClearResults.UseVisualStyleBackColor = false;
        btnClearResults.Click += btnClearResults_Click;
        // 
        // frmMain
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(1806, 960);
        Controls.Add(picCamera);
        Controls.Add(pnlRight);
        Controls.Add(pnlTop);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(1255, 784);
        Name = "frmMain";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Haui Garlic Detector";
        FormClosing += frmMain_FormClosing;
        Load += frmMain_Load;
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picCamera).EndInit();
        pnlRight.ResumeLayout(false);
        panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
        panel1.ResumeLayout(false);
        ResumeLayout(false);
    }
    private Panel panel2;
    private Panel panel1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
}

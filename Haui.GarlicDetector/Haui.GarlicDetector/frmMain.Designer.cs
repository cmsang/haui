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
    private Button btnDetect;
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
        btnDetect = new Button();
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
        label2 = new Label();
        label1 = new Label();
        lblResultTitle = new Label();
        btnClearResults = new Button();
        label3 = new Label();
        label4 = new Label();
        lblLargeCount = new Label();
        lblSmallCount = new Label();
        lblErrorCount = new Label();
        lblTotalCount = new Label();
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
        pnlTop.Controls.Add(btnDetect);
        pnlTop.Controls.Add(lblStatus);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Name = "pnlTop";
        pnlTop.Padding = new Padding(8, 0, 8, 0);
        pnlTop.Size = new Size(1580, 52);
        pnlTop.TabIndex = 2;
        // 
        // lblCamera
        // 
        lblCamera.AutoSize = true;
        lblCamera.ForeColor = Color.White;
        lblCamera.Location = new Point(8, 17);
        lblCamera.Name = "lblCamera";
        lblCamera.Size = new Size(89, 15);
        lblCamera.TabIndex = 0;
        lblCamera.Text = "Nguồn camera:";
        // 
        // cmbCameras
        // 
        cmbCameras.BackColor = Color.FromArgb(60, 60, 60);
        cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCameras.ForeColor = Color.White;
        cmbCameras.Location = new Point(110, 13);
        cmbCameras.Name = "cmbCameras";
        cmbCameras.Size = new Size(150, 23);
        cmbCameras.TabIndex = 1;
        cmbCameras.SelectedIndexChanged += cmbCameras_SelectedIndexChanged;
        // 
        // lblResolution
        // 
        lblResolution.AutoSize = true;
        lblResolution.ForeColor = Color.White;
        lblResolution.Location = new Point(272, 17);
        lblResolution.Name = "lblResolution";
        lblResolution.Size = new Size(77, 15);
        lblResolution.TabIndex = 2;
        lblResolution.Text = "Độ phân giải:";
        // 
        // cmbResolution
        // 
        cmbResolution.BackColor = Color.FromArgb(60, 60, 60);
        cmbResolution.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbResolution.ForeColor = Color.White;
        cmbResolution.Location = new Point(365, 13);
        cmbResolution.Name = "cmbResolution";
        cmbResolution.Size = new Size(175, 23);
        cmbResolution.TabIndex = 3;
        cmbResolution.SelectedIndexChanged += cmbResolution_SelectedIndexChanged;
        // 
        // btnStart
        // 
        btnStart.BackColor = Color.FromArgb(0, 122, 204);
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.ForeColor = Color.White;
        btnStart.Location = new Point(556, 11);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(105, 30);
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
        btnStop.Location = new Point(669, 11);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(90, 30);
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
        btnSelectRegion.Location = new Point(771, 11);
        btnSelectRegion.Name = "btnSelectRegion";
        btnSelectRegion.Size = new Size(115, 30);
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
        btnLabeling.Location = new Point(898, 11);
        btnLabeling.Name = "btnLabeling";
        btnLabeling.Size = new Size(110, 30);
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
        btnSettings.Location = new Point(1020, 11);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(100, 30);
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
        btnTrainSvm.Location = new Point(1132, 11);
        btnTrainSvm.Name = "btnTrainSvm";
        btnTrainSvm.Size = new Size(115, 30);
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
        btnTest.Location = new Point(1259, 11);
        btnTest.Name = "btnTest";
        btnTest.Size = new Size(90, 30);
        btnTest.TabIndex = 10;
        btnTest.Text = "\U0001f9ea Test";
        btnTest.UseVisualStyleBackColor = false;
        btnTest.Click += btnTest_Click;
        // 
        // btnDetect
        // 
        btnDetect.BackColor = Color.FromArgb(180, 100, 0);
        btnDetect.Enabled = false;
        btnDetect.FlatAppearance.BorderSize = 0;
        btnDetect.FlatStyle = FlatStyle.Flat;
        btnDetect.ForeColor = Color.White;
        btnDetect.Location = new Point(1362, 11);
        btnDetect.Margin = new Padding(3, 2, 3, 2);
        btnDetect.Name = "btnDetect";
        btnDetect.Size = new Size(114, 30);
        btnDetect.TabIndex = 11;
        btnDetect.Text = "📷 Nhận diện";
        btnDetect.UseVisualStyleBackColor = false;
        btnDetect.Visible = false;
        btnDetect.Click += btnDetect_Click;
        // 
        // lblStatus
        // 
        lblStatus.ForeColor = Color.LightGray;
        lblStatus.Location = new Point(1488, 17);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(250, 20);
        lblStatus.TabIndex = 11;
        lblStatus.Text = "Sẵn sàng.";
        // 
        // picCamera
        // 
        picCamera.BackColor = Color.Black;
        picCamera.Dock = DockStyle.Fill;
        picCamera.Location = new Point(0, 52);
        picCamera.Name = "picCamera";
        picCamera.Size = new Size(1224, 668);
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
        pnlRight.Location = new Point(1224, 52);
        pnlRight.Name = "pnlRight";
        pnlRight.Padding = new Padding(6);
        pnlRight.Size = new Size(356, 668);
        pnlRight.TabIndex = 1;
        // 
        // panel2
        // 
        panel2.BackColor = SystemColors.Window;
        panel2.Controls.Add(dgvResults);
        panel2.Dock = DockStyle.Fill;
        panel2.Location = new Point(6, 169);
        panel2.Margin = new Padding(3, 2, 3, 2);
        panel2.Name = "panel2";
        panel2.Size = new Size(344, 463);
        panel2.TabIndex = 3;
        // 
        // dgvResults
        // 
        dgvResults.AllowUserToAddRows = false;
        dgvResults.AllowUserToDeleteRows = false;
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvResults.BackgroundColor = SystemColors.Window;
        dgvResults.BorderStyle = BorderStyle.None;
        dataGridViewCellStyle1.BackColor = SystemColors.Control;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
        dgvResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dgvResults.ColumnHeadersHeight = 28;
        dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvResults.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
        dgvResults.Dock = DockStyle.Fill;
        dgvResults.GridColor = SystemColors.ControlLight;
        dgvResults.Location = new Point(0, 0);
        dgvResults.Name = "dgvResults";
        dgvResults.ReadOnly = true;
        dgvResults.RowHeadersVisible = false;
        dgvResults.RowHeadersWidth = 51;
        dgvResults.RowTemplate.Height = 24;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.Size = new Size(344, 463);
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
        panel1.Controls.Add(lblTotalCount);
        panel1.Controls.Add(lblErrorCount);
        panel1.Controls.Add(lblSmallCount);
        panel1.Controls.Add(lblLargeCount);
        panel1.Controls.Add(label3);
        panel1.Controls.Add(label4);
        panel1.Controls.Add(label2);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(lblResultTitle);
        panel1.Dock = DockStyle.Top;
        panel1.Location = new Point(6, 6);
        panel1.Margin = new Padding(3, 2, 3, 2);
        panel1.Name = "panel1";
        panel1.Size = new Size(344, 163);
        panel1.TabIndex = 2;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label2.ForeColor = Color.Black;
        label2.Location = new Point(160, 57);
        label2.Name = "label2";
        label2.Size = new Size(63, 19);
        label2.TabIndex = 2;
        label2.Text = "Tỏi nhỏ:";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label1.ForeColor = Color.Black;
        label1.Location = new Point(14, 57);
        label1.Name = "label1";
        label1.Size = new Size(52, 19);
        label1.TabIndex = 1;
        label1.Text = "Tỏi to:";
        // 
        // lblResultTitle
        // 
        lblResultTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblResultTitle.ForeColor = SystemColors.ControlText;
        lblResultTitle.Location = new Point(0, 0);
        lblResultTitle.Name = "lblResultTitle";
        lblResultTitle.Size = new Size(344, 33);
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
        btnClearResults.Location = new Point(6, 632);
        btnClearResults.Name = "btnClearResults";
        btnClearResults.Size = new Size(344, 30);
        btnClearResults.TabIndex = 1;
        btnClearResults.Text = "🗑 Xóa danh sách";
        btnClearResults.UseVisualStyleBackColor = false;
        btnClearResults.Click += btnClearResults_Click;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label3.ForeColor = Color.Black;
        label3.Location = new Point(160, 112);
        label3.Name = "label3";
        label3.Size = new Size(84, 19);
        label3.TabIndex = 4;
        label3.Text = "Tổng cộng:";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label4.ForeColor = Color.Black;
        label4.Location = new Point(14, 112);
        label4.Name = "label4";
        label4.Size = new Size(59, 19);
        label4.TabIndex = 3;
        label4.Text = "Tỏi lỗi: ";
        // 
        // lblLargeCount
        // 
        lblLargeCount.AutoSize = true;
        lblLargeCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblLargeCount.ForeColor = Color.Black;
        lblLargeCount.Location = new Point(65, 56);
        lblLargeCount.Name = "lblLargeCount";
        lblLargeCount.Size = new Size(19, 21);
        lblLargeCount.TabIndex = 5;
        lblLargeCount.Text = "0";
        // 
        // lblSmallCount
        // 
        lblSmallCount.AutoSize = true;
        lblSmallCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblSmallCount.ForeColor = Color.Black;
        lblSmallCount.Location = new Point(223, 56);
        lblSmallCount.Name = "lblSmallCount";
        lblSmallCount.Size = new Size(19, 21);
        lblSmallCount.TabIndex = 6;
        lblSmallCount.Text = "0";
        // 
        // lblErrorCount
        // 
        lblErrorCount.AutoSize = true;
        lblErrorCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblErrorCount.ForeColor = Color.Black;
        lblErrorCount.Location = new Point(69, 110);
        lblErrorCount.Name = "lblErrorCount";
        lblErrorCount.Size = new Size(19, 21);
        lblErrorCount.TabIndex = 7;
        lblErrorCount.Text = "0";
        // 
        // lblTotalCount
        // 
        lblTotalCount.AutoSize = true;
        lblTotalCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTotalCount.ForeColor = Color.Black;
        lblTotalCount.Location = new Point(243, 109);
        lblTotalCount.Name = "lblTotalCount";
        lblTotalCount.Size = new Size(19, 21);
        lblTotalCount.TabIndex = 8;
        lblTotalCount.Text = "0";
        // 
        // frmMain
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(1580, 720);
        Controls.Add(picCamera);
        Controls.Add(pnlRight);
        Controls.Add(pnlTop);
        Font = new Font("Segoe UI", 9F);
        ForeColor = Color.White;
        MinimumSize = new Size(1100, 598);
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
        panel1.PerformLayout();
        ResumeLayout(false);
    }
    private Panel panel2;
    private Panel panel1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private Label label2;
    private Label label1;
    private Label label3;
    private Label label4;
    private Label lblLargeCount;
    private Label lblTotalCount;
    private Label lblErrorCount;
    private Label lblSmallCount;
}

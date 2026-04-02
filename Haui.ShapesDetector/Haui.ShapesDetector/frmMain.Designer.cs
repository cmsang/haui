using Haui.ShapesDetector.Controls;

namespace Haui.ShapesDetector
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlMain = new Panel();
            pnlCenter = new Panel();
            pnlLeft = new Panel();
            detectionPanel = new DetectionPanel();
            pnlRight = new Panel();
            dgvResults = new DataGridView();
            colObjectName = new DataGridViewTextBoxColumn();
            colConfidence = new DataGridViewTextBoxColumn();
            colTime = new DataGridViewTextBoxColumn();
            lblResults = new Label();
            pnlBottom = new Panel();
            btnPrepareDataset = new Button();
            btnTest = new Button();
            btnSettings = new Button();
            btnSaveResults = new Button();
            btnCapture = new Button();
            btnStop = new Button();
            btnStartCamera = new Button();
            pnlTop = new Panel();
            lblStatus = new Label();
            cmbCameras = new ComboBox();
            lblCamera = new Label();
            timerCheckJob = new System.Windows.Forms.Timer(components);
            pnlMain.SuspendLayout();
            pnlCenter.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            pnlBottom.SuspendLayout();
            pnlTop.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(30, 30, 30);
            pnlMain.Controls.Add(pnlCenter);
            pnlMain.Controls.Add(pnlBottom);
            pnlMain.Controls.Add(pnlTop);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1400, 800);
            pnlMain.TabIndex = 0;
            // 
            // pnlCenter
            // 
            pnlCenter.BackColor = Color.FromArgb(30, 30, 30);
            pnlCenter.Controls.Add(pnlLeft);
            pnlCenter.Controls.Add(pnlRight);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(0, 60);
            pnlCenter.Margin = new Padding(0);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Size = new Size(1400, 660);
            pnlCenter.TabIndex = 1;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(30, 30, 30);
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlLeft.Controls.Add(detectionPanel);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(1050, 660);
            pnlLeft.TabIndex = 0;
            // 
            // detectionPanel
            // 
            detectionPanel.BackColor = Color.FromArgb(20, 20, 20);
            detectionPanel.Dock = DockStyle.Fill;
            detectionPanel.Location = new Point(0, 0);
            detectionPanel.Margin = new Padding(0);
            detectionPanel.Name = "detectionPanel";
            detectionPanel.Padding = new Padding(5);
            detectionPanel.Size = new Size(1048, 658);
            detectionPanel.TabIndex = 1;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(45, 45, 48);
            pnlRight.BorderStyle = BorderStyle.FixedSingle;
            pnlRight.Controls.Add(dgvResults);
            pnlRight.Controls.Add(lblResults);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(1050, 0);
            pnlRight.Margin = new Padding(0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(350, 660);
            pnlRight.TabIndex = 1;
            // 
            // dgvResults
            // 
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.AllowUserToResizeColumns = false;
            dgvResults.AllowUserToResizeRows = false;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.BackgroundColor = Color.FromArgb(30, 30, 30);
            dgvResults.BorderStyle = BorderStyle.None;
            dgvResults.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvResults.ColumnHeadersHeight = 35;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvResults.Columns.AddRange(new DataGridViewColumn[] { colObjectName, colConfidence, colTime });
            dgvResults.Dock = DockStyle.Fill;
            dgvResults.GridColor = Color.FromArgb(60, 60, 60);
            dgvResults.Location = new Point(0, 45);
            dgvResults.Margin = new Padding(0);
            dgvResults.MultiSelect = false;
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowHeadersWidth = 51;
            dgvResults.RowTemplate.Height = 30;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.Size = new Size(348, 613);
            dgvResults.TabIndex = 1;
            // 
            // colObjectName
            // 
            colObjectName.FillWeight = 45F;
            colObjectName.HeaderText = "Object";
            colObjectName.MinimumWidth = 80;
            colObjectName.Name = "colObjectName";
            colObjectName.ReadOnly = true;
            // 
            // colConfidence
            // 
            colConfidence.FillWeight = 30F;
            colConfidence.HeaderText = "Conf.";
            colConfidence.MinimumWidth = 60;
            colConfidence.Name = "colConfidence";
            colConfidence.ReadOnly = true;
            // 
            // colTime
            // 
            colTime.FillWeight = 25F;
            colTime.HeaderText = "Time";
            colTime.MinimumWidth = 80;
            colTime.Name = "colTime";
            colTime.ReadOnly = true;
            // 
            // lblResults
            // 
            lblResults.BackColor = Color.FromArgb(37, 37, 38);
            lblResults.BorderStyle = BorderStyle.FixedSingle;
            lblResults.Dock = DockStyle.Top;
            lblResults.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            lblResults.ForeColor = Color.White;
            lblResults.Location = new Point(0, 0);
            lblResults.Margin = new Padding(0);
            lblResults.Name = "lblResults";
            lblResults.Size = new Size(348, 45);
            lblResults.TabIndex = 0;
            lblResults.Text = "📊 Detection Results";
            lblResults.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Controls.Add(btnPrepareDataset);
            pnlBottom.Controls.Add(btnTest);
            pnlBottom.Controls.Add(btnSettings);
            pnlBottom.Controls.Add(btnSaveResults);
            pnlBottom.Controls.Add(btnCapture);
            pnlBottom.Controls.Add(btnStop);
            pnlBottom.Controls.Add(btnStartCamera);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 720);
            pnlBottom.Margin = new Padding(0);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(16, 15, 16, 15);
            pnlBottom.Size = new Size(1400, 80);
            pnlBottom.TabIndex = 2;
            // 
            // btnPrepareDataset
            // 
            btnPrepareDataset.Anchor = AnchorStyles.Left;
            btnPrepareDataset.BackColor = Color.FromArgb(200, 130, 40);
            btnPrepareDataset.Cursor = Cursors.Hand;
            btnPrepareDataset.FlatAppearance.BorderSize = 0;
            btnPrepareDataset.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 90, 20);
            btnPrepareDataset.FlatAppearance.MouseOverBackColor = Color.FromArgb(180, 110, 30);
            btnPrepareDataset.FlatStyle = FlatStyle.Flat;
            btnPrepareDataset.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnPrepareDataset.ForeColor = Color.White;
            btnPrepareDataset.Location = new Point(1124, 15);
            btnPrepareDataset.Margin = new Padding(12, 0, 0, 0);
            btnPrepareDataset.Name = "btnPrepareDataset";
            btnPrepareDataset.Size = new Size(160, 48);
            btnPrepareDataset.TabIndex = 6;
            btnPrepareDataset.Text = "🗂 Prepare Data";
            btnPrepareDataset.UseVisualStyleBackColor = false;
            btnPrepareDataset.Click += btnPrepareDataset_Click;
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Left;
            btnTest.BackColor = Color.FromArgb(180, 100, 220);
            btnTest.Cursor = Cursors.Hand;
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.FlatAppearance.MouseDownBackColor = Color.FromArgb(130, 60, 170);
            btnTest.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 80, 200);
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnTest.ForeColor = Color.White;
            btnTest.Location = new Point(944, 15);
            btnTest.Margin = new Padding(12, 0, 0, 0);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(160, 48);
            btnTest.TabIndex = 5;
            btnTest.Text = "\U0001f9ea Test";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.Click += btnTest_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Left;
            btnSettings.BackColor = Color.FromArgb(80, 80, 84);
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 50, 54);
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 100, 104);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(764, 15);
            btnSettings.Margin = new Padding(12, 0, 0, 0);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(160, 48);
            btnSettings.TabIndex = 4;
            btnSettings.Text = "Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // btnSaveResults
            // 
            btnSaveResults.Anchor = AnchorStyles.Left;
            btnSaveResults.BackColor = Color.FromArgb(28, 151, 234);
            btnSaveResults.Cursor = Cursors.Hand;
            btnSaveResults.FlatAppearance.BorderSize = 0;
            btnSaveResults.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170);
            btnSaveResults.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 210);
            btnSaveResults.FlatStyle = FlatStyle.Flat;
            btnSaveResults.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnSaveResults.ForeColor = Color.White;
            btnSaveResults.Location = new Point(580, 15);
            btnSaveResults.Margin = new Padding(12, 0, 0, 0);
            btnSaveResults.Name = "btnSaveResults";
            btnSaveResults.Size = new Size(160, 48);
            btnSaveResults.TabIndex = 3;
            btnSaveResults.Text = "Save";
            btnSaveResults.UseVisualStyleBackColor = false;
            btnSaveResults.Click += btnSaveResults_Click;
            // 
            // btnCapture
            // 
            btnCapture.Anchor = AnchorStyles.Left;
            btnCapture.BackColor = Color.FromArgb(28, 151, 234);
            btnCapture.Cursor = Cursors.Hand;
            btnCapture.FlatAppearance.BorderSize = 0;
            btnCapture.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170);
            btnCapture.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 210);
            btnCapture.FlatStyle = FlatStyle.Flat;
            btnCapture.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCapture.ForeColor = Color.White;
            btnCapture.Location = new Point(396, 15);
            btnCapture.Margin = new Padding(12, 0, 0, 0);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new Size(160, 48);
            btnCapture.TabIndex = 2;
            btnCapture.Text = "Capture";
            btnCapture.UseVisualStyleBackColor = false;
            btnCapture.Click += btnCapture_Click;
            // 
            // btnStop
            // 
            btnStop.Anchor = AnchorStyles.Left;
            btnStop.BackColor = Color.FromArgb(231, 72, 86);
            btnStop.Cursor = Cursors.Hand;
            btnStop.Enabled = false;
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 50, 60);
            btnStop.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 60, 70);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnStop.ForeColor = Color.White;
            btnStop.Location = new Point(212, 15);
            btnStop.Margin = new Padding(12, 0, 0, 0);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(160, 48);
            btnStop.TabIndex = 1;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // btnStartCamera
            // 
            btnStartCamera.Anchor = AnchorStyles.Left;
            btnStartCamera.BackColor = Color.FromArgb(16, 185, 129);
            btnStartCamera.Cursor = Cursors.Hand;
            btnStartCamera.FlatAppearance.BorderSize = 0;
            btnStartCamera.FlatAppearance.MouseDownBackColor = Color.FromArgb(10, 140, 100);
            btnStartCamera.FlatAppearance.MouseOverBackColor = Color.FromArgb(13, 160, 115);
            btnStartCamera.FlatStyle = FlatStyle.Flat;
            btnStartCamera.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnStartCamera.ForeColor = Color.White;
            btnStartCamera.Location = new Point(16, 15);
            btnStartCamera.Margin = new Padding(0);
            btnStartCamera.Name = "btnStartCamera";
            btnStartCamera.Size = new Size(180, 48);
            btnStartCamera.TabIndex = 0;
            btnStartCamera.Text = "Start Camera";
            btnStartCamera.UseVisualStyleBackColor = false;
            btnStartCamera.Click += btnStartCamera_Click;
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(37, 37, 38);
            pnlTop.BorderStyle = BorderStyle.FixedSingle;
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(cmbCameras);
            pnlTop.Controls.Add(lblCamera);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(0);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(16, 10, 16, 10);
            pnlTop.Size = new Size(1400, 60);
            pnlTop.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(1316, 18);
            lblStatus.Margin = new Padding(0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(74, 23);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "● Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbCameras
            // 
            cmbCameras.BackColor = Color.FromArgb(62, 62, 64);
            cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCameras.FlatStyle = FlatStyle.Flat;
            cmbCameras.Font = new Font("Segoe UI", 10.2F);
            cmbCameras.ForeColor = Color.White;
            cmbCameras.FormattingEnabled = true;
            cmbCameras.Location = new Point(116, 15);
            cmbCameras.Margin = new Padding(8, 0, 0, 0);
            cmbCameras.Name = "cmbCameras";
            cmbCameras.Size = new Size(300, 31);
            cmbCameras.TabIndex = 1;
            cmbCameras.SelectedIndexChanged += cmbCameras_SelectedIndexChanged;
            // 
            // lblCamera
            // 
            lblCamera.AutoSize = true;
            lblCamera.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            lblCamera.ForeColor = Color.FromArgb(200, 200, 200);
            lblCamera.Location = new Point(16, 18);
            lblCamera.Margin = new Padding(0);
            lblCamera.Name = "lblCamera";
            lblCamera.Size = new Size(76, 23);
            lblCamera.TabIndex = 0;
            lblCamera.Text = "Camera:";
            // 
            // timerCheckJob
            // 
            timerCheckJob.Interval = 500;
            timerCheckJob.Tick += timerCheckJob_Tick;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 48);
            ClientSize = new Size(1400, 800);
            Controls.Add(pnlMain);
            MinimumSize = new Size(1200, 700);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Object Detection Tool - Haui.ShapesDetector";
            Load += frmMain_Load;
            pnlMain.ResumeLayout(false);
            pnlCenter.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlMain;
        private Panel pnlTop;
        private Label lblCamera;
        private ComboBox cmbCameras;
        private Label lblStatus;
        private Panel pnlCenter;
        private Panel pnlLeft;
        private DetectionPanel detectionPanel;
        private Panel pnlRight;
        private Label lblResults;
        private DataGridView dgvResults;
        private DataGridViewTextBoxColumn colObjectName;
        private DataGridViewTextBoxColumn colConfidence;
        private DataGridViewTextBoxColumn colTime;
        private Panel pnlBottom;
        private Button btnStartCamera;
        private Button btnStop;
        private Button btnCapture;
        private Button btnSaveResults;
        private Button btnSettings;
        private Button btnTest;
        private Button btnPrepareDataset;
        private System.Windows.Forms.Timer timerCheckJob;
    }
}

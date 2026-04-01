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
            pnlRight = new Panel();
            dgvResults = new DataGridView();
            colObjectName = new DataGridViewTextBoxColumn();
            colConfidence = new DataGridViewTextBoxColumn();
            colTime = new DataGridViewTextBoxColumn();
            lblResults = new Label();
            detectionPanel = new DetectionPanel();
            pnlBottom = new Panel();
            pnlBottomTop = new Panel();
            btnSaveResults = new Button();
            btnCapture = new Button();
            btnStop = new Button();
            btnStartCamera = new Button();
            pnlBottomBottom = new Panel();
            lblStatus = new Label();
            cmbCameras = new ComboBox();
            lblCamera = new Label();
            timerCheckJob = new System.Windows.Forms.Timer(components);
            pnlMain.SuspendLayout();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            pnlBottom.SuspendLayout();
            pnlBottomTop.SuspendLayout();
            pnlBottomBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(37, 37, 38);
            pnlMain.Controls.Add(pnlRight);
            pnlMain.Controls.Add(detectionPanel);
            pnlMain.Controls.Add(pnlBottom);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1311, 733);
            pnlMain.TabIndex = 1;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(45, 45, 48);
            pnlRight.Controls.Add(dgvResults);
            pnlRight.Controls.Add(lblResults);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(961, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(350, 613);
            pnlRight.TabIndex = 2;
            // 
            // dgvResults
            // 
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AllowUserToDeleteRows = false;
            dgvResults.BackgroundColor = Color.FromArgb(30, 30, 30);
            dgvResults.BorderStyle = BorderStyle.None;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Columns.AddRange(new DataGridViewColumn[] { colObjectName, colConfidence, colTime });
            dgvResults.Dock = DockStyle.Fill;
            dgvResults.Location = new Point(0, 40);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowHeadersWidth = 51;
            dgvResults.Size = new Size(350, 573);
            dgvResults.TabIndex = 1;
            // 
            // colObjectName
            // 
            colObjectName.HeaderText = "Object Name";
            colObjectName.MinimumWidth = 6;
            colObjectName.Name = "colObjectName";
            colObjectName.ReadOnly = true;
            colObjectName.Width = 130;
            // 
            // colConfidence
            // 
            colConfidence.HeaderText = "Confidence";
            colConfidence.MinimumWidth = 6;
            colConfidence.Name = "colConfidence";
            colConfidence.ReadOnly = true;
            colConfidence.Width = 90;
            // 
            // colTime
            // 
            colTime.HeaderText = "Time";
            colTime.MinimumWidth = 6;
            colTime.Name = "colTime";
            colTime.ReadOnly = true;
            colTime.Width = 125;
            // 
            // lblResults
            // 
            lblResults.BackColor = Color.FromArgb(37, 37, 38);
            lblResults.Dock = DockStyle.Top;
            lblResults.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResults.ForeColor = Color.White;
            lblResults.Location = new Point(0, 0);
            lblResults.Name = "lblResults";
            lblResults.Size = new Size(350, 40);
            lblResults.TabIndex = 0;
            lblResults.Text = "Detection Results";
            lblResults.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // detectionPanel
            // 
            detectionPanel.BackColor = Color.FromArgb(30, 30, 30);
            detectionPanel.Dock = DockStyle.Fill;
            detectionPanel.Location = new Point(0, 0);
            detectionPanel.Name = "detectionPanel";
            detectionPanel.Size = new Size(1311, 613);
            detectionPanel.TabIndex = 1;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(30, 30, 30);
            pnlBottom.Controls.Add(pnlBottomTop);
            pnlBottom.Controls.Add(pnlBottomBottom);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 613);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1311, 120);
            pnlBottom.TabIndex = 0;
            // 
            // pnlBottomTop
            // 
            pnlBottomTop.Controls.Add(btnSaveResults);
            pnlBottomTop.Controls.Add(btnCapture);
            pnlBottomTop.Controls.Add(btnStop);
            pnlBottomTop.Controls.Add(btnStartCamera);
            pnlBottomTop.Dock = DockStyle.Top;
            pnlBottomTop.Location = new Point(0, 0);
            pnlBottomTop.Name = "pnlBottomTop";
            pnlBottomTop.Size = new Size(1311, 70);
            pnlBottomTop.TabIndex = 0;
            // 
            // btnSaveResults
            // 
            btnSaveResults.BackColor = Color.FromArgb(0, 122, 204);
            btnSaveResults.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnSaveResults.FlatStyle = FlatStyle.Flat;
            btnSaveResults.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSaveResults.ForeColor = Color.White;
            btnSaveResults.Location = new Point(530, 15);
            btnSaveResults.Name = "btnSaveResults";
            btnSaveResults.Size = new Size(150, 40);
            btnSaveResults.TabIndex = 3;
            btnSaveResults.Text = "Save Results";
            btnSaveResults.UseVisualStyleBackColor = false;
            btnSaveResults.Click += btnSaveResults_Click;
            // 
            // btnCapture
            // 
            btnCapture.BackColor = Color.FromArgb(0, 122, 204);
            btnCapture.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnCapture.FlatStyle = FlatStyle.Flat;
            btnCapture.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCapture.ForeColor = Color.White;
            btnCapture.Location = new Point(362, 15);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new Size(150, 40);
            btnCapture.TabIndex = 2;
            btnCapture.Text = "Capture";
            btnCapture.UseVisualStyleBackColor = false;
            btnCapture.Click += btnCapture_Click;
            // 
            // btnStop
            // 
            btnStop.BackColor = Color.FromArgb(0, 122, 204);
            btnStop.Enabled = false;
            btnStop.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStop.ForeColor = Color.White;
            btnStop.Location = new Point(194, 15);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(150, 40);
            btnStop.TabIndex = 1;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // btnStartCamera
            // 
            btnStartCamera.BackColor = Color.FromArgb(0, 122, 204);
            btnStartCamera.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnStartCamera.FlatStyle = FlatStyle.Flat;
            btnStartCamera.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStartCamera.ForeColor = Color.White;
            btnStartCamera.Location = new Point(26, 15);
            btnStartCamera.Name = "btnStartCamera";
            btnStartCamera.Size = new Size(150, 40);
            btnStartCamera.TabIndex = 0;
            btnStartCamera.Text = "Start Camera";
            btnStartCamera.UseVisualStyleBackColor = false;
            btnStartCamera.Click += btnStartCamera_Click;
            // 
            // pnlBottomBottom
            // 
            pnlBottomBottom.Controls.Add(lblStatus);
            pnlBottomBottom.Controls.Add(cmbCameras);
            pnlBottomBottom.Controls.Add(lblCamera);
            pnlBottomBottom.Dock = DockStyle.Bottom;
            pnlBottomBottom.Location = new Point(0, 70);
            pnlBottomBottom.Name = "pnlBottomBottom";
            pnlBottomBottom.Size = new Size(1311, 50);
            pnlBottomBottom.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(1031, 13);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(250, 23);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "● Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbCameras
            // 
            cmbCameras.BackColor = Color.FromArgb(45, 45, 48);
            cmbCameras.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCameras.FlatStyle = FlatStyle.Flat;
            cmbCameras.Font = new Font("Segoe UI", 9F);
            cmbCameras.ForeColor = Color.White;
            cmbCameras.FormattingEnabled = true;
            cmbCameras.Location = new Point(106, 13);
            cmbCameras.Name = "cmbCameras";
            cmbCameras.Size = new Size(250, 28);
            cmbCameras.TabIndex = 1;
            cmbCameras.SelectedIndexChanged += cmbCameras_SelectedIndexChanged;
            // 
            // lblCamera
            // 
            lblCamera.AutoSize = true;
            lblCamera.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCamera.ForeColor = Color.White;
            lblCamera.Location = new Point(26, 16);
            lblCamera.Name = "lblCamera";
            lblCamera.Size = new Size(66, 20);
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
            ClientSize = new Size(1311, 733);
            Controls.Add(pnlMain);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Object Detection Tool - Haui.ShapesDetector";
            Load += frmMain_Load;
            pnlMain.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottomTop.ResumeLayout(false);
            pnlBottomBottom.ResumeLayout(false);
            pnlBottomBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlMain;
        private DetectionPanel detectionPanel;
        private Panel pnlBottom;
        private Panel pnlBottomTop;
        private Panel pnlBottomBottom;
        private Button btnStartCamera;
        private Button btnStop;
        private Button btnCapture;
        private Button btnSaveResults;
        private Panel pnlRight;
        private DataGridView dgvResults;
        private Label lblResults;
        private DataGridViewTextBoxColumn colObjectName;
        private DataGridViewTextBoxColumn colConfidence;
        private DataGridViewTextBoxColumn colTime;
        private Label lblStatus;
        private ComboBox cmbCameras;
        private Label lblCamera;
        private System.Windows.Forms.Timer timerCheckJob;
    }
}

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
            lblStatus = new Label();
            btnSaveResults = new Button();
            btnCapture = new Button();
            btnStop = new Button();
            btnStartCamera = new Button();
            timerCheckJob = new System.Windows.Forms.Timer(components);
            pnlMain.SuspendLayout();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            pnlBottom.SuspendLayout();
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
            pnlMain.Size = new Size(1282, 661);
            pnlMain.TabIndex = 1;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(45, 45, 48);
            pnlRight.Controls.Add(dgvResults);
            pnlRight.Controls.Add(lblResults);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(932, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(350, 581);
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
            dgvResults.Size = new Size(350, 541);
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
            detectionPanel.Size = new Size(1282, 581);
            detectionPanel.TabIndex = 1;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(30, 30, 30);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Controls.Add(btnSaveResults);
            pnlBottom.Controls.Add(btnCapture);
            pnlBottom.Controls.Add(btnStop);
            pnlBottom.Controls.Add(btnStartCamera);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 581);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1282, 80);
            pnlBottom.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(1002, 30);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(250, 23);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "● Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnSaveResults
            // 
            btnSaveResults.BackColor = Color.FromArgb(0, 122, 204);
            btnSaveResults.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            btnSaveResults.FlatStyle = FlatStyle.Flat;
            btnSaveResults.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSaveResults.ForeColor = Color.White;
            btnSaveResults.Location = new Point(530, 20);
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
            btnCapture.Location = new Point(362, 20);
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
            btnStop.Location = new Point(194, 20);
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
            btnStartCamera.Location = new Point(26, 20);
            btnStartCamera.Name = "btnStartCamera";
            btnStartCamera.Size = new Size(150, 40);
            btnStartCamera.TabIndex = 0;
            btnStartCamera.Text = "Start Camera";
            btnStartCamera.UseVisualStyleBackColor = false;
            btnStartCamera.Click += btnStartCamera_Click;
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
            ClientSize = new Size(1282, 661);
            Controls.Add(pnlMain);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Object Detection Tool - Haui.ShapesDetector";
            Load += frmMain_Load;
            pnlMain.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlMain;
        private DetectionPanel detectionPanel;
        private Panel pnlBottom;
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
        private System.Windows.Forms.Timer timerCheckJob;
    }
}

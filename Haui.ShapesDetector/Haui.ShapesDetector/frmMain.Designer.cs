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
            pnlSidebar = new Panel();
            btnCamera = new Button();
            btnImages = new Button();
            btnSettings = new Button();
            btnLogs = new Button();
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
            pnlSidebar.SuspendLayout();
            pnlMain.SuspendLayout();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(30, 30, 30);
            pnlSidebar.Controls.Add(btnCamera);
            pnlSidebar.Controls.Add(btnImages);
            pnlSidebar.Controls.Add(btnSettings);
            pnlSidebar.Controls.Add(btnLogs);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Margin = new Padding(3, 2, 3, 2);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(98, 496);
            pnlSidebar.TabIndex = 0;
            // 
            // btnCamera
            // 
            btnCamera.BackColor = Color.FromArgb(0, 122, 204);
            btnCamera.FlatAppearance.BorderSize = 0;
            btnCamera.FlatStyle = FlatStyle.Flat;
            btnCamera.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCamera.ForeColor = Color.White;
            btnCamera.Location = new Point(10, 9);
            btnCamera.Margin = new Padding(3, 2, 3, 2);
            btnCamera.Name = "btnCamera";
            btnCamera.Size = new Size(77, 60);
            btnCamera.TabIndex = 0;
            btnCamera.Text = "📷\r\nCamera";
            btnCamera.UseVisualStyleBackColor = false;
            // 
            // btnImages
            // 
            btnImages.BackColor = Color.FromArgb(45, 45, 48);
            btnImages.FlatAppearance.BorderSize = 0;
            btnImages.FlatStyle = FlatStyle.Flat;
            btnImages.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnImages.ForeColor = Color.White;
            btnImages.Location = new Point(10, 74);
            btnImages.Margin = new Padding(3, 2, 3, 2);
            btnImages.Name = "btnImages";
            btnImages.Size = new Size(77, 60);
            btnImages.TabIndex = 1;
            btnImages.Text = "🖼️\r\nImages";
            btnImages.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = Color.FromArgb(45, 45, 48);
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(10, 138);
            btnSettings.Margin = new Padding(3, 2, 3, 2);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(77, 60);
            btnSettings.TabIndex = 2;
            btnSettings.Text = "⚙️\r\nSettings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // btnLogs
            // 
            btnLogs.BackColor = Color.FromArgb(45, 45, 48);
            btnLogs.FlatAppearance.BorderSize = 0;
            btnLogs.FlatStyle = FlatStyle.Flat;
            btnLogs.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogs.ForeColor = Color.White;
            btnLogs.Location = new Point(10, 202);
            btnLogs.Margin = new Padding(3, 2, 3, 2);
            btnLogs.Name = "btnLogs";
            btnLogs.Size = new Size(77, 60);
            btnLogs.TabIndex = 3;
            btnLogs.Text = "📝\r\nLogs";
            btnLogs.UseVisualStyleBackColor = false;
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(37, 37, 38);
            pnlMain.Controls.Add(pnlRight);
            pnlMain.Controls.Add(detectionPanel);
            pnlMain.Controls.Add(pnlBottom);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(98, 0);
            pnlMain.Margin = new Padding(3, 2, 3, 2);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1024, 496);
            pnlMain.TabIndex = 1;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(45, 45, 48);
            pnlRight.Controls.Add(dgvResults);
            pnlRight.Controls.Add(lblResults);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(718, 0);
            pnlRight.Margin = new Padding(3, 2, 3, 2);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(306, 436);
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
            dgvResults.Location = new Point(0, 30);
            dgvResults.Margin = new Padding(3, 2, 3, 2);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowHeadersWidth = 51;
            dgvResults.Size = new Size(306, 406);
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
            lblResults.Size = new Size(306, 30);
            lblResults.TabIndex = 0;
            lblResults.Text = "Detection Results";
            lblResults.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // detectionPanel
            // 
            detectionPanel.BackColor = Color.FromArgb(30, 30, 30);
            detectionPanel.Dock = DockStyle.Fill;
            detectionPanel.Location = new Point(0, 0);
            detectionPanel.Margin = new Padding(3, 2, 3, 2);
            detectionPanel.Name = "detectionPanel";
            detectionPanel.Size = new Size(1024, 436);
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
            pnlBottom.Location = new Point(0, 436);
            pnlBottom.Margin = new Padding(3, 2, 3, 2);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1024, 60);
            pnlBottom.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(779, 22);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(219, 17);
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
            btnSaveResults.Location = new Point(464, 15);
            btnSaveResults.Margin = new Padding(3, 2, 3, 2);
            btnSaveResults.Name = "btnSaveResults";
            btnSaveResults.Size = new Size(131, 30);
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
            btnCapture.Location = new Point(317, 15);
            btnCapture.Margin = new Padding(3, 2, 3, 2);
            btnCapture.Name = "btnCapture";
            btnCapture.Size = new Size(131, 30);
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
            btnStop.Location = new Point(170, 15);
            btnStop.Margin = new Padding(3, 2, 3, 2);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(131, 30);
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
            btnStartCamera.Location = new Point(23, 15);
            btnStartCamera.Margin = new Padding(3, 2, 3, 2);
            btnStartCamera.Name = "btnStartCamera";
            btnStartCamera.Size = new Size(131, 30);
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
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(45, 45, 48);
            ClientSize = new Size(1122, 496);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Margin = new Padding(3, 2, 3, 2);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Object Detection Tool - Haui.ShapesDetector";
            Load += frmMain_Load;
            pnlSidebar.ResumeLayout(false);
            pnlMain.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Button btnCamera;
        private Button btnImages;
        private Button btnSettings;
        private Button btnLogs;
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

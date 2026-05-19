namespace Haui.GT
{
    partial class frmVideoCapture
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlTop = new Panel();
            lblTitle = new Label();
            pnlContent = new Panel();
            grpVideo = new GroupBox();
            lsvVideos = new ListBox();
            pnlVideoButtons = new Panel();
            btnAddVideo = new Button();
            btnRemoveVideo = new Button();
            grpSettings = new GroupBox();
            lblInterval = new Label();
            txtInterval = new TextBox();
            lblIntervalUnit = new Label();
            chkSingleFolder = new CheckBox();
            lblFolder = new Label();
            txtOutputFolder = new TextBox();
            btnBrowseFolder = new Button();
            grpStatus = new GroupBox();
            progressBar = new ProgressBar();
            lblLog = new Label();
            pnlBottom = new Panel();
            btnStart = new Button();
            btnOpenFolder = new Button();
            btnClose = new Button();
            pnlTop.SuspendLayout();
            pnlContent.SuspendLayout();
            grpVideo.SuspendLayout();
            pnlVideoButtons.SuspendLayout();
            grpSettings.SuspendLayout();
            grpStatus.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(37, 37, 38);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(14, 10, 14, 10);
            pnlTop.Size = new Size(580, 60);
            pnlTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = false;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Name = "lblTitle";
            lblTitle.Text = "🎞 Lấy ảnh từ Video";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(30, 30, 30);
            pnlContent.Controls.Add(grpStatus);
            pnlContent.Controls.Add(grpSettings);
            pnlContent.Controls.Add(grpVideo);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 60);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(14, 10, 14, 10);
            pnlContent.Size = new Size(580, 290);
            pnlContent.TabIndex = 1;
            // 
            // grpVideo
            // 
            grpVideo.Controls.Add(lsvVideos);
            grpVideo.Controls.Add(pnlVideoButtons);
            grpVideo.Dock = DockStyle.Top;
            grpVideo.ForeColor = Color.White;
            grpVideo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpVideo.Location = new Point(14, 10);
            grpVideo.Name = "grpVideo";
            grpVideo.Padding = new Padding(10, 6, 10, 6);
            grpVideo.Size = new Size(552, 130);
            grpVideo.TabIndex = 0;
            grpVideo.TabStop = false;
            grpVideo.Text = "Danh sách Video";
            // 
            // lsvVideos
            // 
            lsvVideos.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            lsvVideos.BackColor = Color.FromArgb(45, 45, 48);
            lsvVideos.BorderStyle = BorderStyle.FixedSingle;
            lsvVideos.Font = new Font("Segoe UI", 9.75F);
            lsvVideos.ForeColor = Color.White;
            lsvVideos.Location = new Point(10, 22);
            lsvVideos.Name = "lsvVideos";
            lsvVideos.SelectionMode = SelectionMode.MultiExtended;
            lsvVideos.Size = new Size(432, 98);
            lsvVideos.TabIndex = 0;
            // 
            // pnlVideoButtons
            // 
            pnlVideoButtons.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            pnlVideoButtons.Controls.Add(btnAddVideo);
            pnlVideoButtons.Controls.Add(btnRemoveVideo);
            pnlVideoButtons.Location = new Point(452, 22);
            pnlVideoButtons.Name = "pnlVideoButtons";
            pnlVideoButtons.Size = new Size(90, 98);
            pnlVideoButtons.TabIndex = 1;
            // 
            // btnAddVideo
            // 
            btnAddVideo.BackColor = Color.FromArgb(28, 151, 234);
            btnAddVideo.Cursor = Cursors.Hand;
            btnAddVideo.Dock = DockStyle.Top;
            btnAddVideo.FlatAppearance.BorderSize = 0;
            btnAddVideo.FlatStyle = FlatStyle.Flat;
            btnAddVideo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnAddVideo.ForeColor = Color.White;
            btnAddVideo.Location = new Point(0, 0);
            btnAddVideo.Name = "btnAddVideo";
            btnAddVideo.Size = new Size(90, 44);
            btnAddVideo.TabIndex = 0;
            btnAddVideo.Text = "+ Thêm";
            btnAddVideo.UseVisualStyleBackColor = false;
            btnAddVideo.Click += btnAddVideo_Click;
            // 
            // btnRemoveVideo
            // 
            btnRemoveVideo.BackColor = Color.FromArgb(231, 72, 86);
            btnRemoveVideo.Cursor = Cursors.Hand;
            btnRemoveVideo.Dock = DockStyle.Bottom;
            btnRemoveVideo.FlatAppearance.BorderSize = 0;
            btnRemoveVideo.FlatStyle = FlatStyle.Flat;
            btnRemoveVideo.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnRemoveVideo.ForeColor = Color.White;
            btnRemoveVideo.Location = new Point(0, 54);
            btnRemoveVideo.Name = "btnRemoveVideo";
            btnRemoveVideo.Size = new Size(90, 44);
            btnRemoveVideo.TabIndex = 1;
            btnRemoveVideo.Text = "- Xóa";
            btnRemoveVideo.UseVisualStyleBackColor = false;
            btnRemoveVideo.Click += btnRemoveVideo_Click;
            // 
            // grpSettings
            // 
            grpSettings.Controls.Add(lblInterval);
            grpSettings.Controls.Add(txtInterval);
            grpSettings.Controls.Add(lblIntervalUnit);
            grpSettings.Controls.Add(chkSingleFolder);
            grpSettings.Controls.Add(lblFolder);
            grpSettings.Controls.Add(txtOutputFolder);
            grpSettings.Controls.Add(btnBrowseFolder);
            grpSettings.Dock = DockStyle.Top;
            grpSettings.ForeColor = Color.White;
            grpSettings.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpSettings.Location = new Point(14, 140);
            grpSettings.Name = "grpSettings";
            grpSettings.Padding = new Padding(10, 6, 10, 6);
            grpSettings.Size = new Size(552, 128);
            grpSettings.TabIndex = 1;
            grpSettings.TabStop = false;
            grpSettings.Text = "Cài đặt";
            // 
            // lblInterval
            // 
            lblInterval.AutoSize = true;
            lblInterval.ForeColor = Color.FromArgb(200, 200, 200);
            lblInterval.Font = new Font("Segoe UI", 9.75F);
            lblInterval.Location = new Point(10, 28);
            lblInterval.Name = "lblInterval";
            lblInterval.Text = "Khoảng thời gian (t):";
            // 
            // txtInterval
            // 
            txtInterval.BackColor = Color.FromArgb(62, 62, 64);
            txtInterval.BorderStyle = BorderStyle.FixedSingle;
            txtInterval.ForeColor = Color.White;
            txtInterval.Font = new Font("Segoe UI", 9.75F);
            txtInterval.Location = new Point(165, 25);
            txtInterval.Name = "txtInterval";
            txtInterval.Size = new Size(80, 23);
            txtInterval.TabIndex = 0;
            txtInterval.Text = "5";
            // 
            // lblIntervalUnit
            // 
            lblIntervalUnit.AutoSize = true;
            lblIntervalUnit.ForeColor = Color.FromArgb(200, 200, 200);
            lblIntervalUnit.Font = new Font("Segoe UI", 9.75F);
            lblIntervalUnit.Location = new Point(252, 28);
            lblIntervalUnit.Name = "lblIntervalUnit";
            lblIntervalUnit.Text = "giây";
            // 
            // chkSingleFolder
            // 
            chkSingleFolder.AutoSize = true;
            chkSingleFolder.Checked = false;
            chkSingleFolder.Cursor = Cursors.Hand;
            chkSingleFolder.Font = new Font("Segoe UI", 9.75F);
            chkSingleFolder.ForeColor = Color.FromArgb(200, 200, 200);
            chkSingleFolder.Location = new Point(10, 56);
            chkSingleFolder.Name = "chkSingleFolder";
            chkSingleFolder.Size = new Size(280, 21);
            chkSingleFolder.TabIndex = 5;
            chkSingleFolder.Text = "Lưu tất cả ảnh vào cùng 1 thư mục";
            chkSingleFolder.UseVisualStyleBackColor = false;
            chkSingleFolder.BackColor = Color.Transparent;
            // 
            // lblFolder
            // 
            lblFolder.AutoSize = true;
            lblFolder.ForeColor = Color.FromArgb(200, 200, 200);
            lblFolder.Font = new Font("Segoe UI", 9.75F);
            lblFolder.Location = new Point(10, 85);
            lblFolder.Name = "lblFolder";
            lblFolder.Text = "Thư mục lưu ảnh (A):";
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtOutputFolder.BackColor = Color.FromArgb(62, 62, 64);
            txtOutputFolder.BorderStyle = BorderStyle.FixedSingle;
            txtOutputFolder.ForeColor = Color.White;
            txtOutputFolder.Font = new Font("Segoe UI", 9.75F);
            txtOutputFolder.Location = new Point(165, 82);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new Size(277, 23);
            txtOutputFolder.TabIndex = 2;
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnBrowseFolder.BackColor = Color.FromArgb(28, 151, 234);
            btnBrowseFolder.Cursor = Cursors.Hand;
            btnBrowseFolder.FlatAppearance.BorderSize = 0;
            btnBrowseFolder.FlatStyle = FlatStyle.Flat;
            btnBrowseFolder.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnBrowseFolder.ForeColor = Color.White;
            btnBrowseFolder.Location = new Point(452, 80);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new Size(90, 26);
            btnBrowseFolder.TabIndex = 3;
            btnBrowseFolder.Text = "📁 Chọn";
            btnBrowseFolder.UseVisualStyleBackColor = false;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // grpStatus
            // 
            grpStatus.Controls.Add(progressBar);
            grpStatus.Controls.Add(lblLog);
            grpStatus.Dock = DockStyle.Fill;
            grpStatus.ForeColor = Color.White;
            grpStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpStatus.Location = new Point(14, 166);
            grpStatus.Name = "grpStatus";
            grpStatus.Padding = new Padding(10, 6, 10, 6);
            grpStatus.TabIndex = 2;
            grpStatus.TabStop = false;
            grpStatus.Text = "Trạng thái";
            // 
            // progressBar
            // 
            progressBar.Dock = DockStyle.Top;
            progressBar.Location = new Point(10, 22);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(530, 20);
            progressBar.TabIndex = 0;
            progressBar.Visible = false;
            // 
            // lblLog
            // 
            lblLog.AutoSize = false;
            lblLog.Dock = DockStyle.Fill;
            lblLog.Font = new Font("Segoe UI", 9.75F);
            lblLog.ForeColor = Color.LimeGreen;
            lblLog.Location = new Point(10, 42);
            lblLog.Name = "lblLog";
            lblLog.Text = "Chờ bắt đầu...";
            lblLog.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Controls.Add(btnOpenFolder);
            pnlBottom.Controls.Add(btnStart);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 350);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(14, 10, 14, 10);
            pnlBottom.Size = new Size(580, 58);
            pnlBottom.TabIndex = 2;
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Left;
            btnStart.BackColor = Color.FromArgb(16, 185, 129);
            btnStart.Cursor = Cursors.Hand;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(14, 10);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(150, 36);
            btnStart.TabIndex = 0;
            btnStart.Text = "▶ Bắt đầu";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Anchor = AnchorStyles.Left;
            btnOpenFolder.BackColor = Color.FromArgb(80, 80, 84);
            btnOpenFolder.Cursor = Cursors.Hand;
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.FlatStyle = FlatStyle.Flat;
            btnOpenFolder.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnOpenFolder.ForeColor = Color.White;
            btnOpenFolder.Location = new Point(174, 10);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(150, 36);
            btnOpenFolder.TabIndex = 1;
            btnOpenFolder.Text = "📂 Mở thư mục";
            btnOpenFolder.UseVisualStyleBackColor = false;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(231, 72, 86);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(416, 10);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(150, 36);
            btnClose.TabIndex = 2;
            btnClose.Text = "✖ Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += (s, e) => Close();
            // 
            // frmVideoCapture
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(580, 480);
            Controls.Add(pnlContent);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmVideoCapture";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Lấy ảnh từ Video";
            pnlTop.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            grpVideo.ResumeLayout(false);
            pnlVideoButtons.ResumeLayout(false);
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            grpStatus.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTop;
        private Label lblTitle;
        private Panel pnlContent;
        private GroupBox grpVideo;
        private ListBox lsvVideos;
        private Panel pnlVideoButtons;
        private Button btnAddVideo;
        private Button btnRemoveVideo;
        private GroupBox grpSettings;
        private Label lblInterval;
        private TextBox txtInterval;
        private Label lblIntervalUnit;
        private CheckBox chkSingleFolder;
        private Label lblFolder;
        private TextBox txtOutputFolder;
        private Button btnBrowseFolder;
        private GroupBox grpStatus;
        private ProgressBar progressBar;
        private Label lblLog;
        private Panel pnlBottom;
        private Button btnStart;
        private Button btnOpenFolder;
        private Button btnClose;
    }
}

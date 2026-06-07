namespace UTT.ShapesDetector;

partial class frmPrepareDataset
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlMain = new Panel();
        pnlCenter = new Panel();
        lblProgress = new Label();
        progressBar = new ProgressBar();
        pnlFolderSelect = new Panel();
        btnBrowse = new Button();
        txtFolderPath = new TextBox();
        lblFolder = new Label();
        pnlTop = new Panel();
        lblTitle = new Label();
        pnlBottom = new Panel();
        lblStatus = new Label();
        btnPrepare = new Button();
        pnlMain.SuspendLayout();
        pnlCenter.SuspendLayout();
        pnlFolderSelect.SuspendLayout();
        pnlTop.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();
        // 
        // pnlMain
        // 
        pnlMain.BackColor = Color.FromArgb(30, 30, 30);
        pnlMain.Controls.Add(pnlCenter);
        pnlMain.Controls.Add(pnlFolderSelect);
        pnlMain.Controls.Add(pnlTop);
        pnlMain.Controls.Add(pnlBottom);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 0);
        pnlMain.Name = "pnlMain";
        pnlMain.Size = new Size(700, 360);
        pnlMain.TabIndex = 0;
        // 
        // pnlCenter
        // 
        pnlCenter.BackColor = Color.FromArgb(30, 30, 30);
        pnlCenter.Controls.Add(lblProgress);
        pnlCenter.Controls.Add(progressBar);
        pnlCenter.Dock = DockStyle.Fill;
        pnlCenter.Location = new Point(0, 116);
        pnlCenter.Name = "pnlCenter";
        pnlCenter.Size = new Size(700, 174);
        pnlCenter.TabIndex = 2;
        // 
        // lblProgress
        // 
        lblProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblProgress.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        lblProgress.ForeColor = Color.FromArgb(160, 160, 160);
        lblProgress.Location = new Point(40, 102);
        lblProgress.Name = "lblProgress";
        lblProgress.Size = new Size(620, 24);
        lblProgress.TabIndex = 1;
        lblProgress.Text = "0 / 0";
        lblProgress.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // progressBar
        // 
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(40, 64);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(620, 28);
        progressBar.TabIndex = 0;
        // 
        // pnlFolderSelect
        // 
        pnlFolderSelect.BackColor = Color.FromArgb(37, 37, 38);
        pnlFolderSelect.BorderStyle = BorderStyle.FixedSingle;
        pnlFolderSelect.Controls.Add(btnBrowse);
        pnlFolderSelect.Controls.Add(txtFolderPath);
        pnlFolderSelect.Controls.Add(lblFolder);
        pnlFolderSelect.Dock = DockStyle.Top;
        pnlFolderSelect.Location = new Point(0, 52);
        pnlFolderSelect.Name = "pnlFolderSelect";
        pnlFolderSelect.Padding = new Padding(16, 14, 16, 14);
        pnlFolderSelect.Size = new Size(700, 64);
        pnlFolderSelect.TabIndex = 1;
        // 
        // btnBrowse
        // 
        btnBrowse.BackColor = Color.FromArgb(28, 151, 234);
        btnBrowse.Cursor = Cursors.Hand;
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170);
        btnBrowse.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 210);
        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnBrowse.ForeColor = Color.White;
        btnBrowse.Location = new Point(516, 14);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(168, 36);
        btnBrowse.TabIndex = 1;
        btnBrowse.Text = "Chọn thư mục";
        btnBrowse.UseVisualStyleBackColor = false;
        btnBrowse.Click += btnBrowse_Click;
        // 
        // txtFolderPath
        // 
        txtFolderPath.BackColor = Color.FromArgb(62, 62, 64);
        txtFolderPath.BorderStyle = BorderStyle.FixedSingle;
        txtFolderPath.Font = new Font("Segoe UI", 9.75F);
        txtFolderPath.ForeColor = Color.White;
        txtFolderPath.Location = new Point(112, 17);
        txtFolderPath.Name = "txtFolderPath";
        txtFolderPath.ReadOnly = true;
        txtFolderPath.Size = new Size(388, 29);
        txtFolderPath.TabIndex = 0;
        // 
        // lblFolder
        // 
        lblFolder.AutoSize = true;
        lblFolder.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        lblFolder.ForeColor = Color.FromArgb(200, 200, 200);
        lblFolder.Location = new Point(16, 20);
        lblFolder.Name = "lblFolder";
        lblFolder.Size = new Size(85, 23);
        lblFolder.TabIndex = 2;
        lblFolder.Text = "Thư mục:";
        // 
        // pnlTop
        // 
        pnlTop.BackColor = Color.FromArgb(37, 37, 38);
        pnlTop.BorderStyle = BorderStyle.FixedSingle;
        pnlTop.Controls.Add(lblTitle);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Name = "pnlTop";
        pnlTop.Size = new Size(700, 52);
        pnlTop.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(0, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(698, 50);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Chuẩn Bị Dataset — Grayscale";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlBottom
        // 
        pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
        pnlBottom.BorderStyle = BorderStyle.FixedSingle;
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnPrepare);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 290);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Padding = new Padding(16, 13, 16, 13);
        pnlBottom.Size = new Size(700, 70);
        pnlBottom.TabIndex = 3;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Left;
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
        lblStatus.Location = new Point(192, 21);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(181, 23);
        lblStatus.TabIndex = 1;
        lblStatus.Text = "● Chưa chọn thư mục";
        // 
        // btnPrepare
        // 
        btnPrepare.Anchor = AnchorStyles.Left;
        btnPrepare.BackColor = Color.FromArgb(16, 185, 129);
        btnPrepare.Cursor = Cursors.Hand;
        btnPrepare.Enabled = false;
        btnPrepare.FlatAppearance.BorderSize = 0;
        btnPrepare.FlatAppearance.MouseDownBackColor = Color.FromArgb(10, 140, 100);
        btnPrepare.FlatAppearance.MouseOverBackColor = Color.FromArgb(13, 160, 115);
        btnPrepare.FlatStyle = FlatStyle.Flat;
        btnPrepare.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnPrepare.ForeColor = Color.White;
        btnPrepare.Location = new Point(16, 13);
        btnPrepare.Name = "btnPrepare";
        btnPrepare.Size = new Size(160, 44);
        btnPrepare.TabIndex = 0;
        btnPrepare.Text = "Chuẩn bị";
        btnPrepare.UseVisualStyleBackColor = false;
        btnPrepare.Click += btnPrepare_Click;
        // 
        // frmPrepareDataset
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(700, 360);
        Controls.Add(pnlMain);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "frmPrepareDataset";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Chuẩn Bị Dataset — UTT.ShapesDetector";
        pnlMain.ResumeLayout(false);
        pnlCenter.ResumeLayout(false);
        pnlFolderSelect.ResumeLayout(false);
        pnlFolderSelect.PerformLayout();
        pnlTop.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }

    private Panel   pnlMain         = null!;
    private Panel   pnlTop          = null!;
    private Label   lblTitle        = null!;
    private Panel   pnlFolderSelect = null!;
    private Label   lblFolder       = null!;
    private TextBox txtFolderPath   = null!;
    private Button  btnBrowse       = null!;
    private Panel   pnlCenter       = null!;
    private ProgressBar progressBar = null!;
    private Label   lblProgress     = null!;
    private Panel   pnlBottom       = null!;
    private Button  btnPrepare      = null!;
    private Label   lblStatus       = null!;
}

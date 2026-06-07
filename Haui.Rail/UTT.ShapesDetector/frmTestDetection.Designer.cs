using UTT.ShapesDetector.Controls;

namespace UTT.ShapesDetector;

partial class frmTestDetection
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
        pnlResultContainer = new Panel();
        panelResult = new DetectionPanel();
        lblResult = new Label();
        pnlOriginalContainer = new Panel();
        panelOriginal = new DetectionPanel();
        lblOriginal = new Label();
        pnlBottom = new Panel();
        lblStatus = new Label();
        btnDetect = new Button();
        btnChooseImage = new Button();
        pnlMain.SuspendLayout();
        pnlCenter.SuspendLayout();
        pnlResultContainer.SuspendLayout();
        pnlOriginalContainer.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();
        // 
        // pnlMain
        // 
        pnlMain.BackColor = Color.FromArgb(30, 30, 30);
        pnlMain.Controls.Add(pnlCenter);
        pnlMain.Controls.Add(pnlBottom);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 0);
        pnlMain.Margin = new Padding(3, 2, 3, 2);
        pnlMain.Name = "pnlMain";
        pnlMain.Size = new Size(1050, 525);
        pnlMain.TabIndex = 0;
        // 
        // pnlCenter
        // 
        pnlCenter.BackColor = Color.FromArgb(30, 30, 30);
        pnlCenter.Controls.Add(pnlResultContainer);
        pnlCenter.Controls.Add(pnlOriginalContainer);
        pnlCenter.Dock = DockStyle.Fill;
        pnlCenter.Location = new Point(0, 0);
        pnlCenter.Margin = new Padding(3, 2, 3, 2);
        pnlCenter.Name = "pnlCenter";
        pnlCenter.Size = new Size(1050, 472);
        pnlCenter.TabIndex = 0;
        // 
        // pnlResultContainer
        // 
        pnlResultContainer.BackColor = Color.FromArgb(30, 30, 30);
        pnlResultContainer.BorderStyle = BorderStyle.FixedSingle;
        pnlResultContainer.Controls.Add(panelResult);
        pnlResultContainer.Controls.Add(lblResult);
        pnlResultContainer.Dock = DockStyle.Fill;
        pnlResultContainer.Location = new Point(521, 0);
        pnlResultContainer.Margin = new Padding(3, 2, 3, 2);
        pnlResultContainer.Name = "pnlResultContainer";
        pnlResultContainer.Size = new Size(529, 472);
        pnlResultContainer.TabIndex = 1;
        // 
        // panelResult
        // 
        panelResult.BackColor = Color.FromArgb(20, 20, 20);
        panelResult.Dock = DockStyle.Fill;
        panelResult.Location = new Point(0, 30);
        panelResult.Margin = new Padding(0);
        panelResult.Name = "panelResult";
        panelResult.Padding = new Padding(4, 4, 4, 4);
        panelResult.Size = new Size(527, 440);
        panelResult.TabIndex = 0;
        // 
        // lblResult
        // 
        lblResult.BackColor = Color.FromArgb(37, 37, 38);
        lblResult.BorderStyle = BorderStyle.FixedSingle;
        lblResult.Dock = DockStyle.Top;
        lblResult.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        lblResult.ForeColor = Color.White;
        lblResult.Location = new Point(0, 0);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(527, 30);
        lblResult.TabIndex = 1;
        lblResult.Text = "🔍  Kết quả nhận diện";
        lblResult.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlOriginalContainer
        // 
        pnlOriginalContainer.BackColor = Color.FromArgb(30, 30, 30);
        pnlOriginalContainer.BorderStyle = BorderStyle.FixedSingle;
        pnlOriginalContainer.Controls.Add(panelOriginal);
        pnlOriginalContainer.Controls.Add(lblOriginal);
        pnlOriginalContainer.Dock = DockStyle.Left;
        pnlOriginalContainer.Location = new Point(0, 0);
        pnlOriginalContainer.Margin = new Padding(3, 2, 3, 2);
        pnlOriginalContainer.Name = "pnlOriginalContainer";
        pnlOriginalContainer.Size = new Size(521, 472);
        pnlOriginalContainer.TabIndex = 0;
        // 
        // panelOriginal
        // 
        panelOriginal.BackColor = Color.FromArgb(20, 20, 20);
        panelOriginal.Dock = DockStyle.Fill;
        panelOriginal.Location = new Point(0, 30);
        panelOriginal.Margin = new Padding(0);
        panelOriginal.Name = "panelOriginal";
        panelOriginal.Padding = new Padding(4, 4, 4, 4);
        panelOriginal.Size = new Size(519, 440);
        panelOriginal.TabIndex = 0;
        // 
        // lblOriginal
        // 
        lblOriginal.BackColor = Color.FromArgb(37, 37, 38);
        lblOriginal.BorderStyle = BorderStyle.FixedSingle;
        lblOriginal.Dock = DockStyle.Top;
        lblOriginal.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        lblOriginal.ForeColor = Color.White;
        lblOriginal.Location = new Point(0, 0);
        lblOriginal.Name = "lblOriginal";
        lblOriginal.Size = new Size(519, 30);
        lblOriginal.TabIndex = 1;
        lblOriginal.Text = "🖼  Ảnh gốc";
        lblOriginal.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlBottom
        // 
        pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
        pnlBottom.BorderStyle = BorderStyle.FixedSingle;
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnDetect);
        pnlBottom.Controls.Add(btnChooseImage);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 472);
        pnlBottom.Margin = new Padding(3, 2, 3, 2);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Padding = new Padding(14, 10, 14, 10);
        pnlBottom.Size = new Size(1050, 53);
        pnlBottom.TabIndex = 1;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Left;
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
        lblStatus.Location = new Point(326, 16);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(113, 17);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "● Chưa chọn ảnh";
        // 
        // btnDetect
        // 
        btnDetect.Anchor = AnchorStyles.Left;
        btnDetect.BackColor = Color.FromArgb(16, 185, 129);
        btnDetect.Cursor = Cursors.Hand;
        btnDetect.Enabled = false;
        btnDetect.FlatAppearance.BorderSize = 0;
        btnDetect.FlatAppearance.MouseDownBackColor = Color.FromArgb(10, 140, 100);
        btnDetect.FlatAppearance.MouseOverBackColor = Color.FromArgb(13, 160, 115);
        btnDetect.FlatStyle = FlatStyle.Flat;
        btnDetect.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnDetect.ForeColor = Color.White;
        btnDetect.Location = new Point(168, 10);
        btnDetect.Margin = new Padding(3, 2, 3, 2);
        btnDetect.Name = "btnDetect";
        btnDetect.Size = new Size(140, 33);
        btnDetect.TabIndex = 1;
        btnDetect.Text = "🔍  Nhận diện";
        btnDetect.UseVisualStyleBackColor = false;
        btnDetect.Click += btnDetect_Click;
        // 
        // btnChooseImage
        // 
        btnChooseImage.Anchor = AnchorStyles.Left;
        btnChooseImage.BackColor = Color.FromArgb(28, 151, 234);
        btnChooseImage.Cursor = Cursors.Hand;
        btnChooseImage.FlatAppearance.BorderSize = 0;
        btnChooseImage.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170);
        btnChooseImage.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 210);
        btnChooseImage.FlatStyle = FlatStyle.Flat;
        btnChooseImage.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnChooseImage.ForeColor = Color.White;
        btnChooseImage.Location = new Point(14, 10);
        btnChooseImage.Margin = new Padding(3, 2, 3, 2);
        btnChooseImage.Name = "btnChooseImage";
        btnChooseImage.Size = new Size(140, 33);
        btnChooseImage.TabIndex = 0;
        btnChooseImage.Text = "📂  Chọn ảnh";
        btnChooseImage.UseVisualStyleBackColor = false;
        btnChooseImage.Click += btnChooseImage_Click;
        // 
        // frmTestDetection
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(1050, 525);
        Controls.Add(pnlMain);
        Margin = new Padding(3, 2, 3, 2);
        MinimumSize = new Size(790, 460);
        Name = "frmTestDetection";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "\U0001f9ea Test Nhận Diện — UTT.ShapesDetector";
        pnlMain.ResumeLayout(false);
        pnlCenter.ResumeLayout(false);
        pnlResultContainer.ResumeLayout(false);
        pnlOriginalContainer.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }

    private Panel pnlMain;
    private Panel pnlCenter;
    private Panel pnlOriginalContainer;
    private Label lblOriginal;
    private DetectionPanel panelOriginal;
    private Panel pnlResultContainer;
    private Label lblResult;
    private DetectionPanel panelResult;
    private Panel pnlBottom;
    private Button btnChooseImage;
    private Button btnDetect;
    private Label lblStatus;
}

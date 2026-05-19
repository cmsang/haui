using Haui.ShapesDetector.Controls;

namespace Haui.ShapesDetector;

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
        pnlMain               = new Panel();
        pnlCenter             = new Panel();
        pnlOriginalContainer  = new Panel();
        lblOriginal           = new Label();
        panelOriginal         = new DetectionPanel();
        pnlResultContainer    = new Panel();
        lblResult             = new Label();
        panelResult           = new DetectionPanel();
        pnlBottom             = new Panel();
        btnChooseImage        = new Button();
        btnDetect             = new Button();
        lblStatus             = new Label();

        pnlMain.SuspendLayout();
        pnlCenter.SuspendLayout();
        pnlOriginalContainer.SuspendLayout();
        pnlResultContainer.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // ── pnlMain ────────────────────────────────────────────────────────────
        pnlMain.BackColor = Color.FromArgb(30, 30, 30);
        pnlMain.Controls.Add(pnlCenter);
        pnlMain.Controls.Add(pnlBottom);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 0);
        pnlMain.Name = "pnlMain";
        pnlMain.Size = new Size(1200, 700);
        pnlMain.TabIndex = 0;

        // ── pnlCenter ──────────────────────────────────────────────────────────
        pnlCenter.BackColor = Color.FromArgb(30, 30, 30);
        pnlCenter.Controls.Add(pnlResultContainer);
        pnlCenter.Controls.Add(pnlOriginalContainer);
        pnlCenter.Dock = DockStyle.Fill;
        pnlCenter.Location = new Point(0, 0);
        pnlCenter.Name = "pnlCenter";
        pnlCenter.Size = new Size(1200, 630);
        pnlCenter.TabIndex = 0;

        // ── pnlOriginalContainer (left) ────────────────────────────────────────
        pnlOriginalContainer.BackColor = Color.FromArgb(30, 30, 30);
        pnlOriginalContainer.BorderStyle = BorderStyle.FixedSingle;
        pnlOriginalContainer.Controls.Add(panelOriginal);
        pnlOriginalContainer.Controls.Add(lblOriginal);
        pnlOriginalContainer.Dock = DockStyle.Left;
        pnlOriginalContainer.Location = new Point(0, 0);
        pnlOriginalContainer.Name = "pnlOriginalContainer";
        pnlOriginalContainer.Size = new Size(595, 630);
        pnlOriginalContainer.TabIndex = 0;

        // ── lblOriginal ────────────────────────────────────────────────────────
        lblOriginal.BackColor = Color.FromArgb(37, 37, 38);
        lblOriginal.BorderStyle = BorderStyle.FixedSingle;
        lblOriginal.Dock = DockStyle.Top;
        lblOriginal.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        lblOriginal.ForeColor = Color.White;
        lblOriginal.Location = new Point(0, 0);
        lblOriginal.Name = "lblOriginal";
        lblOriginal.Size = new Size(593, 40);
        lblOriginal.TabIndex = 1;
        lblOriginal.Text = "🖼  Ảnh gốc";
        lblOriginal.TextAlign = ContentAlignment.MiddleCenter;

        // ── panelOriginal ──────────────────────────────────────────────────────
        panelOriginal.BackColor = Color.FromArgb(20, 20, 20);
        panelOriginal.Dock = DockStyle.Fill;
        panelOriginal.Location = new Point(0, 40);
        panelOriginal.Margin = new Padding(0);
        panelOriginal.Name = "panelOriginal";
        panelOriginal.Padding = new Padding(5);
        panelOriginal.Size = new Size(593, 588);
        panelOriginal.TabIndex = 0;

        // ── pnlResultContainer (right, fills remaining) ────────────────────────
        pnlResultContainer.BackColor = Color.FromArgb(30, 30, 30);
        pnlResultContainer.BorderStyle = BorderStyle.FixedSingle;
        pnlResultContainer.Controls.Add(panelResult);
        pnlResultContainer.Controls.Add(lblResult);
        pnlResultContainer.Dock = DockStyle.Fill;
        pnlResultContainer.Location = new Point(595, 0);
        pnlResultContainer.Name = "pnlResultContainer";
        pnlResultContainer.Size = new Size(605, 630);
        pnlResultContainer.TabIndex = 1;

        // ── lblResult ──────────────────────────────────────────────────────────
        lblResult.BackColor = Color.FromArgb(37, 37, 38);
        lblResult.BorderStyle = BorderStyle.FixedSingle;
        lblResult.Dock = DockStyle.Top;
        lblResult.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
        lblResult.ForeColor = Color.White;
        lblResult.Location = new Point(0, 0);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(603, 40);
        lblResult.TabIndex = 1;
        lblResult.Text = "🔍  Kết quả nhận diện";
        lblResult.TextAlign = ContentAlignment.MiddleCenter;

        // ── panelResult ────────────────────────────────────────────────────────
        panelResult.BackColor = Color.FromArgb(20, 20, 20);
        panelResult.Dock = DockStyle.Fill;
        panelResult.Location = new Point(0, 40);
        panelResult.Margin = new Padding(0);
        panelResult.Name = "panelResult";
        panelResult.Padding = new Padding(5);
        panelResult.Size = new Size(603, 588);
        panelResult.TabIndex = 0;

        // ── pnlBottom ──────────────────────────────────────────────────────────
        pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
        pnlBottom.BorderStyle = BorderStyle.FixedSingle;
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Controls.Add(btnDetect);
        pnlBottom.Controls.Add(btnChooseImage);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 630);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Padding = new Padding(16, 13, 16, 13);
        pnlBottom.Size = new Size(1200, 70);
        pnlBottom.TabIndex = 1;

        // ── btnChooseImage ─────────────────────────────────────────────────────
        btnChooseImage.Anchor = AnchorStyles.Left;
        btnChooseImage.BackColor = Color.FromArgb(28, 151, 234);
        btnChooseImage.Cursor = Cursors.Hand;
        btnChooseImage.FlatAppearance.BorderSize = 0;
        btnChooseImage.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 170);
        btnChooseImage.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 210);
        btnChooseImage.FlatStyle = FlatStyle.Flat;
        btnChooseImage.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnChooseImage.ForeColor = Color.White;
        btnChooseImage.Location = new Point(16, 13);
        btnChooseImage.Name = "btnChooseImage";
        btnChooseImage.Size = new Size(160, 44);
        btnChooseImage.TabIndex = 0;
        btnChooseImage.Text = "📂  Chọn ảnh";
        btnChooseImage.UseVisualStyleBackColor = false;
        btnChooseImage.Click += btnChooseImage_Click;

        // ── btnDetect ──────────────────────────────────────────────────────────
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
        btnDetect.Location = new Point(192, 13);
        btnDetect.Name = "btnDetect";
        btnDetect.Size = new Size(160, 44);
        btnDetect.TabIndex = 1;
        btnDetect.Text = "🔍  Nhận diện";
        btnDetect.UseVisualStyleBackColor = false;
        btnDetect.Click += btnDetect_Click;

        // ── lblStatus ──────────────────────────────────────────────────────────
        lblStatus.Anchor = AnchorStyles.Left;
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        lblStatus.ForeColor = Color.FromArgb(160, 160, 160);
        lblStatus.Location = new Point(372, 21);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 2;
        lblStatus.Text = "● Chưa chọn ảnh";

        // ── frmTestDetection ───────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 30);
        ClientSize = new Size(1200, 700);
        Controls.Add(pnlMain);
        MinimumSize = new Size(900, 600);
        Name = "frmTestDetection";
        StartPosition = FormStartPosition.CenterParent;
        Text = "🧪 Test Nhận Diện — Haui.ShapesDetector";

        pnlMain.ResumeLayout(false);
        pnlCenter.ResumeLayout(false);
        pnlOriginalContainer.ResumeLayout(false);
        pnlResultContainer.ResumeLayout(false);
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

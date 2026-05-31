namespace Haui.GarlicDetector;

partial class frmRegionSelector
{
    private System.ComponentModel.IContainer components = null;

    private PictureBox      picSnapshot;
    private Panel           pnlBottom;
    private FlowLayoutPanel flpButtons;
    private Button          btnConfirm;
    private Button          btnClear;
    private Button          btnCancel;
    private Label           lblHint;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components  = new System.ComponentModel.Container();

        picSnapshot = new PictureBox();
        pnlBottom   = new Panel();
        flpButtons  = new FlowLayoutPanel();
        btnConfirm  = new Button();
        btnClear    = new Button();
        btnCancel   = new Button();
        lblHint     = new Label();

        ((System.ComponentModel.ISupportInitialize)picSnapshot).BeginInit();
        pnlBottom.SuspendLayout();
        flpButtons.SuspendLayout();
        SuspendLayout();

        // ── picSnapshot ────────────────────────────────────────────────────────
        picSnapshot.Name      = "picSnapshot";
        picSnapshot.Dock      = DockStyle.Fill;
        picSnapshot.SizeMode  = PictureBoxSizeMode.Zoom;
        picSnapshot.BackColor = Color.Black;
        picSnapshot.Cursor    = Cursors.Cross;

        // ── pnlBottom ──────────────────────────────────────────────────────────
        pnlBottom.Name      = "pnlBottom";
        pnlBottom.Dock      = DockStyle.Bottom;
        pnlBottom.Height    = 50;
        pnlBottom.BackColor = Color.FromArgb(45, 45, 48);
        pnlBottom.Controls.Add(lblHint);
        pnlBottom.Controls.Add(flpButtons);

        // lblHint – neo trái
        lblHint.Name      = "lblHint";
        lblHint.Text      = "Kéo chuột để chọn vùng nhận diện. Nhấn Xác nhận để lưu.";
        lblHint.AutoSize  = true;
        lblHint.ForeColor = Color.LightGray;
        lblHint.Anchor    = AnchorStyles.Left | AnchorStyles.Top;
        lblHint.Location  = new Point(8, 16);

        // flpButtons – FlowLayout neo phải, không cần biết kích thước parent lúc init
        flpButtons.Name          = "flpButtons";
        flpButtons.Dock          = DockStyle.Right;
        flpButtons.FlowDirection = FlowDirection.RightToLeft;
        flpButtons.WrapContents  = false;
        flpButtons.AutoSize      = true;
        flpButtons.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
        flpButtons.Padding       = new Padding(0, 9, 8, 0);
        flpButtons.BackColor     = Color.Transparent;
        // Thêm theo thứ tự: Xác nhận (ngoài cùng phải), Xóa vùng, Hủy
        flpButtons.Controls.Add(btnConfirm);
        flpButtons.Controls.Add(btnClear);
        flpButtons.Controls.Add(btnCancel);

        // btnConfirm
        btnConfirm.Name                      = "btnConfirm";
        btnConfirm.Text                      = "✔ Xác nhận";
        btnConfirm.Width                     = 110;
        btnConfirm.Height                    = 30;
        btnConfirm.Margin                    = new Padding(4, 0, 0, 0);
        btnConfirm.BackColor                 = Color.FromArgb(0, 122, 204);
        btnConfirm.ForeColor                 = Color.White;
        btnConfirm.FlatStyle                 = FlatStyle.Flat;
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Enabled                   = false;
        btnConfirm.Click                    += btnConfirm_Click;

        // btnClear
        btnClear.Name                      = "btnClear";
        btnClear.Text                      = "✖ Xóa vùng";
        btnClear.Width                     = 105;
        btnClear.Height                    = 30;
        btnClear.Margin                    = new Padding(4, 0, 0, 0);
        btnClear.BackColor                 = Color.FromArgb(120, 30, 30);
        btnClear.ForeColor                 = Color.White;
        btnClear.FlatStyle                 = FlatStyle.Flat;
        btnClear.FlatAppearance.BorderSize = 0;
        btnClear.Click                    += btnClear_Click;

        // btnCancel
        btnCancel.Name                      = "btnCancel";
        btnCancel.Text                      = "Hủy";
        btnCancel.Width                     = 75;
        btnCancel.Height                    = 30;
        btnCancel.Margin                    = new Padding(4, 0, 0, 0);
        btnCancel.BackColor                 = Color.FromArgb(60, 60, 60);
        btnCancel.ForeColor                 = Color.White;
        btnCancel.FlatStyle                 = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click                    += btnCancel_Click;

        // ── Form ───────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(850, 540);
        MinimumSize         = new Size(640, 400);
        Name                = "frmRegionSelector";
        Text                = "Chọn vùng nhận diện";
        StartPosition       = FormStartPosition.CenterParent;
        BackColor           = Color.FromArgb(30, 30, 30);
        ForeColor           = Color.White;
        Font                = new Font("Segoe UI", 9F);

        Controls.Add(picSnapshot);
        Controls.Add(pnlBottom);

        flpButtons.ResumeLayout(false);
        flpButtons.PerformLayout();
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picSnapshot).EndInit();
        ResumeLayout(false);
    }
}

namespace Haui.ShapesDetector
{
    partial class frmSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpDetection       = new GroupBox();
            lblConfidenceTitle = new Label();
            trConfidence       = new TrackBar();
            lblConfidenceValue = new Label();
            lblMin             = new Label();
            lblMax             = new Label();
            btnSave            = new Button();
            btnCancel          = new Button();

            grpDetection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trConfidence).BeginInit();
            SuspendLayout();

            // ── grpDetection ──────────────────────────────────────────────────
            grpDetection.Controls.Add(lblConfidenceTitle);
            grpDetection.Controls.Add(trConfidence);
            grpDetection.Controls.Add(lblConfidenceValue);
            grpDetection.Controls.Add(lblMin);
            grpDetection.Controls.Add(lblMax);
            grpDetection.ForeColor = Color.White;
            grpDetection.Location  = new Point(12, 12);
            grpDetection.Name      = "grpDetection";
            grpDetection.Size      = new Size(400, 118);
            grpDetection.TabIndex  = 0;
            grpDetection.TabStop   = false;
            grpDetection.Text      = "Detection";

            // ── lblConfidenceTitle ────────────────────────────────────────────
            lblConfidenceTitle.AutoSize  = true;
            lblConfidenceTitle.Font      = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            lblConfidenceTitle.ForeColor = Color.White;
            lblConfidenceTitle.Location  = new Point(8, 22);
            lblConfidenceTitle.Name      = "lblConfidenceTitle";
            lblConfidenceTitle.TabIndex  = 0;
            lblConfidenceTitle.Text      = "Confidence Threshold";

            // ── trConfidence ──────────────────────────────────────────────────
            trConfidence.Location      = new Point(8, 45);
            trConfidence.Maximum       = 100;
            trConfidence.Minimum       = 1;
            trConfidence.Name          = "trConfidence";
            trConfidence.Size          = new Size(318, 45);
            trConfidence.TabIndex      = 1;
            trConfidence.TickFrequency = 5;
            trConfidence.Value         = 75;
            trConfidence.ValueChanged += trConfidence_ValueChanged;

            // ── lblConfidenceValue ────────────────────────────────────────────
            lblConfidenceValue.AutoSize  = false;
            lblConfidenceValue.Font      = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblConfidenceValue.ForeColor = Color.LimeGreen;
            lblConfidenceValue.Location  = new Point(332, 48);
            lblConfidenceValue.Name      = "lblConfidenceValue";
            lblConfidenceValue.Size      = new Size(60, 28);
            lblConfidenceValue.TabIndex  = 2;
            lblConfidenceValue.Text      = "75%";
            lblConfidenceValue.TextAlign = ContentAlignment.MiddleCenter;

            // ── lblMin ────────────────────────────────────────────────────────
            lblMin.AutoSize  = true;
            lblMin.ForeColor = Color.DimGray;
            lblMin.Location  = new Point(8, 95);
            lblMin.Name      = "lblMin";
            lblMin.TabIndex  = 3;
            lblMin.Text      = "1%";

            // ── lblMax ────────────────────────────────────────────────────────
            lblMax.AutoSize  = true;
            lblMax.ForeColor = Color.DimGray;
            lblMax.Location  = new Point(291, 95);
            lblMax.Name      = "lblMax";
            lblMax.TabIndex  = 4;
            lblMax.Text      = "100%";

            // ── btnSave ───────────────────────────────────────────────────────
            btnSave.BackColor                = Color.FromArgb(0, 122, 204);
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle                = FlatStyle.Flat;
            btnSave.Font                     = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            btnSave.ForeColor                = Color.White;
            btnSave.Location                 = new Point(254, 142);
            btnSave.Name                     = "btnSave";
            btnSave.Size                     = new Size(75, 28);
            btnSave.TabIndex                 = 1;
            btnSave.Text                     = "Lưu";
            btnSave.UseVisualStyleBackColor  = false;
            btnSave.Click                   += btnSave_Click;

            // ── btnCancel ─────────────────────────────────────────────────────
            btnCancel.FlatStyle              = FlatStyle.Flat;
            btnCancel.ForeColor              = Color.White;
            btnCancel.Font                   = new Font("Segoe UI", 9.75F);
            btnCancel.Location               = new Point(337, 142);
            btnCancel.Name                   = "btnCancel";
            btnCancel.Size                   = new Size(75, 28);
            btnCancel.TabIndex               = 2;
            btnCancel.Text                   = "Hủy";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click                 += btnCancel_Click;

            // ── frmSettings ───────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            BackColor           = Color.FromArgb(30, 30, 30);
            ClientSize          = new Size(424, 182);
            Controls.Add(grpDetection);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            Name                = "frmSettings";
            StartPosition       = FormStartPosition.CenterParent;
            Text                = "Settings";
            Load               += frmSettings_Load;

            grpDetection.ResumeLayout(false);
            grpDetection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trConfidence).EndInit();
            ResumeLayout(false);
        }

        private GroupBox grpDetection;
        private Label    lblConfidenceTitle;
        private TrackBar trConfidence;
        private Label    lblConfidenceValue;
        private Label    lblMin;
        private Label    lblMax;
        private Button   btnSave;
        private Button   btnCancel;
    }
}

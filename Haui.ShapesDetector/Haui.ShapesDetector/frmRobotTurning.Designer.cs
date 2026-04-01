namespace Haui.ShapesDetector
{
    partial class frmRobotTurning
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            trJ1 = new TrackBar();
            trJ2 = new TrackBar();
            trJ4 = new TrackBar();
            trJ3 = new TrackBar();
            txtPointLocate = new TextBox();
            cboPos = new ComboBox();
            groupBox1 = new GroupBox();
            label4 = new Label();
            label5 = new Label();
            btnCancel = new Button();
            btnSave = new Button();
            J1 = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)trJ1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trJ2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trJ4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trJ3).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // trJ1
            // 
            trJ1.Location = new Point(62, 24);
            trJ1.Maximum = 180;
            trJ1.Name = "trJ1";
            trJ1.Size = new Size(413, 45);
            trJ1.TabIndex = 0;
            trJ1.ValueChanged += tr_ValueChanged;
            // 
            // trJ2
            // 
            trJ2.Location = new Point(62, 82);
            trJ2.Maximum = 180;
            trJ2.Name = "trJ2";
            trJ2.Size = new Size(413, 45);
            trJ2.TabIndex = 1;
            trJ2.ValueChanged += tr_ValueChanged;
            // 
            // trJ4
            // 
            trJ4.Location = new Point(62, 198);
            trJ4.Maximum = 180;
            trJ4.Name = "trJ4";
            trJ4.Size = new Size(413, 45);
            trJ4.TabIndex = 3;
            trJ4.ValueChanged += tr_ValueChanged;
            // 
            // trJ3
            // 
            trJ3.Location = new Point(62, 140);
            trJ3.Maximum = 180;
            trJ3.Name = "trJ3";
            trJ3.Size = new Size(413, 45);
            trJ3.TabIndex = 2;
            trJ3.ValueChanged += tr_ValueChanged;
            // 
            // txtPointLocate
            // 
            txtPointLocate.Location = new Point(573, 63);
            txtPointLocate.Name = "txtPointLocate";
            txtPointLocate.Size = new Size(209, 23);
            txtPointLocate.TabIndex = 5;
            txtPointLocate.TextChanged += txtPointLocate_TextChanged;
            // 
            // cboPos
            // 
            cboPos.FormattingEnabled = true;
            cboPos.Items.AddRange(new object[] { "Pickup", "Material 1", "Material 2", "Material 3", "Material 4", "Material 5", "Material 6" });
            cboPos.Location = new Point(92, 22);
            cboPos.Name = "cboPos";
            cboPos.Size = new Size(209, 23);
            cboPos.TabIndex = 6;
            cboPos.SelectedIndexChanged += cboPos_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(btnCancel);
            groupBox1.Controls.Add(btnSave);
            groupBox1.Controls.Add(cboPos);
            groupBox1.Location = new Point(481, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(307, 137);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Setting infor";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(6, 54);
            label4.Name = "label4";
            label4.Size = new Size(66, 17);
            label4.TabIndex = 12;
            label4.Text = "Pos Value";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
            label5.Location = new Point(6, 25);
            label5.Name = "label5";
            label5.Size = new Size(57, 17);
            label5.TabIndex = 12;
            label5.Text = "Position";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(226, 108);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Hủy";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(145, 108);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 7;
            btnSave.Text = "Lưu";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // J1
            // 
            J1.AutoSize = true;
            J1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            J1.Location = new Point(19, 23);
            J1.Name = "J1";
            J1.Size = new Size(25, 20);
            J1.TabIndex = 8;
            J1.Text = "J1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(19, 82);
            label1.Name = "label1";
            label1.Size = new Size(25, 20);
            label1.TabIndex = 9;
            label1.Text = "J2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(19, 140);
            label2.Name = "label2";
            label2.Size = new Size(25, 20);
            label2.TabIndex = 10;
            label2.Text = "J3";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(19, 198);
            label3.Name = "label3";
            label3.Size = new Size(25, 20);
            label3.TabIndex = 11;
            label3.Text = "J4";
            // 
            // frmRobotTurning
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 259);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(J1);
            Controls.Add(txtPointLocate);
            Controls.Add(groupBox1);
            Controls.Add(trJ4);
            Controls.Add(trJ3);
            Controls.Add(trJ2);
            Controls.Add(trJ1);
            Name = "frmRobotTurning";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmRobotTurning";
            Load += frmRobotTurning_Load;
            ((System.ComponentModel.ISupportInitialize)trJ1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trJ2).EndInit();
            ((System.ComponentModel.ISupportInitialize)trJ4).EndInit();
            ((System.ComponentModel.ISupportInitialize)trJ3).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar trJ1;
        private TrackBar trJ2;
        private TrackBar trJ4;
        private TrackBar trJ3;
        private TextBox txtPointLocate;
        private ComboBox cboPos;
        private GroupBox groupBox1;
        private Button btnCancel;
        private Button btnSave;
        private Label label4;
        private Label label5;
        private Label J1;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}
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
            btnHome = new Button();
            label6 = new Label();
            txtChangeValue = new TextBox();
            label4 = new Label();
            label5 = new Label();
            btnCancel = new Button();
            btnSave = new Button();
            J1 = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnUp1 = new Button();
            btnDown1 = new Button();
            btnDown2 = new Button();
            btnUp2 = new Button();
            btnDown3 = new Button();
            btnUp3 = new Button();
            btnDown4 = new Button();
            btnUp4 = new Button();
            btnTest = new Button();
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
            trJ1.Value = 90;
            trJ1.ValueChanged += tr_ValueChanged;
            // 
            // trJ2
            // 
            trJ2.Location = new Point(62, 82);
            trJ2.Maximum = 180;
            trJ2.Name = "trJ2";
            trJ2.Size = new Size(413, 45);
            trJ2.TabIndex = 1;
            trJ2.Value = 90;
            trJ2.ValueChanged += tr_ValueChanged;
            // 
            // trJ4
            // 
            trJ4.Location = new Point(62, 198);
            trJ4.Maximum = 180;
            trJ4.Name = "trJ4";
            trJ4.Size = new Size(413, 45);
            trJ4.TabIndex = 3;
            trJ4.Value = 90;
            trJ4.ValueChanged += tr_ValueChanged;
            // 
            // trJ3
            // 
            trJ3.Location = new Point(62, 140);
            trJ3.Maximum = 180;
            trJ3.Name = "trJ3";
            trJ3.Size = new Size(413, 45);
            trJ3.TabIndex = 2;
            trJ3.Value = 90;
            trJ3.ValueChanged += tr_ValueChanged;
            // 
            // txtPointLocate
            // 
            txtPointLocate.Location = new Point(687, 63);
            txtPointLocate.Name = "txtPointLocate";
            txtPointLocate.Size = new Size(199, 23);
            txtPointLocate.TabIndex = 5;
            txtPointLocate.TextChanged += txtPointLocate_TextChanged;
            // 
            // cboPos
            // 
            cboPos.FormattingEnabled = true;
            cboPos.Items.AddRange(new object[] { "Pickup", "Material 1", "Material 2", "Material 3", "Material 4", "Material 5", "Material 6" });
            cboPos.Location = new Point(102, 22);
            cboPos.Name = "cboPos";
            cboPos.Size = new Size(199, 23);
            cboPos.TabIndex = 6;
            cboPos.SelectedIndexChanged += cboPos_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnTest);
            groupBox1.Controls.Add(btnHome);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(txtChangeValue);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(btnCancel);
            groupBox1.Controls.Add(btnSave);
            groupBox1.Controls.Add(cboPos);
            groupBox1.ForeColor = Color.White;
            groupBox1.Location = new Point(585, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(307, 192);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Setting infor";
            // 
            // btnHome
            // 
            btnHome.BackColor = Color.FromArgb(0, 122, 204);
            btnHome.FlatStyle = FlatStyle.Flat;
            btnHome.ForeColor = Color.White;
            btnHome.Location = new Point(226, 130);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(75, 25);
            btnHome.TabIndex = 15;
            btnHome.Text = "Home";
            btnHome.UseVisualStyleBackColor = false;
            btnHome.Click += btnHome_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.Location = new Point(6, 81);
            label6.Name = "label6";
            label6.Size = new Size(90, 17);
            label6.TabIndex = 14;
            label6.Text = "Change Value";
            // 
            // txtChangeValue
            // 
            txtChangeValue.Location = new Point(102, 80);
            txtChangeValue.Name = "txtChangeValue";
            txtChangeValue.Size = new Size(199, 23);
            txtChangeValue.TabIndex = 13;
            txtChangeValue.Text = "5";
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
            btnCancel.BackColor = Color.FromArgb(30, 30, 30);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(226, 161);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 25);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(145, 161);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 25);
            btnSave.TabIndex = 7;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
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
            // btnUp1
            // 
            btnUp1.BackColor = Color.Transparent;
            btnUp1.FlatStyle = FlatStyle.Flat;
            btnUp1.ForeColor = Color.White;
            btnUp1.Location = new Point(481, 23);
            btnUp1.Name = "btnUp1";
            btnUp1.Size = new Size(44, 25);
            btnUp1.TabIndex = 15;
            btnUp1.Text = "+";
            btnUp1.UseVisualStyleBackColor = false;
            btnUp1.Click += btnUp_Click;
            // 
            // btnDown1
            // 
            btnDown1.BackColor = Color.Transparent;
            btnDown1.FlatStyle = FlatStyle.Flat;
            btnDown1.ForeColor = Color.Transparent;
            btnDown1.Location = new Point(532, 23);
            btnDown1.Name = "btnDown1";
            btnDown1.Size = new Size(44, 25);
            btnDown1.TabIndex = 16;
            btnDown1.Text = "-";
            btnDown1.UseVisualStyleBackColor = false;
            btnDown1.Click += btnDown_Click;
            // 
            // btnDown2
            // 
            btnDown2.BackColor = Color.Transparent;
            btnDown2.FlatStyle = FlatStyle.Flat;
            btnDown2.ForeColor = Color.Transparent;
            btnDown2.Location = new Point(532, 77);
            btnDown2.Name = "btnDown2";
            btnDown2.Size = new Size(44, 25);
            btnDown2.TabIndex = 18;
            btnDown2.Text = "-";
            btnDown2.UseVisualStyleBackColor = false;
            btnDown2.Click += btnDown_Click;
            // 
            // btnUp2
            // 
            btnUp2.BackColor = Color.Transparent;
            btnUp2.FlatStyle = FlatStyle.Flat;
            btnUp2.ForeColor = Color.White;
            btnUp2.Location = new Point(481, 77);
            btnUp2.Name = "btnUp2";
            btnUp2.Size = new Size(44, 25);
            btnUp2.TabIndex = 17;
            btnUp2.Text = "+";
            btnUp2.UseVisualStyleBackColor = false;
            btnUp2.Click += btnUp_Click;
            // 
            // btnDown3
            // 
            btnDown3.BackColor = Color.Transparent;
            btnDown3.FlatStyle = FlatStyle.Flat;
            btnDown3.ForeColor = Color.Transparent;
            btnDown3.Location = new Point(532, 135);
            btnDown3.Name = "btnDown3";
            btnDown3.Size = new Size(44, 25);
            btnDown3.TabIndex = 20;
            btnDown3.Text = "-";
            btnDown3.UseVisualStyleBackColor = false;
            btnDown3.Click += btnDown_Click;
            // 
            // btnUp3
            // 
            btnUp3.BackColor = Color.Transparent;
            btnUp3.FlatStyle = FlatStyle.Flat;
            btnUp3.ForeColor = Color.White;
            btnUp3.Location = new Point(481, 135);
            btnUp3.Name = "btnUp3";
            btnUp3.Size = new Size(44, 25);
            btnUp3.TabIndex = 19;
            btnUp3.Text = "+";
            btnUp3.UseVisualStyleBackColor = false;
            btnUp3.Click += btnUp_Click;
            // 
            // btnDown4
            // 
            btnDown4.BackColor = Color.Transparent;
            btnDown4.FlatStyle = FlatStyle.Flat;
            btnDown4.ForeColor = Color.Transparent;
            btnDown4.Location = new Point(532, 193);
            btnDown4.Name = "btnDown4";
            btnDown4.Size = new Size(44, 25);
            btnDown4.TabIndex = 22;
            btnDown4.Text = "-";
            btnDown4.UseVisualStyleBackColor = false;
            btnDown4.Click += btnDown_Click;
            // 
            // btnUp4
            // 
            btnUp4.BackColor = Color.Transparent;
            btnUp4.FlatStyle = FlatStyle.Flat;
            btnUp4.ForeColor = Color.White;
            btnUp4.Location = new Point(481, 193);
            btnUp4.Name = "btnUp4";
            btnUp4.Size = new Size(44, 25);
            btnUp4.TabIndex = 21;
            btnUp4.Text = "+";
            btnUp4.UseVisualStyleBackColor = false;
            btnUp4.Click += btnUp_Click;
            // 
            // btnTest
            // 
            btnTest.BackColor = Color.FromArgb(0, 122, 204);
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.ForeColor = Color.White;
            btnTest.Location = new Point(145, 130);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(75, 25);
            btnTest.TabIndex = 16;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.Click += btnTest_Click;
            // 
            // frmRobotTurning
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(900, 256);
            Controls.Add(btnDown4);
            Controls.Add(btnUp4);
            Controls.Add(btnDown3);
            Controls.Add(btnUp3);
            Controls.Add(btnDown2);
            Controls.Add(btnUp2);
            Controls.Add(btnDown1);
            Controls.Add(btnUp1);
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
            ForeColor = SystemColors.Control;
            Name = "frmRobotTurning";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmRobotTurning";
            FormClosing += frmRobotTurning_FormClosing;
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
        private Label label6;
        private TextBox txtChangeValue;
        private Button btnUp1;
        private Button btnDown1;
        private Button btnDown2;
        private Button btnUp2;
        private Button btnDown3;
        private Button btnUp3;
        private Button btnDown4;
        private Button btnUp4;
        private Button btnHome;
        private Button btnTest;
    }
}
using UTT.ShapesDetector.Controls;

namespace UTT.ShapesDetector
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
            pnlCenter = new Panel();
            pnlLeft = new Panel();
            zg = new ZedGraph.ZedGraphControl();
            pnlRight = new Panel();
            dataGridView1 = new DataGridView();
            Time = new DataGridViewTextBoxColumn();
            Value = new DataGridViewTextBoxColumn();
            pnlBottom = new Panel();
            btnStop = new Button();
            btnStart = new Button();
            btnTest = new Button();
            lblStatus = new Label();
            pnlTop = new Panel();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            timerCheckJob = new System.Windows.Forms.Timer(components);
            pnlMain.SuspendLayout();
            pnlCenter.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            pnlBottom.SuspendLayout();
            pnlTop.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(30, 30, 30);
            pnlMain.Controls.Add(pnlCenter);
            pnlMain.Controls.Add(pnlBottom);
            pnlMain.Controls.Add(pnlTop);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1192, 735);
            pnlMain.TabIndex = 0;
            // 
            // pnlCenter
            // 
            pnlCenter.BackColor = Color.FromArgb(30, 30, 30);
            pnlCenter.Controls.Add(pnlLeft);
            pnlCenter.Controls.Add(pnlRight);
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(0, 114);
            pnlCenter.Margin = new Padding(0);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Size = new Size(1192, 561);
            pnlCenter.TabIndex = 1;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(30, 30, 30);
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlLeft.Controls.Add(zg);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Margin = new Padding(0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(859, 561);
            pnlLeft.TabIndex = 0;
            // 
            // zg
            // 
            zg.Location = new Point(-1, -1);
            zg.Margin = new Padding(4, 3, 4, 3);
            zg.Name = "zg";
            zg.ScrollGrace = 0D;
            zg.ScrollMaxX = 0D;
            zg.ScrollMaxY = 0D;
            zg.ScrollMaxY2 = 0D;
            zg.ScrollMinX = 0D;
            zg.ScrollMinY = 0D;
            zg.ScrollMinY2 = 0D;
            zg.Size = new Size(859, 561);
            zg.TabIndex = 0;
            zg.UseExtendedPrintDialog = true;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(45, 45, 48);
            pnlRight.BorderStyle = BorderStyle.FixedSingle;
            pnlRight.Controls.Add(dataGridView1);
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(859, 0);
            pnlRight.Margin = new Padding(0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(333, 561);
            pnlRight.TabIndex = 1;
            // 
            // dataGridView1
            // 
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Time, Value });
            dataGridView1.Location = new Point(-1, -1);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(333, 561);
            dataGridView1.TabIndex = 0;
            // 
            // Time
            // 
            Time.HeaderText = "Thời gian";
            Time.Name = "Time";
            Time.Width = 140;
            // 
            // Value
            // 
            Value.HeaderText = "Độ rung";
            Value.Name = "Value";
            Value.Width = 150;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(37, 37, 38);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Controls.Add(btnStop);
            pnlBottom.Controls.Add(btnStart);
            pnlBottom.Controls.Add(btnTest);
            pnlBottom.Controls.Add(lblStatus);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 675);
            pnlBottom.Margin = new Padding(0);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(14, 11, 14, 11);
            pnlBottom.Size = new Size(1192, 60);
            pnlBottom.TabIndex = 2;
            // 
            // btnStop
            // 
            btnStop.Anchor = AnchorStyles.Left;
            btnStop.BackColor = Color.FromArgb(180, 100, 220);
            btnStop.Cursor = Cursors.Hand;
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.FlatAppearance.MouseDownBackColor = Color.FromArgb(130, 60, 170);
            btnStop.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 80, 200);
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnStop.ForeColor = Color.White;
            btnStop.Location = new Point(318, 11);
            btnStop.Margin = new Padding(10, 0, 0, 0);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(140, 36);
            btnStop.TabIndex = 7;
            btnStop.Text = "\U0001f9ea Stop";
            btnStop.UseVisualStyleBackColor = false;
            btnStop.Click += btnStop_Click;
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Left;
            btnStart.BackColor = Color.FromArgb(180, 100, 220);
            btnStart.Cursor = Cursors.Hand;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.FlatAppearance.MouseDownBackColor = Color.FromArgb(130, 60, 170);
            btnStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 80, 200);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(168, 11);
            btnStart.Margin = new Padding(10, 0, 0, 0);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(140, 36);
            btnStart.TabIndex = 6;
            btnStart.Text = "\U0001f9ea Start";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnTest
            // 
            btnTest.Anchor = AnchorStyles.Left;
            btnTest.BackColor = Color.FromArgb(180, 100, 220);
            btnTest.Cursor = Cursors.Hand;
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.FlatAppearance.MouseDownBackColor = Color.FromArgb(130, 60, 170);
            btnTest.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 80, 200);
            btnTest.FlatStyle = FlatStyle.Flat;
            btnTest.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnTest.ForeColor = Color.White;
            btnTest.Location = new Point(18, 11);
            btnTest.Margin = new Padding(10, 0, 0, 0);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(140, 36);
            btnTest.TabIndex = 5;
            btnTest.Text = "\U0001f9ea Check Image";
            btnTest.UseVisualStyleBackColor = false;
            btnTest.Click += btnTest_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Right;
            lblStatus.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            lblStatus.ForeColor = Color.LimeGreen;
            lblStatus.Location = new Point(1113, 11);
            lblStatus.Margin = new Padding(0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(63, 19);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "● Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(37, 37, 38);
            pnlTop.BorderStyle = BorderStyle.FixedSingle;
            pnlTop.Controls.Add(label3);
            pnlTop.Controls.Add(label2);
            pnlTop.Controls.Add(label1);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(0);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(14, 8, 14, 8);
            pnlTop.Size = new Size(1192, 114);
            pnlTop.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 13.75F, FontStyle.Bold);
            label3.ForeColor = SystemColors.ControlDark;
            label3.Location = new Point(367, 69);
            label3.Name = "label3";
            label3.Size = new Size(458, 25);
            label3.TabIndex = 2;
            label3.Text = "HỆ THỐNG GIÁM SÁT TÌNH TRẠNG RAY TÀU HỎA";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 13.75F, FontStyle.Bold);
            label2.ForeColor = SystemColors.ControlDark;
            label2.Location = new Point(499, 38);
            label2.Name = "label2";
            label2.Size = new Size(210, 25);
            label2.TabIndex = 1;
            label2.Text = "KHOA ĐIỆN - ĐIỆN TỬ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ControlDark;
            label1.Location = new Point(320, 8);
            label1.Name = "label1";
            label1.Size = new Size(562, 30);
            label1.TabIndex = 0;
            label1.Text = "TRƯỜNG ĐẠI HỌC CÔNG NGHỆ GIAO THÔNG VẬN TẢI";
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
            ClientSize = new Size(1192, 735);
            Controls.Add(pnlMain);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(1052, 535);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Object Detection Tool - UTT.ShapesDetector";
            Load += frmMain_Load;
            pnlMain.ResumeLayout(false);
            pnlCenter.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlMain;
        private Panel pnlTop;
        private Label lblStatus;
        private Panel pnlCenter;
        private Panel pnlLeft;
        private Panel pnlRight;
        private Panel pnlBottom;
        private Button btnTest;
        private System.Windows.Forms.Timer timerCheckJob;
        private Label label1;
        private Label label3;
        private Label label2;
        private ComboBox comboBox1;
        private ZedGraph.ZedGraphControl zg;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Time;
        private DataGridViewTextBoxColumn Value;
        private Button btnStop;
        private Button btnStart;
    }
}

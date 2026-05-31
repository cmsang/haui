namespace Haui.GarlicDetector;

partial class frmHistory
{
    private System.ComponentModel.IContainer components = null;

    // ─── Controls ────────────────────────────────────────────────────────────
    private Panel pnlTop;
    private Label lblTitle;
    private Label lblPeriod;

    private Panel pnlButtons;
    private Button btnToday;
    private Button btnYesterday;
    private Button btnThisWeek;
    private Button btnThisMonth;
    private Label lblFrom;
    private DateTimePicker dtpFrom;
    private Label lblTo;
    private DateTimePicker dtpTo;
    private Button btnCustom;
    private Button btnRefresh;
    private Button btnExport;

    private Panel pnlStats;
    private Label label1;
    private Label lblLargeCount;
    private Label label2;
    private Label lblSmallCount;
    private Label label3;
    private Label lblErrorCount;
    private Label label4;
    private Label lblTotalCount;

    private DataGridView dgvHistory;

    private Panel pnlBottom;
    private Button btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        pnlTop = new Panel();
        lblPeriod = new Label();
        lblTitle = new Label();
        pnlButtons = new Panel();
        btnExport = new Button();
        btnRefresh = new Button();
        btnCustom = new Button();
        dtpTo = new DateTimePicker();
        lblTo = new Label();
        dtpFrom = new DateTimePicker();
        lblFrom = new Label();
        btnThisMonth = new Button();
        btnThisWeek = new Button();
        btnYesterday = new Button();
        btnToday = new Button();
        pnlStats = new Panel();
        lblTotalCount = new Label();
        label4 = new Label();
        lblErrorCount = new Label();
        label3 = new Label();
        lblSmallCount = new Label();
        label2 = new Label();
        lblLargeCount = new Label();
        label1 = new Label();
        dgvHistory = new DataGridView();
        pnlBottom = new Panel();
        btnClose = new Button();
        pnlTop.SuspendLayout();
        pnlButtons.SuspendLayout();
        pnlStats.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvHistory).BeginInit();
        pnlBottom.SuspendLayout();
        SuspendLayout();
        // 
        // pnlTop
        // 
        pnlTop.BackColor = Color.FromArgb(45, 45, 48);
        pnlTop.Controls.Add(lblPeriod);
        pnlTop.Controls.Add(lblTitle);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Name = "pnlTop";
        pnlTop.Size = new Size(1095, 60);
        pnlTop.TabIndex = 0;
        // 
        // lblPeriod
        // 
        lblPeriod.Font = new Font("Segoe UI", 10F);
        lblPeriod.ForeColor = Color.LightGray;
        lblPeriod.Location = new Point(15, 35);
        lblPeriod.Name = "lblPeriod";
        lblPeriod.Size = new Size(500, 20);
        lblPeriod.TabIndex = 1;
        lblPeriod.Text = "Hôm nay";
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(15, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(300, 25);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "📊 Lịch sử phân loại tỏi";
        // 
        // pnlButtons
        // 
        pnlButtons.BackColor = SystemColors.Control;
        pnlButtons.Controls.Add(btnExport);
        pnlButtons.Controls.Add(btnRefresh);
        pnlButtons.Controls.Add(btnCustom);
        pnlButtons.Controls.Add(dtpTo);
        pnlButtons.Controls.Add(lblTo);
        pnlButtons.Controls.Add(dtpFrom);
        pnlButtons.Controls.Add(lblFrom);
        pnlButtons.Controls.Add(btnThisMonth);
        pnlButtons.Controls.Add(btnThisWeek);
        pnlButtons.Controls.Add(btnYesterday);
        pnlButtons.Controls.Add(btnToday);
        pnlButtons.Dock = DockStyle.Top;
        pnlButtons.Location = new Point(0, 60);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Padding = new Padding(10);
        pnlButtons.Size = new Size(1095, 50);
        pnlButtons.TabIndex = 1;
        // 
        // btnExport
        // 
        btnExport.BackColor = Color.FromArgb(180, 100, 50);
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.ForeColor = Color.White;
        btnExport.Location = new Point(986, 10);
        btnExport.Name = "btnExport";
        btnExport.Size = new Size(100, 30);
        btnExport.TabIndex = 10;
        btnExport.Text = "📤 Xuất Excel";
        btnExport.UseVisualStyleBackColor = false;
        btnExport.Click += btnExport_Click;
        // 
        // btnRefresh
        // 
        btnRefresh.BackColor = Color.FromArgb(150, 150, 150);
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.ForeColor = Color.White;
        btnRefresh.Location = new Point(896, 10);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(80, 30);
        btnRefresh.TabIndex = 9;
        btnRefresh.Text = "🔄 Làm mới";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // btnCustom
        // 
        btnCustom.BackColor = Color.FromArgb(100, 150, 80);
        btnCustom.FlatAppearance.BorderSize = 0;
        btnCustom.FlatStyle = FlatStyle.Flat;
        btnCustom.ForeColor = Color.White;
        btnCustom.Location = new Point(806, 10);
        btnCustom.Name = "btnCustom";
        btnCustom.Size = new Size(80, 30);
        btnCustom.TabIndex = 8;
        btnCustom.Text = "Tìm kiếm";
        btnCustom.UseVisualStyleBackColor = false;
        btnCustom.Click += btnCustom_Click;
        // 
        // dtpTo
        // 
        dtpTo.Format = DateTimePickerFormat.Short;
        dtpTo.Location = new Point(676, 13);
        dtpTo.Name = "dtpTo";
        dtpTo.Size = new Size(120, 23);
        dtpTo.TabIndex = 7;
        // 
        // lblTo
        // 
        lblTo.ForeColor = Color.Black;
        lblTo.Location = new Point(641, 18);
        lblTo.Name = "lblTo";
        lblTo.Size = new Size(35, 20);
        lblTo.TabIndex = 6;
        lblTo.Text = "Đến:";
        // 
        // dtpFrom
        // 
        dtpFrom.Format = DateTimePickerFormat.Short;
        dtpFrom.Location = new Point(514, 13);
        dtpFrom.Name = "dtpFrom";
        dtpFrom.Size = new Size(120, 23);
        dtpFrom.TabIndex = 5;
        // 
        // lblFrom
        // 
        lblFrom.ForeColor = Color.Black;
        lblFrom.Location = new Point(479, 18);
        lblFrom.Name = "lblFrom";
        lblFrom.Size = new Size(30, 20);
        lblFrom.TabIndex = 4;
        lblFrom.Text = "Từ:";
        // 
        // btnThisMonth
        // 
        btnThisMonth.BackColor = Color.FromArgb(60, 130, 180);
        btnThisMonth.FlatAppearance.BorderSize = 0;
        btnThisMonth.FlatStyle = FlatStyle.Flat;
        btnThisMonth.ForeColor = Color.White;
        btnThisMonth.Location = new Point(340, 10);
        btnThisMonth.Name = "btnThisMonth";
        btnThisMonth.Size = new Size(100, 30);
        btnThisMonth.TabIndex = 3;
        btnThisMonth.Text = "Tháng này";
        btnThisMonth.UseVisualStyleBackColor = false;
        btnThisMonth.Click += btnThisMonth_Click;
        // 
        // btnThisWeek
        // 
        btnThisWeek.BackColor = Color.FromArgb(60, 130, 180);
        btnThisWeek.FlatAppearance.BorderSize = 0;
        btnThisWeek.FlatStyle = FlatStyle.Flat;
        btnThisWeek.ForeColor = Color.White;
        btnThisWeek.Location = new Point(230, 10);
        btnThisWeek.Name = "btnThisWeek";
        btnThisWeek.Size = new Size(100, 30);
        btnThisWeek.TabIndex = 2;
        btnThisWeek.Text = "Tuần này";
        btnThisWeek.UseVisualStyleBackColor = false;
        btnThisWeek.Click += btnThisWeek_Click;
        // 
        // btnYesterday
        // 
        btnYesterday.BackColor = Color.FromArgb(60, 130, 180);
        btnYesterday.FlatAppearance.BorderSize = 0;
        btnYesterday.FlatStyle = FlatStyle.Flat;
        btnYesterday.ForeColor = Color.White;
        btnYesterday.Location = new Point(120, 10);
        btnYesterday.Name = "btnYesterday";
        btnYesterday.Size = new Size(100, 30);
        btnYesterday.TabIndex = 1;
        btnYesterday.Text = "Hôm qua";
        btnYesterday.UseVisualStyleBackColor = false;
        btnYesterday.Click += btnYesterday_Click;
        // 
        // btnToday
        // 
        btnToday.BackColor = Color.FromArgb(60, 130, 180);
        btnToday.FlatAppearance.BorderSize = 0;
        btnToday.FlatStyle = FlatStyle.Flat;
        btnToday.ForeColor = Color.White;
        btnToday.Location = new Point(10, 10);
        btnToday.Name = "btnToday";
        btnToday.Size = new Size(100, 30);
        btnToday.TabIndex = 0;
        btnToday.Text = "Hôm nay";
        btnToday.UseVisualStyleBackColor = false;
        btnToday.Click += btnToday_Click;
        // 
        // pnlStats
        // 
        pnlStats.BackColor = SystemColors.Control;
        pnlStats.Controls.Add(lblTotalCount);
        pnlStats.Controls.Add(label4);
        pnlStats.Controls.Add(lblErrorCount);
        pnlStats.Controls.Add(label3);
        pnlStats.Controls.Add(lblSmallCount);
        pnlStats.Controls.Add(label2);
        pnlStats.Controls.Add(lblLargeCount);
        pnlStats.Controls.Add(label1);
        pnlStats.Dock = DockStyle.Top;
        pnlStats.Location = new Point(0, 110);
        pnlStats.Name = "pnlStats";
        pnlStats.Padding = new Padding(10);
        pnlStats.Size = new Size(1095, 60);
        pnlStats.TabIndex = 2;
        // 
        // lblTotalCount
        // 
        lblTotalCount.AutoSize = true;
        lblTotalCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTotalCount.ForeColor = Color.Blue;
        lblTotalCount.Location = new Point(685, 18);
        lblTotalCount.Name = "lblTotalCount";
        lblTotalCount.Size = new Size(19, 21);
        lblTotalCount.TabIndex = 7;
        lblTotalCount.Text = "0";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label4.ForeColor = Color.Black;
        label4.Location = new Point(600, 20);
        label4.Name = "label4";
        label4.Size = new Size(84, 19);
        label4.TabIndex = 6;
        label4.Text = "Tổng cộng:";
        // 
        // lblErrorCount
        // 
        lblErrorCount.AutoSize = true;
        lblErrorCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblErrorCount.ForeColor = Color.Red;
        lblErrorCount.Location = new Point(460, 18);
        lblErrorCount.Name = "lblErrorCount";
        lblErrorCount.Size = new Size(19, 21);
        lblErrorCount.TabIndex = 5;
        lblErrorCount.Text = "0";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label3.ForeColor = Color.Black;
        label3.Location = new Point(400, 20);
        label3.Name = "label3";
        label3.Size = new Size(55, 19);
        label3.TabIndex = 4;
        label3.Text = "Tỏi lỗi:";
        // 
        // lblSmallCount
        // 
        lblSmallCount.AutoSize = true;
        lblSmallCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblSmallCount.ForeColor = Color.Orange;
        lblSmallCount.Location = new Point(265, 18);
        lblSmallCount.Name = "lblSmallCount";
        lblSmallCount.Size = new Size(19, 21);
        lblSmallCount.TabIndex = 3;
        lblSmallCount.Text = "0";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label2.ForeColor = Color.Black;
        label2.Location = new Point(200, 20);
        label2.Name = "label2";
        label2.Size = new Size(63, 19);
        label2.TabIndex = 2;
        label2.Text = "Tỏi nhỏ:";
        // 
        // lblLargeCount
        // 
        lblLargeCount.AutoSize = true;
        lblLargeCount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblLargeCount.ForeColor = Color.Green;
        lblLargeCount.Location = new Point(75, 18);
        lblLargeCount.Name = "lblLargeCount";
        lblLargeCount.Size = new Size(19, 21);
        lblLargeCount.TabIndex = 1;
        lblLargeCount.Text = "0";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        label1.ForeColor = Color.Black;
        label1.Location = new Point(20, 20);
        label1.Name = "label1";
        label1.Size = new Size(52, 19);
        label1.TabIndex = 0;
        label1.Text = "Tỏi to:";
        // 
        // dgvHistory
        // 
        dgvHistory.AllowUserToAddRows = false;
        dgvHistory.AllowUserToDeleteRows = false;
        dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvHistory.BackgroundColor = SystemColors.Window;
        dgvHistory.BorderStyle = BorderStyle.None;
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = SystemColors.Control;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        dgvHistory.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        dgvHistory.ColumnHeadersHeight = 30;
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = SystemColors.Window;
        dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle2.ForeColor = Color.Black;
        dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        dgvHistory.DefaultCellStyle = dataGridViewCellStyle2;
        dgvHistory.Dock = DockStyle.Fill;
        dgvHistory.Location = new Point(0, 170);
        dgvHistory.Name = "dgvHistory";
        dgvHistory.ReadOnly = true;
        dgvHistory.RowHeadersWidth = 50;
        dgvHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvHistory.Size = new Size(1095, 480);
        dgvHistory.TabIndex = 3;
        // 
        // pnlBottom
        // 
        pnlBottom.BackColor = SystemColors.Control;
        pnlBottom.Controls.Add(btnClose);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 650);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Padding = new Padding(10);
        pnlBottom.Size = new Size(1095, 50);
        pnlBottom.TabIndex = 4;
        // 
        // btnClose
        // 
        btnClose.BackColor = Color.FromArgb(220, 60, 60);
        btnClose.Dock = DockStyle.Right;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnClose.ForeColor = Color.White;
        btnClose.Location = new Point(985, 10);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(100, 30);
        btnClose.TabIndex = 0;
        btnClose.Text = "✖ Đóng";
        btnClose.UseVisualStyleBackColor = false;
        btnClose.Click += btnClose_Click;
        // 
        // frmHistory
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1095, 700);
        Controls.Add(dgvHistory);
        Controls.Add(pnlBottom);
        Controls.Add(pnlStats);
        Controls.Add(pnlButtons);
        Controls.Add(pnlTop);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(1000, 600);
        Name = "frmHistory";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Lịch sử phát hiện tỏi";
        Load += frmHistory_Load;
        pnlTop.ResumeLayout(false);
        pnlButtons.ResumeLayout(false);
        pnlStats.ResumeLayout(false);
        pnlStats.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvHistory).EndInit();
        pnlBottom.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
}

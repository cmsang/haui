using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Services;
using System.Data;

namespace Haui.GarlicDetector;

/// <summary>
/// Form hiển thị lịch sử phát hiện tỏi theo thời gian.
/// </summary>
public partial class frmHistory : Form
{
    public frmHistory()
    {
        InitializeComponent();

        // Đăng ký events
        dgvHistory.CellFormatting += dgvHistory_CellFormatting;
        dgvHistory.RowPostPaint += dgvHistory_RowPostPaint;
    }

    private void frmHistory_Load(object sender, EventArgs e)
    {
        // Mặc định hiển thị lịch sử hôm nay
        LoadTodayHistory();
    }

    // ─── Load History Methods ────────────────────────────────────────────────

    /// <summary>Tải lịch sử hôm nay.</summary>
    private void LoadTodayHistory()
    {
        try
        {
            lblPeriod.Text = $"Lịch sử hôm nay - {DateTime.Today:dd/MM/yyyy}";

            DataTable dt = DatabaseService.GetTodayHistory();
            dgvHistory.DataSource = dt;

            UpdateStatistics(dt);
            FormatDataGridView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử hôm nay:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Tải lịch sử hôm qua.</summary>
    private void LoadYesterdayHistory()
    {
        try
        {
            var yesterday = DateTime.Today.AddDays(-1);
            lblPeriod.Text = $"Lịch sử hôm qua - {yesterday:dd/MM/yyyy}";

            DataTable dt = DatabaseService.GetYesterdayHistory();
            dgvHistory.DataSource = dt;

            UpdateStatistics(dt);
            FormatDataGridView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử hôm qua:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Tải lịch sử tuần này.</summary>
    private void LoadThisWeekHistory()
    {
        try
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            lblPeriod.Text = $"Lịch sử tuần này - {startOfWeek:dd/MM} đến {endOfWeek:dd/MM/yyyy}";

            DataTable dt = DatabaseService.GetThisWeekHistory();
            dgvHistory.DataSource = dt;

            UpdateStatistics(dt);
            FormatDataGridView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử tuần này:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Tải lịch sử tháng này.</summary>
    private void LoadThisMonthHistory()
    {
        try
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            lblPeriod.Text = $"Lịch sử tháng này - {startOfMonth:dd/MM} đến {endOfMonth:dd/MM/yyyy}";

            DataTable dt = DatabaseService.GetThisMonthHistory();
            dgvHistory.DataSource = dt;

            UpdateStatistics(dt);
            FormatDataGridView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử tháng này:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Tải lịch sử theo khoảng thời gian tùy chỉnh.</summary>
    private void LoadCustomHistory()
    {
        try
        {
            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date.AddDays(1).AddTicks(-1); // End of day

            if (from > to)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn ngày kết thúc!", "Cảnh báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblPeriod.Text = $"Lịch sử từ {from:dd/MM/yyyy} đến {dtpTo.Value:dd/MM/yyyy}";

            DataTable dt = DatabaseService.GetDetectHistoryByTime(from, to);
            dgvHistory.DataSource = dt;

            UpdateStatistics(dt);
            FormatDataGridView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải lịch sử tùy chỉnh:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─── Helper Methods ──────────────────────────────────────────────────────

    /// <summary>Cập nhật thống kê từ DataTable.</summary>
    private void UpdateStatistics(DataTable dt)
    {
        int totalCount = dt.Rows.Count;
        int largeCount = 0;
        int smallCount = 0;
        int errorCount = 0;

        foreach (DataRow row in dt.Rows)
        {
            if (row["GarlicType"] != DBNull.Value)
            {
                int garlicType = Convert.ToInt32(row["GarlicType"]);
                switch ((GarlicLabel)garlicType)
                {
                    case GarlicLabel.ToTo:
                        largeCount++;
                        break;
                    case GarlicLabel.ToNho:
                        smallCount++;
                        break;
                    case GarlicLabel.ToHong:
                        errorCount++;
                        break;
                }
            }
        }

        lblLargeCount.Text = largeCount.ToString();
        lblSmallCount.Text = smallCount.ToString();
        lblErrorCount.Text = errorCount.ToString();
        lblTotalCount.Text = totalCount.ToString();
    }

    /// <summary>Format DataGridView columns.</summary>
    private void FormatDataGridView()
    {
        if (dgvHistory.Columns.Count == 0) return;

        // Ẩn cột ID
        if (dgvHistory.Columns.Contains("ID"))
            dgvHistory.Columns["ID"].Visible = false;

        // Đặt tên và format các cột
        if (dgvHistory.Columns.Contains("GarlicType"))
        {
            dgvHistory.Columns["GarlicType"].HeaderText = "Loại tỏi";
            dgvHistory.Columns["GarlicType"].Width = 120;
        }

        if (dgvHistory.Columns.Contains("UpdateTime"))
        {
            dgvHistory.Columns["UpdateTime"].HeaderText = "Thời gian";
            dgvHistory.Columns["UpdateTime"].Width = 150;
            dgvHistory.Columns["UpdateTime"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        if (dgvHistory.Columns.Contains("UpdateBy"))
        {
            dgvHistory.Columns["UpdateBy"].HeaderText = "Người cập nhật";
            dgvHistory.Columns["UpdateBy"].Width = 120;
        }

        // Auto resize
        dgvHistory.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
    }

    /// <summary>
    /// Format cell khi hiển thị - convert GarlicType từ số sang text và set màu nền.
    /// </summary>
    private void dgvHistory_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Chỉ xử lý cột GarlicType
        if (dgvHistory.Columns[e.ColumnIndex].Name != "GarlicType") return;

        if (e.Value != null && e.Value != DBNull.Value)
        {
            try
            {
                int garlicType = Convert.ToInt32(e.Value);

                // Chuyển số thành text
                e.Value = ((GarlicLabel)garlicType) switch
                {
                    GarlicLabel.ToTo => "Tỏi to",
                    GarlicLabel.ToNho => "Tỏi nhỏ",
                    GarlicLabel.ToHong => "Tỏi hỏng",
                    _ => "N/A"
                };

                // Set màu nền theo loại
                e.CellStyle.BackColor = ((GarlicLabel)garlicType) switch
                {
                    GarlicLabel.ToTo => Color.LightGreen,
                    GarlicLabel.ToNho => Color.LightYellow,
                    GarlicLabel.ToHong => Color.LightCoral,
                    _ => Color.White
                };

                e.CellStyle.ForeColor = Color.Black;
                e.FormattingApplied = true;
            }
            catch
            {
                // Nếu có lỗi convert, giữ nguyên giá trị
                e.FormattingApplied = false;
            }
        }
    }

    /// <summary>
    /// Vẽ số thứ tự vào RowHeader của DataGridView.
    /// </summary>
    private void dgvHistory_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        // Lấy số thứ tự (bắt đầu từ 1)
        string rowNumber = (e.RowIndex + 1).ToString();

        // Tính toán kích thước text
        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
        var textSize = e.Graphics.MeasureString(rowNumber, font);

        // Vị trí vẽ (giữa RowHeader)
        float x = e.RowBounds.Left + (dgvHistory.RowHeadersWidth - textSize.Width) / 2;
        float y = e.RowBounds.Top + (e.RowBounds.Height - textSize.Height) / 2;

        // Vẽ số thứ tự
        using var brush = new SolidBrush(dgvHistory.RowHeadersDefaultCellStyle.ForeColor);
        e.Graphics.DrawString(rowNumber, font, brush, x, y);
    }

    /// <summary>Export lịch sử ra file Excel/CSV.</summary>
    private void ExportToExcel()
    {
        try
        {
            if (dgvHistory.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File|*.csv",
                Title = "Xuất lịch sử",
                FileName = $"GarlicHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var sb = new System.Text.StringBuilder();

            // Header
            var headers = dgvHistory.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .Select(c => c.HeaderText);
            sb.AppendLine(string.Join(",", headers));

            // Data
            foreach (DataGridViewRow row in dgvHistory.Rows)
            {
                var cells = row.Cells.Cast<DataGridViewCell>()
                    .Where(c => c.OwningColumn.Visible)
                    .Select(c => c.Value?.ToString() ?? "");
                sb.AppendLine(string.Join(",", cells));
            }

            File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            MessageBox.Show("Xuất file thành công!", "Thông báo", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi xuất file:\n{ex.Message}", "Lỗi", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─── Event Handlers ──────────────────────────────────────────────────────

    private void btnToday_Click(object? sender, EventArgs e) => LoadTodayHistory();

    private void btnYesterday_Click(object? sender, EventArgs e) => LoadYesterdayHistory();

    private void btnThisWeek_Click(object? sender, EventArgs e) => LoadThisWeekHistory();

    private void btnThisMonth_Click(object? sender, EventArgs e) => LoadThisMonthHistory();

    private void btnCustom_Click(object? sender, EventArgs e) => LoadCustomHistory();

    private void btnExport_Click(object? sender, EventArgs e) => ExportToExcel();

    private void btnClose_Click(object? sender, EventArgs e) => Close();

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        // Reload current view
        LoadTodayHistory();
    }
}

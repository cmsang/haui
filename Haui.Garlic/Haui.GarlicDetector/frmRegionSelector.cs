namespace Haui.GarlicDetector;

/// <summary>
/// Form cho phép người dùng kéo chuột để chọn một vùng hình chữ nhật
/// trên ảnh snapshot từ camera. Kết quả trả về qua <see cref="SelectedRegion"/>
/// dưới dạng tọa độ pixel trên ảnh gốc (không phải tọa độ hiển thị).
/// </summary>
public partial class frmRegionSelector : Form
{
    // ─── State kéo chuột ─────────────────────────────────────────────────────

    private Point _dragStart;
    private Point _dragEnd;
    private bool  _isDragging;

    // ─── Kết quả ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Vùng đã chọn (tọa độ ảnh thực tế), hoặc <c>null</c> nếu người dùng
    /// bấm Hủy hoặc chọn Xóa vùng.
    /// </summary>
    public Rectangle? SelectedRegion { get; private set; }

    // ─── Constructor ─────────────────────────────────────────────────────────

    /// <param name="snapshot">Ảnh chụp từ camera để làm nền vẽ vùng.</param>
    /// <param name="currentRegion">Vùng đang lưu (hiển thị sẵn nếu có).</param>
    public frmRegionSelector(Bitmap snapshot, Rectangle? currentRegion = null)
    {
        InitializeComponent();

        picSnapshot.Image = (Bitmap)snapshot.Clone();

        // Hiển thị vùng đã lưu sẵn lên overlay
        if (currentRegion.HasValue)
        {
            SelectedRegion     = currentRegion;
            btnConfirm.Enabled = true;
        }

        picSnapshot.MouseDown += PicSnapshot_MouseDown;
        picSnapshot.MouseMove += PicSnapshot_MouseMove;
        picSnapshot.MouseUp   += PicSnapshot_MouseUp;
        picSnapshot.Paint     += PicSnapshot_Paint;

        // Vẽ lại khi resize để overlay theo kịch thước mới
        Resize += (_, _) => picSnapshot.Invalidate();

        // Tính toán dragStart/dragEnd từ vùng đã có (sau khi form load xong)
        Shown += (_, _) =>
        {
            if (currentRegion.HasValue)
            {
                var ir     = GetImageRect();
                _dragStart = ImageToDisplay(currentRegion.Value.Location, ir);
                _dragEnd   = ImageToDisplay(
                    new Point(currentRegion.Value.Right, currentRegion.Value.Bottom), ir);
                picSnapshot.Invalidate();
            }
        };
    }

    // ─── Sự kiện chuột ───────────────────────────────────────────────────────

    private void PicSnapshot_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _dragStart  = e.Location;
        _dragEnd    = e.Location;
        _isDragging = true;
        picSnapshot.Invalidate();
    }

    private void PicSnapshot_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        _dragEnd = e.Location;
        picSnapshot.Invalidate();
    }

    private void PicSnapshot_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!_isDragging || e.Button != MouseButtons.Left) return;
        _isDragging = false;
        _dragEnd    = e.Location;

        var ir       = GetImageRect();
        var imgStart = DisplayToImage(_dragStart, ir);
        var imgEnd   = DisplayToImage(_dragEnd,   ir);

        int x = Math.Min(imgStart.X, imgEnd.X);
        int y = Math.Min(imgStart.Y, imgEnd.Y);
        int w = Math.Abs(imgEnd.X - imgStart.X);
        int h = Math.Abs(imgEnd.Y - imgStart.Y);

        // Bỏ qua nếu vùng quá nhỏ (< 10×10 px)
        if (w >= 10 && h >= 10)
        {
            SelectedRegion     = new Rectangle(x, y, w, h);
            btnConfirm.Enabled = true;
        }

        picSnapshot.Invalidate();
    }

    // ─── Vẽ overlay ──────────────────────────────────────────────────────────

    private void PicSnapshot_Paint(object? sender, PaintEventArgs e)
    {
        if (_dragStart == _dragEnd) return;

        int x = Math.Min(_dragStart.X, _dragEnd.X);
        int y = Math.Min(_dragStart.Y, _dragEnd.Y);
        int w = Math.Abs(_dragEnd.X - _dragStart.X);
        int h = Math.Abs(_dragEnd.Y - _dragStart.Y);

        var rect = new Rectangle(x, y, w, h);

        // Nền bán trong suốt
        using var fill = new SolidBrush(Color.FromArgb(50, 0, 180, 255));
        e.Graphics.FillRectangle(fill, rect);

        // Viền xanh dương
        using var borderPen = new Pen(Color.DodgerBlue, 2);
        e.Graphics.DrawRectangle(borderPen, rect);

        // Hiển thị kích thước vùng theo tọa độ ảnh thực tế
        if (picSnapshot.Image != null)
        {
            var ir       = GetImageRect();
            var imgStart = DisplayToImage(_dragStart, ir);
            var imgEnd   = DisplayToImage(_dragEnd,   ir);
            int iw       = Math.Abs(imgEnd.X - imgStart.X);
            int ih       = Math.Abs(imgEnd.Y - imgStart.Y);
            string info  = $"{iw} × {ih} px";
            var    infoFont = SystemFonts.SmallCaptionFont ?? SystemFonts.DefaultFont;
            e.Graphics.DrawString(info, infoFont, Brushes.White,
                new PointF(x + 4, y + 4));
        }
    }

    // ─── Nút bấm ─────────────────────────────────────────────────────────────

    private void btnConfirm_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnClear_Click(object? sender, EventArgs e)
    {
        // Xóa vùng → trả null cho caller
        SelectedRegion     = null;
        _dragStart         = Point.Empty;
        _dragEnd           = Point.Empty;
        btnConfirm.Enabled = false;
        picSnapshot.Invalidate();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ─── Helpers tọa độ ──────────────────────────────────────────────────────

    /// <summary>
    /// Tính hình chữ nhật ảnh hiển thị bên trong PictureBox Zoom mode (display coords).
    /// </summary>
    private RectangleF GetImageRect()
    {
        if (picSnapshot.Image == null) return RectangleF.Empty;

        float imgW  = picSnapshot.Image.Width;
        float imgH  = picSnapshot.Image.Height;
        float pbW   = picSnapshot.ClientSize.Width;
        float pbH   = picSnapshot.ClientSize.Height;
        float scale = Math.Min(pbW / imgW, pbH / imgH);
        float dw    = imgW * scale;
        float dh    = imgH * scale;

        return new RectangleF((pbW - dw) / 2f, (pbH - dh) / 2f, dw, dh);
    }

    /// <summary>Chuyển điểm display → điểm ảnh thực tế (clamp trong bounds).</summary>
    private Point DisplayToImage(Point pt, RectangleF ir)
    {
        if (ir.IsEmpty || picSnapshot.Image == null) return Point.Empty;

        int x = (int)Math.Round((pt.X - ir.X) / ir.Width  * picSnapshot.Image.Width);
        int y = (int)Math.Round((pt.Y - ir.Y) / ir.Height * picSnapshot.Image.Height);

        return new Point(
            Math.Clamp(x, 0, picSnapshot.Image.Width  - 1),
            Math.Clamp(y, 0, picSnapshot.Image.Height - 1));
    }

    /// <summary>Chuyển điểm ảnh thực tế → điểm display (để render vùng có sẵn).</summary>
    private Point ImageToDisplay(Point pt, RectangleF ir)
    {
        if (ir.IsEmpty || picSnapshot.Image == null) return Point.Empty;

        int x = (int)(pt.X / (float)picSnapshot.Image.Width  * ir.Width  + ir.X);
        int y = (int)(pt.Y / (float)picSnapshot.Image.Height * ir.Height + ir.Y);
        return new Point(x, y);
    }

    // ─── Giải phóng ảnh khi đóng ─────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        picSnapshot.Image?.Dispose();
        picSnapshot.Image = null;
    }
}

using System.Drawing.Imaging;
using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.Models;

namespace Haui.GarlicDetector;

/// <summary>
/// Form gán nhãn tỏi — cho phép mở/chụp ảnh, khoanh chọn nhiều vùng,
/// gán nhãn từng vùng và lưu từng vùng thành ảnh riêng.
/// </summary>
public partial class frmLabeling : Form
{
    // ─── Dữ liệu vùng đã chọn ────────────────────────────────────────────────

    private readonly List<LabeledRegion> _regions = new();

    // ─── Trạng thái kéo chuột ────────────────────────────────────────────────

    private Point _dragStart;
    private Point _dragCurrent;
    private bool  _isDragging;

    // ─── Index vùng đang được highlight ──────────────────────────────────────

    private int _highlightedIndex = -1;

    // ─── Khởi tạo ────────────────────────────────────────────────────────────

    /// <summary>Khởi tạo form gán nhãn; tuỳ chọn truyền ảnh ban đầu (chụp từ camera).</summary>
    public frmLabeling(Bitmap? initialImage = null)
    {
        InitializeComponent();

        // Tải thư mục lưu gần nhất
        var savedFolder = AppSettings.Instance.LabelSaveFolder;
        if (!string.IsNullOrWhiteSpace(savedFolder) && Directory.Exists(savedFolder))
            txtFolder.Text = savedFolder;

        if (initialImage != null)
            LoadImage(initialImage);
    }

    // ─── Mở / Chụp ảnh ───────────────────────────────────────────────────────

    private void btnOpenImage_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Chọn ảnh tỏi",
            Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|Tất cả|*.*",
        };

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        LoadImage(new Bitmap(dlg.FileName));
    }

    private void LoadImage(Bitmap bmp)
    {
        // Giải phóng ảnh cũ
        var old = picImage.Image;
        picImage.Image = bmp;
        old?.Dispose();

        _regions.Clear();
        _isDragging       = false;
        _highlightedIndex = -1;

        RefreshList();
        picImage.Invalidate();
    }

    // ─── Sự kiện chuột trên PictureBox ───────────────────────────────────────

    private void picImage_MouseDown(object sender, MouseEventArgs e)
    {
        if (picImage.Image == null || e.Button != MouseButtons.Left) return;

        _dragStart   = e.Location;
        _dragCurrent = e.Location;
        _isDragging  = true;
    }

    private void picImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        _dragCurrent = e.Location;
        picImage.Invalidate();
    }

    private void picImage_MouseUp(object sender, MouseEventArgs e)
    {
        if (!_isDragging || picImage.Image == null) return;

        _isDragging = false;

        var dispRect = NormalizeRect(_dragStart, _dragCurrent);

        // Bỏ qua vùng quá nhỏ (dưới 5px)
        if (dispRect.Width < 5 || dispRect.Height < 5)
        {
            picImage.Invalidate();
            return;
        }

        var imgRect = DisplayToImage(dispRect);
        if (imgRect.IsEmpty)
        {
            picImage.Invalidate();
            return;
        }

        var label = cmbLabel.SelectedIndex switch
        {
            1 => GarlicLabel.ToNho,
            2 => GarlicLabel.ToHong,
            _ => GarlicLabel.ToTo,
        };

        _regions.Add(new LabeledRegion(imgRect, label));
        RefreshList();
        picImage.Invalidate();
    }

    // ─── Vẽ overlay lên PictureBox ────────────────────────────────────────────

    private void picImage_Paint(object sender, PaintEventArgs e)
    {
        if (picImage.Image == null) return;

        // Vẽ các vùng đã thêm
        for (int i = 0; i < _regions.Count; i++)
        {
            var region   = _regions[i];
            var dispRect = ImageToDisplay(region.ImageRect);
            if (dispRect.IsEmpty) continue;

            var color       = GetLabelColor(region.Label);
            bool highlighted = i == _highlightedIndex;

            using var pen = new Pen(color, highlighted ? 3 : 2);
            if (highlighted)
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            e.Graphics.DrawRectangle(pen, Rectangle.Round(dispRect));

            // Nhãn ngắn phía trên góc trái
            var font    = SystemFonts.SmallCaptionFont ?? SystemFonts.DefaultFont;
            var labelTx = GetLabelShort(region.Label, i + 1);
            var labelPt = new PointF(dispRect.X + 3, dispRect.Y + 3);

            // Nền mờ để nhãn dễ đọc
            var textSize = e.Graphics.MeasureString(labelTx, font);
            using var bgBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
            e.Graphics.FillRectangle(bgBrush, labelPt.X - 1, labelPt.Y - 1, textSize.Width + 2, textSize.Height + 2);

            using var textBrush = new SolidBrush(color);
            e.Graphics.DrawString(labelTx, font, textBrush, labelPt);
        }

        // Vẽ hình chữ nhật đang kéo (preview)
        if (_isDragging)
        {
            var dragRect = NormalizeRect(_dragStart, _dragCurrent);
            using var dragPen = new Pen(Color.Yellow, 2)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            };
            e.Graphics.DrawRectangle(dragPen, dragRect);
        }
    }

    private static Color GetLabelColor(GarlicLabel label) => label switch
    {
        GarlicLabel.ToTo   => Color.LimeGreen,
        GarlicLabel.ToNho  => Color.DeepSkyBlue,
        GarlicLabel.ToHong => Color.OrangeRed,
        _                  => Color.White,
    };

    private static string GetLabelShort(GarlicLabel label, int index) => label switch
    {
        GarlicLabel.ToTo   => $"To {index}",
        GarlicLabel.ToNho  => $"Nhỏ {index}",
        GarlicLabel.ToHong => $"Hỏng {index}",
        _                  => $"#{index}",
    };

    // ─── Danh sách vùng ───────────────────────────────────────────────────────

    private void RefreshList()
    {
        lstRegions.Items.Clear();

        for (int i = 0; i < _regions.Count; i++)
        {
            var r       = _regions[i];
            var name    = r.Label switch
            {
                GarlicLabel.ToTo   => "Tỏi to",
                GarlicLabel.ToNho  => "Tỏi nhỏ",
                GarlicLabel.ToHong => "Tỏi hỏng",
                _                  => "Không rõ",
            };
            lstRegions.Items.Add($"#{i + 1} {name} ({r.ImageRect.Width}×{r.ImageRect.Height})");
        }
    }

    private void lstRegions_SelectedIndexChanged(object sender, EventArgs e)
    {
        _highlightedIndex = lstRegions.SelectedIndex;
        picImage.Invalidate();
    }

    private void btnDeleteRegion_Click(object sender, EventArgs e)
    {
        int idx = lstRegions.SelectedIndex;
        if (idx < 0 || idx >= _regions.Count) return;

        _regions.RemoveAt(idx);
        _highlightedIndex = -1;
        RefreshList();
        picImage.Invalidate();
    }

    private void btnClearAll_Click(object sender, EventArgs e)
    {
        if (_regions.Count == 0) return;

        if (MessageBox.Show("Xóa tất cả vùng đã chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _regions.Clear();
        _highlightedIndex = -1;
        RefreshList();
        picImage.Invalidate();
    }

    // ─── Chọn thư mục lưu ────────────────────────────────────────────────────

    private void btnBrowseFolder_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description            = "Chọn thư mục lưu ảnh vùng tỏi",
            UseDescriptionForTitle = true,
        };

        if (!string.IsNullOrWhiteSpace(txtFolder.Text) && Directory.Exists(txtFolder.Text))
            dlg.InitialDirectory = txtFolder.Text;

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        txtFolder.Text = dlg.SelectedPath;

        // Lưu thư mục gần nhất vào settings.json
        AppSettings.Instance.LabelSaveFolder = dlg.SelectedPath;
        AppSettings.Instance.Save();
    }

    // ─── Lưu tất cả vùng ─────────────────────────────────────────────────────

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (picImage.Image is not Bitmap sourceBmp)
        {
            MessageBox.Show("Chưa có ảnh để lưu.", "Chưa có ảnh",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_regions.Count == 0)
        {
            MessageBox.Show("Chưa chọn vùng nào.", "Chưa có vùng",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var folder = txtFolder.Text.Trim();
        if (string.IsNullOrWhiteSpace(folder))
        {
            MessageBox.Show("Vui lòng chọn thư mục lưu.", "Chưa chọn thư mục",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Directory.CreateDirectory(folder);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            int saved     = 0;

            for (int i = 0; i < _regions.Count; i++)
            {
                var rect = _regions[i].ImageRect;

                // Clamp rect vào bounds ảnh để tránh lỗi khi crop
                rect = Rectangle.Intersect(rect,
                    new Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height));
                if (rect.IsEmpty) continue;

                var labelName = _regions[i].Label switch
                {
                    GarlicLabel.ToTo   => "toi_to",
                    GarlicLabel.ToNho  => "toi_nho",
                    GarlicLabel.ToHong => "toi_hong",
                    _                  => "unknown",
                };

                var fileName = $"{labelName}_{timestamp}_{i + 1:D3}.png";
                var filePath = Path.Combine(folder, fileName);

                using var crop = sourceBmp.Clone(rect, sourceBmp.PixelFormat);
                crop.Save(filePath, ImageFormat.Png);
                saved++;
            }

            MessageBox.Show($"Đã lưu {saved} ảnh vào:\n{folder}", "Lưu thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─── Tiện ích chuyển đổi tọa độ ──────────────────────────────────────────

    /// <summary>
    /// Tính hình chữ nhật (display coords) của ảnh thực tế bên trong PictureBox Zoom.
    /// </summary>
    private RectangleF GetImageRect()
    {
        var img = picImage.Image;
        if (img == null) return RectangleF.Empty;

        float imgW  = img.Width;
        float imgH  = img.Height;
        float pbW   = picImage.ClientSize.Width;
        float pbH   = picImage.ClientSize.Height;
        float scale = Math.Min(pbW / imgW, pbH / imgH);
        float dw    = imgW * scale;
        float dh    = imgH * scale;

        return new RectangleF((pbW - dw) / 2f, (pbH - dh) / 2f, dw, dh);
    }

    /// <summary>Chuyển hình chữ nhật display coords sang image coords.</summary>
    private Rectangle DisplayToImage(Rectangle displayRect)
    {
        var ir  = GetImageRect();
        var img = picImage.Image;
        if (ir.IsEmpty || img == null) return Rectangle.Empty;

        float scaleX = img.Width  / ir.Width;
        float scaleY = img.Height / ir.Height;

        int x = (int)((displayRect.X - ir.X) * scaleX);
        int y = (int)((displayRect.Y - ir.Y) * scaleY);
        int w = (int)(displayRect.Width  * scaleX);
        int h = (int)(displayRect.Height * scaleY);

        x = Math.Clamp(x, 0, img.Width);
        y = Math.Clamp(y, 0, img.Height);
        w = Math.Clamp(w, 0, img.Width  - x);
        h = Math.Clamp(h, 0, img.Height - y);

        return new Rectangle(x, y, w, h);
    }

    /// <summary>Chuyển hình chữ nhật image coords sang display coords.</summary>
    private RectangleF ImageToDisplay(Rectangle imageRect)
    {
        var ir  = GetImageRect();
        var img = picImage.Image;
        if (ir.IsEmpty || img == null) return RectangleF.Empty;

        float scaleX = ir.Width  / img.Width;
        float scaleY = ir.Height / img.Height;

        return new RectangleF(
            ir.X + imageRect.X * scaleX,
            ir.Y + imageRect.Y * scaleY,
            imageRect.Width  * scaleX,
            imageRect.Height * scaleY);
    }

    /// <summary>Chuẩn hóa hình chữ nhật từ hai điểm (xử lý kéo ngược).</summary>
    private static Rectangle NormalizeRect(Point p1, Point p2)
    {
        int x = Math.Min(p1.X, p2.X);
        int y = Math.Min(p1.Y, p2.Y);
        int w = Math.Abs(p1.X - p2.X);
        int h = Math.Abs(p1.Y - p2.Y);
        return new Rectangle(x, y, w, h);
    }

    // ─── Dọn dẹp ─────────────────────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        picImage.Image?.Dispose();
        picImage.Image = null;
        base.OnFormClosed(e);
    }
}

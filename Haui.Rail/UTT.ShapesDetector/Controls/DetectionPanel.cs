using UTT.ShapesDetector.Models;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UTT.ShapesDetector.Controls;

public class DetectionPanel : Panel
{
    private Image? _currentImage;
    private List<DetectionResult> _detections = new();
    private readonly Color[] _colors = new[]
    {
        Color.DodgerBlue,
        Color.LimeGreen,
        Color.Orange,
        Color.DeepPink,
        Color.Yellow,
        Color.Cyan,
        Color.Red,
        Color.Purple
    };

    public DetectionPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(30, 30, 30);
    }

    public void UpdateFrame(Image? image, List<DetectionResult>? detections = null)
    {
        _currentImage?.Dispose();
        _currentImage = image;
        _detections = detections ?? new List<DetectionResult>();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        if (_currentImage == null)
        {
            // Draw placeholder
            using var font = new Font("Segoe UI", 16, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(100, 100, 100));
            var text = "No Image";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush,
                (Width - size.Width) / 2,
                (Height - size.Height) / 2);
            return;
        }

        // Calculate scaled image rect
        var imageRect = GetScaledImageRect();

        // Draw image
        g.DrawImage(_currentImage, imageRect);

        // Draw detections
        if (_detections.Count > 0)
        {
            var scaleX = imageRect.Width / (float)_currentImage.Width;
            var scaleY = imageRect.Height / (float)_currentImage.Height;

            foreach (var (detection, index) in _detections.Select((d, i) => (d, i)))
            {
                var color = _colors[index % _colors.Length];
                DrawDetection(g, detection, imageRect, scaleX, scaleY, color);
            }
        }

        // Draw stats
        DrawStats(g);
    }

    private void DrawDetection(Graphics g, DetectionResult detection, RectangleF imageRect, float scaleX, float scaleY, Color color)
    {
        var box = detection.BoundingBox;
        var rect = new RectangleF(
            imageRect.X + box.X * scaleX,
            imageRect.Y + box.Y * scaleY,
            box.Width * scaleX,
            box.Height * scaleY
        );

        // Draw bounding box
        using var pen = new Pen(color, 3);
        g.DrawRectangle(pen, Rectangle.Round(rect));

        // Draw label background
        var label = $"{detection.ClassName} {detection.Confidence:P0}";
        using var font = new Font("Segoe UI", 10, FontStyle.Bold);
        var labelSize = g.MeasureString(label, font);
        var labelRect = new RectangleF(rect.X, rect.Y - labelSize.Height - 4, labelSize.Width + 8, labelSize.Height + 4);

        using var brush = new SolidBrush(Color.FromArgb(220, color));
        g.FillRectangle(brush, labelRect);

        // Draw label text
        g.DrawString(label, font, Brushes.White, rect.X + 4, rect.Y - labelSize.Height - 2);
    }

    private void DrawStats(Graphics g)
    {
        if (_detections.Count == 0) return;

        var stats = $"Detections: {_detections.Count}";
        using var font = new Font("Segoe UI", 9, FontStyle.Bold);
        using var brush = new SolidBrush(Color.White);
        using var bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));

        var size = g.MeasureString(stats, font);
        var rect = new RectangleF(10, 10, size.Width + 10, size.Height + 6);

        g.FillRoundedRectangle(bgBrush, rect, 4);
        g.DrawString(stats, font, brush, 15, 13);
    }

    private RectangleF GetScaledImageRect()
    {
        if (_currentImage == null) return RectangleF.Empty;

        var panelRatio = Width / (float)Height;
        var imageRatio = _currentImage.Width / (float)_currentImage.Height;

        if (imageRatio > panelRatio)
        {
            var scaledHeight = Width / imageRatio;
            return new RectangleF(0, (Height - scaledHeight) / 2, Width, scaledHeight);
        }
        else
        {
            var scaledWidth = Height * imageRatio;
            return new RectangleF((Width - scaledWidth) / 2, 0, scaledWidth, Height);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
    {
        using var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}

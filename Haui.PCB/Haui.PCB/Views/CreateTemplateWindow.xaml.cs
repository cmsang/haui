using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views;

/// <summary>
/// Code-behind của CreateTemplateWindow — chỉ chứa logic giao diện.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="CreateTemplateViewModel"/>.
/// </summary>
public partial class CreateTemplateWindow : System.Windows.Window
{
    private readonly CreateTemplateViewModel _viewModel;

    // Trạng thái kéo thả
    private bool _isDragging;
    private System.Windows.Point _dragStart;

    // Danh sách hình chữ nhật vùng đã vẽ (ánh xạ 1-1 với Regions)
    private readonly List<Rectangle> _regionRects = [];

    public CreateTemplateWindow()
    {
        InitializeComponent();
        _viewModel = new CreateTemplateViewModel(
            new TemplateRegionService(),
            new PcbSegmentationService(),
            new TemplateLibraryService());

        DataContext = _viewModel;
        RegionsGrid.ItemsSource = _viewModel.Regions;

        // Lắng nghe ảnh bo mạch sẵn sàng
        _viewModel.BoardImageReady += bitmap =>
            Dispatcher.InvokeAsync(() =>
            {
                BoardImage.Source = bitmap;
                BoardPlaceholder.Visibility = Visibility.Collapsed;
                RedrawRegionRects();
            });

        // Lắng nghe thay đổi StatusText
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CreateTemplateViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };

        // Vẽ lại khi danh sách vùng thay đổi
        _viewModel.Regions.CollectionChanged += (_, _) =>
            Dispatcher.InvokeAsync(RedrawRegionRects);
    }

    // ──── Public API ──────────────────────────────────────────────────────────

    /// <summary>Nạp frame chụp từ camera vào form.</summary>
    public void LoadFrame(Mat frame)
    {
        _viewModel.LoadFrame(frame);
    }

    // ──── Kéo thả tạo vùng ───────────────────────────────────────────────────

    private void RegionCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(RegionCanvas);
        _isDragging = true;
        Canvas.SetLeft(DragRect, _dragStart.X);
        Canvas.SetTop(DragRect, _dragStart.Y);
        DragRect.Width = 0;
        DragRect.Height = 0;
        DragRect.Visibility = Visibility.Visible;
        RegionCanvas.CaptureMouse();
    }

    private void RegionCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var pos = e.GetPosition(RegionCanvas);
        double x = Math.Min(pos.X, _dragStart.X);
        double y = Math.Min(pos.Y, _dragStart.Y);
        double w = Math.Abs(pos.X - _dragStart.X);
        double h = Math.Abs(pos.Y - _dragStart.Y);
        Canvas.SetLeft(DragRect, x);
        Canvas.SetTop(DragRect, y);
        DragRect.Width = w;
        DragRect.Height = h;
    }

    private void RegionCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        RegionCanvas.ReleaseMouseCapture();
        _isDragging = false;
        DragRect.Visibility = Visibility.Collapsed;

        var pos = e.GetPosition(RegionCanvas);
        double rx = Math.Min(pos.X, _dragStart.X);
        double ry = Math.Min(pos.Y, _dragStart.Y);
        double rw = Math.Abs(pos.X - _dragStart.X);
        double rh = Math.Abs(pos.Y - _dragStart.Y);

        if (rw < 5 || rh < 5) return;

        // Chuyển tọa độ canvas → tọa độ tương đối trên ảnh bo mạch
        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0 || renderRect.Height <= 0) return;

        double relX = (rx - renderRect.X) / renderRect.Width;
        double relY = (ry - renderRect.Y) / renderRect.Height;
        double relW = rw / renderRect.Width;
        double relH = rh / renderRect.Height;

        // Giới hạn trong [0, 1]
        relX = Math.Clamp(relX, 0, 1);
        relY = Math.Clamp(relY, 0, 1);
        relW = Math.Clamp(relW, 0, 1 - relX);
        relH = Math.Clamp(relH, 0, 1 - relY);

        if (relW > 0.001 && relH > 0.001)
            _viewModel.AddRegion(relX, relY, relW, relH);
    }

    // ──── Vẽ lại các hình chữ nhật vùng đã chọn ─────────────────────────────

    private void RedrawRegionRects()
    {
        // Xóa các rect cũ khỏi canvas
        foreach (var rect in _regionRects)
            RegionCanvas.Children.Remove(rect);
        _regionRects.Clear();

        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0) return;

        var selectedItem = RegionsGrid.SelectedItem as TemplateRegionItem;

        for (int i = 0; i < _viewModel.Regions.Count; i++)
        {
            var region = _viewModel.Regions[i];
            var color = region.RegionColor;
            bool isSelected = region == selectedItem;

            double x = renderRect.X + region.RelX * renderRect.Width;
            double y = renderRect.Y + region.RelY * renderRect.Height;
            double w = region.RelWidth * renderRect.Width;
            double h = region.RelHeight * renderRect.Height;

            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(isSelected
                    ? Color.FromRgb(255, 255, 255)
                    : color),
                StrokeThickness = isSelected ? 3 : 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)),
                Width = w,
                Height = h
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            RegionCanvas.Children.Add(rect);
            _regionRects.Add(rect);
        }
    }

    /// <summary>
    /// Tính toán hình chữ nhật hiển thị thực sự của ảnh bo mạch trên canvas (Stretch=Uniform).
    /// </summary>
    private System.Windows.Rect GetImageRenderRect()
    {
        int imgW = _viewModel.BoardWidth;
        int imgH = _viewModel.BoardHeight;
        if (imgW <= 0 || imgH <= 0) return System.Windows.Rect.Empty;

        double canvasW = RegionCanvas.ActualWidth;
        double canvasH = RegionCanvas.ActualHeight;
        if (canvasW <= 0 || canvasH <= 0) return System.Windows.Rect.Empty;

        double scale = Math.Min(canvasW / imgW, canvasH / imgH);
        double renderW = imgW * scale;
        double renderH = imgH * scale;
        double offsetX = (canvasW - renderW) / 2;
        double offsetY = (canvasH - renderH) / 2;
        return new System.Windows.Rect(offsetX, offsetY, renderW, renderH);
    }

    // ──── Vẽ lại khi canvas thay đổi kích thước ──────────────────────────────

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RedrawRegionRects();
    }

    // ──── Event Handlers ──────────────────────────────────────────────────────

    private void RegionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Highlight vùng được chọn: viền trắng khi chọn, màu của vùng khi không chọn
        var selectedItem = RegionsGrid.SelectedItem as TemplateRegionItem;
        for (int i = 0; i < _viewModel.Regions.Count && i < _regionRects.Count; i++)
        {
            bool isSelected = _viewModel.Regions[i] == selectedItem;
            var color = _viewModel.Regions[i].RegionColor;
            _regionRects[i].Stroke = new SolidColorBrush(isSelected
                ? Color.FromRgb(255, 255, 255)
                : color);
            _regionRects[i].StrokeThickness = isSelected ? 3 : 2;
        }
    }

    private void BtnDeleteRegion_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is TemplateRegionItem item)
            _viewModel.RemoveRegion(item);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveRegions();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}

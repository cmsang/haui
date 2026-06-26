using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind của CreateTemplateWindow — chỉ chứa logic giao diện.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="CreateTemplateViewModel"/>.
/// </summary>
public partial class CreateTemplateWindow : System.Windows.Window
{
    private readonly CreateTemplateViewModel _viewModel;

    private const double MinZoom = 0.25;
    private const double MaxZoom = 8.0;
    private const double ZoomStep = 1.15;

    // Trạng thái kéo thả
    private bool _isDragging;
    private System.Windows.Point _dragStart;

    // Zoom: 1.0 = vừa khung viewport
    private double _zoomFactor = 1.0;

    // Kích thước hiển thị cơ sở (zoom = 1); canvas luôn dùng hệ tọa độ này
    private double _boardDisplayWidth;
    private double _boardDisplayHeight;

    // Danh sách hình chữ nhật vùng đã vẽ (ánh xạ 1-1 với Regions)
    private readonly List<Rectangle> _regionRects = [];
    private Rectangle? _orientationRect;

    public CreateTemplateWindow()
    {
        InitializeComponent();
        _viewModel = new CreateTemplateViewModel(
            new PcbSegmentationService(),
            new TemplateLibraryService());

        DataContext = _viewModel;
        RegionsGrid.ItemsSource = _viewModel.Regions;
        _viewModel.RefreshModeFromConfig();

        BoardScrollViewer.Loaded += (_, _) => UpdateBoardLayout();

        // Lắng nghe ảnh bo mạch sẵn sàng
        _viewModel.BoardImageReady += bitmap =>
            Dispatcher.InvokeAsync(() =>
            {
                BoardImage.Source = bitmap;
                BoardPlaceholder.Visibility = Visibility.Collapsed;
                _zoomFactor = 1.0;
                UpdateBoardLayout();
            }, System.Windows.Threading.DispatcherPriority.Loaded);

        BoardScrollViewer.SizeChanged += (_, _) => UpdateBoardLayout();

        // Lắng nghe thay đổi StatusText
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CreateTemplateViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
            if (e.PropertyName is nameof(CreateTemplateViewModel.CanSave)
                or nameof(CreateTemplateViewModel.RegionProgressText))
                Dispatcher.InvokeAsync(() => BtnSave.IsEnabled = _viewModel.CanSave);
            if (e.PropertyName is nameof(CreateTemplateViewModel.OrientationRegion)
                or nameof(CreateTemplateViewModel.IsSettingOrientationPosition))
                Dispatcher.InvokeAsync(RedrawRegionRects);
        };

        // Vẽ lại khi danh sách vùng thay đổi
        _viewModel.Regions.CollectionChanged += (_, _) =>
            Dispatcher.InvokeAsync(RedrawRegionRects);
    }

    // ──── Public API ──────────────────────────────────────────────────────────

    /// <summary>Nạp frame chụp từ camera vào form.</summary>
    public Task LoadFrameAsync(Mat frame) => _viewModel.LoadFrameAsync(frame);

    /// <summary>Nạp mẫu có sẵn để chỉnh sửa (ảnh + danh sách vùng).</summary>
    public void LoadExistingTemplate(Mat boardImage, TemplateEntry entry)
    {
        _viewModel.LoadExistingTemplate(boardImage, entry);
    }

    // ──── Kéo thả tạo vùng ───────────────────────────────────────────────────

    private System.Windows.Point GetAnnotationPoint(MouseEventArgs e)
        => e.GetPosition(BoardViewHost);

    private void BoardViewHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_boardDisplayWidth <= 0 || _boardDisplayHeight <= 0)
            return;

        _dragStart = GetAnnotationPoint(e);
        _isDragging = true;
        Canvas.SetLeft(DragRect, _dragStart.X);
        Canvas.SetTop(DragRect, _dragStart.Y);
        DragRect.Width = 0;
        DragRect.Height = 0;
        DragRect.Visibility = Visibility.Visible;
        BoardViewHost.CaptureMouse();
        e.Handled = true;
    }

    private void BoardViewHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var pos = GetAnnotationPoint(e);
        double x = Math.Min(pos.X, _dragStart.X);
        double y = Math.Min(pos.Y, _dragStart.Y);
        double w = Math.Abs(pos.X - _dragStart.X);
        double h = Math.Abs(pos.Y - _dragStart.Y);
        Canvas.SetLeft(DragRect, x);
        Canvas.SetTop(DragRect, y);
        DragRect.Width = w;
        DragRect.Height = h;
    }

    private void BoardViewHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        FinishRegionDrag(e);
    }

    private void BoardViewHost_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        FinishRegionDrag(e);
    }

    private void FinishRegionDrag(MouseEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        DragRect.Visibility = Visibility.Collapsed;

        if (BoardViewHost.IsMouseCaptured)
            BoardViewHost.ReleaseMouseCapture();

        var pos = GetAnnotationPoint(e);
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
        {
            if (_viewModel.IsSettingOrientationPosition)
                _viewModel.AddOrUpdateOrientationRegion(relX, relY, relW, relH);
            else
                _viewModel.AddRegion(relX, relY, relW, relH);
        }
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
                IsHitTestVisible = false,
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

        if (_orientationRect is not null)
        {
            RegionCanvas.Children.Remove(_orientationRect);
            _orientationRect = null;
        }

        var orientation = _viewModel.OrientationRegion;
        if (orientation is not null)
        {
            double ox = renderRect.X + orientation.RelX * renderRect.Width;
            double oy = renderRect.Y + orientation.RelY * renderRect.Height;
            double ow = orientation.RelWidth * renderRect.Width;
            double oh = orientation.RelHeight * renderRect.Height;

            var color = CreateTemplateViewModel.OrientationRegionColor;
            _orientationRect = new Rectangle
            {
                IsHitTestVisible = false,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 3,
                StrokeDashArray = [6, 3],
                Fill = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
                Width = ow,
                Height = oh
            };
            Canvas.SetLeft(_orientationRect, ox);
            Canvas.SetTop(_orientationRect, oy);
            RegionCanvas.Children.Add(_orientationRect);
        }
    }

    /// <summary>
    /// Hình chữ nhật ảnh bo mạch trên canvas (hệ tọa độ cố định trước LayoutTransform zoom).
    /// </summary>
    private System.Windows.Rect GetImageRenderRect()
    {
        if (_boardDisplayWidth <= 0 || _boardDisplayHeight <= 0)
            return System.Windows.Rect.Empty;
        return new System.Windows.Rect(0, 0, _boardDisplayWidth, _boardDisplayHeight);
    }

    private void UpdateBoardLayout()
    {
        if (BoardImage.Source is not BitmapSource bitmap)
        {
            _boardDisplayWidth = 0;
            _boardDisplayHeight = 0;
            UpdateZoomLabel();
            return;
        }

        int imgW = bitmap.PixelWidth;
        int imgH = bitmap.PixelHeight;
        if (imgW <= 0 || imgH <= 0)
        {
            UpdateZoomLabel();
            return;
        }

        double viewportW = BoardScrollViewer.ViewportWidth > 0
            ? BoardScrollViewer.ViewportWidth
            : BoardScrollViewer.ActualWidth;
        double viewportH = BoardScrollViewer.ViewportHeight > 0
            ? BoardScrollViewer.ViewportHeight
            : BoardScrollViewer.ActualHeight;
        if (viewportW <= 0 || viewportH <= 0)
        {
            Dispatcher.BeginInvoke(UpdateBoardLayout, System.Windows.Threading.DispatcherPriority.Loaded);
            UpdateZoomLabel();
            return;
        }

        double fitScale = Math.Min(viewportW / imgW, viewportH / imgH);
        _boardDisplayWidth = imgW * fitScale;
        _boardDisplayHeight = imgH * fitScale;

        BoardViewHost.Width = _boardDisplayWidth;
        BoardViewHost.Height = _boardDisplayHeight;
        BoardImage.Width = _boardDisplayWidth;
        BoardImage.Height = _boardDisplayHeight;
        RegionCanvas.Width = _boardDisplayWidth;
        RegionCanvas.Height = _boardDisplayHeight;

        BoardZoomTransform.ScaleX = _zoomFactor;
        BoardZoomTransform.ScaleY = _zoomFactor;

        UpdateZoomLabel();
        RedrawRegionRects();
    }

    private void ApplyBoardZoomTransform()
    {
        BoardZoomTransform.ScaleX = _zoomFactor;
        BoardZoomTransform.ScaleY = _zoomFactor;
        BoardViewHost.InvalidateMeasure();
        BoardScrollViewer.UpdateLayout();
    }

    private void UpdateZoomLabel()
        => ZoomLevelText.Text = $"{(int)Math.Round(_zoomFactor * 100)}%";

    private void ApplyZoom(double newZoom, System.Windows.Point zoomCenterInScrollViewer)
    {
        newZoom = Math.Clamp(newZoom, MinZoom, MaxZoom);
        if (Math.Abs(newZoom - _zoomFactor) < 0.001)
            return;

        var contentBefore = new System.Windows.Point(
            BoardScrollViewer.HorizontalOffset + zoomCenterInScrollViewer.X,
            BoardScrollViewer.VerticalOffset + zoomCenterInScrollViewer.Y);

        double ratio = newZoom / _zoomFactor;
        _zoomFactor = newZoom;
        ApplyBoardZoomTransform();
        UpdateZoomLabel();

        BoardScrollViewer.ScrollToHorizontalOffset(contentBefore.X * ratio - zoomCenterInScrollViewer.X);
        BoardScrollViewer.ScrollToVerticalOffset(contentBefore.Y * ratio - zoomCenterInScrollViewer.Y);
    }

    private void ZoomInAtCenter()
    {
        var center = new System.Windows.Point(
            BoardScrollViewer.ViewportWidth / 2,
            BoardScrollViewer.ViewportHeight / 2);
        ApplyZoom(_zoomFactor * ZoomStep, center);
    }

    private void ZoomOutAtCenter()
    {
        var center = new System.Windows.Point(
            BoardScrollViewer.ViewportWidth / 2,
            BoardScrollViewer.ViewportHeight / 2);
        ApplyZoom(_zoomFactor / ZoomStep, center);
    }

    private void BoardScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control || BoardImage.Source is null)
            return;

        e.Handled = true;
        var zoomCenter = e.GetPosition(BoardScrollViewer);
        double factor = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;
        ApplyZoom(_zoomFactor * factor, zoomCenter);
    }

    private void BtnZoomIn_Click(object sender, RoutedEventArgs e) => ZoomInAtCenter();

    private void BtnZoomOut_Click(object sender, RoutedEventArgs e) => ZoomOutAtCenter();

    private void BtnZoomFit_Click(object sender, RoutedEventArgs e)
    {
        _zoomFactor = 1.0;
        ApplyBoardZoomTransform();
        BoardScrollViewer.ScrollToHorizontalOffset(0);
        BoardScrollViewer.ScrollToVerticalOffset(0);
        UpdateZoomLabel();
    }

    // ──── Vẽ lại khi cửa sổ thay đổi kích thước ──────────────────────────────

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateBoardLayout();
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
        if (sender is FrameworkElement { Tag: TemplateRegionItem item })
            _viewModel.RemoveRegion(item);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var regions = _viewModel.GetCurrentRegions();
        if (!_viewModel.ValidateRegions(regions, out var error))
        {
            MessageBox.Show(error, "Tên vùng không hợp lệ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!_viewModel.TrySaveRegions(out error))
        {
            MessageBox.Show(error, "Không thể lưu", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!_viewModel.IsEditing)
        {
            MessageBox.Show(
                $"Đã lưu ảnh mẫu với {_viewModel.RegionCount} vùng linh kiện vào thư viện.",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        DialogResult = true;
        Close();
    }

    private void BtnRotate180_Click(object sender, RoutedEventArgs e)
        => _viewModel.RotateBoard180();

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}

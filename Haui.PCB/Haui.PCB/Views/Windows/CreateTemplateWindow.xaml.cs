using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind cá»§a CreateTemplateWindow â€” chá»‰ chá»©a logic giao diá»‡n.
/// ToÃ n bá»™ nghiá»‡p vá»¥ Ä‘Æ°á»£c uá»· thÃ¡c cho <see cref="CreateTemplateViewModel"/>.
/// </summary>
public partial class CreateTemplateWindow : System.Windows.Window
{
    private readonly CreateTemplateViewModel _viewModel;

    // Tráº¡ng thÃ¡i kÃ©o tháº£
    private bool _isDragging;
    private System.Windows.Point _dragStart;

    // Danh sÃ¡ch hÃ¬nh chá»¯ nháº­t vÃ¹ng Ä‘Ã£ váº½ (Ã¡nh xáº¡ 1-1 vá»›i Regions)
    private readonly List<Rectangle> _regionRects = [];

    /// <summary>
    /// Sá»± kiá»‡n phÃ¡t ra khi ngÆ°á»i dÃ¹ng báº¥m LÆ°u á»Ÿ cháº¿ Ä‘á»™ chá»‰nh sá»­a máº«u.
    /// Tham sá»‘ lÃ  danh sÃ¡ch vÃ¹ng Ä‘Ã£ cáº­p nháº­t.
    /// </summary>
    public event Action<List<TemplateRegion>>? RegionsSaved;

    public CreateTemplateWindow()
    {
        InitializeComponent();
        _viewModel = new CreateTemplateViewModel(
            new PcbSegmentationService(),
            new TemplateLibraryService());

        DataContext = _viewModel;
        RegionsGrid.ItemsSource = _viewModel.Regions;

        // Láº¯ng nghe áº£nh bo máº¡ch sáºµn sÃ ng
        _viewModel.BoardImageReady += bitmap =>
            Dispatcher.InvokeAsync(() =>
            {
                BoardImage.Source = bitmap;
                BoardPlaceholder.Visibility = Visibility.Collapsed;
                RedrawRegionRects();
            });

        // Láº¯ng nghe thay Ä‘á»•i StatusText
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CreateTemplateViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
            if (e.PropertyName is nameof(CreateTemplateViewModel.CanSave)
                or nameof(CreateTemplateViewModel.RegionProgressText))
                Dispatcher.InvokeAsync(() => BtnSave.IsEnabled = _viewModel.CanSave);
        };

        // Váº½ láº¡i khi danh sÃ¡ch vÃ¹ng thay Ä‘á»•i
        _viewModel.Regions.CollectionChanged += (_, _) =>
            Dispatcher.InvokeAsync(RedrawRegionRects);
    }

    // â”€â”€â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Náº¡p frame chá»¥p tá»« camera vÃ o form.</summary>
    public Task LoadFrameAsync(Mat frame) => _viewModel.LoadFrameAsync(frame);

    /// <summary>Náº¡p máº«u cÃ³ sáºµn Ä‘á»ƒ chá»‰nh sá»­a (áº£nh + danh sÃ¡ch vÃ¹ng).</summary>
    public void LoadExistingTemplate(Mat boardImage, IEnumerable<TemplateRegion> regions)
    {
        _viewModel.LoadExistingTemplate(boardImage, regions);
    }

    // â”€â”€â”€â”€ KÃ©o tháº£ táº¡o vÃ¹ng â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

        // Chuyá»ƒn tá»a Ä‘á»™ canvas â†’ tá»a Ä‘á»™ tÆ°Æ¡ng Ä‘á»‘i trÃªn áº£nh bo máº¡ch
        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0 || renderRect.Height <= 0) return;

        double relX = (rx - renderRect.X) / renderRect.Width;
        double relY = (ry - renderRect.Y) / renderRect.Height;
        double relW = rw / renderRect.Width;
        double relH = rh / renderRect.Height;

        // Giá»›i háº¡n trong [0, 1]
        relX = Math.Clamp(relX, 0, 1);
        relY = Math.Clamp(relY, 0, 1);
        relW = Math.Clamp(relW, 0, 1 - relX);
        relH = Math.Clamp(relH, 0, 1 - relY);

        if (relW > 0.001 && relH > 0.001)
            _viewModel.AddRegion(relX, relY, relW, relH);
    }

    // â”€â”€â”€â”€ Váº½ láº¡i cÃ¡c hÃ¬nh chá»¯ nháº­t vÃ¹ng Ä‘Ã£ chá»n â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void RedrawRegionRects()
    {
        // XÃ³a cÃ¡c rect cÅ© khá»i canvas
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
    /// TÃ­nh toÃ¡n hÃ¬nh chá»¯ nháº­t hiá»ƒn thá»‹ thá»±c sá»± cá»§a áº£nh bo máº¡ch trÃªn canvas (Stretch=Uniform).
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

    // â”€â”€â”€â”€ Váº½ láº¡i khi canvas thay Ä‘á»•i kÃ­ch thÆ°á»›c â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RedrawRegionRects();
    }

    // â”€â”€â”€â”€ Event Handlers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void RegionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Highlight vÃ¹ng Ä‘Æ°á»£c chá»n: viá»n tráº¯ng khi chá»n, mÃ u cá»§a vÃ¹ng khi khÃ´ng chá»n
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
        var regions = _viewModel.GetCurrentRegions();
        if (!_viewModel.ValidateRegions(regions, out var error))
        {
            MessageBox.Show(error, "TÃªn vÃ¹ng khÃ´ng há»£p lá»‡",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RegionsSaved is not null)
        {
            RegionsSaved.Invoke(regions);
            return;
        }

        if (!_viewModel.TrySaveRegions(out error))
        {
            MessageBox.Show(error, "KhÃ´ng thá»ƒ lÆ°u", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show(
            $"ÄÃ£ lÆ°u áº£nh máº«u vá»›i {_viewModel.RegionCount} vÃ¹ng linh kiá»‡n vÃ o thÆ° viá»‡n.",
            "ThÃ nh cÃ´ng",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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

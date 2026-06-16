using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Cửa sổ thêm mẫu lỗ tròn vào thư viện (Morphology Close).
/// </summary>
public partial class FiducialTemplateWindow : System.Windows.Window
{
    private readonly FiducialTemplateViewModel _viewModel;
    private bool _isDragging;
    private System.Windows.Point _dragStart;
    private readonly List<Rectangle> _regionRects = [];

    public FiducialTemplateWindow(IFiducialHoleTemplateService templateService)
    {
        InitializeComponent();
        _viewModel = new FiducialTemplateViewModel(templateService);
        DataContext = _viewModel;
        PendingRegionsList.ItemsSource = _viewModel.PendingRegions;
        SavedTemplatesList.ItemsSource = _viewModel.SavedTemplateFiles;

        _viewModel.ImageReady += bitmap =>
            Dispatcher.InvokeAsync(() =>
            {
                SourceImage.Source = bitmap;
                ImagePlaceholder.Visibility = Visibility.Collapsed;
                RedrawRegionRects();
            });

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FiducialTemplateViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };

        _viewModel.PendingRegionsChanged += () =>
            Dispatcher.InvokeAsync(RedrawRegionRects);

        _viewModel.SavedTemplatesChanged += () =>
            Dispatcher.InvokeAsync(() => SavedTemplatesList.Items.Refresh());
    }

    public FiducialTemplateWindow()
        : this(FiducialHoleServices.TemplateService)
    {
    }

    public void LoadFrame(Mat frame) => _viewModel.LoadFrame(frame);

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

        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0 || renderRect.Height <= 0) return;

        double relX = (rx - renderRect.X) / renderRect.Width;
        double relY = (ry - renderRect.Y) / renderRect.Height;
        double relW = rw / renderRect.Width;
        double relH = rh / renderRect.Height;

        relX = Math.Clamp(relX, 0, 1);
        relY = Math.Clamp(relY, 0, 1);
        relW = Math.Clamp(relW, 0, 1 - relX);
        relH = Math.Clamp(relH, 0, 1 - relY);

        if (relW > 0.001 && relH > 0.001)
            _viewModel.AddRegion(relX, relY, relW, relH);
    }

    private void PendingRegionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => RedrawRegionRects();

    private void BtnRemovePendingRegion_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: FiducialHoleRegionItem item }) return;
        _viewModel.RemovePendingRegion(item);
    }

    private void BtnClearSelection_Click(object sender, RoutedEventArgs e)
        => _viewModel.ClearPendingRegions();

    private void BtnDeleteTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string fileName }) return;
        _viewModel.DeleteSavedTemplate(fileName);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
        => _viewModel.SavePendingTemplates();

    private void RedrawRegionRects()
    {
        foreach (var rect in _regionRects)
            RegionCanvas.Children.Remove(rect);
        _regionRects.Clear();

        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0) return;

        var selectedItem = PendingRegionsList.SelectedItem as FiducialHoleRegionItem;

        foreach (var region in _viewModel.PendingRegions)
        {
            var color = region.RegionColor;
            bool isSelected = region == selectedItem;

            double x = renderRect.X + region.RelX * renderRect.Width;
            double y = renderRect.Y + region.RelY * renderRect.Height;
            double w = region.RelWidth * renderRect.Width;
            double h = region.RelHeight * renderRect.Height;

            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(isSelected ? Colors.White : color),
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

    private System.Windows.Rect GetImageRenderRect()
    {
        int imgW = _viewModel.ImageWidth;
        int imgH = _viewModel.ImageHeight;
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

    private void Window_Closed(object sender, EventArgs e)
        => _viewModel.Dispose();
}

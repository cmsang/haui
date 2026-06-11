using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind của TemplateViewerWindow — chỉ chứa logic giao diện.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="TemplateViewerViewModel"/>.
/// </summary>
public partial class TemplateViewerWindow : System.Windows.Window
{
    private readonly TemplateViewerViewModel _viewModel;

    // Danh sách hình chữ nhật vùng đã vẽ lên ảnh mẫu
    private readonly List<Rectangle> _regionRects = [];

    public TemplateViewerWindow()
    {
        InitializeComponent();
        _viewModel = new TemplateViewerViewModel(new TemplateLibraryService());
        DataContext = _viewModel;

        TemplatesGrid.ItemsSource = _viewModel.Templates;
        RegionsGrid.ItemsSource = _viewModel.Regions;

        // Đồng bộ StatusText từ ViewModel
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TemplateViewerViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };

        // Cập nhật ảnh preview
        _viewModel.PreviewImageChanged += bitmap =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                PreviewImage.Source = bitmap;
                PreviewPlaceholder.Visibility = bitmap is null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                RedrawRegionRects();
            });
        };

        Loaded += (_, _) =>
        {
            ApplyDeveloperModeUi();
            _viewModel.LoadTemplates();
        };
    }

    private void ApplyDeveloperModeUi()
    {
        var developerMode = new AppSettingService().Load().DeveloperMode;
        BtnEditTemplate.Visibility = developerMode ? Visibility.Visible : Visibility.Collapsed;
    }

    // ──── Vẽ lại các hình chữ nhật vùng lên ảnh mẫu ─────────────────────────

    private void RedrawRegionRects()
    {
        foreach (var rect in _regionRects)
            RegionOverlayCanvas.Children.Remove(rect);
        _regionRects.Clear();

        if (_viewModel.Regions.Count == 0) return;

        var renderRect = GetImageRenderRect();
        if (renderRect.Width <= 0) return;

        foreach (var region in _viewModel.Regions)
        {
            var color = region.RegionColor;
            double x = renderRect.X + region.RelX * renderRect.Width;
            double y = renderRect.Y + region.RelY * renderRect.Height;
            double w = region.RelWidth * renderRect.Width;
            double h = region.RelHeight * renderRect.Height;

            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B)),
                Width = w,
                Height = h
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            RegionOverlayCanvas.Children.Add(rect);
            _regionRects.Add(rect);
        }
    }

    /// <summary>
    /// Tính hình chữ nhật hiển thị thực sự của ảnh mẫu trên canvas (Stretch=Uniform).
    /// Canvas có Margin=8 giống Image nên không cần bù thêm.
    /// </summary>
    private System.Windows.Rect GetImageRenderRect()
    {
        int imgW = _viewModel.BoardWidth;
        int imgH = _viewModel.BoardHeight;
        if (imgW <= 0 || imgH <= 0) return System.Windows.Rect.Empty;

        double canvasW = RegionOverlayCanvas.ActualWidth;
        double canvasH = RegionOverlayCanvas.ActualHeight;
        if (canvasW <= 0 || canvasH <= 0) return System.Windows.Rect.Empty;

        double scale = Math.Min(canvasW / imgW, canvasH / imgH);
        double renderW = imgW * scale;
        double renderH = imgH * scale;
        double offsetX = (canvasW - renderW) / 2;
        double offsetY = (canvasH - renderH) / 2;
        return new System.Windows.Rect(offsetX, offsetY, renderW, renderH);
    }

    private void RegionOverlayCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        => RedrawRegionRects();

    // ──── Event Handlers ──────────────────────────────────────────────────────

    private void TemplatesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TemplatesGrid.SelectedItem is TemplateEntryItem item)
            _viewModel.SelectTemplate(item);
    }

    private void BtnEditTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (!new AppSettingService().Load().DeveloperMode)
            return;

        if (TemplatesGrid.SelectedItem is not TemplateEntryItem item)
        {
            MessageBox.Show("Vui lòng chọn một ảnh mẫu để sửa.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var boardMat = _viewModel.GetSelectedBoardImage(item);
        if (boardMat is null)
        {
            MessageBox.Show("Không tìm thấy ảnh bo mạch của mẫu này.", "Lỗi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var editWindow = new CreateTemplateWindow
        {
            Owner = this,
            Title = $"Sửa mẫu — {item.Name}"
        };

        // Nạp ảnh và vùng hiện tại vào form tạo mẫu
        editWindow.Show();
        editWindow.LoadExistingTemplate(boardMat, item.Source.Regions);
        boardMat.Dispose();

        // Khi người dùng bấm Lưu trong form chỉnh sửa → cập nhật lại vùng
        editWindow.RegionsSaved += newRegions =>
        {
            _viewModel.UpdateTemplateRegions(item, newRegions);
            editWindow.Close();
        };
    }

    private void BtnDeleteTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (TemplatesGrid.SelectedItem is not TemplateEntryItem item)
        {
            MessageBox.Show("Vui lòng chọn một ảnh mẫu để xóa.", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Bạn có chắc muốn xóa mẫu \"{item.Name}\"?\nThao tác này chưa lưu file cho đến khi bạn bấm Lưu.",
            "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
            _viewModel.DeleteTemplate(item);
    }

    private void BtnSaveLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.TrySaveLibrary(out var error))
        {
            MessageBox.Show(error, "Không thể lưu thư viện",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show("Đã lưu thư viện ảnh mẫu thành công.", "Thành công",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "Có thay đổi chưa được lưu. Bạn có muốn lưu trước khi đóng không?",
                "Lưu thay đổi?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!_viewModel.TrySaveLibrary(out var error))
                {
                    MessageBox.Show(error, "Không thể lưu thư viện",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else if (result == MessageBoxResult.Cancel)
                return;
        }
        Close();
    }

    private void Window_Closed(object sender, EventArgs e)
        => _viewModel.Dispose();
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind cá»§a TemplateViewerWindow â€” chá»‰ chá»©a logic giao diá»‡n.
/// ToÃ n bá»™ nghiá»‡p vá»¥ Ä‘Æ°á»£c uá»· thÃ¡c cho <see cref="TemplateViewerViewModel"/>.
/// </summary>
public partial class TemplateViewerWindow : System.Windows.Window
{
    private readonly TemplateViewerViewModel _viewModel;

    // Danh sÃ¡ch hÃ¬nh chá»¯ nháº­t vÃ¹ng Ä‘Ã£ váº½ lÃªn áº£nh máº«u
    private readonly List<Rectangle> _regionRects = [];

    public TemplateViewerWindow()
    {
        InitializeComponent();
        _viewModel = new TemplateViewerViewModel(new TemplateLibraryService());
        DataContext = _viewModel;

        TemplatesGrid.ItemsSource = _viewModel.Templates;
        RegionsGrid.ItemsSource = _viewModel.Regions;

        // Äá»“ng bá»™ StatusText tá»« ViewModel
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TemplateViewerViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };

        // Cáº­p nháº­t áº£nh preview
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

        Loaded += (_, _) => _viewModel.LoadTemplates();
    }

    // â”€â”€â”€â”€ Váº½ láº¡i cÃ¡c hÃ¬nh chá»¯ nháº­t vÃ¹ng lÃªn áº£nh máº«u â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
    /// TÃ­nh hÃ¬nh chá»¯ nháº­t hiá»ƒn thá»‹ thá»±c sá»± cá»§a áº£nh máº«u trÃªn canvas (Stretch=Uniform).
    /// Canvas cÃ³ Margin=8 giá»‘ng Image nÃªn khÃ´ng cáº§n bÃ¹ thÃªm.
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

    // â”€â”€â”€â”€ Event Handlers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void TemplatesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TemplatesGrid.SelectedItem is TemplateEntryItem item)
            _viewModel.SelectTemplate(item);
    }

    private void BtnEditTemplate_Click(object sender, RoutedEventArgs e)
    {
        if (TemplatesGrid.SelectedItem is not TemplateEntryItem item)
        {
            MessageBox.Show("Vui lÃ²ng chá»n má»™t áº£nh máº«u Ä‘á»ƒ sá»­a.", "ThÃ´ng bÃ¡o",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var boardMat = _viewModel.GetSelectedBoardImage(item);
        if (boardMat is null)
        {
            MessageBox.Show("KhÃ´ng tÃ¬m tháº¥y áº£nh bo máº¡ch cá»§a máº«u nÃ y.", "Lá»—i",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var editWindow = new CreateTemplateWindow
        {
            Owner = this,
            Title = $"Sá»­a máº«u â€” {item.Name}"
        };

        // Náº¡p áº£nh vÃ  vÃ¹ng hiá»‡n táº¡i vÃ o form táº¡o máº«u
        editWindow.Show();
        editWindow.LoadExistingTemplate(boardMat, item.Source.Regions);
        boardMat.Dispose();

        // Khi ngÆ°á»i dÃ¹ng báº¥m LÆ°u trong form chá»‰nh sá»­a â†’ cáº­p nháº­t láº¡i vÃ¹ng
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
            MessageBox.Show("Vui lÃ²ng chá»n má»™t áº£nh máº«u Ä‘á»ƒ xÃ³a.", "ThÃ´ng bÃ¡o",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Báº¡n cÃ³ cháº¯c muá»‘n xÃ³a máº«u \"{item.Name}\"?\nThao tÃ¡c nÃ y chÆ°a lÆ°u file cho Ä‘áº¿n khi báº¡n báº¥m LÆ°u.",
            "XÃ¡c nháº­n xÃ³a", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
            _viewModel.DeleteTemplate(item);
    }

    private void BtnSaveLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.TrySaveLibrary(out var error))
        {
            MessageBox.Show(error, "KhÃ´ng thá»ƒ lÆ°u thÆ° viá»‡n",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show("ÄÃ£ lÆ°u thÆ° viá»‡n áº£nh máº«u thÃ nh cÃ´ng.", "ThÃ nh cÃ´ng",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "CÃ³ thay Ä‘á»•i chÆ°a Ä‘Æ°á»£c lÆ°u. Báº¡n cÃ³ muá»‘n lÆ°u trÆ°á»›c khi Ä‘Ã³ng khÃ´ng?",
                "LÆ°u thay Ä‘á»•i?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!_viewModel.TrySaveLibrary(out var error))
                {
                    MessageBox.Show(error, "KhÃ´ng thá»ƒ lÆ°u thÆ° viá»‡n",
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

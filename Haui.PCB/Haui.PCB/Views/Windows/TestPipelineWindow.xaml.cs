using System.Windows;
using Microsoft.Win32;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind cá»§a TestPipelineWindow â€” chá»‰ chá»©a logic giao diá»‡n thuáº§n tÃºy.
/// ToÃ n bá»™ nghiá»‡p vá»¥ xá»­ lÃ½ áº£nh Ä‘Æ°á»£c uá»· thÃ¡c cho <see cref="TestPipelineViewModel"/>.
/// </summary>
public partial class TestPipelineWindow : System.Windows.Window
{
    private readonly TestPipelineViewModel _viewModel;

    public TestPipelineWindow()
    {
        InitializeComponent();
        var libraryService = new TemplateLibraryService();
        var comparisonService = new RegionComparisonService();
        _viewModel = new TestPipelineViewModel(
            new PcbSegmentationService(),
            new CompositeTemplateMatchService(libraryService, comparisonService));
        DataContext = _viewModel;

        // Láº¯ng nghe áº£nh bo máº¡ch Ä‘Ã£ cáº¯t sáºµn sÃ ng
        _viewModel.ProcessedImageReady += bitmap =>
        {
            if (bitmap is null)
            {
                ProcessedImage.Source = null;
                ProcessedPlaceholder.Visibility = Visibility.Visible;
                ProcessedPlaceholder.Text = "KhÃ´ng tÃ¬m tháº¥y bo máº¡ch";
            }
            else
            {
                ProcessedImage.Source = bitmap;
                ProcessedPlaceholder.Visibility = Visibility.Collapsed;
            }
        };

        // Äá»“ng bá»™ StatusText vÃ  tráº¡ng thÃ¡i nÃºt tá»« ViewModel
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestPipelineViewModel.StatusText))
                StatusText.Text = _viewModel.StatusText;
            else if (e.PropertyName == nameof(TestPipelineViewModel.IsBusy))
            {
                BtnTest.IsEnabled = !_viewModel.IsBusy && _viewModel.HasSource;
                BtnSelectImage.IsEnabled = !_viewModel.IsBusy;
            }
            else if (e.PropertyName == nameof(TestPipelineViewModel.HasSource))
                BtnTest.IsEnabled = _viewModel.HasSource && !_viewModel.IsBusy;
        };

        MatchedGrid.ItemsSource = _viewModel.MatchedRegions;
        DifferentGrid.ItemsSource = _viewModel.DifferentRegions;

        // Hiá»ƒn thá»‹ áº£nh Ä‘Ã£ váº½ annotations lÃªn áº£nh bo máº¡ch sau khi so sÃ¡nh xong
        _viewModel.AnnotatedImageReady += bitmap =>
        {
            if (bitmap is not null)
            {
                ProcessedImage.Source = bitmap;
                ProcessedPlaceholder.Visibility = Visibility.Collapsed;
            }
        };
    }

    // â”€â”€â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Náº¡p áº£nh tá»« bÃªn ngoÃ i (tá»« camera chá»¥p) â€” pipeline tá»± Ä‘á»™ng cháº¡y.</summary>
    public void LoadImage(Mat mat)
    {
        _viewModel.LoadImage(mat);
    }

    // â”€â”€â”€â”€ Event Handlers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chá»n áº£nh bo máº¡ch",
            Filter = "áº¢nh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        _viewModel.LoadImageFromFile(dialog.FileName);
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.RunSegmentationAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}

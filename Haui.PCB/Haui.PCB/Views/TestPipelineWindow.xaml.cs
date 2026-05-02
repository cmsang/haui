using System.Windows;
using Microsoft.Win32;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views;

/// <summary>
/// Code-behind của TestPipelineWindow — chỉ chứa logic giao diện thuần túy.
/// Toàn bộ nghiệp vụ xử lý ảnh được uỷ thác cho <see cref="TestPipelineViewModel"/>.
/// </summary>
public partial class TestPipelineWindow : System.Windows.Window
{
    private readonly TestPipelineViewModel _viewModel;

    public TestPipelineWindow()
    {
        InitializeComponent();
        _viewModel = new TestPipelineViewModel(
            new PcbSegmentationService(),
            new TemplateRegionService(),
            new RegionComparisonService());
        DataContext = _viewModel;

        // Lắng nghe ảnh bo mạch đã cắt sẵn sàng
        _viewModel.ProcessedImageReady += bitmap =>
        {
            if (bitmap is null)
            {
                ProcessedImage.Source = null;
                ProcessedPlaceholder.Visibility = Visibility.Visible;
                ProcessedPlaceholder.Text = "Không tìm thấy bo mạch";
            }
            else
            {
                ProcessedImage.Source = bitmap;
                ProcessedPlaceholder.Visibility = Visibility.Collapsed;
            }
        };

        // Đồng bộ StatusText và trạng thái nút từ ViewModel
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

        // Bind 2 grid kết quả
        MatchedGrid.ItemsSource = _viewModel.MatchedRegions;
        DifferentGrid.ItemsSource = _viewModel.DifferentRegions;

        // Hiển thị ảnh đã vẽ annotations lên ảnh bo mạch sau khi so sánh xong
        _viewModel.AnnotatedImageReady += bitmap =>
        {
            if (bitmap is not null)
            {
                ProcessedImage.Source = bitmap;
                ProcessedPlaceholder.Visibility = Visibility.Collapsed;
            }
        };
    }

    // ──── Public API ──────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (từ camera chụp) — pipeline tự động chạy.</summary>
    public void LoadImage(Mat mat)
    {
        _viewModel.LoadImage(mat);
    }

    // ──── Event Handlers ──────────────────────────────────────────────────────

    private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh bo mạch",
            Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
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

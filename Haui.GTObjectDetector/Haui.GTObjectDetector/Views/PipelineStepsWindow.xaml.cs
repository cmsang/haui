using System.Windows;
using Haui.GTObjectDetector.Processing;
using Haui.GTObjectDetector.ViewModels;
using OpenCvSharp;

namespace Haui.GTObjectDetector.Views;

/// <summary>
/// Code-behind của PipelineStepsWindow — chỉ chứa logic giao diện thuần túy.
/// Toàn bộ nghiệp vụ xử lý ảnh được uỷ thác cho <see cref="PipelineStepsViewModel"/>.
/// </summary>
public partial class PipelineStepsWindow : System.Windows.Window
{
    private readonly PipelineStepsViewModel _viewModel;

    public PipelineStepsWindow()
    {
        InitializeComponent();
        _viewModel = new PipelineStepsViewModel(new PipelineDebugService());
        DataContext = _viewModel;

        // Bind danh sách bước vào ItemsControl
        StepsPanel.ItemsSource = _viewModel.Steps;

        // Đồng bộ trạng thái
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PipelineStepsViewModel.StatusText))
                StatusText.Text = _viewModel.StatusText;
            else if (e.PropertyName == nameof(PipelineStepsViewModel.IsBusy))
                BusyIndicator.Visibility = _viewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
        };
    }

    // ──── Public API ──────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (từ camera chụp) — pipeline tự động chạy.</summary>
    public void LoadImage(Mat mat)
    {
        _viewModel.LoadImage(mat);
    }

    // ──── Lifecycle ───────────────────────────────────────────────────────────

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}

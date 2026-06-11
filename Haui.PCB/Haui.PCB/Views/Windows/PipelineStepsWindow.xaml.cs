using System.Windows;
using Haui.PCB.ViewModels;
using OpenCvSharp;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Code-behind cá»§a PipelineStepsWindow â€” chá»‰ chá»©a logic giao diá»‡n thuáº§n tÃºy.
/// ToÃ n bá»™ nghiá»‡p vá»¥ xá»­ lÃ½ áº£nh Ä‘Æ°á»£c uá»· thÃ¡c cho <see cref="PipelineStepsViewModel"/>.
/// </summary>
public partial class PipelineStepsWindow : System.Windows.Window
{
    private readonly PipelineStepsViewModel _viewModel;

    public PipelineStepsWindow()
    {
        InitializeComponent();
        _viewModel = new PipelineStepsViewModel(new PipelineDebugService());
        DataContext = _viewModel;

        // Bind danh sÃ¡ch bÆ°á»›c vÃ o ItemsControl
        StepsPanel.ItemsSource = _viewModel.Steps;

        // Äá»“ng bá»™ tráº¡ng thÃ¡i
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PipelineStepsViewModel.StatusText))
                StatusText.Text = _viewModel.StatusText;
            else if (e.PropertyName == nameof(PipelineStepsViewModel.IsBusy))
                BusyIndicator.Visibility = _viewModel.IsBusy ? Visibility.Visible : Visibility.Collapsed;
        };
    }

    // â”€â”€â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Náº¡p áº£nh tá»« bÃªn ngoÃ i (tá»« camera chá»¥p) â€” pipeline tá»± Ä‘á»™ng cháº¡y.</summary>
    public void LoadImage(Mat mat)
    {
        _viewModel.LoadImage(mat);
    }

    // â”€â”€â”€â”€ Lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}

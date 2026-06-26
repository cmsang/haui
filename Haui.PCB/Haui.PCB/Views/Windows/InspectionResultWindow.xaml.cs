using System.Windows;
using System.Windows.Media;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Developer-mode inspection result detail — annotated image and full result list.
/// </summary>
public partial class InspectionResultWindow : Window
{
    private static readonly SolidColorBrush PassBackgroundBrush = new(Color.FromRgb(0xE8, 0xF5, 0xE9));
    private static readonly SolidColorBrush PassForegroundBrush = new(Color.FromRgb(0x00, 0x75, 0x2A));
    private static readonly SolidColorBrush FailBackgroundBrush = new(Color.FromRgb(0xFF, 0xED, 0xED));
    private static readonly SolidColorBrush FailForegroundBrush = new(Color.FromRgb(0xD1, 0x34, 0x38));
    private static readonly SolidColorBrush IdleBackgroundBrush = new(Color.FromRgb(0xFA, 0xFA, 0xFA));
    private static readonly SolidColorBrush IdleForegroundBrush = new(Color.FromRgb(0x88, 0x88, 0x88));

    public InspectionResultWindow(TestPipelineViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ApplyPassFailStyle(viewModel.IsFullMatch);
        BindResultImage(viewModel.ResultAnnotatedImage);
    }

    private void BindResultImage(System.Windows.Media.Imaging.BitmapSource? bitmap)
    {
        if (bitmap is null)
        {
            ResultImage.Source = null;
            ImagePlaceholder.Visibility = Visibility.Visible;
            return;
        }

        ResultImage.Source = bitmap;
        ImagePlaceholder.Visibility = Visibility.Collapsed;
    }

    private void ApplyPassFailStyle(bool? isFullMatch)
    {
        switch (isFullMatch)
        {
            case true:
                PassFailPanel.Background = PassBackgroundBrush;
                PassFailText.Foreground = PassForegroundBrush;
                break;
            case false:
                PassFailPanel.Background = FailBackgroundBrush;
                PassFailText.Foreground = FailForegroundBrush;
                break;
            default:
                PassFailPanel.Background = IdleBackgroundBrush;
                PassFailText.Foreground = IdleForegroundBrush;
                break;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}

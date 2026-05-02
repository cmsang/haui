using System.Windows;
using System.Windows.Controls;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views;

/// <summary>
/// Code-behind của TemplateViewerWindow — chỉ chứa logic giao diện.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="TemplateViewerViewModel"/>.
/// </summary>
public partial class TemplateViewerWindow : System.Windows.Window
{
    private readonly TemplateViewerViewModel _viewModel;

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
            });
        };

        Loaded += (_, _) => _viewModel.LoadTemplates();
    }

    // ──── Event Handlers ──────────────────────────────────────────────────────

    private void TemplatesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TemplatesGrid.SelectedItem is TemplateEntryItem item)
            _viewModel.SelectTemplate(item);
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
        => Close();

    private void Window_Closed(object sender, EventArgs e)
        => _viewModel.Dispose();
}

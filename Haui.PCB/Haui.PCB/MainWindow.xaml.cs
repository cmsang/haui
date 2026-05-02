using System.Windows;
using System.Windows.Controls;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;
using Haui.PCB.Views;

namespace Haui.PCB;

/// <summary>
/// Code-behind của MainWindow — chỉ chứa logic giao diện thuần túy.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="MainViewModel"/>.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel(new CameraService());
        DataContext = _viewModel;

        // Lắng nghe frame mới để hiển thị lên UI
        _viewModel.FrameReady += bitmap =>
            Dispatcher.InvokeAsync(() => CameraImage.Source = bitmap);

        // Đồng bộ StatusText và FPS từ ViewModel
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentFps))
                Dispatcher.InvokeAsync(() =>
                    FpsText.Text = _viewModel.CurrentFps > 0 ? $"FPS: {_viewModel.CurrentFps}" : string.Empty);
            else if (e.PropertyName == nameof(MainViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };

        // Nhận frame test → mở TestPipelineWindow trên UI thread
        _viewModel.TestFrameCaptured += frame =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                var testWindow = new TestPipelineWindow { Owner = this };
                testWindow.LoadImage(frame);
                frame.Dispose();
                testWindow.Show();
            });
        };

        Loaded += async (_, _) => await LoadCamerasAsync();
    }

    // ──── Camera Loading ──────────────────────────────────────────────────────

    private async Task LoadCamerasAsync()
    {
        SetToolbarEnabled(false);
        await _viewModel.RefreshCamerasAsync();

        CameraComboBox.Items.Clear();

        if (_viewModel.Cameras.Count == 0)
        {
            CameraComboBox.Items.Add("Không tìm thấy camera");
            BtnStart.IsEnabled = false;
            return;
        }

        foreach (var cam in _viewModel.Cameras)
            CameraComboBox.Items.Add(cam.Name);

        CameraComboBox.SelectedIndex = 0;
        // LoadResolutionsAsync được gọi tự động qua SelectionChanged
    }

    private async Task LoadResolutionsAsync(string monikerString)
    {
        ResolutionComboBox.IsEnabled = false;
        ResolutionComboBox.Items.Clear();
        BtnStart.IsEnabled = false;

        await _viewModel.LoadResolutionsAsync(monikerString);

        if (_viewModel.Resolutions.Count == 0)
        {
            ResolutionComboBox.Items.Add("N/A");
            ResolutionComboBox.SelectedIndex = 0;
            return;
        }

        foreach (var r in _viewModel.Resolutions)
            ResolutionComboBox.Items.Add(r.Label);

        ResolutionComboBox.SelectedIndex = _viewModel.GetDefaultResolutionIndex();
        ResolutionComboBox.IsEnabled = true;
        BtnStart.IsEnabled = true;
    }

    // ──── Event Handlers ──────────────────────────────────────────────────────

    private async void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.Cameras.Count == 0 || CameraComboBox.SelectedIndex < 0)
            return;

        var camera = _viewModel.Cameras[CameraComboBox.SelectedIndex];
        await LoadResolutionsAsync(camera.MonikerString);
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Cameras.Count == 0 || CameraComboBox.SelectedIndex < 0) return;

        var camera = _viewModel.Cameras[CameraComboBox.SelectedIndex];

        var parts = ResolutionComboBox.SelectedItem?.ToString()?.Split('×');
        if (parts is null || parts.Length != 2) return;
        if (!int.TryParse(parts[0], out int w) || !int.TryParse(parts[1], out int h)) return;

        try
        {
            _viewModel.StartCamera(camera.MonikerString, w, h);

            SetToolbarEnabled(false);
            BtnStop.IsEnabled = true;
            BtnTest.IsEnabled = true;
            CameraPlaceholder.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Lỗi: {ex.Message}";
            SetToolbarEnabled(true);
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StopCamera();

        SetToolbarEnabled(true);
        BtnStop.IsEnabled = false;
        BtnTest.IsEnabled = false;
        CameraImage.Source = null;
        CameraPlaceholder.Visibility = Visibility.Visible;
        FpsText.Text = string.Empty;
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        BtnTest.IsEnabled = false;
        try
        {
            await _viewModel.CaptureTestFrameAsync();
        }
        finally
        {
            BtnTest.IsEnabled = _viewModel.IsRunning;
        }
    }

    private void SetToolbarEnabled(bool enabled)
    {
        CameraComboBox.IsEnabled = enabled;
        ResolutionComboBox.IsEnabled = enabled;
        BtnStart.IsEnabled = enabled && _viewModel.Cameras.Count > 0;
        BtnStop.IsEnabled = false;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}

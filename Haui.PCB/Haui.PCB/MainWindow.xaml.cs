using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Haui.PCB.Processing;
using Haui.PCB.Views;

namespace Haui.PCB;

public partial class MainWindow : System.Windows.Window
{
    private readonly CameraService _cameraService = new();

    // Danh sách camera dò được trên máy
    private List<CameraInfo> _cameras = [];

    // Đo FPS
    private int _frameCount;
    private readonly Stopwatch _fpsStopwatch = Stopwatch.StartNew();

    public MainWindow()
    {
        InitializeComponent();
        _cameraService.FrameArrived += OnFrameArrived;
        Loaded += async (_, _) => await RefreshCamerasAsync();
    }

    /// <summary>
    /// Dò tìm camera thực tế trên máy và nạp vào ComboBox.
    /// </summary>
    private async Task RefreshCamerasAsync()
    {
        SetToolbarEnabled(false);
        StatusText.Text = "Đang dò tìm camera...";
        CameraComboBox.Items.Clear();
        ResolutionComboBox.Items.Clear();

        _cameras = await Task.Run(CameraService.EnumerateCameras);

        if (_cameras.Count == 0)
        {
            CameraComboBox.Items.Add("Không tìm thấy camera");
            StatusText.Text = "Không phát hiện camera nào.";
            BtnStart.IsEnabled = false;
            return;
        }

        foreach (var cam in _cameras)
            CameraComboBox.Items.Add(cam.Name);

        CameraComboBox.SelectedIndex = 0;
        // LoadResolutionsAsync được gọi tự động qua SelectionChanged
    }

    /// <summary>
    /// Tải độ phân giải hỗ trợ của camera đang chọn.
    /// </summary>
    private async Task LoadResolutionsAsync(string monikerString)
    {
        ResolutionComboBox.IsEnabled = false;
        ResolutionComboBox.Items.Clear();
        StatusText.Text = "Đang đọc độ phân giải...";

        var resolutions = await Task.Run(() => CameraService.GetSupportedResolutions(monikerString));

        if (resolutions.Count == 0)
        {
            ResolutionComboBox.Items.Add("N/A");
            ResolutionComboBox.SelectedIndex = 0;
            BtnStart.IsEnabled = false;
            StatusText.Text = "Camera không phản hồi độ phân giải.";
            return;
        }

        foreach (var r in resolutions)
            ResolutionComboBox.Items.Add(r.Label);

        // Ưu tiên chọn 1280×720 nếu có, không thì chọn giữa danh sách
        int defaultIdx = resolutions.FindIndex(r => r.Width == 1280 && r.Height == 720);
        ResolutionComboBox.SelectedIndex = defaultIdx >= 0 ? defaultIdx : resolutions.Count / 2;

        ResolutionComboBox.IsEnabled = true;
        BtnStart.IsEnabled = true;
        StatusText.Text = $"Sẵn sàng. {resolutions.Count} độ phân giải hỗ trợ.";
    }

    private async void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cameras.Count == 0 || CameraComboBox.SelectedIndex < 0)
            return;

        var camera = _cameras[CameraComboBox.SelectedIndex];
        await LoadResolutionsAsync(camera.MonikerString);
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_cameras.Count == 0 || CameraComboBox.SelectedIndex < 0) return;

        var camera = _cameras[CameraComboBox.SelectedIndex];

        // Lấy độ phân giải từ tag đã lưu
        var parts = ResolutionComboBox.SelectedItem?.ToString()?.Split('×');
        if (parts is null || parts.Length != 2) return;
        if (!int.TryParse(parts[0], out int w) || !int.TryParse(parts[1], out int h)) return;

        try
        {
            _cameraService.Start(camera.MonikerString, w, h);

            SetToolbarEnabled(false);
            BtnStop.IsEnabled = true;
            BtnTest.IsEnabled = true;
            CameraPlaceholder.Visibility = Visibility.Collapsed;
            StatusText.Text = $"Camera {camera.Name} đang chạy ({w}×{h})";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Lỗi: {ex.Message}";
            SetToolbarEnabled(true);
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        _cameraService.Stop();

        SetToolbarEnabled(true);
        BtnStop.IsEnabled = false;
        BtnTest.IsEnabled = false;
        CameraImage.Source = null;
        CameraPlaceholder.Visibility = Visibility.Visible;
        FpsText.Text = string.Empty;
        StatusText.Text = "Camera đã dừng.";
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Đang chụp ảnh...";
        BtnTest.IsEnabled = false;

        Mat? frame = null;
        try
        {
            frame = await Task.Run(() => _cameraService.GrabFrame());

            if (frame is null || frame.Empty())
            {
                StatusText.Text = "Không thể chụp ảnh từ camera.";
                return;
            }

            var testWindow = new TestPipelineWindow { Owner = this };
            testWindow.LoadImage(frame);
            testWindow.Show();

            StatusText.Text = "Đã mở Test Pipeline.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Lỗi: {ex.Message}";
        }
        finally
        {
            frame?.Dispose();
            BtnTest.IsEnabled = _cameraService.IsRunning;
        }
    }

    /// <summary>
    /// Bật/tắt các điều khiển chọn camera khi đang chạy hoặc đang dò.
    /// </summary>
    private void SetToolbarEnabled(bool enabled)
    {
        CameraComboBox.IsEnabled = enabled;
        ResolutionComboBox.IsEnabled = enabled;
        BtnStart.IsEnabled = enabled && _cameras.Count > 0;
        BtnStop.IsEnabled = false;
    }

    /// <summary>
    /// Nhận frame từ CameraService và hiển thị lên UI.
    /// </summary>
    private void OnFrameArrived(Mat frame)
    {
        _frameCount++;
        if (_fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
        {
            int fps = _frameCount;
            _frameCount = 0;
            _fpsStopwatch.Restart();

            Dispatcher.InvokeAsync(() => FpsText.Text = $"FPS: {fps}");
        }

        // Convert sang BitmapSource trên background thread, freeze để cross-thread an toàn
        var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(frame);
        bitmap.Freeze();

        Dispatcher.InvokeAsync(() => CameraImage.Source = bitmap);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _cameraService.Dispose();
    }
}

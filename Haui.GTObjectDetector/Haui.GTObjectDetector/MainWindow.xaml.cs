using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Haui.GTObjectDetector.Processing;
using Haui.GTObjectDetector.ViewModels;
using Haui.GTObjectDetector.Views;

namespace Haui.GTObjectDetector;

/// <summary>
/// Code-behind của MainWindow — chỉ chứa logic giao diện thuần túy.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="MainViewModel"/>.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;

    // Trạng thái kéo thả chọn vùng
    private bool _isSelectingRegion;
    private bool _isDragging;
    private System.Windows.Point _dragStart;

    public MainWindow()
    {
        InitializeComponent();

        // Khởi tạo detection service nếu model tồn tại
        IObjectDetectionService? detectionService = null;
        const string modelPath = "best.onnx";
        const string classPath = "object_class.txt";
        if (File.Exists(modelPath) && File.Exists(classPath))
        {
            detectionService = new YoloDetectionService(modelPath, classPath);
        }

        _viewModel = new MainViewModel(new CameraService(), detectionService);
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

        // Nhận frame Test 2 → mở PipelineStepsWindow trên UI thread
        _viewModel.Test2FrameCaptured += frame =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                var stepsWindow = new PipelineStepsWindow { Owner = this };
                stepsWindow.LoadImage(frame);
                frame.Dispose();
                stepsWindow.Show();
            });
        };

        // Nhận frame tạo mẫu → mở CreateTemplateWindow trên UI thread
        _viewModel.TemplateFrameCaptured += frame =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                var templateWindow = new CreateTemplateWindow { Owner = this };
                templateWindow.LoadFrame(frame);
                frame.Dispose();
                templateWindow.Show();
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
            BtnTest2.IsEnabled = true;
            BtnSelectRegion.IsEnabled = true;
            BtnCreateTemplate.IsEnabled = true;
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
        BtnTest2.IsEnabled = false;
        BtnSelectRegion.IsEnabled = false;
        BtnCreateTemplate.IsEnabled = false;
        // Thoát chế độ chọn vùng nếu đang chọn
        ExitSelectMode();
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

    private async void BtnTest2_Click(object sender, RoutedEventArgs e)
    {
        BtnTest2.IsEnabled = false;
        try
        {
            await _viewModel.CaptureTest2FrameAsync();
        }
        finally
        {
            BtnTest2.IsEnabled = _viewModel.IsRunning;
        }
    }

    private async void BtnCreateTemplate_Click(object sender, RoutedEventArgs e)
    {
        BtnCreateTemplate.IsEnabled = false;
        try
        {
            await _viewModel.CaptureTemplateFrameAsync();
        }
        finally
        {
            BtnCreateTemplate.IsEnabled = _viewModel.IsRunning;
        }
    }

    private void BtnViewTemplates_Click(object sender, RoutedEventArgs e)
    {
        var viewerWindow = new Views.TemplateViewerWindow { Owner = this };
        viewerWindow.Show();
    }

    private void SetToolbarEnabled(bool enabled)
    {
        CameraComboBox.IsEnabled = enabled;
        ResolutionComboBox.IsEnabled = enabled;
        BtnStart.IsEnabled = enabled && _viewModel.Cameras.Count > 0;
        BtnStop.IsEnabled = false;
    }

    private void BtnDetection_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.IsDetectionAvailable)
        {
            StatusText.Text = "Model YOLO chưa được nạp (kiểm tra best.onnx và object_class.txt).";
            return;
        }
        _viewModel.IsDetectionEnabled = !_viewModel.IsDetectionEnabled;
        if (sender is Button btn)
            btn.Content = _viewModel.IsDetectionEnabled ? "🟢 Detection: BẬT" : "🔴 Detection: TẮT";
        StatusText.Text = _viewModel.IsDetectionEnabled
            ? "Nhận diện YOLO đang chạy."
            : "Nhận diện YOLO đã tắt.";
    }

    private void BtnSelectRegion_Click(object sender, RoutedEventArgs e)
    {
        if (_isSelectingRegion)
        {
            ExitSelectMode();
            return;
        }
        _isSelectingRegion = true;
        BtnSelectRegion.Content = "⏹ Hủy chọn";
        SelectionCanvas.IsHitTestVisible = true;
        SelectionCanvas.Cursor = Cursors.Cross;
        StatusText.Text = "Kéo thả để chọn vùng nhận diện...";
    }

    private void ExitSelectMode()
    {
        _isSelectingRegion = false;
        _isDragging = false;
        BtnSelectRegion.Content = "🔲 Chọn vùng";
        SelectionCanvas.IsHitTestVisible = false;
        SelectionCanvas.Cursor = Cursors.Arrow;
        DragRect.Visibility = Visibility.Collapsed;
    }

    private void SelectionCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelectingRegion) return;
        _dragStart = e.GetPosition(SelectionCanvas);
        _isDragging = true;
        Canvas.SetLeft(DragRect, _dragStart.X);
        Canvas.SetTop(DragRect, _dragStart.Y);
        DragRect.Width = 0;
        DragRect.Height = 0;
        DragRect.Visibility = Visibility.Visible;
        SelectionCanvas.CaptureMouse();
    }

    private void SelectionCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var pos = e.GetPosition(SelectionCanvas);
        double x = Math.Min(pos.X, _dragStart.X);
        double y = Math.Min(pos.Y, _dragStart.Y);
        double w = Math.Abs(pos.X - _dragStart.X);
        double h = Math.Abs(pos.Y - _dragStart.Y);
        Canvas.SetLeft(DragRect, x);
        Canvas.SetTop(DragRect, y);
        DragRect.Width = w;
        DragRect.Height = h;
    }

    private void SelectionCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        SelectionCanvas.ReleaseMouseCapture();
        _isDragging = false;

        var pos = e.GetPosition(SelectionCanvas);
        double rx = Math.Min(pos.X, _dragStart.X);
        double ry = Math.Min(pos.Y, _dragStart.Y);
        double rw = Math.Abs(pos.X - _dragStart.X);
        double rh = Math.Abs(pos.Y - _dragStart.Y);

        if (rw < 5 || rh < 5)
        {
            // Vùng quá nhỏ, bỏ qua
            ExitSelectMode();
            return;
        }

        // Chuyển tọado điểm ảnh hiển thị sang tọa độ frame thực tế
        var canvasSize = new System.Windows.Size(SelectionCanvas.ActualWidth, SelectionCanvas.ActualHeight);
        var frameRect = GetImageRenderRect(canvasSize,
            _viewModel.LastFrameWidth, _viewModel.LastFrameHeight);

        if (frameRect.Width <= 0 || frameRect.Height <= 0)
        {
            ExitSelectMode();
            return;
        }

        double scaleX = _viewModel.LastFrameWidth / frameRect.Width;
        double scaleY = _viewModel.LastFrameHeight / frameRect.Height;

        int fx = (int)((rx - frameRect.X) * scaleX);
        int fy = (int)((ry - frameRect.Y) * scaleY);
        int fw = (int)(rw * scaleX);
        int fh = (int)(rh * scaleY);

        // Giới hạn trong frame
        fx = Math.Max(0, fx);
        fy = Math.Max(0, fy);
        fw = Math.Min(fw, _viewModel.LastFrameWidth - fx);
        fh = Math.Min(fh, _viewModel.LastFrameHeight - fy);

        if (fw > 0 && fh > 0)
        {
            _viewModel.SelectedRegion = new OpenCvSharp.Rect(fx, fy, fw, fh);
            StatusText.Text = $"Đã chọn vùng: ({fx},{fy}) {fw}×{fh} px";
        }

        ExitSelectMode();
    }

    /// <summary>
    /// Tính toán hình chữ nhật hiển thị thực sự của ảnh trên canvas (Stretch=Uniform).
    /// </summary>
    private static System.Windows.Rect GetImageRenderRect(
        System.Windows.Size canvas, int imgW, int imgH)
    {
        if (imgW <= 0 || imgH <= 0) return System.Windows.Rect.Empty;

        double scaleX = canvas.Width / imgW;
        double scaleY = canvas.Height / imgH;
        double scale = Math.Min(scaleX, scaleY);
        double renderW = imgW * scale;
        double renderH = imgH * scale;
        double offsetX = (canvas.Width - renderW) / 2;
        double offsetY = (canvas.Height - renderH) / 2;
        return new System.Windows.Rect(offsetX, offsetY, renderW, renderH);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}

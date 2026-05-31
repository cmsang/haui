using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;
using Haui.PCB.Views;
using Microsoft.Win32;

namespace Haui.PCB;

/// <summary>
/// Code-behind của MainWindow — chỉ chứa logic giao diện thuần túy.
/// Toàn bộ nghiệp vụ được uỷ thác cho <see cref="MainViewModel"/>.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;
    private readonly IFiducialHoleTemplateService _fiducialTemplateService = FiducialHoleServices.TemplateService;
    private bool _syncingParamsFromViewModel;

    private bool _isSelectingRegion;
    private bool _isDragging;
    private System.Windows.Point _dragStart;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _viewModel.FrameReady += bitmap =>
            Dispatcher.InvokeAsync(() => CameraImage.Source = bitmap);

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentFps))
                Dispatcher.InvokeAsync(() =>
                    FpsText.Text = _viewModel.CurrentFps > 0 ? $"FPS: {_viewModel.CurrentFps}" : string.Empty);
            else if (e.PropertyName == nameof(MainViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
            else if (e.PropertyName is nameof(MainViewModel.CanEditCameraParameters)
                     or nameof(MainViewModel.ExposureTimeUs)
                     or nameof(MainViewModel.GainDb)
                     or nameof(MainViewModel.Gamma))
                Dispatcher.InvokeAsync(UpdateCameraParametersUi);
            else if (e.PropertyName is nameof(MainViewModel.CannyThreshold1)
                     or nameof(MainViewModel.CannyThreshold2))
                Dispatcher.InvokeAsync(UpdatePipelineParametersUi);
        };

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

        _viewModel.TemplateFrameCaptured += frame =>
        {
            Dispatcher.InvokeAsync(async () =>
            {
                var templateWindow = new CreateTemplateWindow { Owner = this };
                try
                {
                    await templateWindow.LoadFrameAsync(frame);
                }
                finally
                {
                    frame.Dispose();
                }
                templateWindow.Show();
            });
        };

        _viewModel.FiducialTemplateFrameCaptured += frame =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                var fiducialWindow = new FiducialTemplateWindow(_fiducialTemplateService) { Owner = this };
                fiducialWindow.Closed += (_, _) => UpdateFiducialTemplateUi();
                fiducialWindow.LoadFrame(frame);
                frame.Dispose();
                fiducialWindow.Show();
            });
        };

        WireParameterControls();
        UpdatePipelineParametersUi();
        UpdateFiducialTemplateUi();
        Loaded += async (_, _) => await LoadCamerasAsync();
    }

    private void WireParameterControls()
    {
        ExposureSlider.ValueChanged += (_, _) => SyncExposureFromSlider();
        GainSlider.ValueChanged += (_, _) => SyncGainFromSlider();
        GammaSlider.ValueChanged += (_, _) => SyncGammaFromSlider();

        ExposureTextBox.LostFocus += (_, _) => SyncExposureFromTextBox();
        GainTextBox.LostFocus += (_, _) => SyncGainFromTextBox();
        GammaTextBox.LostFocus += (_, _) => SyncGammaFromTextBox();

        Canny1Slider.ValueChanged += (_, _) => SyncCanny1FromSlider();
        Canny2Slider.ValueChanged += (_, _) => SyncCanny2FromSlider();
        Canny1TextBox.LostFocus += (_, _) => SyncCanny1FromTextBox();
        Canny2TextBox.LostFocus += (_, _) => SyncCanny2FromTextBox();
    }

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
            CameraComboBox.Items.Add(cam.DisplayName);

        CameraComboBox.SelectedIndex = 0;
    }

    private async Task LoadResolutionsAsync(CameraInfo camera)
    {
        ResolutionComboBox.IsEnabled = false;
        ResolutionComboBox.Items.Clear();
        BtnStart.IsEnabled = false;

        await _viewModel.LoadResolutionsAsync(camera);

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

    private async void CameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel.Cameras.Count == 0 || CameraComboBox.SelectedIndex < 0)
            return;

        var camera = _viewModel.Cameras[CameraComboBox.SelectedIndex];
        await LoadResolutionsAsync(camera);
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
            _viewModel.StartCamera(camera, w, h);

            SetToolbarEnabled(false);
            BtnStop.IsEnabled = true;
            BtnTest.IsEnabled = true;
            BtnTest2.IsEnabled = true;
            BtnSelectRegion.IsEnabled = true;
            BtnCreateTemplate.IsEnabled = true;
            BtnCreateFiducialTemplates.IsEnabled = true;
            CameraPlaceholder.Visibility = Visibility.Collapsed;
            UpdateCameraParametersUi();
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
        BtnCreateFiducialTemplates.IsEnabled = false;
        ExitSelectMode();
        CameraImage.Source = null;
        CameraPlaceholder.Visibility = Visibility.Visible;
        FpsText.Text = string.Empty;
        UpdateCameraParametersUi();
    }

    private void BtnApplyParams_Click(object sender, RoutedEventArgs e)
    {
        PushParameterEditsToViewModel();
        _viewModel.ApplyCameraParameters();
    }

    private void BtnResetParams_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetCameraParameters();
    }

    private void BtnResetCanny_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetPipelineParameters();
    }

    private void UpdateCameraParametersUi()
    {
        CameraParamsPanel.Visibility = Visibility.Visible;

        bool canEdit = _viewModel.CanEditCameraParameters;
        ExposureSlider.IsEnabled = canEdit;
        GainSlider.IsEnabled = canEdit;
        GammaSlider.IsEnabled = canEdit;
        ExposureTextBox.IsEnabled = canEdit;
        GainTextBox.IsEnabled = canEdit;
        GammaTextBox.IsEnabled = canEdit;
        BtnApplyParams.IsEnabled = canEdit;
        BtnResetParams.IsEnabled = true;

        _syncingParamsFromViewModel = true;
        try
        {
            ExposureSlider.Value = Clamp(ExposureSlider, _viewModel.ExposureTimeUs);
            GainSlider.Value = Clamp(GainSlider, _viewModel.GainDb);
            GammaSlider.Value = Clamp(GammaSlider, _viewModel.Gamma);

            ExposureTextBox.Text = _viewModel.ExposureTimeUs.ToString("F0", CultureInfo.InvariantCulture);
            GainTextBox.Text = _viewModel.GainDb.ToString("F1", CultureInfo.InvariantCulture);
            GammaTextBox.Text = _viewModel.Gamma.ToString("F2", CultureInfo.InvariantCulture);
        }
        finally
        {
            _syncingParamsFromViewModel = false;
        }
    }

    private void PushParameterEditsToViewModel()
    {
        if (double.TryParse(ExposureTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double exposure))
            _viewModel.ExposureTimeUs = exposure;
        if (double.TryParse(GainTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double gain))
            _viewModel.GainDb = gain;
        if (double.TryParse(GammaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double gamma))
            _viewModel.Gamma = gamma;
    }

    private void SyncExposureFromSlider()
    {
        if (_syncingParamsFromViewModel) return;
        _viewModel.ExposureTimeUs = ExposureSlider.Value;
        ExposureTextBox.Text = ExposureSlider.Value.ToString("F0", CultureInfo.InvariantCulture);
    }

    private void SyncGainFromSlider()
    {
        if (_syncingParamsFromViewModel) return;
        _viewModel.GainDb = GainSlider.Value;
        GainTextBox.Text = GainSlider.Value.ToString("F1", CultureInfo.InvariantCulture);
    }

    private void SyncGammaFromSlider()
    {
        if (_syncingParamsFromViewModel) return;
        _viewModel.Gamma = GammaSlider.Value;
        GammaTextBox.Text = GammaSlider.Value.ToString("F2", CultureInfo.InvariantCulture);
    }

    private void SyncExposureFromTextBox()
    {
        if (_syncingParamsFromViewModel) return;
        if (!double.TryParse(ExposureTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return;
        _viewModel.ExposureTimeUs = value;
        ExposureSlider.Value = Clamp(ExposureSlider, value);
    }

    private void SyncGainFromTextBox()
    {
        if (_syncingParamsFromViewModel) return;
        if (!double.TryParse(GainTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return;
        _viewModel.GainDb = value;
        GainSlider.Value = Clamp(GainSlider, value);
    }

    private void SyncGammaFromTextBox()
    {
        if (_syncingParamsFromViewModel) return;
        if (!double.TryParse(GammaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return;
        _viewModel.Gamma = value;
        GammaSlider.Value = Clamp(GammaSlider, value);
    }

    private static double Clamp(Slider slider, double value)
        => Math.Max(slider.Minimum, Math.Min(slider.Maximum, value));

    private void UpdatePipelineParametersUi()
    {
        _syncingParamsFromViewModel = true;
        try
        {
            Canny1Slider.Value = Clamp(Canny1Slider, _viewModel.CannyThreshold1);
            Canny2Slider.Value = Clamp(Canny2Slider, _viewModel.CannyThreshold2);
            Canny1TextBox.Text = _viewModel.CannyThreshold1.ToString("F0", CultureInfo.InvariantCulture);
            Canny2TextBox.Text = _viewModel.CannyThreshold2.ToString("F0", CultureInfo.InvariantCulture);
        }
        finally
        {
            _syncingParamsFromViewModel = false;
        }
    }

    private void SyncCanny1FromSlider()
    {
        if (_syncingParamsFromViewModel) return;
        _viewModel.CannyThreshold1 = Canny1Slider.Value;
        Canny1TextBox.Text = Canny1Slider.Value.ToString("F0", CultureInfo.InvariantCulture);
    }

    private void SyncCanny2FromSlider()
    {
        if (_syncingParamsFromViewModel) return;
        _viewModel.CannyThreshold2 = Canny2Slider.Value;
        Canny2TextBox.Text = Canny2Slider.Value.ToString("F0", CultureInfo.InvariantCulture);
    }

    private void SyncCanny1FromTextBox()
    {
        if (_syncingParamsFromViewModel) return;
        if (!double.TryParse(Canny1TextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return;
        _viewModel.CannyThreshold1 = value;
        Canny1Slider.Value = Clamp(Canny1Slider, value);
    }

    private void SyncCanny2FromTextBox()
    {
        if (_syncingParamsFromViewModel) return;
        if (!double.TryParse(Canny2TextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return;
        _viewModel.CannyThreshold2 = value;
        Canny2Slider.Value = Clamp(Canny2Slider, value);
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

    private async void BtnCreateFiducialTemplates_Click(object sender, RoutedEventArgs e)
    {
        BtnCreateFiducialTemplates.IsEnabled = false;
        try
        {
            await _viewModel.CaptureFiducialTemplateFrameAsync();
        }
        finally
        {
            BtnCreateFiducialTemplates.IsEnabled = _viewModel.IsRunning;
        }
    }

    private void BtnBrowseFiducialFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Chọn thư mục lưu mẫu 4 lỗ tròn",
            InitialDirectory = _viewModel.FiducialTemplateFolder
        };

        if (dialog.ShowDialog() != true)
            return;

        _viewModel.SetFiducialTemplateFolder(dialog.FolderName);
        UpdateFiducialTemplateUi();
    }

    private void UpdateFiducialTemplateUi()
    {
        _viewModel.RefreshFiducialTemplateStatus();
        FiducialFolderTextBox.Text = _viewModel.FiducialTemplateFolder;
        FiducialStatusText.Text = _viewModel.HasFiducialTemplates
            ? $"Đã có {_fiducialTemplateService.ListTemplateFileNames().Count} mẫu lỗ — pipeline so khớp tất cả trên ảnh Close."
            : "Chưa có mẫu lỗ — pipeline dùng contour như trước.";
    }

    private void BtnViewTemplates_Click(object sender, RoutedEventArgs e)
    {
        var viewerWindow = new TemplateViewerWindow { Owner = this };
        viewerWindow.Show();
    }

    private void SetToolbarEnabled(bool enabled)
    {
        CameraComboBox.IsEnabled = enabled;
        ResolutionComboBox.IsEnabled = enabled;
        BtnStart.IsEnabled = enabled && _viewModel.Cameras.Count > 0;
        BtnStop.IsEnabled = false;
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
            ExitSelectMode();
            return;
        }

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
        _startupHandshake.Cancel();
        _serialService.DataReceived -= Serial_DataReceived;
        _robotViewModel.Dispose();
        _serialService.Dispose();
        _viewModel.Dispose();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel.IsMaterialTransferRunning)
        {
            var result = MessageBox.Show(
                "Chu trình Pass/Fail đang chạy. Hủy và thoát?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            _viewModel.CancelMaterialTransfer();
            _startupHandshake.Cancel();
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _robotViewModel.ReloadAppSettings();
        _robotViewModel.RefreshAvailablePorts();
        if (!_robotViewModel.EnsureSerialConnected())
        {
            RobotSerialDetail.Text = _robotViewModel.StatusText;
            TxtRobotRxLog.Text = $"Chưa mở được COM — kiểm tra Config/setting.json ({Processing.AppConfigPaths.SettingFile})";
        }
        else
        {
            await _startupHandshake.RunAsync(msg =>
                Dispatcher.InvokeAsync(() => RobotSerialDetail.Text = msg));
        }

        UpdateRobotSerialStatus();
    }

    /// <summary>
    /// Gọi mỗi khi SerialPort.DataReceived có byte mới (RobotSerialService.Port_DataReceived).
    /// </summary>
    private void Serial_DataReceived(string chunk)
    {
        Dispatcher.InvokeAsync(() =>
        {
            var visible = chunk.Replace("\r", "\\r").Replace("\n", "\\n");
            var hex = BitConverter.ToString(Encoding.ASCII.GetBytes(chunk));
            var entry = $"{DateTime.Now:HH:mm:ss}  RX: {visible}  [{hex}]";
            _robotRxLog.Enqueue(entry);
            while (_robotRxLog.Count > MaxRobotRxLines)
                _robotRxLog.Dequeue();

            TxtRobotRxLog.Text = string.Join(Environment.NewLine, _robotRxLog);
            RobotSerialDetail.Text = $"RX: {visible}";
        });
    }

    private void UpdateRobotSerialStatus()
    {
        if (_robotViewModel.IsSerialConnected)
        {
            RobotSerialText.Text = $"● Robot {_robotViewModel.SerialPortName} Online";
            RobotSerialText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x27, 0xAE, 0x60));
        }
        else
        {
            RobotSerialText.Text = "● Robot Offline";
            RobotSerialText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xE7, 0x4C, 0x3C));
        }

        if (!string.IsNullOrWhiteSpace(_robotViewModel.StatusText)
            && _robotViewModel.StatusText.StartsWith("Serial", StringComparison.OrdinalIgnoreCase))
            RobotSerialDetail.Text = _robotViewModel.StatusText;
    }

    private void EnsureMainSerialDataReceiver()
    {
        _serialService.DataReceived -= Serial_DataReceived;
        _serialService.DataReceived += Serial_DataReceived;
    }

    private void btnDashboard_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnManualControl_Click(object sender, RoutedEventArgs e)
    {
        var win = new wdManualControl(_serialService) { Owner = this };
        win.ShowDialog();
        EnsureMainSerialDataReceiver();
        _robotViewModel.SyncConnectionState();
        UpdateRobotSerialStatus();
    }

    private void btnSetting_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnExit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnTeaching_Click(object sender, RoutedEventArgs e)
    {
        var win = new wdTeaching(_serialService) { Owner = this };
        win.ShowDialog();
        EnsureMainSerialDataReceiver();
        _robotViewModel.SyncConnectionState();
        UpdateRobotSerialStatus();
    }

    private async void btnPass_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsMaterialTransferRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            await _viewModel.TransferPassMaterial();
        }
        finally
        {
            btnPass.IsEnabled = true;
            btnFail.IsEnabled = true;
            StatusText.Text = _viewModel.StatusText;
        }
    }

    private async void btnFail_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsMaterialTransferRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            await _viewModel.TransferFailMaterial();
        }
        finally
        {
            btnPass.IsEnabled = true;
            btnFail.IsEnabled = true;
            StatusText.Text = _viewModel.StatusText;
        }
    }
}

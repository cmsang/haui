using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Haui.PCB.Processing.Detection;
using Haui.PCB.Processing.Segmentation;
using Haui.PCB.ViewModels;
using Haui.PCB.Views.Windows;

namespace Haui.PCB.Views.Tabs;

/// <summary>
/// Dashboard tab — live camera, inline inspection results, and pipeline step gallery.
/// </summary>
public partial class DashboardTabView : UserControl
{
    private static readonly SolidColorBrush PassBackgroundBrush = new(Color.FromRgb(0xE8, 0xF5, 0xE9));
    private static readonly SolidColorBrush PassForegroundBrush = new(Color.FromRgb(0x00, 0x75, 0x2A));
    private static readonly SolidColorBrush FailBackgroundBrush = new(Color.FromRgb(0xFF, 0xED, 0xED));
    private static readonly SolidColorBrush FailForegroundBrush = new(Color.FromRgb(0xD1, 0x34, 0x38));
    private static readonly SolidColorBrush IdleBackgroundBrush = new(Color.FromRgb(0xFA, 0xFA, 0xFA));
    private static readonly SolidColorBrush IdleForegroundBrush = new(Color.FromRgb(0x88, 0x88, 0x88));

    private readonly MainViewModel _viewModel;
    private readonly TestPipelineViewModel _inspectionViewModel;
    private readonly Window _owner;
    private readonly Func<bool, Task>? _onInspectionCompleted;
    private readonly IAppSettingService _appSettingService = new AppSettingService();
    private bool _developerMode;
    private bool _showInspectionResultAfterRecognition;
    private bool _isSelectingRegion;
    private bool _isDragging;
    private Point _dragStart;
    private bool _wired;
    private BitmapSource? _latestCameraFrame;
    private int _cameraFrameDispatchQueued;

    public DashboardTabView(MainViewModel viewModel, Window owner, Func<bool, Task>? onInspectionCompleted = null)
    {
        _viewModel = viewModel;
        _owner = owner;
        _onInspectionCompleted = onInspectionCompleted;

        _inspectionViewModel = new TestPipelineViewModel(
            new PcbSegmentationService(),
            new MissingComponentDetectionService());

        DataContext = _viewModel;
        InitializeComponent();
        WireViewModel();
        WireInspectionViewModel();
    }

    private void WireViewModel()
    {
        if (_wired) return;
        _wired = true;

        _viewModel.FrameReady += ScheduleCameraFrameUpdate;

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentFps))
                Dispatcher.InvokeAsync(() =>
                    FpsText.Text = _viewModel.CurrentFps > 0 ? $"FPS: {_viewModel.CurrentFps}" : string.Empty);
            else if (e.PropertyName == nameof(MainViewModel.StatusText))
                Dispatcher.InvokeAsync(() => StatusText.Text = _viewModel.StatusText);
        };
    }

    private void WireInspectionViewModel()
    {
        PipelineStepsPanel.ItemsSource = _inspectionViewModel.Steps;
        ComponentResultsPanel.DataContext = _inspectionViewModel;

        _inspectionViewModel.ProcessedImageReady += bitmap =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (bitmap is null)
                    return;

                ResultImage.Source = bitmap;
                ResultPlaceholder.Visibility = Visibility.Collapsed;
            });
        };

        _inspectionViewModel.AnnotatedImageReady += bitmap =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (bitmap is null)
                {
                    ResultImage.Source = null;
                    ResultPlaceholder.Visibility = Visibility.Visible;
                    ResultPlaceholder.Text = "Không tìm thấy bo mạch";
                    return;
                }

                ResultImage.Source = bitmap;
                ResultPlaceholder.Visibility = Visibility.Collapsed;
            });
        };

        _inspectionViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestPipelineViewModel.IsFullMatch))
                Dispatcher.InvokeAsync(UpdatePassFailDisplay);
            else if (e.PropertyName is nameof(TestPipelineViewModel.IsBusy) or nameof(TestPipelineViewModel.HasPipelineSteps))
                Dispatcher.InvokeAsync(UpdatePipelineStepsPlaceholder, DispatcherPriority.Background);
            else if (e.PropertyName == nameof(TestPipelineViewModel.StatusText))
                Dispatcher.InvokeAsync(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_inspectionViewModel.StatusText))
                        StatusText.Text = _inspectionViewModel.StatusText;
                });
        };

        _inspectionViewModel.InspectionCompleted += isPass =>
        {
            Dispatcher.InvokeAsync(async () =>
            {
                if (_showInspectionResultAfterRecognition)
                    ShowInspectionResultWindow();

                if (_onInspectionCompleted is not null)
                    await RunInspectionCompletedHandlerAsync(isPass);
            });
        };
    }

    private void ShowInspectionResultWindow()
    {
        var window = new InspectionResultWindow(_inspectionViewModel)
        {
            Owner = _owner
        };
        window.ShowDialog();
    }

    private async Task RunInspectionCompletedHandlerAsync(bool isPass)
    {
        try
        {
            await _onInspectionCompleted(isPass);
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
                StatusText.Text = $"Lỗi phân loại robot sau nhận dạng: {ex.Message}");
        }
    }

    private void UpdatePassFailDisplay()
    {
        switch (_inspectionViewModel.IsFullMatch)
        {
            case true:
                PassFailText.Text = "PASS";
                PassFailPanel.Background = PassBackgroundBrush;
                PassFailText.Foreground = PassForegroundBrush;
                break;
            case false:
                PassFailText.Text = "FAIL";
                PassFailPanel.Background = FailBackgroundBrush;
                PassFailText.Foreground = FailForegroundBrush;
                break;
            default:
                PassFailText.Text = "—";
                PassFailPanel.Background = IdleBackgroundBrush;
                PassFailText.Foreground = IdleForegroundBrush;
                break;
        }
    }

    private void ScheduleCameraFrameUpdate(BitmapSource bitmap)
    {
        _latestCameraFrame = bitmap;
        if (Interlocked.CompareExchange(ref _cameraFrameDispatchQueued, 1, 0) != 0)
            return;

        Dispatcher.InvokeAsync(() =>
        {
            if (_latestCameraFrame is not null)
                CameraImage.Source = _latestCameraFrame;
            Interlocked.Exchange(ref _cameraFrameDispatchQueued, 0);
        }, DispatcherPriority.Render);
    }

    private void UpdatePipelineStepsPlaceholder()
    {
        PipelineStepsPlaceholder.Visibility = _inspectionViewModel.Steps.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ResetInspectionDisplay()
    {
        ResultImage.Source = null;
        ResultPlaceholder.Text = "Chưa kiểm tra";
        ResultPlaceholder.Visibility = Visibility.Visible;
        _inspectionViewModel.ClearInspectionResults();
        _inspectionViewModel.ClearPipelineSteps();
        UpdatePipelineStepsPlaceholder();
        PassFailText.Text = "—";
        PassFailPanel.Background = IdleBackgroundBrush;
        PassFailText.Foreground = IdleForegroundBrush;
    }

    public void SetStatusMessage(string message) => StatusText.Text = message;

    public void ApplyDeveloperModeUi()
    {
        var setting = _appSettingService.Load();
        _developerMode = setting.DeveloperMode;
        _showInspectionResultAfterRecognition = setting.ShowInspectionResultAfterRecognition;
        var visibility = _developerMode ? Visibility.Visible : Visibility.Collapsed;
        BtnSelectImage.Visibility = visibility;
        BtnCannyThreshold.Visibility = visibility;
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyDeveloperModeUi();
        UpdatePipelineStepsPlaceholder();
        if (CameraComboBox.Items.Count > 0) return;
        await LoadCamerasAsync();
    }

    private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
            ApplyDeveloperModeUi();
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
        _ = StartCameraFromUiAsync();
    }

    private async Task StartCameraFromUiAsync()
    {
        if (_viewModel.Cameras.Count == 0 || CameraComboBox.SelectedIndex < 0) return;

        var camera = _viewModel.Cameras[CameraComboBox.SelectedIndex];

        var parts = ResolutionComboBox.SelectedItem?.ToString()?.Split('×');
        if (parts is null || parts.Length != 2) return;
        if (!int.TryParse(parts[0], out int w) || !int.TryParse(parts[1], out int h)) return;

        SetToolbarEnabled(false);
        BtnStart.IsEnabled = false;

        try
        {
            await _viewModel.StartCameraAsync(camera, w, h);

            BtnStop.IsEnabled = true;
            BtnTest.IsEnabled = true;
            BtnSelectRegion.IsEnabled = true;
            BtnCapture.IsEnabled = true;
            BtnCannyThreshold.IsEnabled = true;
            CameraPlaceholder.Visibility = Visibility.Collapsed;
            ResetInspectionDisplay();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Lỗi: {ex.Message}";
            SetToolbarEnabled(true);
            BtnStart.IsEnabled = _viewModel.Cameras.Count > 0;
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StopCamera();

        SetToolbarEnabled(true);
        BtnStop.IsEnabled = false;
        BtnTest.IsEnabled = false;
        BtnSelectRegion.IsEnabled = false;
        BtnCapture.IsEnabled = false;
        BtnCannyThreshold.IsEnabled = false;
        ExitSelectMode();
        CameraImage.Source = null;
        CameraPlaceholder.Visibility = Visibility.Visible;
        FpsText.Text = string.Empty;
        ResetInspectionDisplay();
    }

    private void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        ProcessImage();
    }

    /// <summary>
    /// Trigger an inspection from outside the UI (e.g. warehouse CAPx). Marshals to the UI thread.
    /// </summary>
    public void RequestInspection()
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.InvokeAsync(RequestInspection);
            return;
        }

        if (!_viewModel.IsRunning)
        {
            StatusText.Text = "Nhận CAPx nhưng camera chưa chạy — bỏ qua.";
            return;
        }

        if (_inspectionViewModel.IsBusy)
        {
            StatusText.Text = "Đang kiểm tra — bỏ qua CAPx.";
            return;
        }

        ProcessImage();
    }

    private async void ProcessImage()
    {
        BtnTest.IsEnabled = false;
        try
        {
            using var frame = await _viewModel.CaptureFrameAsync();
            if (frame is null)
                return;

            ResetInspectionDisplay();
            ResultPlaceholder.Text = "Đang xử lý...";
            ResultPlaceholder.Visibility = Visibility.Visible;

            await _inspectionViewModel.InspectAsync(frame);
        }
        finally
        {
            BtnTest.IsEnabled = _viewModel.IsRunning;
        }
    }

    private async void BtnSelectImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn ảnh để kiểm tra",
            Filter = "Ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.tif)|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff"
        };

        if (dialog.ShowDialog() != true)
            return;

        BtnSelectImage.IsEnabled = false;
        try
        {
            ResetInspectionDisplay();
            ResultPlaceholder.Text = "Đang xử lý...";
            ResultPlaceholder.Visibility = Visibility.Visible;
            await _inspectionViewModel.InspectFromFileAsync(dialog.FileName);
        }
        finally
        {
            BtnSelectImage.IsEnabled = true;
        }
    }

    private async void BtnCannyThreshold_Click(object sender, RoutedEventArgs e)
    {
        BtnCannyThreshold.IsEnabled = false;
        try
        {
            using var frame = await _viewModel.CaptureFrameAsync();
            if (frame is null)
                return;

            var window = new CannyThresholdWindow(frame, _owner)
            {
                Owner = _owner
            };
            window.ShowDialog();
        }
        finally
        {
            BtnCannyThreshold.IsEnabled = _viewModel.IsRunning;
        }
    }

    private async void BtnCapture_Click(object sender, RoutedEventArgs e)
    {
        BtnCapture.IsEnabled = false;
        try
        {
            await _viewModel.CaptureAndSaveFrameAsync();
        }
        finally
        {
            BtnCapture.IsEnabled = _viewModel.IsRunning;
        }
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

        var canvasSize = new Size(SelectionCanvas.ActualWidth, SelectionCanvas.ActualHeight);
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

    private static Rect GetImageRenderRect(Size canvas, int imgW, int imgH)
    {
        if (imgW <= 0 || imgH <= 0) return Rect.Empty;

        double scaleX = canvas.Width / imgW;
        double scaleY = canvas.Height / imgH;
        double scale = Math.Min(scaleX, scaleY);
        double renderW = imgW * scale;
        double renderH = imgH * scale;
        double offsetX = (canvas.Width - renderW) / 2;
        double offsetY = (canvas.Height - renderH) / 2;
        return new Rect(offsetX, offsetY, renderW, renderH);
    }
}

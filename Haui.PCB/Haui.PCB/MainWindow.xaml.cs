using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;
using Haui.PCB.Views;
using Haui.PCB.Views.Tabs;

namespace Haui.PCB;

/// <summary>
/// Main shell — robot flow from develop; Dashboard and Setting as content tabs.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private static readonly SolidColorBrush SidebarActiveBrush = new(Color.FromRgb(0x1A, 0x52, 0x76));
    private static readonly SolidColorBrush SidebarIdleBrush = new(Color.FromRgb(0x2E, 0x40, 0x53));

    private readonly MainViewModel _viewModel;
    private readonly RobotTeachViewModel _robotViewModel;
    private readonly RobotSerialService _serialService = new();
    private readonly WarehouseSerialService _warehouseSerialService;
    private readonly RobotStartupHandshakeService _startupHandshake;
    private readonly RobotPositionTracker _positionTracker = new();

    private readonly Queue<(string Line, Brush Brush)> _robotSerialLog = new();
    private const int MaxRobotSerialLogLines = 30;
    private static readonly Brush TxLogBrush = new SolidColorBrush(Color.FromRgb(0x7B, 0x1F, 0xA2));
    private static readonly Brush RxLogBrush = Brushes.Black;
    private static readonly Brush WarehouseTxLogBrush = new SolidColorBrush(Color.FromRgb(0x0D, 0x65, 0x6F));
    private static readonly Brush WarehouseRxLogBrush = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20));

    private DashboardTabView? _dashboardTab;
    private SettingTabView? _settingTab;

    public MainWindow()
    {
        InitializeComponent();
        var appSettingService = new AppSettingService();
        _warehouseSerialService = new WarehouseSerialService(appSettingService);
        var materialTransfer = new MaterialTransferService(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService,
            _warehouseSerialService,
            _positionTracker);
        _viewModel = new MainViewModel(materialTransfer);
        _robotViewModel = new RobotTeachViewModel(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService,
            disposeSerialService: false,
            enableSerialEvents: false);
        DataContext = _viewModel;

        _serialService.FrameReceived += Serial_FrameReceived;
        _serialService.DataSent += Serial_DataSent;
        _warehouseSerialService.CaptureRequested += OnWarehouseCaptureRequested;
        _warehouseSerialService.FrameReceived += Warehouse_FrameReceived;
        _warehouseSerialService.DataSent += Warehouse_DataSent;
        _startupHandshake = new RobotStartupHandshakeService(_serialService);

        _robotViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(RobotTeachViewModel.StatusText)
                or nameof(RobotTeachViewModel.IsSerialConnected)
                or nameof(RobotTeachViewModel.SerialPortName))
            {
                Dispatcher.InvokeAsync(UpdateRobotSerialStatus);
            }
        };

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StatusText))
                Dispatcher.InvokeAsync(() => _dashboardTab?.SetStatusMessage(_viewModel.StatusText));
            else if (e.PropertyName == nameof(MainViewModel.IsMaterialTransferRunning))
                Dispatcher.InvokeAsync(UpdateRobotOperationButtons);
        };

        Loaded += Window_Loaded;
        Closing += Window_Closing;
        NavigateTo(MainTabKind.Dashboard);
    }

    private void NavigateTo(MainTabKind kind)
    {
        MainContentHost.Content = kind switch
        {
            MainTabKind.Dashboard => _dashboardTab ??= new DashboardTabView(_viewModel, this, HandleInspectionCompletedAsync),
            MainTabKind.Setting => _settingTab ??= new SettingTabView(),
            _ => _dashboardTab ??= new DashboardTabView(_viewModel, this, HandleInspectionCompletedAsync)
        };

        SidebarDashboard.Background = kind == MainTabKind.Dashboard ? SidebarActiveBrush : SidebarIdleBrush;
        SidebarSetting.Background = kind == MainTabKind.Setting ? SidebarActiveBrush : SidebarIdleBrush;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _startupHandshake.Cancel();
        _serialService.FrameReceived -= Serial_FrameReceived;
        _serialService.DataSent -= Serial_DataSent;
        _warehouseSerialService.CaptureRequested -= OnWarehouseCaptureRequested;
        _warehouseSerialService.FrameReceived -= Warehouse_FrameReceived;
        _warehouseSerialService.DataSent -= Warehouse_DataSent;
        _warehouseSerialService.Dispose();
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
            AppendRobotSerialLog(
                $"Chưa mở được COM — kiểm tra Config/setting.json ({AppConfigPaths.SettingFile})",
                RxLogBrush);
        }
        else
        {
            await _startupHandshake.RunAsync(msg =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    RobotSerialDetail.Text = msg;
                    UpdateRobotSerialStatus();
                });
            });

            if (_startupHandshake.IsCompleted)
                _positionTracker.SetHome();
        }

        UpdateRobotSerialStatus();
        UpdateRobotOperationButtons();

        if (_warehouseSerialService.TryStartListening(out var warehouseCom, out var warehouseError))
            AppendRobotSerialLog($"Warehouse {warehouseCom} — đang lắng nghe CAPx", RxLogBrush);
        else
            AppendRobotSerialLog($"Không lắng nghe warehouse: {warehouseError}", RxLogBrush);
    }

    private void OnWarehouseCaptureRequested()
        => Dispatcher.InvokeAsync(() => _dashboardTab?.RequestInspection());

    private void UpdateRobotOperationButtons()
    {
        var ready = RobotConnectionHelper.IsRobotArmReady(_serialService, _startupHandshake);
        var busy = _viewModel.IsMaterialTransferRunning;

        btnTeaching.IsEnabled = ready && !busy;
        btnManualControl.IsEnabled = ready && !busy;
        btnPass.IsEnabled = ready && !busy;
        btnFail.IsEnabled = ready && !busy;
    }

    private void Serial_FrameReceived(string frame)
        => LogSerial("Robot", "R", frame, RxLogBrush);

    private void Serial_DataSent(string chunk)
        => LogSerial("Robot", "S", chunk, TxLogBrush);

    private void Warehouse_FrameReceived(string frame)
        => LogSerial("Kho", "R", frame, WarehouseRxLogBrush);

    private void Warehouse_DataSent(string chunk)
        => LogSerial("Kho", "S", chunk, WarehouseTxLogBrush);

    private void LogSerial(string device, string direction, string raw, Brush brush)
    {
        Dispatcher.InvokeAsync(() =>
        {
            var visible = EscapeSerialText(raw);
            AppendRobotSerialLog(FormatSerialLogEntry(device, direction, visible, raw), brush);
            RobotSerialDetail.Text = $"{device} {direction}: {visible}";
        });
    }

    private void AppendRobotSerialLog(string line, Brush brush)
    {
        _robotSerialLog.Enqueue((line, brush));
        while (_robotSerialLog.Count > MaxRobotSerialLogLines)
            _robotSerialLog.Dequeue();

        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 10,
            PagePadding = new Thickness(4, 2, 4, 2)
        };
        var paragraph = new Paragraph { Margin = new Thickness(0) };

        foreach (var (entry, entryBrush) in _robotSerialLog)
            paragraph.Inlines.Add(new Run(entry + Environment.NewLine) { Foreground = entryBrush });

        doc.Blocks.Add(paragraph);
        TxtRobotRxLog.Document = doc;
        TxtRobotRxLog.ScrollToEnd();
    }

    private static string EscapeSerialText(string text)
        => text.Replace("\r", "\\r").Replace("\n", "\\n");

    private static string FormatSerialLogEntry(string device, string direction, string visible, string raw)
    {
        var hex = BitConverter.ToString(Encoding.ASCII.GetBytes(raw));
        return $"{DateTime.Now:HH:mm:ss}  {device,-5} {direction}: {visible}  [{hex}]";
    }

    private void UpdateRobotSerialStatus()
    {
        if (_robotViewModel.IsSerialConnected)
        {
            if (_startupHandshake.IsRunning)
            {
                RobotSerialText.Text = $"● Robot {_robotViewModel.SerialPortName} — đang kết nối/homing";
                RobotSerialText.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12));
            }
            else if (_startupHandshake.IsCompleted)
            {
                RobotSerialText.Text = $"● Robot {_robotViewModel.SerialPortName} Sẵn sàng";
                RobotSerialText.Foreground = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60));
            }
            else
            {
                RobotSerialText.Text = $"● Robot {_robotViewModel.SerialPortName} Online";
                RobotSerialText.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x9C, 0x12));
            }
        }
        else
        {
            RobotSerialText.Text = "● Robot Offline";
            RobotSerialText.Foreground = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
        }

        if (!string.IsNullOrWhiteSpace(_robotViewModel.StatusText)
            && _robotViewModel.StatusText.StartsWith("Serial", StringComparison.OrdinalIgnoreCase))
            RobotSerialDetail.Text = _robotViewModel.StatusText;

        UpdateRobotOperationButtons();
    }

    private void EnsureMainSerialDataReceiver()
    {
        _serialService.FrameReceived -= Serial_FrameReceived;
        _serialService.DataSent -= Serial_DataSent;
        _serialService.FrameReceived += Serial_FrameReceived;
        _serialService.DataSent += Serial_DataSent;
    }

    private async Task HandleInspectionCompletedAsync(bool isPass)
    {
        var label = isPass ? "PASS" : "FAIL";

        if (!RobotConnectionHelper.IsRobotArmReady(_serialService, _startupHandshake))
        {
            await Dispatcher.InvokeAsync(() =>
                _dashboardTab?.SetStatusMessage(
                    $"Nhận dạng: {label} — robot chưa sẵn sàng, không chạy phân loại tự động."));
            return;
        }

        if (_viewModel.IsMaterialTransferRunning)
        {
            await Dispatcher.InvokeAsync(() =>
                _dashboardTab?.SetStatusMessage(
                    $"Nhận dạng: {label} — chu trình robot đang chạy, bỏ qua."));
            return;
        }

        await Dispatcher.InvokeAsync(UpdateRobotOperationButtons);
        await Dispatcher.InvokeAsync(() =>
            _dashboardTab?.SetStatusMessage($"Nhận dạng: {label} — đang phân loại bằng robot..."));

        await _viewModel.TransferMaterialByInspectionResultAsync(isPass);

        await Dispatcher.InvokeAsync(() =>
        {
            UpdateRobotOperationButtons();
            _dashboardTab?.SetStatusMessage(_viewModel.StatusText);
        });
    }

    private void btnDashboard_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.Dashboard);

    private void btnManualControl_Click(object sender, RoutedEventArgs e)
    {
        if (!RobotConnectionHelper.EnsureRobotArmReady(_serialService, _startupHandshake))
            return;

        var win = new wdManualControl(_serialService, _positionTracker) { Owner = this };
        win.ShowDialog();
        EnsureMainSerialDataReceiver();
        _robotViewModel.SyncConnectionState();
        UpdateRobotSerialStatus();
    }

    private void btnSetting_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.Setting);

    private void btnExit_Click(object sender, RoutedEventArgs e)
        => Close();

    private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
    {
    }

    private void btnTeaching_Click(object sender, RoutedEventArgs e)
    {
        if (!RobotConnectionHelper.EnsureRobotArmReady(_serialService, _startupHandshake))
            return;

        var win = new wdTeaching(_serialService, _positionTracker) { Owner = this };
        win.ShowDialog();
        EnsureMainSerialDataReceiver();
        _robotViewModel.SyncConnectionState();
        UpdateRobotSerialStatus();
    }

    private async void btnPass_Click(object sender, RoutedEventArgs e)
    {
        if (!RobotConnectionHelper.EnsureRobotArmReady(_serialService, _startupHandshake))
            return;

        if (_viewModel.IsMaterialTransferRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            await _viewModel.TransferPassMaterial();
        }
        finally
        {
            UpdateRobotOperationButtons();
            _dashboardTab?.SetStatusMessage(_viewModel.StatusText);
        }
    }

    private async void btnFail_Click(object sender, RoutedEventArgs e)
    {
        if (!RobotConnectionHelper.EnsureRobotArmReady(_serialService, _startupHandshake))
            return;

        if (_viewModel.IsMaterialTransferRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            await _viewModel.TransferFailMaterial();
        }
        finally
        {
            UpdateRobotOperationButtons();
            _dashboardTab?.SetStatusMessage(_viewModel.StatusText);
        }
    }
}

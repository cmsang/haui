using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Haui.PCB.ViewModels;
using Haui.PCB.Views.Tabs;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Shell chính — sidebar điều hướng tab; nội dung từng tab nằm trong <see cref="Tabs"/>.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly SolidColorBrush SidebarActiveBrush = new(Color.FromRgb(0x1A, 0x52, 0x76));
    private static readonly SolidColorBrush SidebarIdleBrush = new(Color.FromRgb(0x2E, 0x40, 0x53));

    private readonly MainViewModel _viewModel;
    private readonly MonitorViewModel _monitor = new();
    private readonly RobotTeachViewModel _robotViewModel;
    private readonly IRobotSerialService _serialService;
    private readonly RobotStartupHandshakeService _startupHandshake;
    private readonly IMaterialTransferService _materialTransfer;
    private readonly Queue<string> _robotRxLog = new();
    private const int MaxRobotRxLines = 30;

    private readonly Dictionary<MainTabKind, UserControl> _tabs = new();
    private DashboardTabView? _dashboardTab;
    private RobotTeachingTabView? _robotTeachingTab;
    private ManualControlTabView? _manualControlTab;
    private MainTabKind _currentTab = MainTabKind.Dashboard;

    /// <summary>Shared line-status state bound to the shell Monitor panel.</summary>
    public MonitorViewModel Monitor => _monitor;

    public MainWindow()
    {
        InitializeComponent();
        MonitorPanel.DataContext = _monitor;
        var appSettingService = new AppSettingService();
        _serialService = RobotSerialServiceFactory.Create(appSettingService);
        _materialTransfer = new MaterialTransferService(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService);
        _viewModel = new MainViewModel();
        _robotViewModel = new RobotTeachViewModel(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService,
            disposeSerialService: false,
            enableSerialEvents: false);

        _serialService.DataReceived += Serial_DataReceived;
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

        NavigateTo(MainTabKind.Dashboard);
    }

    private UserControl GetOrCreateTab(MainTabKind kind) => kind switch
    {
        MainTabKind.Dashboard => _dashboardTab ??= new DashboardTabView(_viewModel, this),
        MainTabKind.JobHistory => GetCachedTab(kind, () => new JobHistoryTabView(_monitor)),
        MainTabKind.RobotTeaching => _robotTeachingTab ??= CreateRobotTeachingTab(),
        MainTabKind.ManualControl => _manualControlTab ??= CreateManualControlTab(),
        MainTabKind.Setting => GetCachedTab(kind, () => new SettingTabView()),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private UserControl GetCachedTab(MainTabKind kind, Func<UserControl> factory)
    {
        if (!_tabs.TryGetValue(kind, out var tab))
        {
            tab = factory();
            _tabs[kind] = tab;
        }

        return tab;
    }

    private RobotTeachingTabView CreateRobotTeachingTab()
    {
        var tab = new RobotTeachingTabView();
        tab.Initialize(_serialService, _monitor);
        return tab;
    }

    private ManualControlTabView CreateManualControlTab()
    {
        var tab = new ManualControlTabView();
        tab.Initialize(_serialService, _monitor);
        return tab;
    }

    private void NavigateTo(MainTabKind kind)
    {
        _currentTab = kind;
        MainContentHost.Content = GetOrCreateTab(kind);
        UpdateSidebarSelection(kind);
    }

    private void UpdateSidebarSelection(MainTabKind kind)
    {
        SidebarDashboard.Background = kind == MainTabKind.Dashboard ? SidebarActiveBrush : SidebarIdleBrush;
        SidebarJobHistory.Background = kind == MainTabKind.JobHistory ? SidebarActiveBrush : SidebarIdleBrush;
        SidebarRobotTeaching.Background = kind == MainTabKind.RobotTeaching ? SidebarActiveBrush : SidebarIdleBrush;
        SidebarManualControl.Background = kind == MainTabKind.ManualControl ? SidebarActiveBrush : SidebarIdleBrush;
        SidebarSetting.Background = kind == MainTabKind.Setting ? SidebarActiveBrush : SidebarIdleBrush;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _startupHandshake.Cancel();
        _serialService.DataReceived -= Serial_DataReceived;
        _robotTeachingTab?.DisposePanel();
        _manualControlTab?.DisposePanel();
        _robotViewModel.Dispose();
        _serialService.Dispose();
        _viewModel.Dispose();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_materialTransfer.IsRunning)
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

            _materialTransfer.Cancel();
            _startupHandshake.Cancel();
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _robotViewModel.ReloadAppSettings();
        _robotViewModel.RefreshAvailablePorts();

        if (_serialService.IsVirtual && !_robotViewModel.IsSerialConnected)
            _robotViewModel.SerialPortName = VirtualRobotSerialService.VirtualPortName;

        if (!_robotViewModel.EnsureSerialConnected())
        {
            _dashboardTab?.SetRobotSerialDetail(_robotViewModel.StatusText);
            if (!_serialService.IsVirtual)
            {
                _dashboardTab?.AppendRobotRxLog(
                    $"Chưa mở được COM — kiểm tra Config/setting.json ({AppConfigPaths.SettingFile})");
            }
        }
        else
        {
            await _startupHandshake.RunAsync(msg =>
                Dispatcher.InvokeAsync(() => _dashboardTab?.SetRobotSerialDetail(msg)));
        }

        UpdateRobotSerialStatus();
    }

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

            _dashboardTab?.AppendRobotRxLog(string.Join(Environment.NewLine, _robotRxLog));
            _dashboardTab?.SetRobotSerialDetail($"RX: {visible}");
        });
    }

    private void UpdateRobotSerialStatus()
    {
        _dashboardTab?.UpdateRobotSerialStatus(
            _robotViewModel.IsSerialConnected,
            _robotViewModel.SerialPortName,
            _serialService.IsVirtual);

        if (!string.IsNullOrWhiteSpace(_robotViewModel.StatusText)
            && _robotViewModel.StatusText.StartsWith("Serial", StringComparison.OrdinalIgnoreCase))
            _dashboardTab?.SetRobotSerialDetail(_robotViewModel.StatusText);
    }

    private void btnDashboard_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.Dashboard);

    private void btnCommandHistory_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.JobHistory);

    private void btnManualControl_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.ManualControl);

    private void btnSetting_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.Setting);

    private void btnExit_Click(object sender, RoutedEventArgs e)
        => Close();

    private void btnTeaching_Click(object sender, RoutedEventArgs e)
        => NavigateTo(MainTabKind.RobotTeaching);

    private async void btnPass_Click(object sender, RoutedEventArgs e)
    {
        if (_materialTransfer.IsRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            var completed = false;
            await _materialTransfer.TransferPassAsync(msg =>
            {
                if (msg.Contains("hoàn tất", StringComparison.OrdinalIgnoreCase))
                    completed = true;
                Dispatcher.InvokeAsync(() => _dashboardTab?.SetStatusMessage(msg));
            });
            if (completed)
                _monitor.IncrementLoad();
        }
        finally
        {
            btnPass.IsEnabled = true;
            btnFail.IsEnabled = true;
        }
    }

    private async void btnFail_Click(object sender, RoutedEventArgs e)
    {
        if (_materialTransfer.IsRunning) return;

        btnPass.IsEnabled = false;
        btnFail.IsEnabled = false;
        try
        {
            var completed = false;
            await _materialTransfer.TransferFailAsync(msg =>
            {
                if (msg.Contains("hoàn tất", StringComparison.OrdinalIgnoreCase))
                    completed = true;
                Dispatcher.InvokeAsync(() => _dashboardTab?.SetStatusMessage(msg));
            });
            if (completed)
                _monitor.IncrementUnload();
        }
        finally
        {
            btnPass.IsEnabled = true;
            btnFail.IsEnabled = true;
        }
    }
}

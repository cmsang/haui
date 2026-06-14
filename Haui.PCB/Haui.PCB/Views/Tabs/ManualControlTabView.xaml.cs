using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Tabs;

/// <summary>
/// Tab Manual Control — test di chuyển PickUp → vị trí OK/NG đã teach.
/// </summary>
public partial class ManualControlTabView : UserControl
{
    private ManualControlViewModel? _viewModel;
    private IRobotSerialService? _serialService;
    private bool _ownsSerialService;
    private MonitorViewModel? _lineMonitor;
    private bool _initialized;

    public event EventHandler? CloseRequested;

    public MonitorViewModel? LineMonitor => _lineMonitor;

    public bool ShowCloseButton
    {
        get => BtnClose.Visibility == Visibility.Visible;
        set => BtnClose.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public ManualControlTabView() => InitializeComponent();

    public void Initialize(IRobotSerialService? sharedSerialService = null, MonitorViewModel? lineMonitor = null)
    {
        if (_initialized) return;
        _initialized = true;
        _lineMonitor = lineMonitor;

        if (sharedSerialService != null)
        {
            _serialService = sharedSerialService;
            _ownsSerialService = false;
        }
        else
        {
            _serialService = RobotSerialServiceFactory.Create(new AppSettingService());
            _ownsSerialService = true;
        }

        var appSettingService = new AppSettingService();
        _viewModel = new ManualControlViewModel(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService,
            disposeSerialService: _ownsSerialService);
        DataContext = _viewModel;

        DestinationGrid.ItemsSource = _viewModel.DestinationPoints;
        CboComPort.ItemsSource = _viewModel.AvailablePorts;

        foreach (var baud in _viewModel.BaudRateOptions)
            CboBaudRate.Items.Add(baud);

        CboComPort.Text = _viewModel.SerialPortName;
        CboBaudRate.SelectedItem = _viewModel.BaudRate;

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ManualControlViewModel.StatusText))
                TxtStatus.Text = _viewModel.StatusText;
            else if (e.PropertyName is nameof(ManualControlViewModel.IsSerialConnected))
                UpdateSerialStateUi();
            else if (e.PropertyName is nameof(ManualControlViewModel.SerialConnectButtonText))
                BtnSerialToggle.Content = _viewModel.SerialConnectButtonText;
            else if (e.PropertyName is nameof(ManualControlViewModel.SelectedDestinationSummary))
                TxtSelectedSummary.Text = _viewModel.SelectedDestinationSummary;
            else if (e.PropertyName is nameof(ManualControlViewModel.CanRunTest))
                BtnRunTest.IsEnabled = _viewModel.CanRunTest;
        };

        UpdateSerialStateUi();
        BtnSerialToggle.Content = _viewModel.SerialConnectButtonText;
        TxtStatus.Text = _viewModel.StatusText;
        TxtSelectedSummary.Text = _viewModel.SelectedDestinationSummary;
    }

    public void DisposePanel()
    {
        if (_viewModel == null) return;

        _viewModel.CancelPendingOperations();

        if (_ownsSerialService)
            _viewModel.Dispose();
        else
            _viewModel.Release();

        _viewModel = null;
        _initialized = false;
    }

    private void UpdateSerialStateUi()
    {
        if (_viewModel == null) return;

        var serialControlsEnabled = !_viewModel.IsVirtualSerial;
        CboComPort.IsEnabled = serialControlsEnabled;
        CboBaudRate.IsEnabled = serialControlsEnabled;
        BtnRefreshPorts.IsEnabled = serialControlsEnabled;

        if (_viewModel.IsSerialConnected && _viewModel.IsVirtualSerial)
        {
            TxtSerialState.Text = "● Serial ảo";
            TxtSerialState.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22));
            return;
        }

        if (_viewModel.IsSerialConnected)
        {
            TxtSerialState.Text = $"● {_viewModel.SerialPortName} Online";
            TxtSerialState.Foreground = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60));
        }
        else
        {
            TxtSerialState.Text = "● Offline";
            TxtSerialState.Foreground = new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
        }
    }

    private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.RefreshAvailablePorts();
        if (CboComPort.SelectedItem == null && _viewModel?.AvailablePorts.Count > 0)
            CboComPort.Text = _viewModel.AvailablePorts[0];
    }

    private void BtnSerialToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.SerialPortName = CboComPort.Text.Trim();
        if (CboBaudRate.SelectedItem is int baud)
            _viewModel.BaudRate = baud;

        _viewModel.ToggleSerialConnection();
    }

    private void BtnReloadTeach_Click(object sender, RoutedEventArgs e)
        => _viewModel?.ReloadDestinationPoints();

    private void DestinationGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DestinationGrid.SelectedItem is RobotTeachPoint point)
            _viewModel!.SelectedDestination = point;
    }

    private async void BtnRunTest_Click(object sender, RoutedEventArgs e)
    {
        BtnRunTest.IsEnabled = false;
        try
        {
            if (_viewModel != null)
                await _viewModel.RunPickUpToDestinationTestAsync();
        }
        finally
        {
            if (_viewModel != null)
                BtnRunTest.IsEnabled = _viewModel.CanRunTest;
        }
    }

    private void BtnGoPickUp_Click(object sender, RoutedEventArgs e)
        => _viewModel?.GoToPickUp();

    private void BtnGoDestination_Click(object sender, RoutedEventArgs e)
        => _viewModel?.GoToSelectedDestination();

    private void BtnClose_Click(object sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.ReloadDestinationPoints();
        CboComPort.Text = _viewModel.SerialPortName;
        CboBaudRate.SelectedItem = _viewModel.BaudRate;

        if (_ownsSerialService)
            _viewModel.EnsureSerialConnected();
        else
            _viewModel.SyncConnectionState();

        UpdateSerialStateUi();
        TxtStatus.Text = _viewModel.StatusText;
    }
}

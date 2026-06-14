using System.Windows;
using System.Windows.Controls;
using Haui.PCB.Models;
using Haui.PCB.Processing;
using Haui.PCB.ViewModels;

namespace Haui.PCB;

/// <summary>
/// Manual Control — test di chuyển PickUp → vị trí OK/NG đã teach.
/// </summary>
public partial class wdManualControl : Window
{
    private readonly ManualControlViewModel _viewModel;
    private readonly IRobotSerialService _serialService;
    private readonly bool _ownsSerialService;
    private bool _allowClose;

    public wdManualControl(IRobotSerialService? sharedSerialService = null)
    {
        InitializeComponent();

        if (sharedSerialService != null)
        {
            _serialService = sharedSerialService;
            _ownsSerialService = false;
        }
        else
        {
            _serialService = new RobotSerialService();
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
        };

        UpdateSerialStateUi();
        BtnSerialToggle.Content = _viewModel.SerialConnectButtonText;
        TxtStatus.Text = _viewModel.StatusText;
        TxtSelectedSummary.Text = _viewModel.SelectedDestinationSummary;
    }

    private void UpdateSerialStateUi()
    {
        if (_viewModel.IsSerialConnected)
        {
            TxtSerialState.Text = $"● {_viewModel.SerialPortName} Online";
            TxtSerialState.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x27, 0xAE, 0x60));
        }
        else
        {
            TxtSerialState.Text = "● Offline";
            TxtSerialState.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xE7, 0x4C, 0x3C));
        }
    }

    private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshAvailablePorts();
        if (CboComPort.SelectedItem == null && _viewModel.AvailablePorts.Count > 0)
            CboComPort.Text = _viewModel.AvailablePorts[0];
    }

    private void BtnSerialToggle_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SerialPortName = CboComPort.Text.Trim();
        if (CboBaudRate.SelectedItem is int baud)
            _viewModel.BaudRate = baud;

        _viewModel.ToggleSerialConnection();
    }

    private void BtnReloadTeach_Click(object sender, RoutedEventArgs e)
        => _viewModel.ReloadDestinationPoints();

    private void DestinationGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => _viewModel.SelectedDestination = DestinationGrid.SelectedItem as RobotTeachPoint;

    private async void BtnRunTest_Click(object sender, RoutedEventArgs e)
        => await _viewModel.RunPickUpToDestinationTestAsync();

    private async void BtnGoPickUp_Click(object sender, RoutedEventArgs e)
        => await _viewModel.GoToPickUpAsync();

    private async void BtnGoDestination_Click(object sender, RoutedEventArgs e)
        => await _viewModel.GoToSelectedDestinationAsync();

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.CanCloseWindow)
        {
            if (_viewModel.IsOperationInProgress)
                RobotWindowCloseHelper.ShowBusyCloseWarning();
            return;
        }

        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (RobotWindowCloseHelper.TryBlockCloseIfBusy(_viewModel.IsOperationInProgress, e))
            return;

        if (!_allowClose)
        {
            e.Cancel = true;
            _ = ReturnHomeAndCloseAsync();
            return;
        }

        FinalizeClose();
        base.OnClosing(e);
    }

    private async Task ReturnHomeAndCloseAsync()
    {
        try
        {
            await _viewModel.ReturnToHomeAsync();
        }
        catch
        {
            // Vẫn đóng màn hình nếu homing H0 thất bại.
        }
        finally
        {
            _allowClose = true;
            await Dispatcher.InvokeAsync(Close);
        }
    }

    private void FinalizeClose()
    {
        if (_ownsSerialService)
            _viewModel.Dispose();
        else
            _viewModel.Release();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
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

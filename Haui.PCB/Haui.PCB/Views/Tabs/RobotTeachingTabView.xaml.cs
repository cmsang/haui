using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Tabs;

/// <summary>
/// Tab Robot Teaching — teach vị trí robot 5 DOF + gripper qua SerialPort.
/// </summary>
public partial class RobotTeachingTabView : UserControl
{
    private RobotTeachViewModel? _viewModel;
    private IRobotSerialService? _serialService;
    private bool _ownsSerialService;
    private bool _initialized;

    public event EventHandler? CloseRequested;

    public bool ShowCloseButton
    {
        get => BtnClose.Visibility == Visibility.Visible;
        set => BtnClose.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public RobotTeachingTabView() => InitializeComponent();

    public void Initialize(IRobotSerialService? sharedSerialService = null)
    {
        if (_initialized) return;
        _initialized = true;

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
        _viewModel = new RobotTeachViewModel(
            new RobotConfigService(appSettingService),
            _serialService,
            appSettingService,
            disposeSerialService: _ownsSerialService);
        DataContext = _viewModel;

        JointsPanel.ItemsSource = _viewModel.Joints;
        TeachPointsGrid.ItemsSource = _viewModel.TeachPoints;
        CboComPort.ItemsSource = _viewModel.AvailablePorts;

        foreach (var step in _viewModel.JogStepOptions)
            CboJogStep.Items.Add(step);

        foreach (var baud in _viewModel.BaudRateOptions)
            CboBaudRate.Items.Add(baud);

        CboJogStep.SelectedItem = _viewModel.JogStep;
        CboComPort.Text = _viewModel.SerialPortName;
        CboBaudRate.SelectedItem = _viewModel.BaudRate;
        TxtStepsPerDeg.Text = _viewModel.StepsPerDeg.ToString();
        SldSpeed.Value = _viewModel.SpeedPercent;
        TxtSpeed.Text = $"{_viewModel.SpeedPercent}%";

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(RobotTeachViewModel.StatusText))
                TxtStatus.Text = _viewModel.StatusText;
            else if (e.PropertyName is nameof(RobotTeachViewModel.IsSerialConnected))
                UpdateSerialStateUi();
            else if (e.PropertyName is nameof(RobotTeachViewModel.SerialConnectButtonText))
                BtnSerialToggle.Content = _viewModel.SerialConnectButtonText;
        };

        foreach (var joint in _viewModel.Joints)
            joint.PropertyChanged += (_, _) => UpdateJointSummary();

        UpdateJointSummary();
        UpdateSerialStateUi();
        BtnSerialToggle.Content = _viewModel.SerialConnectButtonText;
        TxtStatus.Text = _viewModel.StatusText;

        if (_viewModel.SelectedPoint != null)
            TeachPointsGrid.SelectedItem = _viewModel.SelectedPoint;
    }

    public void DisposePanel()
    {
        if (_viewModel == null) return;

        _viewModel.SaveConfiguration();
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

    private void UpdateJointSummary()
        => TxtJointSummary.Text = _viewModel?.GetJointSummary() ?? string.Empty;

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
        if (int.TryParse(TxtStepsPerDeg.Text, out var steps))
            _viewModel.StepsPerDeg = steps;

        _viewModel.ToggleSerialConnection();
    }

    private void CboJogStep_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CboJogStep.SelectedItem is double step)
            _viewModel!.JogStep = step;
    }

    private void SldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || _viewModel == null) return;
        _viewModel.SpeedPercent = (int)e.NewValue;
        TxtSpeed.Text = $"{_viewModel.SpeedPercent}%";
    }

    private static RobotJointItem? GetJointFromSender(object sender)
    {
        if (sender is not DependencyObject current) return null;

        while (current != null)
        {
            if (current is FrameworkElement { Tag: RobotJointItem tagJoint })
                return tagJoint;
            if (current is FrameworkElement { DataContext: RobotJointItem dcJoint })
                return dcJoint;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void JogMinus_Click(object sender, RoutedEventArgs e)
    {
        if (GetJointFromSender(sender) is { } joint)
        {
            _viewModel?.JogJoint(joint, -1);
            UpdateJointSummary();
        }
    }

    private void JointHome_Click(object sender, RoutedEventArgs e)
    {
        if (GetJointFromSender(sender) is { } joint)
            _viewModel?.PerformHomingAxis(joint);
    }

    private void JogPlus_Click(object sender, RoutedEventArgs e)
    {
        if (GetJointFromSender(sender) is { } joint)
        {
            _viewModel?.JogJoint(joint, 1);
            UpdateJointSummary();
        }
    }

    private void JointSlider_Released(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider { DataContext: RobotJointItem joint }) return;

        if (joint.IsGripper)
            _viewModel?.SendGripperOnly();
        else
            _viewModel?.SendMoveCurrentJoints();

        UpdateJointSummary();
    }

    private void BtnHoming_Click(object sender, RoutedEventArgs e)
        => _viewModel?.PerformHoming();

    private void BtnZeroAll_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.ZeroAllJoints();
        UpdateJointSummary();
    }

    private void TeachPointsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TeachPointsGrid.SelectedItem is RobotTeachPoint point)
        {
            _viewModel!.SelectedPoint = point;
            UpdateJointSummary();
        }
    }

    private void BtnTeach_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.TeachSelectedPoint();
        TeachPointsGrid.Items.Refresh();
    }

    private void BtnGoTo_Click(object sender, RoutedEventArgs e)
    {
        _viewModel?.GoToSelectedPoint();
        UpdateJointSummary();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.SerialPortName = CboComPort.Text.Trim();
        if (CboBaudRate.SelectedItem is int baud)
            _viewModel.BaudRate = baud;
        if (int.TryParse(TxtStepsPerDeg.Text, out var steps))
            _viewModel.StepsPerDeg = steps;

        _viewModel.SaveConfiguration();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
        => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        _viewModel.ReloadAppSettings();
        _viewModel.ReloadTeachPoints();
        TeachPointsGrid.Items.Refresh();

        CboComPort.Text = _viewModel.SerialPortName;
        CboBaudRate.SelectedItem = _viewModel.BaudRate;

        if (_viewModel.SelectedPoint != null)
            TeachPointsGrid.SelectedItem = _viewModel.SelectedPoint;

        if (_ownsSerialService)
            _viewModel.EnsureSerialConnected();
        else
            _viewModel.SyncConnectionState();

        UpdateSerialStateUi();
        TxtStatus.Text = _viewModel.StatusText;
    }
}

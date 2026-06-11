using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel mÃ n Manual Control â€” test PickUp â†’ vá»‹ trÃ­ OK/NG theo chu trÃ¬nh cÃ³ chá» Dx.
/// </summary>
public class ManualControlViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IRobotConfigService _robotConfigService;
    private readonly IRobotSerialService _serialService;
    private readonly IAppSettingService _appSettingService;
    private readonly RobotPickPlaceExecutor _pickPlaceExecutor;
    private IReadOnlyList<RobotTeachPoint> _allTeachPoints = [];
    private readonly bool _disposeSerialService;
    private AppSetting _appSetting;
    private RobotTeachPoint? _selectedDestination;
    private string _statusText = "Káº¿t ná»‘i SerialPort, chá»n vá»‹ trÃ­ OK/NG vÃ  cháº¡y test.";
    private string _serialPort = "COM3";
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private int _speedPercent = 50;
    private bool _isSerialConnected;
    private bool _isTestRunning;
    private bool _disposed;
    private bool _closing;
    private CancellationTokenSource? _testCts;

    public ManualControlViewModel(
        IRobotConfigService robotConfigService,
        IRobotSerialService serialService,
        IAppSettingService appSettingService,
        bool disposeSerialService = true)
    {
        _robotConfigService = robotConfigService;
        _serialService = serialService;
        _appSettingService = appSettingService;
        _pickPlaceExecutor = new RobotPickPlaceExecutor(serialService);
        _disposeSerialService = disposeSerialService;
        _appSetting = appSettingService.Load();

        _serialPort = _appSetting.Com;
        _baudRate = _appSetting.BaudRate;
        _stepsPerDeg = _appSetting.StepsPerDeg;
        _speedPercent = _appSetting.SpeedPercent;

        DestinationPoints = [];
        ReloadDestinationPoints();

        RefreshAvailablePorts();
        _serialService.LineReceived += OnSerialLineReceived;
        SyncConnectionState();
    }

    public ObservableCollection<RobotTeachPoint> DestinationPoints { get; }
    public ObservableCollection<string> AvailablePorts { get; } = [];

    public bool IsTestRunning
    {
        get => _isTestRunning;
        private set
        {
            _isTestRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRunTest));
        }
    }

    public bool CanRunTest => !IsTestRunning && HasSelectedDestination;

    public RobotTeachPoint? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedDestination));
            OnPropertyChanged(nameof(SelectedDestinationSummary));
            OnPropertyChanged(nameof(CanRunTest));
        }
    }

    public bool HasSelectedDestination => SelectedDestination != null;

    public string SelectedDestinationSummary =>
        SelectedDestination == null
            ? "ChÆ°a chá»n vá»‹ trÃ­ Ä‘Ã­ch"
            : $"{SelectedDestination.Group} Â· {SelectedDestination.Name} â€” " +
              $"J1={SelectedDestination.J1:F1}, J2={SelectedDestination.J2:F1}, " +
              $"J3={SelectedDestination.J3:F1}, J4={SelectedDestination.J4:F1}, " +
              $"J5={SelectedDestination.J5:F1}, G={SelectedDestination.GripperAngle:F1}";

    public bool IsSerialConnected
    {
        get => _isSerialConnected;
        private set
        {
            _isSerialConnected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SerialConnectButtonText));
        }
    }

    public string SerialConnectButtonText => IsSerialConnected ? "Ngáº¯t káº¿t ná»‘i" : "Káº¿t ná»‘i";

    public string SerialPortName
    {
        get => _serialPort;
        set { _serialPort = value; OnPropertyChanged(); }
    }

    public int BaudRate
    {
        get => _baudRate;
        set { _baudRate = value; OnPropertyChanged(); }
    }

    public int SpeedPercent
    {
        get => _speedPercent;
        set { _speedPercent = Math.Clamp(value, 1, 100); OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public IReadOnlyList<int> BaudRateOptions { get; } = [9600, 19200, 38400, 57600, 115200];

    public event PropertyChangedEventHandler? PropertyChanged;

    public void ReloadDestinationPoints()
    {
        _appSetting = _appSettingService.Load();
        _speedPercent = _appSetting.SpeedPercent;
        OnPropertyChanged(nameof(SpeedPercent));

        _allTeachPoints = LoadAllTeachPoints();

        var selectedName = SelectedDestination?.Name;
        DestinationPoints.Clear();

        foreach (var point in RobotTeachPositions.Normalize(_allTeachPoints))
        {
            if (point.Group is not ("OK" or "NG")) continue;
            DestinationPoints.Add(point);
        }

        SelectedDestination = selectedName == null
            ? null
            : DestinationPoints.FirstOrDefault(p =>
                p.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));

        StatusText = $"ÄÃ£ táº£i {DestinationPoints.Count} vá»‹ trÃ­ OK/NG tá»« Database.";
    }

    public void RefreshAvailablePorts()
    {
        AvailablePorts.Clear();
        foreach (var port in _serialService.GetAvailablePorts())
            AvailablePorts.Add(port);

        if (string.IsNullOrWhiteSpace(SerialPortName) && AvailablePorts.Count > 0)
            SerialPortName = AvailablePorts[0];
    }

    public void SyncConnectionState()
        => IsSerialConnected = _serialService.IsConnected;

    public bool EnsureSerialConnected()
    {
        SyncConnectionState();
        if (IsSerialConnected)
        {
            StatusText = $"Serial online â€” {SerialPortName} @ {BaudRate}.";
            return true;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(SerialPortName))
            {
                StatusText = "ChÆ°a cáº¥u hÃ¬nh cá»•ng COM trong setting.json.";
                return false;
            }

            _serialService.Connect(SerialPortName, BaudRate);
            IsSerialConnected = true;
            StatusText = $"ÄÃ£ káº¿t ná»‘i {SerialPortName} @ {BaudRate}.";
            return true;
        }
        catch (Exception ex)
        {
            IsSerialConnected = false;
            StatusText = $"Káº¿t ná»‘i Serial tháº¥t báº¡i: {ex.Message}";
            return false;
        }
    }

    public void DisconnectSerial()
    {
        if (!IsSerialConnected) return;
        _serialService.Disconnect();
        IsSerialConnected = false;
    }

    public void ToggleSerialConnection()
    {
        if (IsSerialConnected)
        {
            DisconnectSerial();
            StatusText = "ÄÃ£ ngáº¯t káº¿t ná»‘i SerialPort.";
            return;
        }

        EnsureSerialConnected();
    }

    public void CancelTest()
    {
        if (!IsTestRunning) return;
        CancelPendingOperations();
        StatusText = "Äang há»§y chu trÃ¬nh test...";
    }

    /// <summary>Há»§y chu trÃ¬nh test vÃ  cÃ¡c thao tÃ¡c Ä‘ang chá» Dx trÃªn mÃ n hÃ¬nh nÃ y.</summary>
    public void CancelPendingOperations()
    {
        _closing = true;

        if (_testCts != null)
        {
            try { _testCts.Cancel(); }
            catch (ObjectDisposedException) { }

            _testCts.Dispose();
            _testCts = null;
        }

        IsTestRunning = false;
    }

    /// <summary>
    /// Chu trÃ¬nh: G180x â†’ PickUp â†’ G0x â†’ Wait â†’ Destination â†’ G180x â†’ Wait â†’ G0x.
    /// Má»—i bÆ°á»›c chá» pháº£n há»“i Dx tá»« robot.
    /// </summary>
    public async Task RunPickUpToDestinationTestAsync()
    {
        if (_closing || _disposed)
            return;

        if (IsTestRunning)
        {
            StatusText = "Chu trÃ¬nh test Ä‘ang cháº¡y.";
            return;
        }

        if (SelectedDestination == null)
        {
            StatusText = "Chá»n vá»‹ trÃ­ Ä‘Ã­ch (OK hoáº·c NG) trong báº£ng.";
            return;
        }

        var pickUp = GetTeachPoint(RobotTeachPositions.PickUp);
        var wait = GetTeachPoint(RobotTeachPositions.Wait);
        var destination = SelectedDestination;

        if (pickUp == null)
        {
            StatusText = "KhÃ´ng tÃ¬m tháº¥y vá»‹ trÃ­ PickUp trong cáº¥u hÃ¬nh teach.";
            return;
        }

        if (wait == null)
        {
            StatusText = "KhÃ´ng tÃ¬m tháº¥y vá»‹ trÃ­ Wait trong cáº¥u hÃ¬nh teach.";
            return;
        }

        if (!EnsureSerialConnected())
            return;

        _closing = false;
        _testCts = new CancellationTokenSource();
        IsTestRunning = true;

        try
        {
            var ct = _testCts.Token;

            await _pickPlaceExecutor.RunPickUpToDestinationAsync(
                pickUp, wait, destination, msg => StatusText = msg, ct);

            StatusText = $"Test hoÃ n táº¥t: PickUp â†’ {destination.Group} {destination.Name} â†’ Wait.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "ÄÃ£ há»§y chu trÃ¬nh test.";
        }
        catch (TimeoutException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            StatusText = $"Lá»—i chu trÃ¬nh test: {ex.Message}";
        }
        finally
        {
            IsTestRunning = false;
            _testCts?.Dispose();
            _testCts = null;
        }
    }

    public void GoToPickUp()
    {
        var pickUp = GetTeachPoint(RobotTeachPositions.PickUp);
        if (pickUp == null)
        {
            StatusText = "KhÃ´ng tÃ¬m tháº¥y vá»‹ trÃ­ PickUp.";
            return;
        }

        SendMoveOnly(pickUp, "PickUp");
    }

    public void GoToSelectedDestination()
    {
        if (SelectedDestination == null)
        {
            StatusText = "Chá»n vá»‹ trÃ­ Ä‘Ã­ch trong báº£ng.";
            return;
        }

        SendMoveOnly(SelectedDestination, SelectedDestination.Name);
    }

    public void Release()
    {
        if (_disposed) return;
        CancelPendingOperations();
        _serialService.LineReceived -= OnSerialLineReceived;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelPendingOperations();
        _serialService.LineReceived -= OnSerialLineReceived;

        if (_disposeSerialService)
        {
            DisconnectSerial();
            _serialService.Dispose();
        }
    }

    private RobotTeachPoint? GetTeachPoint(string name)
        => RobotTeachPositions.Normalize(_allTeachPoints)
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<RobotTeachPoint> LoadAllTeachPoints()
    {
        if (!_robotConfigService.TryLoadTeachPoints(out var dbPoints, out var error))
        {
            StatusText = $"KhÃ´ng táº£i Ä‘Æ°á»£c Database: {error}";
            return RobotTeachPositions.CreateDefault();
        }

        return dbPoints.Count > 0
            ? dbPoints
            : RobotTeachPositions.CreateDefault();
    }

    private void SendMoveOnly(RobotTeachPoint point, string label)
    {
        if (!TrySend(() =>
        {
            var cmd = RobotSerialProtocol.MoveCommand(
                point.J1, point.J2, point.J3, point.J4, point.J5);
            _serialService.SendAscii(cmd);
        }, out var err))
            StatusText = err;
        else
            StatusText = $"TX move â†’ {label} (khÃ´ng chá» Dx)";
    }

    private bool TrySend(Action send, out string error)
    {
        if (_closing || _disposed)
        {
            error = "Äang Ä‘Ã³ng mÃ n hÃ¬nh â€” thao tÃ¡c bá»‹ há»§y.";
            return false;
        }

        if (!IsSerialConnected)
        {
            error = "ChÆ°a káº¿t ná»‘i SerialPort.";
            return false;
        }

        try
        {
            send();
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Lá»—i Serial: {ex.Message}";
            return false;
        }
    }

    private void OnSerialLineReceived(string line)
    {
        if (IsTestRunning) return;

        var msg = line switch
        {
            _ when line.StartsWith('A') => $"Robot báº¯t Ä‘áº§u homing trá»¥c {line[1..]}...",
            _ when line.StartsWith('D') => $"Robot hoÃ n thÃ nh trá»¥c {line[1..]}.",
            _ => $"RX: {line}"
        };

        Application.Current?.Dispatcher.InvokeAsync(() => StatusText = msg);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

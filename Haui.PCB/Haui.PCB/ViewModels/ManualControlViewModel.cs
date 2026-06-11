using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Haui.PCB.Models;
using Haui.PCB.Processing;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel màn Manual Control — test PickUp → vị trí OK/NG theo chu trình có chờ Dx.
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
    private string _statusText = "Kết nối SerialPort, chọn vị trí OK/NG và chạy test.";
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
            ? "Chưa chọn vị trí đích"
            : $"{SelectedDestination.Group} · {SelectedDestination.Name} — " +
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

    public string SerialConnectButtonText => IsSerialConnected ? "Ngắt kết nối" : "Kết nối";

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

        StatusText = $"Đã tải {DestinationPoints.Count} vị trí OK/NG từ Database.";
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
            StatusText = $"Serial online — {SerialPortName} @ {BaudRate}.";
            return true;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(SerialPortName))
            {
                StatusText = "Chưa cấu hình cổng COM trong setting.json.";
                return false;
            }

            _serialService.Connect(SerialPortName, BaudRate);
            IsSerialConnected = true;
            StatusText = $"Đã kết nối {SerialPortName} @ {BaudRate}.";
            return true;
        }
        catch (Exception ex)
        {
            IsSerialConnected = false;
            StatusText = $"Kết nối Serial thất bại: {ex.Message}";
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
            StatusText = "Đã ngắt kết nối SerialPort.";
            return;
        }

        EnsureSerialConnected();
    }

    public void CancelTest()
    {
        if (!IsTestRunning) return;
        CancelPendingOperations();
        StatusText = "Đang hủy chu trình test...";
    }

    /// <summary>Hủy chu trình test và các thao tác đang chờ Dx trên màn hình này.</summary>
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
    /// Chu trình: G180x → PickUp → G0x → Wait → Destination → G180x → Wait → G0x.
    /// Mỗi bước chờ phản hồi Dx từ robot.
    /// </summary>
    public async Task RunPickUpToDestinationTestAsync()
    {
        if (_closing || _disposed)
            return;

        if (IsTestRunning)
        {
            StatusText = "Chu trình test đang chạy.";
            return;
        }

        if (SelectedDestination == null)
        {
            StatusText = "Chọn vị trí đích (OK hoặc NG) trong bảng.";
            return;
        }

        var pickUp = GetTeachPoint(RobotTeachPositions.PickUp);
        var wait = GetTeachPoint(RobotTeachPositions.Wait);
        var destination = SelectedDestination;

        if (pickUp == null)
        {
            StatusText = "Không tìm thấy vị trí PickUp trong cấu hình teach.";
            return;
        }

        if (wait == null)
        {
            StatusText = "Không tìm thấy vị trí Wait trong cấu hình teach.";
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

            StatusText = $"Test hoàn tất: PickUp → {destination.Group} {destination.Name} → Wait.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Đã hủy chu trình test.";
        }
        catch (TimeoutException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi chu trình test: {ex.Message}";
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
            StatusText = "Không tìm thấy vị trí PickUp.";
            return;
        }

        SendMoveOnly(pickUp, "PickUp");
    }

    public void GoToSelectedDestination()
    {
        if (SelectedDestination == null)
        {
            StatusText = "Chọn vị trí đích trong bảng.";
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
            StatusText = $"Không tải được Database: {error}";
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
            StatusText = $"TX move → {label} (không chờ Dx)";
    }

    private bool TrySend(Action send, out string error)
    {
        if (_closing || _disposed)
        {
            error = "Đang đóng màn hình — thao tác bị hủy.";
            return false;
        }

        if (!IsSerialConnected)
        {
            error = "Chưa kết nối SerialPort.";
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
            error = $"Lỗi Serial: {ex.Message}";
            return false;
        }
    }

    private void OnSerialLineReceived(string line)
    {
        if (IsTestRunning) return;

        var msg = line switch
        {
            _ when line.StartsWith('A') => $"Robot bắt đầu homing trục {line[1..]}...",
            _ when line.StartsWith('D') => $"Robot hoàn thành trục {line[1..]}.",
            _ => $"RX: {line}"
        };

        Application.Current?.Dispatcher.InvokeAsync(() => StatusText = msg);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}


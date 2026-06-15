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
    private bool _isAwaitingRobotDone;
    private bool _isReturningHome;
    private bool _disposed;
    private bool _closing;
    private CancellationTokenSource? _testCts;
    private CancellationTokenSource? _commandCts;

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
        _serialService.FrameReceived += OnSerialFrameReceived;
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
            NotifyControlStateChanged();
        }
    }

    public bool IsAwaitingRobotDone
    {
        get => _isAwaitingRobotDone;
        private set
        {
            if (_isAwaitingRobotDone == value) return;
            _isAwaitingRobotDone = value;
            OnPropertyChanged();
            NotifyControlStateChanged();
        }
    }

    /// <summary>Cho phép thao tác điều khiển robot trên màn hình.</summary>
    public bool CanOperateControls =>
        !IsAwaitingRobotDone && !IsTestRunning && !_isReturningHome && !_closing && !_disposed;

    public bool CanRunTest =>
        CanOperateControls && HasSelectedDestination && IsSerialConnected;

    public bool CanGoToPickUp => CanOperateControls && IsSerialConnected;

    public bool CanGoToDestination =>
        CanOperateControls && HasSelectedDestination && IsSerialConnected;

    /// <summary>Chu trình / lệnh đang chạy — chưa nhận Dx.</summary>
    public bool IsOperationInProgress => IsTestRunning || IsAwaitingRobotDone;

    /// <summary>Cho phép nhấn nút Đóng (không đang chờ Dx, không đang homing H0).</summary>
    public bool CanCloseWindow =>
        !IsTestRunning && !IsAwaitingRobotDone && !_isReturningHome;

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
            OnPropertyChanged(nameof(CanGoToDestination));
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
            NotifyControlStateChanged();
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

        if (_commandCts != null)
        {
            try { _commandCts.Cancel(); }
            catch (ObjectDisposedException) { }

            _commandCts.Dispose();
            _commandCts = null;
        }

        IsAwaitingRobotDone = false;
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

        if (IsTestRunning || IsAwaitingRobotDone)
        {
            StatusText = "Đang chờ robot hoàn thành (Dx)...";
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

    public async Task GoToPickUpAsync()
    {
        if (!CanGoToPickUp)
            return;

        var pickUp = GetTeachPoint(RobotTeachPositions.PickUp);
        if (pickUp == null)
        {
            StatusText = "Không tìm thấy vị trí PickUp.";
            return;
        }

        if (!EnsureSerialConnected())
            return;

        await RunMoveCommandAsync(pickUp, "PickUp");
    }

    public async Task GoToSelectedDestinationAsync()
    {
        if (!CanGoToDestination)
            return;

        if (SelectedDestination == null)
        {
            StatusText = "Chọn vị trí đích trong bảng.";
            return;
        }

        if (!EnsureSerialConnected())
            return;

        await RunMoveCommandAsync(SelectedDestination, SelectedDestination.Name);
    }

    private async Task RunMoveCommandAsync(RobotTeachPoint point, string label)
    {
        _closing = false;
        _commandCts?.Cancel();
        _commandCts?.Dispose();
        _commandCts = new CancellationTokenSource();
        var cts = _commandCts;
        IsAwaitingRobotDone = true;

        try
        {
            await _pickPlaceExecutor.MoveToPointAsync(
                point, msg => StatusText = msg, cts.Token);
            StatusText = $"Đã tới {label}.";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Đã hủy lệnh di chuyển.";
        }
        catch (TimeoutException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi di chuyển → {label}: {ex.Message}";
        }
        finally
        {
            if (ReferenceEquals(_commandCts, cts))
            {
                _commandCts.Dispose();
                _commandCts = null;
            }

            IsAwaitingRobotDone = false;
        }
    }

    /// <summary>Gửi H0x (homing firmware) và chờ Dx trước khi đóng màn hình.</summary>
    public async Task ReturnToHomeAsync(CancellationToken ct = default)
    {
        SyncConnectionState();
        if (!_serialService.IsConnected)
            return;

        SetReturningHome(true);
        try
        {
            await _pickPlaceExecutor.HomeAllAxesAsync(msg => StatusText = msg, ct);
        }
        catch (TimeoutException ex)
        {
            StatusText = ex.Message;
        }
        catch (Exception ex)
        {
            StatusText = $"Homing H0 khi đóng màn hình: {ex.Message}";
        }
        finally
        {
            SetReturningHome(false);
        }
    }

    public void Release()
    {
        if (_disposed) return;
        CancelPendingOperations();
        _serialService.FrameReceived -= OnSerialFrameReceived;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelPendingOperations();
        _serialService.FrameReceived -= OnSerialFrameReceived;

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

    private void OnSerialFrameReceived(string frame)
    {
        if (IsTestRunning || IsAwaitingRobotDone)
        {
            if (IsAwaitingRobotDone && RobotPickPlaceExecutor.IsDoneSignal(frame))
            {
                Application.Current?.Dispatcher.InvokeAsync(() =>
                    StatusText = $"Robot hoàn thành — R: {frame}");
            }

            return;
        }

        var msg = frame switch
        {
            _ when frame.StartsWith('A') => $"Robot bắt đầu homing trục {frame[1..^1]}...",
            _ when frame.StartsWith('D') => $"Robot hoàn thành trục {frame[1..^1]}.",
            _ => $"R: {frame}"
        };

        Application.Current?.Dispatcher.InvokeAsync(() => StatusText = msg);
    }

    private void SetReturningHome(bool value)
    {
        if (_isReturningHome == value) return;
        _isReturningHome = value;
        NotifyControlStateChanged();
    }

    private void NotifyControlStateChanged()
    {
        OnPropertyChanged(nameof(CanOperateControls));
        OnPropertyChanged(nameof(CanRunTest));
        OnPropertyChanged(nameof(CanGoToPickUp));
        OnPropertyChanged(nameof(CanGoToDestination));
        OnPropertyChanged(nameof(IsOperationInProgress));
        OnPropertyChanged(nameof(CanCloseWindow));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}


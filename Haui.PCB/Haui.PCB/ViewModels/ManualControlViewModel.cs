using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Haui.PCB.Models;
using Haui.PCB.Processing;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel màn Manual Control — test PickUp → vị trí OK/NG.
/// </summary>
public class ManualControlViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IRobotTeachService _teachService;
    private readonly IRobotSerialService _serialService;
    private readonly IAppSettingService _appSettingService;
    private RobotTeachConfig _config;
    private AppSetting _appSetting;
    private RobotTeachPoint? _selectedDestination;
    private string _statusText = "Kết nối SerialPort, chọn vị trí OK/NG và chạy test.";
    private string _serialPort = "COM3";
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private int _speedPercent = 50;
    private bool _isSerialConnected;
    private bool _disposed;

    public ManualControlViewModel(
        IRobotTeachService teachService,
        IRobotSerialService serialService,
        IAppSettingService appSettingService)
    {
        _teachService = teachService;
        _serialService = serialService;
        _appSettingService = appSettingService;
        _config = teachService.Load();
        _appSetting = appSettingService.Load();

        _serialPort = _appSetting.Com;
        _baudRate = _appSetting.BaudRate;
        _stepsPerDeg = _appSetting.StepsPerDeg;
        _speedPercent = _config.SpeedPercent;

        DestinationPoints = [];
        ReloadDestinationPoints();

        RefreshAvailablePorts();
        _serialService.LineReceived += OnSerialLineReceived;
    }

    public ObservableCollection<RobotTeachPoint> DestinationPoints { get; }
    public ObservableCollection<string> AvailablePorts { get; } = [];

    public RobotTeachPoint? SelectedDestination
    {
        get => _selectedDestination;
        set
        {
            _selectedDestination = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedDestination));
            OnPropertyChanged(nameof(SelectedDestinationSummary));
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
        _config = _teachService.Load();
        _speedPercent = _config.SpeedPercent;
        OnPropertyChanged(nameof(SpeedPercent));

        var selectedName = SelectedDestination?.Name;
        DestinationPoints.Clear();

        foreach (var point in RobotTeachPositions.Normalize(_config.TeachPoints))
        {
            if (point.Group is not ("OK" or "NG")) continue;
            DestinationPoints.Add(point);
        }

        SelectedDestination = selectedName == null
            ? null
            : DestinationPoints.FirstOrDefault(p =>
                p.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase));

        StatusText = $"Đã tải {DestinationPoints.Count} vị trí OK/NG từ cấu hình teach.";
    }

    public void RefreshAvailablePorts()
    {
        AvailablePorts.Clear();
        foreach (var port in _serialService.GetAvailablePorts())
            AvailablePorts.Add(port);

        if (string.IsNullOrWhiteSpace(SerialPortName) && AvailablePorts.Count > 0)
            SerialPortName = AvailablePorts[0];
    }

    public void ToggleSerialConnection()
    {
        if (IsSerialConnected)
        {
            _serialService.Disconnect();
            IsSerialConnected = false;
            StatusText = "Đã ngắt kết nối SerialPort.";
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(SerialPortName))
            {
                StatusText = "Chọn cổng COM.";
                return;
            }

            _serialService.Connect(SerialPortName, BaudRate);
            IsSerialConnected = true;
            StatusText = $"Đã kết nối {SerialPortName} @ {BaudRate}.";
        }
        catch (Exception ex)
        {
            IsSerialConnected = false;
            StatusText = $"Kết nối thất bại: {ex.Message}";
        }
    }

    /// <summary>PickUp → vị trí OK/NG đã chọn.</summary>
    public void RunPickUpToDestinationTest()
    {
        if (SelectedDestination == null)
        {
            StatusText = "Chọn vị trí đích (OK hoặc NG) trong bảng.";
            return;
        }

        var pickUp = GetPickUpPoint();
        if (pickUp == null)
        {
            StatusText = "Không tìm thấy vị trí PickUp trong cấu hình teach.";
            return;
        }

        if (!SendMoveToPoint(pickUp, "PickUp"))
            return;

        if (!SendMoveToPoint(SelectedDestination, SelectedDestination.Name))
            return;

        StatusText = $"Test hoàn tất: PickUp → {SelectedDestination.Group} {SelectedDestination.Name}.";
    }

    public void GoToPickUp()
    {
        var pickUp = GetPickUpPoint();
        if (pickUp == null)
        {
            StatusText = "Không tìm thấy vị trí PickUp.";
            return;
        }

        SendMoveToPoint(pickUp, "PickUp");
    }

    public void GoToSelectedDestination()
    {
        if (SelectedDestination == null)
        {
            StatusText = "Chọn vị trí đích trong bảng.";
            return;
        }

        SendMoveToPoint(SelectedDestination, SelectedDestination.Name);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _serialService.LineReceived -= OnSerialLineReceived;
        _serialService.Disconnect();
        _serialService.Dispose();
    }

    private RobotTeachPoint? GetPickUpPoint()
        => RobotTeachPositions.Normalize(_config.TeachPoints)
            .FirstOrDefault(p =>
                p.Name.Equals(RobotTeachPositions.PickUp, StringComparison.OrdinalIgnoreCase));

    private bool SendMoveToPoint(RobotTeachPoint point, string label)
    {
        var moveCmd = RobotSerialProtocol.MoveCommand(
            point.J1, point.J2, point.J3, point.J4, point.J5);

        if (!TrySend(() =>
        {
            _serialService.SendAscii(moveCmd);
            _serialService.SendGripperAngle(point.GripperAngle);
        }, out var err))
        {
            StatusText = err;
            return false;
        }

        StatusText = $"TX {moveCmd} + G → {label}";
        return true;
    }

    private bool TrySend(Action send, out string error)
    {
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
        var msg = line switch
        {
            _ when line.StartsWith('A') => $"Robot bắt đầu homing trục {line[1..]}...",
            _ when line.StartsWith('D') => $"Robot hoàn thành homing trục {line[1..]}.",
            _ => $"RX: {line}"
        };

        Application.Current?.Dispatcher.InvokeAsync(() => StatusText = msg);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

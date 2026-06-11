using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Má»™t khá»›p quay / gripper trÃªn panel Ä‘iá»u khiá»ƒn.
/// </summary>
public class RobotJointItem : INotifyPropertyChanged
{
    private double _angle;

    public int Index { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public double MinAngle { get; init; }
    public double MaxAngle { get; init; }
    public bool IsGripper { get; init; }

    /// <summary>Sá»‘ trá»¥c firmware 1â€“5 (J1â€“J5); gripper = 0.</summary>
    public int AxisNumber => IsGripper ? 0 : Index + 1;

    public double Angle
    {
        get => _angle;
        set
        {
            var clamped = Math.Clamp(Math.Round(value, 2), MinAngle, MaxAngle);
            if (Math.Abs(_angle - clamped) < 0.001) return;
            _angle = clamped;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AngleText));
        }
    }

    public string AngleText => $"{Angle:F1}Â°";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public static RobotJointItem FromLimits(RobotJointLimits limits, int index, double angle = 0) => new()
    {
        Index = index,
        Key = limits.Key,
        Label = limits.Label,
        MinAngle = limits.MinAngle,
        MaxAngle = limits.MaxAngle,
        IsGripper = limits.IsGripper,
        Angle = angle
    };
}

/// <summary>
/// ViewModel mÃ n hÃ¬nh teach vá»‹ trÃ­ robot 5 DOF RRRRR + gripper â€” gá»­i lá»‡nh qua SerialPort.
/// </summary>
public class RobotTeachViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IRobotConfigService _robotConfigService;
    private readonly IRobotSerialService _serialService;
    private readonly IAppSettingService _appSettingService;
    private readonly bool _disposeSerialService;
    private readonly bool _enableSerialEvents;
    private AppSetting _appSetting;
    private RobotTeachPoint? _selectedPoint;
    private string _statusText = "Káº¿t ná»‘i SerialPort Ä‘á»ƒ báº¯t Ä‘áº§u teach.";
    private string _serialPort = "COM3";
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private double _jogStep = 1.0;
    private int _speedPercent = 50;
    private bool _isSerialConnected;
    private bool _disposed;
    private bool _closing;

    public RobotTeachViewModel(
        IRobotConfigService robotConfigService,
        IRobotSerialService serialService,
        IAppSettingService appSettingService,
        bool disposeSerialService = true,
        bool enableSerialEvents = true)
    {
        _robotConfigService = robotConfigService;
        _serialService = serialService;
        _appSettingService = appSettingService;
        _disposeSerialService = disposeSerialService;
        _enableSerialEvents = enableSerialEvents;
        _appSetting = appSettingService.Load();

        _jogStep = _appSetting.JogStepDegrees;
        _speedPercent = _appSetting.SpeedPercent;
        _serialPort = _appSetting.Com;
        _baudRate = _appSetting.BaudRate;
        _stepsPerDeg = _appSetting.StepsPerDeg;

        Joints = new ObservableCollection<RobotJointItem>(
            RobotJointLimits.CreateDefault().Select((l, i) => RobotJointItem.FromLimits(l, i)));

        TeachPoints = new ObservableCollection<RobotTeachPoint>();
        ReloadTeachPoints();

        RefreshAvailablePorts();
        if (_enableSerialEvents)
            _serialService.LineReceived += OnSerialLineReceived;
        SyncConnectionState();
    }

    public ObservableCollection<RobotJointItem> Joints { get; }
    public ObservableCollection<RobotTeachPoint> TeachPoints { get; }
    public ObservableCollection<string> AvailablePorts { get; } = [];

    public RobotTeachPoint? SelectedPoint
    {
        get => _selectedPoint;
        set
        {
            _selectedPoint = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedPoint));
            OnPropertyChanged(nameof(IsSelectedPointStandard));

            if (value != null)
                ApplyPointToJoints(value);
        }
    }

    public bool HasSelectedPoint => SelectedPoint != null;

    public bool IsSelectedPointStandard =>
        SelectedPoint != null && RobotTeachPositions.IsStandard(SelectedPoint.Name);

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

    public int StepsPerDeg
    {
        get => _stepsPerDeg;
        set { _stepsPerDeg = Math.Max(1, value); OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public double JogStep
    {
        get => _jogStep;
        set
        {
            _jogStep = Math.Clamp(value, 0.1, 45);
            OnPropertyChanged();
        }
    }

    public int SpeedPercent
    {
        get => _speedPercent;
        set
        {
            _speedPercent = Math.Clamp(value, 1, 100);
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<double> JogStepOptions { get; } = [0.5, 1, 2, 5, 10, 15];
    public IReadOnlyList<int> BaudRateOptions { get; } = [9600, 19200, 38400, 57600, 115200];

    public event PropertyChangedEventHandler? PropertyChanged;

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

    public void JogJoint(RobotJointItem joint, int direction)
    {
        if (_closing || _disposed) return;

        joint.Angle += direction * JogStep;

        if (!IsSerialConnected)
        {
            StatusText = $"Jog {joint.Key} (offline): {joint.AngleText}";
            return;
        }

        try
        {
            var axisName = joint.IsGripper
                ? joint.Key
                : joint.AxisNumber.ToString(CultureInfo.InvariantCulture);
            var cmd = RobotSerialProtocol.JogCommand(axisName, direction > 0, JogStep);
            _serialService.SendAscii(cmd);
            StatusText = $"TX {cmd} â†’ {joint.AngleText}";
        }
        catch (Exception ex)
        {
            StatusText = $"Lá»—i Jog: {ex.Message}";
        }
    }

    public void SendMoveCurrentJoints()
    {
        if (Joints.Count < 6) return;

        var moveCmd = RobotSerialProtocol.MoveCommand(
            Joints[0].Angle, Joints[1].Angle, Joints[2].Angle,
            Joints[3].Angle, Joints[4].Angle);

        if (!TrySend(() =>
        {
            _serialService.SendAscii(moveCmd);
        }, out var err))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {moveCmd}";
    }

    public void SendGripperOnly()
    {
        if (Joints.Count < 6) return;
        var g = Joints[5];

        if (!TrySend(() => _serialService.SendGripperAngle(g.Angle), out var err))
        {
            StatusText = err;
            return;
        }

        StatusText = $"G â†’ {g.AngleText}";
    }

    public void PerformHoming()
    {
        const string cmd = "H0";
        if (!TrySend(() => _serialService.SendAscii(cmd), out var err))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {cmd} â€” Homing táº¥t cáº£ trá»¥c (3â†’2â†’1â†’4â†’5)...";
    }

    /// <summary>Homing má»™t trá»¥c J1â€“J5 â€” gá»­i "H1".."H5".</summary>
    public void PerformHomingAxis(RobotJointItem joint)
    {
        if (joint.IsGripper)
        {
            StatusText = "Gripper khÃ´ng dÃ¹ng lá»‡nh H â€” dÃ¹ng G hoáº·c jog.";
            return;
        }

        var cmd = RobotSerialProtocol.HomeCommand(joint.AxisNumber);
        if (!TrySend(() => _serialService.SendAscii(cmd), out var err))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {cmd} â€” Homing {joint.Key}...";
    }

    public void TeachSelectedPoint()
    {
        if (SelectedPoint == null)
        {
            StatusText = "Chá»n vá»‹ trÃ­ cáº§n teach.";
            return;
        }

        ApplyCurrentJointsToPoint(SelectedPoint);
        RefreshTeachPointBinding();

        if (!_robotConfigService.TrySaveTeachPoint(SelectedPoint, out var error))
        {
            StatusText = $"Teach \"{SelectedPoint.Name}\" tháº¥t báº¡i â€” {error}";
            return;
        }

        StatusText = $"ÄÃ£ teach \"{SelectedPoint.Name}\" â†’ lÆ°u Database.";
    }

    public void GoToSelectedPoint()
    {
        if (SelectedPoint == null)
        {
            StatusText = "Chá»n vá»‹ trÃ­ Ä‘á»ƒ di chuyá»ƒn tá»›i.";
            return;
        }

        SendMoveToPoint(SelectedPoint);
    }

    public void SaveConfiguration()
    {
        _appSetting.Com = SerialPortName;
        _appSetting.BaudRate = BaudRate;
        _appSetting.StepsPerDeg = StepsPerDeg;
        _appSetting.JogStepDegrees = JogStep;
        _appSetting.SpeedPercent = SpeedPercent;
        _appSettingService.Save(_appSetting);
        StatusText = "ÄÃ£ lÆ°u cáº¥u hÃ¬nh (setting.json).";
    }

    public void ReloadAppSettings()
    {
        _appSetting = _appSettingService.Load();
        SerialPortName = _appSetting.Com;
        BaudRate = _appSetting.BaudRate;
        StepsPerDeg = _appSetting.StepsPerDeg;
        JogStep = _appSetting.JogStepDegrees;
        SpeedPercent = _appSetting.SpeedPercent;
        StatusText = $"ÄÃ£ táº£i setting.json â€” COM={SerialPortName}, STEPS_PER_DEG={StepsPerDeg}.";
    }

    /// <summary>Táº£i láº¡i danh sÃ¡ch vá»‹ trÃ­ teach tá»« Database (BL â†’ DL).</summary>
    public void ReloadTeachPoints()
    {
        var selectedName = SelectedPoint?.Name;

        if (!_robotConfigService.TryLoadTeachPoints(out var dbPoints, out var error))
        {
            StatusText = $"KhÃ´ng táº£i Ä‘Æ°á»£c Database: {error}";
            return;
        }

        var points = dbPoints.Count > 0
            ? RobotTeachPositions.Normalize(dbPoints)
            : RobotTeachPositions.CreateDefault();

        TeachPoints.Clear();
        foreach (var point in points)
            TeachPoints.Add(point);

        SelectedPoint = selectedName == null
            ? TeachPoints.FirstOrDefault(p =>
                p.Name.Equals(RobotTeachPositions.Home, StringComparison.OrdinalIgnoreCase))
              ?? TeachPoints.FirstOrDefault()
            : TeachPoints.FirstOrDefault(p =>
                p.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase))
              ?? TeachPoints.FirstOrDefault();

        StatusText = dbPoints.Count > 0
            ? $"ÄÃ£ táº£i {dbPoints.Count} vá»‹ trÃ­ tá»« Database."
            : "Database trá»‘ng â€” hiá»ƒn thá»‹ danh sÃ¡ch máº·c Ä‘á»‹nh (cháº¡y seed SQL).";
    }

    public void ZeroAllJoints()
    {
        foreach (var joint in Joints)
            joint.Angle = 0;
        StatusText = "ÄÃ£ Ä‘áº·t táº¥t cáº£ khá»›p vá» 0Â° trÃªn UI.";
    }

    public string GetJointSummary()
        => string.Join("  |  ", Joints.Select(j => $"{j.Key}: {j.AngleText}"));

    public RobotTeachPoint? GetPoint(string name)
        => TeachPoints.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Há»§y cÃ¡c lá»‡nh robot Ä‘ang thá»±c hiá»‡n trÃªn mÃ n hÃ¬nh nÃ y.</summary>
    public void CancelPendingOperations()
        => _closing = true;

    public void Release()
    {
        if (_disposed) return;
        CancelPendingOperations();
        if (_enableSerialEvents)
            _serialService.LineReceived -= OnSerialLineReceived;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelPendingOperations();
        if (_enableSerialEvents)
            _serialService.LineReceived -= OnSerialLineReceived;

        if (_disposeSerialService)
        {
            DisconnectSerial();
            _serialService.Dispose();
        }
    }

    private void SendMoveToPoint(RobotTeachPoint point)
    {
        var moveCmd = RobotSerialProtocol.MoveCommand(
            point.J1, point.J2, point.J3, point.J4, point.J5);

        if (!TrySend(() =>
        {
            _serialService.SendAscii(moveCmd);
        }, out var err))
        {
            StatusText = err;
            return;
        }

        StatusText = $"Go To \"{point.Name}\" â€” TX {moveCmd} + G @ {SpeedPercent}%";
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
            error = "ChÆ°a káº¿t ná»‘i SerialPort â€” thao tÃ¡c chá»‰ cáº­p nháº­t UI.";
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
        var msg = line switch
        {
            _ when line.StartsWith('A') => $"Robot báº¯t Ä‘áº§u homing trá»¥c {line[1..]}...",
            _ when line.StartsWith('D') => $"Robot hoÃ n thÃ nh homing trá»¥c {line[1..]}.",
            _ => $"RX: {line}"
        };

        Application.Current?.Dispatcher.InvokeAsync(() => StatusText = msg);
    }

    private void ApplyCurrentJointsToPoint(RobotTeachPoint point)
    {
        point.J1 = Joints[0].Angle;
        point.J2 = Joints[1].Angle;
        point.J3 = Joints[2].Angle;
        point.J4 = Joints[3].Angle;
        point.J5 = Joints[4].Angle;
        point.GripperAngle = Joints[5].Angle;
    }

    private void ApplyPointToJoints(RobotTeachPoint point)
    {
        if (Joints.Count < 6) return;
        Joints[0].Angle = point.J1;
        Joints[1].Angle = point.J2;
        Joints[2].Angle = point.J3;
        Joints[3].Angle = point.J4;
        Joints[4].Angle = point.J5;
        Joints[5].Angle = point.GripperAngle;
    }

    private void RefreshTeachPointBinding()
    {
        var idx = SelectedPoint != null ? TeachPoints.IndexOf(SelectedPoint) : -1;
        if (idx >= 0)
        {
            var copy = SelectedPoint!;
            TeachPoints[idx] = copy;
            SelectedPoint = copy;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

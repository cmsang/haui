using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using Haui.PCB.Models;
using Haui.PCB.Processing;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Một khớp quay / gripper trên panel điều khiển.
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

    /// <summary>Số trục firmware 1–5 (J1–J5); gripper = 0.</summary>
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

    public string AngleText => $"{Angle:F1}°";

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
/// ViewModel màn hình teach vị trí robot 5 DOF RRRRR + gripper — gửi lệnh qua SerialPort.
/// </summary>
public class RobotTeachViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IRobotConfigService _robotConfigService;
    private readonly IRobotSerialService _serialService;
    private readonly IAppSettingService _appSettingService;
    private readonly RobotPickPlaceExecutor _pickPlaceExecutor;
    private readonly bool _disposeSerialService;
    private readonly bool _enableSerialEvents;
    private AppSetting _appSetting;
    private RobotTeachPoint? _selectedPoint;
    private string _statusText = "Kết nối SerialPort để bắt đầu teach.";
    private string _serialPort = "COM3";
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private double _jogStep = 1.0;
    private int _speedPercent = 50;
    private bool _isSerialConnected;
    private bool _isAwaitingRobotDone;
    private bool _isReturningHome;
    private bool _disposed;
    private bool _closing;
    private CancellationTokenSource? _awaitDoneCts;
    private static readonly TimeSpan AwaitDoneTimeout = TimeSpan.FromSeconds(120);

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
        _pickPlaceExecutor = new RobotPickPlaceExecutor(serialService);
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
        {
            _serialService.FrameReceived += OnSerialFrameReceived;
        }
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
            OnPropertyChanged(nameof(CanOperateRobot));
            OnPropertyChanged(nameof(CanAdjustJoints));
        }
    }

    public string SerialConnectButtonText => IsSerialConnected ? "Ngắt kết nối" : "Kết nối";

    /// <summary>Đang chờ robot phản hồi Dx sau lệnh jog/move/home/gripper.</summary>
    public bool IsAwaitingRobotDone
    {
        get => _isAwaitingRobotDone;
        private set
        {
            if (_isAwaitingRobotDone == value) return;
            _isAwaitingRobotDone = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanOperateRobot));
            OnPropertyChanged(nameof(CanAdjustJoints));
            OnPropertyChanged(nameof(CanCloseWindow));
        }
    }

    /// <summary>Cho phép gửi lệnh điều khiển robot (jog, move, home, go to).</summary>
    public bool CanOperateRobot =>
        IsSerialConnected && CanAdjustJoints;

    /// <summary>Cho phép thao tác khớp trên UI (jog/slider); khóa khi đang chờ Dx.</summary>
    public bool CanAdjustJoints =>
        !IsAwaitingRobotDone && !_isReturningHome && !_closing && !_disposed;

    /// <summary>Đang chờ Dx sau lệnh robot — chưa hoàn thành chu trình.</summary>
    public bool IsOperationInProgress => IsAwaitingRobotDone;

    /// <summary>Cho phép nhấn nút Đóng (không đang chờ Dx, không đang về Home).</summary>
    public bool CanCloseWindow => !IsAwaitingRobotDone && !_isReturningHome;

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

    public void JogJoint(RobotJointItem joint, int direction)
    {
        if (_closing || _disposed) return;

        joint.Angle += direction * JogStep;

        if (!IsSerialConnected)
        {
            StatusText = $"Jog {joint.Key} (offline): {joint.AngleText}";
            return;
        }

        if (IsAwaitingRobotDone)
        {
            StatusText = "Đang chờ robot hoàn thành (Dx)...";
            return;
        }

        try
        {
            var axisName = joint.IsGripper
                ? joint.Key
                : joint.AxisNumber.ToString(CultureInfo.InvariantCulture);
            var cmd = RobotSerialProtocol.JogCommand(axisName, direction > 0, JogStep);
            _serialService.SendAscii(cmd);
            BeginAwaitDone();
            StatusText = $"TX {cmd} → {joint.AngleText} — chờ Dx...";
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi Jog: {ex.Message}";
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
        }, out var err, awaitDone: true))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {moveCmd} — chờ Dx...";
    }

    public void SendGripperOnly()
    {
        if (Joints.Count < 6) return;
        var g = Joints[5];

        if (!TrySend(() => _serialService.SendGripperAngle(g.Angle), out var err, awaitDone: true))
        {
            StatusText = err;
            return;
        }

        StatusText = $"G → {g.AngleText} — chờ Dx...";
    }

    public void PerformHoming()
    {
        const string cmd = "H0";
        if (!TrySend(() => _serialService.SendAscii(cmd), out var err, awaitDone: true))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {cmd} — Homing tất cả trục (3→2→1→4→5) — chờ Dx...";
    }

    /// <summary>Homing một trục J1–J5 — gửi "H1".."H5".</summary>
    public void PerformHomingAxis(RobotJointItem joint)
    {
        if (joint.IsGripper)
        {
            StatusText = "Gripper không dùng lệnh H — dùng G hoặc jog.";
            return;
        }

        var cmd = RobotSerialProtocol.HomeCommand(joint.AxisNumber);
        if (!TrySend(() => _serialService.SendAscii(cmd), out var err, awaitDone: true))
        {
            StatusText = err;
            return;
        }

        StatusText = $"TX {cmd} — Homing {joint.Key} — chờ Dx...";
    }

    public void TeachSelectedPoint()
    {
        if (SelectedPoint == null)
        {
            StatusText = "Chọn vị trí cần teach.";
            return;
        }

        ApplyCurrentJointsToPoint(SelectedPoint);
        RefreshTeachPointBinding();

        if (!_robotConfigService.TrySaveTeachPoint(SelectedPoint, out var error))
        {
            StatusText = $"Teach \"{SelectedPoint.Name}\" thất bại — {error}";
            return;
        }

        StatusText = $"Đã teach \"{SelectedPoint.Name}\" → lưu Database.";
    }

    public void GoToSelectedPoint()
    {
        if (SelectedPoint == null)
        {
            StatusText = "Chọn vị trí để di chuyển tới.";
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
        StatusText = "Đã lưu cấu hình (setting.json).";
    }

    public void ReloadAppSettings()
    {
        _appSetting = _appSettingService.Load();
        SerialPortName = _appSetting.Com;
        BaudRate = _appSetting.BaudRate;
        StepsPerDeg = _appSetting.StepsPerDeg;
        JogStep = _appSetting.JogStepDegrees;
        SpeedPercent = _appSetting.SpeedPercent;
        StatusText = $"Đã tải setting.json — COM={SerialPortName}, STEPS_PER_DEG={StepsPerDeg}.";
    }

    /// <summary>Tải lại danh sách vị trí teach từ Database (BL → DL).</summary>
    public void ReloadTeachPoints()
    {
        var selectedName = SelectedPoint?.Name;

        if (!_robotConfigService.TryLoadTeachPoints(out var dbPoints, out var error))
        {
            StatusText = $"Không tải được Database: {error}";
            return;
        }

        var points = dbPoints.Count > 0
            ? RobotTeachPositions.Normalize(dbPoints)
            : RobotTeachPositions.CreateDefault();

        TeachPoints.Clear();
        foreach (var point in points)
        {
            if (point.Name.Equals("Home", StringComparison.OrdinalIgnoreCase))
                continue;
            TeachPoints.Add(point);
        }

        SelectedPoint = selectedName == null
            ? TeachPoints.FirstOrDefault(p =>
                p.Name.Equals(RobotTeachPositions.PickUp, StringComparison.OrdinalIgnoreCase))
              ?? TeachPoints.FirstOrDefault()
            : TeachPoints.FirstOrDefault(p =>
                p.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase))
              ?? TeachPoints.FirstOrDefault();

        StatusText = dbPoints.Count > 0
            ? $"Đã tải {dbPoints.Count} vị trí từ Database."
            : "Database trống — hiển thị danh sách mặc định (chạy seed SQL).";
    }

    public void ZeroAllJoints()
    {
        foreach (var joint in Joints)
            joint.Angle = 0;
        StatusText = "Đã đặt tất cả khớp về 0° trên UI.";
    }

    public string GetJointSummary()
        => string.Join("  |  ", Joints.Select(j => $"{j.Key}: {j.AngleText}"));

    public RobotTeachPoint? GetPoint(string name)
        => TeachPoints.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

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

    /// <summary>Hủy các lệnh robot đang thực hiện trên màn hình này.</summary>
    public void CancelPendingOperations()
    {
        _closing = true;
        CompleteAwaitDone();
    }

    public void Release()
    {
        if (_disposed) return;
        CancelPendingOperations();
        if (_enableSerialEvents)
            _serialService.FrameReceived -= OnSerialFrameReceived;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelPendingOperations();
        if (_enableSerialEvents)
            _serialService.FrameReceived -= OnSerialFrameReceived;

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
        }, out var err, awaitDone: true))
        {
            StatusText = err;
            return;
        }

        StatusText = $"Go To \"{point.Name}\" — TX {moveCmd} — chờ Dx...";
    }


    private bool TrySend(Action send, out string error, bool awaitDone = false)
    {
        if (_closing || _disposed)
        {
            error = "Đang đóng màn hình — thao tác bị hủy.";
            return false;
        }

        if (awaitDone && IsAwaitingRobotDone)
        {
            error = "Đang chờ robot hoàn thành (Dx)...";
            return false;
        }

        if (!IsSerialConnected)
        {
            error = "Chưa kết nối SerialPort — thao tác chỉ cập nhật UI.";
            return false;
        }

        try
        {
            send();
            if (awaitDone)
                BeginAwaitDone();
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Lỗi Serial: {ex.Message}";
            return false;
        }
    }

    private void BeginAwaitDone()
    {
        _awaitDoneCts?.Cancel();
        _awaitDoneCts?.Dispose();
        _awaitDoneCts = new CancellationTokenSource();
        var cts = _awaitDoneCts;
        IsAwaitingRobotDone = true;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(AwaitDoneTimeout, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (!IsAwaitingRobotDone || !ReferenceEquals(_awaitDoneCts, cts))
                    return;

                CompleteAwaitDone();
                StatusText = "Timeout — không nhận được Dx từ robot.";
            });
        });
    }

    private void CompleteAwaitDone()
    {
        if (!IsAwaitingRobotDone && _awaitDoneCts == null)
            return;

        _awaitDoneCts?.Cancel();
        _awaitDoneCts?.Dispose();
        _awaitDoneCts = null;
        IsAwaitingRobotDone = false;
    }

    private void SetReturningHome(bool value)
    {
        if (_isReturningHome == value) return;
        _isReturningHome = value;
        OnPropertyChanged(nameof(CanAdjustJoints));
        OnPropertyChanged(nameof(CanOperateRobot));
        OnPropertyChanged(nameof(CanCloseWindow));
    }

    private void OnSerialFrameReceived(string frame)
    {
        if (RobotPickPlaceExecutor.IsDoneSignal(frame))
        {
            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (!IsAwaitingRobotDone) return;
                CompleteAwaitDone();
                StatusText = $"Robot hoàn thành — R: {frame}";
            });
            return;
        }

        if (IsAwaitingRobotDone) return;

        var msg = frame switch
        {
            _ when frame.StartsWith('A') => $"Robot bắt đầu homing trục {frame[1..^1]}...",
            _ when frame.StartsWith('D') => $"Robot hoàn thành — R: {frame}",
            _ => $"R: {frame}"
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

using BL.PCBDetect.Models;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Luồng Pass/Fail material — PickUp → ô OK/NG trống (EMPTY), đánh dấu FULL, báo warehouse khi buffer đầy.
/// </summary>
public class MaterialTransferService : IMaterialTransferService
{
    private readonly IRobotConfigService _robotConfigService;
    private readonly IRobotSerialService _serialService;
    private readonly IAppSettingService _appSettingService;
    private readonly WarehouseSerialService _warehouseSerialService;
    private readonly RobotPickPlaceExecutor _pickPlaceExecutor;

    private bool _isRunning;
    private CancellationTokenSource? _cts;

    public MaterialTransferService(
        IRobotConfigService robotConfigService,
        IRobotSerialService serialService,
        IAppSettingService appSettingService)
    {
        _robotConfigService = robotConfigService;
        _serialService = serialService;
        _appSettingService = appSettingService;
        _warehouseSerialService = new WarehouseSerialService(appSettingService);
        _pickPlaceExecutor = new RobotPickPlaceExecutor(serialService);
    }

    public bool IsRunning => _isRunning;

    public void Cancel()
    {
        if (!_isRunning) return;

        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    public Task TransferPassAsync(Action<string> reportStatus)
        => TransferAsync(isPass: true, reportStatus);

    public Task TransferFailAsync(Action<string> reportStatus)
        => TransferAsync(isPass: false, reportStatus);

    private async Task TransferAsync(bool isPass, Action<string> reportStatus)
    {
        if (_isRunning)
        {
            reportStatus("Chu trình chuyển material đang chạy.");
            return;
        }

        if (!_robotConfigService.TryLoadTeachPoints(out var teachPoints, out var dbError))
        {
            reportStatus($"Không tải được vị trí teach: {dbError}");
            return;
        }

        var offset = _appSettingService.Load().WaitPointJoint234OffsetDegrees;
        var points = RobotTeachPositions.Normalize(teachPoints, offset);
        var pickUp = FindPoint(points, RobotTeachPositions.PickUp);
        var wait = FindPoint(points, RobotTeachPositions.Wait);

        if (pickUp == null)
        {
            reportStatus("Không tìm thấy vị trí PickUp trong Database.");
            return;
        }

        if (wait == null)
        {
            reportStatus("Không tìm thấy vị trí Wait trong Database — cần teach Wait.");
            return;
        }

        var waitPickUp = RobotTeachPositions.ResolveWaitPickUp(points, offset);
        if (waitPickUp == null)
        {
            reportStatus("Không tìm thấy Wait PickUp — teach Wait PickUp hoặc PickUp.");
            return;
        }

        var slotNames = isPass ? RobotTeachPositions.OkSlotNames : RobotTeachPositions.NgSlotNames;
        var slotName = FindFirstEmptySlot(points, slotNames);
        if (slotName == null)
        {
            var group = isPass ? "OK" : "NG";
            NotifyWarehouseIfBufferFull(points, isPass, reportStatus);
            reportStatus($"Tất cả slot {group} đã FULL — không còn chỗ trống.");
            return;
        }

        var destination = FindPoint(points, slotName);
        if (destination == null)
        {
            reportStatus($"Không tìm thấy vị trí {slotName} trong Database.");
            return;
        }

        var referenceName = isPass ? RobotTeachPositions.Ok1 : RobotTeachPositions.Ng1;
        if (FindPoint(points, referenceName) == null)
        {
            reportStatus($"Không tìm thấy {referenceName} — cần teach Wait Place {(isPass ? "OK" : "NG")} hoặc {referenceName}.");
            return;
        }

        var waitPlace = RobotTeachPositions.FindWaitPlaceForDestination(points, destination, offset);
        if (waitPlace == null)
        {
            reportStatus($"Không tính được Wait Place {(isPass ? "OK" : "NG")}.");
            return;
        }

        _cts = new CancellationTokenSource();
        _isRunning = true;

        var label = isPass ? "PASS" : "FAIL";

        try
        {
            await _warehouseSerialService.RequestMaterialTransferAsync(reportStatus, _cts.Token);

            if (!EnsureRobotSerialConnected(reportStatus))
                return;

            reportStatus($"{label} — bắt đầu: PickUp → {slotName} (EMPTY)...");

            await _pickPlaceExecutor.RunPickUpToDestinationAsync(
                pickUp, waitPickUp, wait, waitPlace, destination, reportStatus, _cts.Token);

            if (!_robotConfigService.TryMarkSlotFull(slotName, out var markError))
            {
                reportStatus(
                    $"{label} — robot xong nhưng cập nhật FULL thất bại ({slotName}): {markError}");
                return;
            }

            if (_robotConfigService.TryLoadTeachPoints(out var updatedPoints, out _))
            {
                var updated = RobotTeachPositions.Normalize(updatedPoints, offset);
                NotifyWarehouseIfBufferFull(updated, isPass, reportStatus);
            }

            reportStatus(
                $"{label} hoàn tất: PickUp → {destination.Group} {slotName} → Wait. Slot {slotName} = FULL.");
        }
        catch (OperationCanceledException)
        {
            reportStatus($"Đã hủy chu trình {label}.");
        }
        catch (TimeoutException ex)
        {
            reportStatus(ex.Message);
        }
        catch (Exception ex)
        {
            reportStatus($"Lỗi chu trình {label}: {ex.Message}");
        }
        finally
        {
            _isRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void NotifyWarehouseIfBufferFull(
        IEnumerable<RobotTeachPoint> points,
        bool isPass,
        Action<string> reportStatus)
    {
        var slotNames = isPass ? RobotTeachPositions.OkSlotNames : RobotTeachPositions.NgSlotNames;
        if (!AreAllSlotsFull(points, slotNames))
            return;

        var command = isPass
            ? RobotSerialProtocol.WarehouseOkBufferFull
            : RobotSerialProtocol.WarehouseNgBufferFull;

        var group = isPass ? "OK" : "NG";

        if (!TrySendWarehouseCommand(command, out var warehouseCom, out var error))
        {
            reportStatus($"Tất cả slot {group} FULL — gửi {command}x thất bại: {error}");
            return;
        }

        reportStatus($"Tất cả slot {group} FULL — đã gửi {command}x → {warehouseCom}.");
    }

    private bool TrySendWarehouseCommand(string command, out string warehouseCom, out string? error)
        => _warehouseSerialService.TrySendCommand(command, out warehouseCom, out error);

    private static bool AreAllSlotsFull(
        IEnumerable<RobotTeachPoint> points,
        IReadOnlyList<string> slotNames)
    {
        var map = points.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in slotNames)
        {
            if (!map.TryGetValue(name, out var point))
                return false;

            if (SlotFullState.IsEmpty(point.FullState))
                return false;
        }

        return slotNames.Count > 0;
    }

    private static string? FindFirstEmptySlot(
        IEnumerable<RobotTeachPoint> points,
        IReadOnlyList<string> slotNames)
    {
        var map = points.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in slotNames)
        {
            if (!map.TryGetValue(name, out var point))
                continue;

            if (SlotFullState.IsEmpty(point.FullState))
                return name;
        }

        return null;
    }

    private bool EnsureRobotSerialConnected(Action<string> reportStatus)
    {
        if (_serialService.IsConnected)
            return true;

        var setting = _appSettingService.Load();
        if (string.IsNullOrWhiteSpace(setting.Com))
        {
            reportStatus("Chưa cấu hình cổng COM trong setting.json.");
            return false;
        }

        try
        {
            _serialService.Connect(setting.Com, setting.BaudRate);
            reportStatus($"Đã kết nối {setting.Com} @ {setting.BaudRate}.");
            return true;
        }
        catch (Exception ex)
        {
            reportStatus($"Kết nối Serial thất bại: {ex.Message}");
            return false;
        }
    }

    private static RobotTeachPoint? FindPoint(IEnumerable<RobotTeachPoint> points, string name)
        => points.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

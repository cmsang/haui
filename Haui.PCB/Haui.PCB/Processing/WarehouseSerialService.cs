namespace Haui.PCB.Processing;

/// <summary>
/// Giao tiếp cổng warehouse — CMx/COx trước khi robot gắp hàng; C1x/C2x khi buffer đầy.
/// </summary>
public class WarehouseSerialService
{
    private static readonly TimeSpan ReadyResponseTimeout = TimeSpan.FromSeconds(120);

    private readonly IAppSettingService _appSettingService;

    public WarehouseSerialService(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    /// <summary>Gửi CMx, chờ COx từ nhà kho trước khi chạy robot.</summary>
    public async Task RequestMaterialTransferAsync(
        Action<string> reportStatus,
        CancellationToken ct)
    {
        var setting = _appSettingService.Load();

        if (setting.UseVirtualSerial())
        {
            reportStatus("Virtual serial — bỏ qua CMx/COx (coi như nhà kho OK).");
            return;
        }

        var warehouseCom = setting.WarehouseCom?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(warehouseCom))
            throw new InvalidOperationException("Chưa cấu hình warehouseCom trong setting.json.");

        reportStatus($"Gửi {RobotSerialProtocol.WarehouseMaterialRequest}x → {warehouseCom}, chờ {RobotSerialProtocol.WarehouseMaterialReady}x...");

        using var warehouseSerial = new RobotSerialService();
        warehouseSerial.Connect(warehouseCom, setting.BaudRate);

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnFrame(string frame)
        {
            if (IsReadyResponse(frame))
                tcs.TrySetResult();
        }

        warehouseSerial.FrameReceived += OnFrame;

        try
        {
            warehouseSerial.SendAscii(RobotSerialProtocol.WarehouseMaterialRequest);

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(ReadyResponseTimeout);

            var completed = await Task.WhenAny(
                tcs.Task,
                Task.Delay(Timeout.InfiniteTimeSpan, linked.Token));

            if (completed != tcs.Task)
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException(ct);

                throw new TimeoutException(
                    $"Timeout — không nhận được {RobotSerialProtocol.WarehouseMaterialReady}x từ nhà kho ({warehouseCom}).");
            }

            await tcs.Task;
            reportStatus($"Nhà kho phản hồi {RobotSerialProtocol.WarehouseMaterialReady}x — bắt đầu robot.");
        }
        finally
        {
            warehouseSerial.FrameReceived -= OnFrame;
        }
    }

    public bool TrySendCommand(string command, out string warehouseCom, out string? error)
    {
        warehouseCom = string.Empty;
        error = null;

        var setting = _appSettingService.Load();
        warehouseCom = setting.WarehouseCom?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(warehouseCom))
        {
            error = "Chưa cấu hình warehouseCom trong setting.json.";
            return false;
        }

        if (setting.UseVirtualSerial())
            return true;

        try
        {
            using var warehouseSerial = new RobotSerialService();
            warehouseSerial.Connect(warehouseCom, setting.BaudRate);
            warehouseSerial.SendAscii(command);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool IsReadyResponse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        foreach (var segment in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (IsCoFrame(segment.Trim()))
                return true;
        }

        return IsCoFrame(text.Trim());
    }

    private static bool IsCoFrame(string frame)
    {
        if (frame.Length < 2)
            return false;

        return frame[0] == 'C'
               && char.ToUpperInvariant(frame[1]) == 'O';
    }
}

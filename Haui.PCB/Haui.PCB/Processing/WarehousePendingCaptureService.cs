namespace Haui.PCB.Processing;

/// <summary>
/// Lưu yêu cầu CAPx từ nhà kho khi PC chưa thể chụp ngay (Teaching/Manual, camera tắt, đang bận).
/// Nhà kho chỉ gửi CAP một lần — pending được giữ cho tới khi chụp thành công hoặc bị thay bằng CAP mới.
/// </summary>
public sealed class WarehousePendingCaptureService
{
    private volatile bool _pending;

    public bool HasPending => _pending;

    public void MarkPending() => _pending = true;

    public void Clear() => _pending = false;

    /// <summary>Atomically lấy và xóa cờ pending.</summary>
    public bool TryTakePending()
    {
        if (!_pending)
            return false;

        _pending = false;
        return true;
    }
}

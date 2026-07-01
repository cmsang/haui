namespace Haui.PCB.Processing;

/// <summary>
/// Chặn luồng tự động (CAPx → nhận dạng → robot) khi operator mở Teaching hoặc Manual Control.
/// </summary>
public sealed class RobotManualInterventionGate
{
    private int _depth;
    private string? _activeLabel;

    public bool IsActive => Volatile.Read(ref _depth) > 0;

    public string ActiveLabel => _activeLabel ?? "Teaching/Manual Control";

    public void Enter(string label)
    {
        if (Interlocked.Increment(ref _depth) == 1)
            _activeLabel = label;
    }

    public void Exit()
    {
        if (Interlocked.Decrement(ref _depth) == 0)
            _activeLabel = null;
    }
}

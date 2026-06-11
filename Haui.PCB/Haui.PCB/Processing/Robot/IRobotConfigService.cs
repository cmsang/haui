
namespace Haui.PCB.Processing.Robot;

public interface IRobotConfigService
{
    bool TryLoadTeachPoints(out IReadOnlyList<RobotTeachPoint> points, out string? error);

    bool TrySaveTeachPoint(RobotTeachPoint point, out string? error);

    bool TryMarkSlotFull(string posName, out string? error);
}

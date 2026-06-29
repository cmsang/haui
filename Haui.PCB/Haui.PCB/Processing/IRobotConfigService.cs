using Haui.PCB.Models;

namespace Haui.PCB.Processing;

public interface IRobotConfigService
{
    bool TryLoadTeachPoints(out IReadOnlyList<RobotTeachPoint> points, out string? error);

    bool TrySaveTeachPoint(RobotTeachPoint point, out string? error);

    bool TryMarkSlotFull(string posName, out string? error);

    bool TryMarkSlotEmpty(string posName, out string? error);
}

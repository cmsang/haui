using BL.PCBDetect.Models;

namespace BL.PCBDetect;

public interface IRobotConfigBL
{
    bool TryLoadTeachPoints(out IReadOnlyList<RobotTeachPointInfo> points, out string? error);

    bool TrySaveTeachPoint(RobotTeachPointInfo point, out string? error);
}

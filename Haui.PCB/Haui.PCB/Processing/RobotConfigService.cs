using BL.PCBDetect;
using BL.PCBDetect.Models;
using DL.PCBDetect;
using Haui.PCB.Models;

namespace Haui.PCB.Processing;

/// <summary>
/// Adapter UI — uỷ thác đọc/ghi RobotConfig cho tầng BL.
/// </summary>
public class RobotConfigService : IRobotConfigService
{
    private readonly IRobotConfigBL _robotConfigBL;

    public RobotConfigService(IAppSettingService appSettingService)
    {
        _robotConfigBL = new RobotConfigBL(
            new RobotConfigRepository(),
            new AppSettingConnectionProvider(appSettingService));
    }

    public RobotConfigService(IRobotConfigBL robotConfigBL)
    {
        _robotConfigBL = robotConfigBL;
    }

    public bool TryLoadTeachPoints(out IReadOnlyList<RobotTeachPoint> points, out string? error)
    {
        if (!_robotConfigBL.TryLoadTeachPoints(out var infos, out error))
        {
            points = [];
            return false;
        }

        points = infos.Select(ToModel).ToList();
        return true;
    }

    public bool TrySaveTeachPoint(RobotTeachPoint point, out string? error)
        => _robotConfigBL.TrySaveTeachPoint(ToInfo(point), out error);

    public bool TryMarkSlotFull(string posName, out string? error)
        => _robotConfigBL.TryMarkSlotFull(posName, out error);

    public bool TryMarkSlotEmpty(string posName, out string? error)
        => _robotConfigBL.TryMarkSlotEmpty(posName, out error);

    private static RobotTeachPoint ToModel(RobotTeachPointInfo info) => new()
    {
        Name = info.Name,
        Group = info.Group,
        J1 = info.J1,
        J2 = info.J2,
        J3 = info.J3,
        J4 = info.J4,
        J5 = info.J5,
        FullState = info.FullState
    };

    private static RobotTeachPointInfo ToInfo(RobotTeachPoint point) => new()
    {
        Name = point.Name,
        Group = point.Group,
        J1 = point.J1,
        J2 = point.J2,
        J3 = point.J3,
        J4 = point.J4,
        J5 = point.J5
    };
}

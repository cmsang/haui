using Haui.PCB.Models;

namespace Haui.PCB.Processing;

public interface IRobotTeachService
{
    RobotTeachConfig Load();
    void Save(RobotTeachConfig config);
}

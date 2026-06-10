using DL.PCBDetect.Entities;

namespace DL.PCBDetect;

public interface IRobotConfigRepository
{
    List<RobotConfigEntity> GetAll(string connectionString);

    void UpdateTeachPoint(
        string connectionString,
        string posName,
        string j1,
        string j2,
        string j3,
        string j4,
        string j5);

    void UpdateFullState(string connectionString, string posName, string fullState);
}

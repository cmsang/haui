using BL.PCBDetect.Models;
using DL.PCBDetect;

namespace BL.PCBDetect;

/// <summary>
/// Nghiệp vụ lưu / tải vị trí teach robot qua tầng DL.
/// </summary>
public class RobotConfigBL : IRobotConfigBL
{
    private readonly IRobotConfigRepository _repository;
    private readonly IDatabaseConnectionProvider _connectionProvider;

    public RobotConfigBL(
        IRobotConfigRepository repository,
        IDatabaseConnectionProvider connectionProvider)
    {
        _repository = repository;
        _connectionProvider = connectionProvider;
    }

    public bool TryLoadTeachPoints(out IReadOnlyList<RobotTeachPointInfo> points, out string? error)
    {
        points = [];
        error = null;

        if (!TryGetConnectionString(out var connectionString))
        {
            error = "Chưa cấu hình DatabaseConnection trong setting.json.";
            return false;
        }

        try
        {
            points = _repository.GetAll(connectionString)
                .Select(entity => new RobotTeachPointInfo
                {
                    Name = entity.PosName,
                    Group = entity.PosGroup,
                    J1 = RobotConfigRepository.ParseAngle(entity.J1),
                    J2 = RobotConfigRepository.ParseAngle(entity.J2),
                    J3 = RobotConfigRepository.ParseAngle(entity.J3),
                    J4 = RobotConfigRepository.ParseAngle(entity.J4),
                    J5 = RobotConfigRepository.ParseAngle(entity.J5),
                    FullState = string.IsNullOrWhiteSpace(entity.FullState)
                        ? SlotFullState.Empty
                        : entity.FullState
                })
                .ToList();

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TrySaveTeachPoint(RobotTeachPointInfo point, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(point.Name))
        {
            error = "Tên vị trí không hợp lệ.";
            return false;
        }

        if (!TryGetConnectionString(out var connectionString))
        {
            error = "Chưa cấu hình DatabaseConnection trong setting.json.";
            return false;
        }

        try
        {
            _repository.UpdateTeachPoint(
                connectionString,
                point.Name.Trim(),
                RobotConfigRepository.FormatAngle(point.J1),
                RobotConfigRepository.FormatAngle(point.J2),
                RobotConfigRepository.FormatAngle(point.J3),
                RobotConfigRepository.FormatAngle(point.J4),
                RobotConfigRepository.FormatAngle(point.J5));

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TryMarkSlotFull(string posName, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(posName))
        {
            error = "Tên slot không hợp lệ.";
            return false;
        }

        if (!TryGetConnectionString(out var connectionString))
        {
            error = "Chưa cấu hình DatabaseConnection trong setting.json.";
            return false;
        }

        try
        {
            _repository.UpdateFullState(connectionString, posName.Trim(), SlotFullState.Full);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool TryGetConnectionString(out string connectionString)
    {
        connectionString = _connectionProvider.GetConnectionString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(connectionString);
    }
}

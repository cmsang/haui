namespace Haui.PCB.Processing.Robot;

/// <summary>
/// Creates the robot serial service implementation from application settings.
/// </summary>
public static class RobotSerialServiceFactory
{
    public static IRobotSerialService Create(IAppSettingService appSettingService)
        => new SettingsAwareRobotSerialService(appSettingService);
}

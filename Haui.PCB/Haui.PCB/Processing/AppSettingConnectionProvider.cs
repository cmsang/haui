using BL.PCBDetect;

namespace Haui.PCB.Processing;

/// <summary>
/// Cung cấp connection string SQL Server từ setting.json cho tầng BL.
/// </summary>
public class AppSettingConnectionProvider : IDatabaseConnectionProvider
{
    private readonly IAppSettingService _appSettingService;

    public AppSettingConnectionProvider(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public string? GetConnectionString()
        => _appSettingService.Load().DatabaseConnection;
}

using Haui.PCB.Models;

namespace Haui.PCB.Processing;

public interface IAppSettingService
{
    AppSetting Load();
    void Save(AppSetting setting);
}

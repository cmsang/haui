using System.Windows;
using System.Windows.Controls;

namespace Haui.PCB.Views.Tabs;

public partial class SettingTabView : UserControl
{
    private readonly IAppSettingService _appSettingService = new AppSettingService();
    private bool _loading;

    public SettingTabView() => InitializeComponent();

    private void UserControl_Loaded(object sender, RoutedEventArgs e) => LoadFromSettings();

    private void LoadFromSettings()
    {
        _loading = true;
        try
        {
            var setting = _appSettingService.Load();
            ChkDeveloperMode.IsChecked = setting.DeveloperMode;
            ChkVirtualSerialPort.IsChecked = setting.VirtualSerialPort;
            UpdateVirtualSerialPanelVisibility();
            TxtSaveStatus.Text = string.Empty;
        }
        finally
        {
            _loading = false;
        }
    }

    private void ChkDeveloperMode_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        UpdateVirtualSerialPanelVisibility();
        if (ChkDeveloperMode.IsChecked != true)
            ChkVirtualSerialPort.IsChecked = false;
    }

    private void UpdateVirtualSerialPanelVisibility()
        => PanelVirtualSerial.Visibility = ChkDeveloperMode.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;

    private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var setting = _appSettingService.Load();
        setting.DeveloperMode = ChkDeveloperMode.IsChecked == true;
        setting.VirtualSerialPort = ChkVirtualSerialPort.IsChecked == true;
        _appSettingService.Save(setting);

        TxtSaveStatus.Text = setting.DeveloperMode && setting.VirtualSerialPort
            ? "Đã lưu. Serial ảo áp dụng ngay ở lần kết nối tiếp theo (không cần COM thật)."
            : setting.DeveloperMode
                ? "Đã lưu. DeveloperMode đã bật — làm mới Dashboard để thấy nút tạo mẫu."
                : "Đã lưu.";
    }
}

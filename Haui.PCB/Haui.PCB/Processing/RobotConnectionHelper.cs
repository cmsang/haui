using System.Windows;

namespace Haui.PCB.Processing;

/// <summary>
/// Kiểm tra cánh tay robot đã sẵn sàng (Rx → Yx → H0x → Dx) trước khi thao tác.
/// </summary>
public static class RobotConnectionHelper
{
    public const string NotConnectedTitle = "Chưa kết nối robot";

    public const string NotConnectedWarning =
        "Cánh tay robot chưa sẵn sàng.\n\n" +
        "Kiểm tra:\n" +
        "• Robot đã bật nguồn\n" +
        "• Cổng COM đúng trong file cấu hình\n" +
        "• Dây kết nối ổn định\n\n" +
        "Đợi màn hình chính báo \"Robot sẵn sàng\" rồi thử lại.";

    public const string HandshakeInProgressTitle = "Robot chưa sẵn sàng";

    public const string HandshakeInProgressWarning =
        "Robot đang kết nối hoặc thực hiện homing.\n\n" +
        "Vui lòng đợi homing hoàn tất rồi thực hiện thao tác.";

    /// <summary>Serial online và homing khởi động đã hoàn tất.</summary>
    public static bool IsRobotArmReady(
        IRobotSerialService serialService,
        RobotStartupHandshakeService handshake)
        => serialService.IsConnected && handshake.IsCompleted;

    public static bool EnsureRobotArmReady(
        IRobotSerialService serialService,
        RobotStartupHandshakeService handshake)
    {
        if (IsRobotArmReady(serialService, handshake))
            return true;

        if (handshake.IsRunning)
        {
            MessageBox.Show(
                HandshakeInProgressWarning,
                HandshakeInProgressTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return false;
        }

        MessageBox.Show(
            NotConnectedWarning,
            NotConnectedTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        return false;
    }
}

using System.ComponentModel;
using System.Windows;

namespace Haui.PCB.Processing;

/// <summary>
/// Hỗ trợ đóng màn hình robot: chặn khi đang chạy lệnh.
/// </summary>
public static class RobotWindowCloseHelper
{
    public const string BusyCloseTitle = "Không thể đóng";

    public const string BusyCloseWarning =
        "Robot đang thực hiện lệnh.\n\n" +
        "Vui lòng đợi robot dừng hẳn rồi mới đóng màn hình.";

    public static bool TryBlockCloseIfBusy(bool isOperationInProgress, CancelEventArgs e)
    {
        if (!isOperationInProgress) return false;

        e.Cancel = true;
        ShowBusyCloseWarning();
        return true;
    }

    public static void ShowBusyCloseWarning()
    {
        MessageBox.Show(
            BusyCloseWarning,
            BusyCloseTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    /// <summary>Nút Đóng — trả về true nếu có thể gọi Window.Close().</summary>
    public static bool CanInitiateClose(bool canCloseWindow, bool isOperationInProgress)
    {
        if (canCloseWindow) return true;
        if (isOperationInProgress) ShowBusyCloseWarning();
        return false;
    }

    /// <summary>H0x → Dx rồi đóng cửa sổ (gọi từ OnClosing sau khi e.Cancel = true).</summary>
    public static async Task CloseAfterHomingAsync(
        Window window,
        Func<Task> returnToHomeAsync,
        Action markAllowClose)
    {
        await returnToHomeAsync();
        markAllowClose();
        window.Close();
    }
}

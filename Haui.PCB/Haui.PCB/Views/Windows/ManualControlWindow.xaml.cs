using System.Windows;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Cửa sổ popup Manual Control — bọc <see cref="Tabs.ManualControlTabView"/>.
/// </summary>
public partial class ManualControlWindow : Window
{
    public ManualControlWindow(IRobotSerialService? sharedSerialService = null)
    {
        InitializeComponent();
        ManualPanel.ShowCloseButton = true;
        ManualPanel.Initialize(sharedSerialService);
        ManualPanel.CloseRequested += (_, _) => Close();
        Closed += (_, _) => ManualPanel.DisposePanel();
    }
}

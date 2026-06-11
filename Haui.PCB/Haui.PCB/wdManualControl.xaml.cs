using System.Windows;
using Haui.PCB.Processing;

namespace Haui.PCB;

/// <summary>
/// Cửa sổ popup Manual Control — bọc <see cref="Views.MainTabs.ManualControlTabView"/>.
/// </summary>
public partial class wdManualControl : Window
{
    public wdManualControl(IRobotSerialService? sharedSerialService = null)
    {
        InitializeComponent();
        ManualPanel.ShowCloseButton = true;
        ManualPanel.Initialize(sharedSerialService);
        ManualPanel.CloseRequested += (_, _) => Close();
        Closed += (_, _) => ManualPanel.DisposePanel();
    }
}

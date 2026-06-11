using System.Windows;
using Haui.PCB.Processing;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Cửa sổ popup Robot Teaching — bọc <see cref="Tabs.RobotTeachingTabView"/>.
/// </summary>
public partial class RobotTeachingWindow : Window
{
    public RobotTeachingWindow(IRobotSerialService? sharedSerialService = null)
    {
        InitializeComponent();
        TeachingPanel.ShowCloseButton = true;
        TeachingPanel.Initialize(sharedSerialService);
        TeachingPanel.CloseRequested += (_, _) => Close();
        Closed += (_, _) => TeachingPanel.DisposePanel();
    }
}

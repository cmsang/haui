using System.Windows;
using Haui.PCB.Processing;

namespace Haui.PCB;

/// <summary>
/// Cửa sổ popup Robot Teaching — bọc <see cref="Views.MainTabs.RobotTeachingTabView"/>.
/// </summary>
public partial class wdTeaching : Window
{
    public wdTeaching(IRobotSerialService? sharedSerialService = null)
    {
        InitializeComponent();
        TeachingPanel.ShowCloseButton = true;
        TeachingPanel.Initialize(sharedSerialService);
        TeachingPanel.CloseRequested += (_, _) => Close();
        Closed += (_, _) => TeachingPanel.DisposePanel();
    }
}

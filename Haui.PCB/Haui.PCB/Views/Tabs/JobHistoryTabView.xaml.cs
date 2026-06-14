using System.Windows.Controls;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Tabs;

public partial class JobHistoryTabView : UserControl
{
    public MonitorViewModel? LineMonitor { get; }

    public JobHistoryTabView(MonitorViewModel? lineMonitor = null)
    {
        LineMonitor = lineMonitor;
        InitializeComponent();
    }
}

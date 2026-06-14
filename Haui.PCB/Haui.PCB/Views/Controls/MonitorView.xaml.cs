using System.Windows.Controls;

namespace Haui.PCB.Views.Controls;

/// <summary>
/// Shell-level line status panel (Load/Unload, AGV gauges). DataContext → <see cref="ViewModels.MonitorViewModel"/>.
/// </summary>
public partial class MonitorView : UserControl
{
    public MonitorView() => InitializeComponent();
}

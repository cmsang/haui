using System.Windows;
using System.Windows.Controls;

namespace Haui.PCB.Views.Controls;

/// <summary>
/// Standard inline delete action: Material Design trash icon with app-wide hover/disabled styling.
/// </summary>
public partial class DeleteIconButton : UserControl
{
    public static readonly RoutedEvent ClickEvent =
        Button.ClickEvent.AddOwner(typeof(DeleteIconButton));

    public DeleteIconButton()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }
}

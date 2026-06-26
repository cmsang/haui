using System.Windows;
using System.Windows.Controls;
using Haui.PCB.ViewModels;

namespace Haui.PCB.Views.Tabs;

public partial class SettingTabView : UserControl
{
    private readonly SettingViewModel _viewModel = new();

    public SettingTabView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) => _viewModel.Load();

    private void BtnAddClassName_Click(object sender, RoutedEventArgs e) => _viewModel.AddClassName();

    private void BtnDeleteClassName_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ClassNameItem item }) return;
        _viewModel.RemoveClassName(item);
    }

    private void BtnSaveSettings_Click(object sender, RoutedEventArgs e) => _viewModel.Save();

    private void BtnResetSettings_Click(object sender, RoutedEventArgs e) => _viewModel.Reset();
}

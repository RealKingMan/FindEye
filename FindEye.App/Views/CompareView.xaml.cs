using System.Windows;
using FindEye.Core.Models;
using WpfUserControl = System.Windows.Controls.UserControl;

namespace FindEye.App.Views;

public partial class CompareView : WpfUserControl
{
    public CompareView()
    {
        InitializeComponent();
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is ViewModels.CompareViewModel vm)
            vm.SelectedItem = e.NewValue as CompareTreeNode;
    }
}

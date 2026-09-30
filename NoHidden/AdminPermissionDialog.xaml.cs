using NoHidden.Managers;
using System.Windows;

namespace NoHidden;

public partial class AdminPermissionDialog : Window
{
    public AdminPermissionDialog()
    {
        InitializeComponent();
        ContentRoot.FlowDirection = LocalizationManager.GetFlowDirection();
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

using NoHidden.Managers;
using System.Diagnostics;
using System.Windows;

namespace NoHidden;

public partial class AntivirusDetailsDialog : Window
{
    private const string LearnMoreUrl =
        "https://www.mehrdad32.ir/7042/why-antivirus-is-important-now/";

    public AntivirusDetailsDialog(AntivirusInfo? antivirusInfo)
    {
        InitializeComponent();

        ContentRoot.FlowDirection = LocalizationManager.GetFlowDirection();

        ProductNameText.Text =
            antivirusInfo?.DisplayName ??
            Resource("NoAntivirusProduct");

        StateCodeText.Text =
            antivirusInfo?.ProductStateHex ??
            Resource("NotAvailable");
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LearnMore_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo(LearnMoreUrl)
            {
                UseShellExecute = true
            });
    }

    private static string Resource(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }
}

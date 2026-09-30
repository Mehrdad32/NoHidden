using NoHidden.Managers;
using System.Threading;
using System.Windows;

namespace NoHidden;

public partial class App : Application
{
    private const string MutexName = "NoHiddenSingleInstanceMutex";

    private Mutex? _appMutex;
    private bool _ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        int? parentProcessId =
            ElevationManager.GetParentProcessId(e.Args);

        ElevationManager.WaitForParentProcessExit(parentProcessId);

        base.OnStartup(e);

        LocalizationManager.LoadLanguage();

        _appMutex = new Mutex(
            initiallyOwned: true,
            name: MutexName,
            createdNew: out bool isNewInstance);

        _ownsMutex = isNewInstance;

        if (!isNewInstance)
        {
            MessageBox.Show(
                Resource("ApplicationAlreadyRunning"),
                Resource("AppTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Shutdown();
            return;
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        bool applyAutorunProtection =
            e.Args.Any(
                argument => string.Equals(
                    argument,
                    ElevationManager.ApplyAutorunProtectionArgument,
                    StringComparison.OrdinalIgnoreCase));

        if (applyAutorunProtection &&
            mainWindow.DataContext is MainViewModel viewModel)
        {
            viewModel.ApplyAutorunProtectionFromElevatedStartup();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex)
        {
            try
            {
                _appMutex?.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The mutex ownership was already released.
            }
        }

        _appMutex?.Dispose();

        base.OnExit(e);
    }

    private static string Resource(string key)
    {
        return Current.TryFindResource(key) as string ?? key;
    }
}

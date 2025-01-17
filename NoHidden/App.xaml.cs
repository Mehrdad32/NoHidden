using System;
using System.Threading;
using System.Windows;
using NoHidden.Managers;

namespace NoHidden
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? appMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "NoHiddenSingleInstanceMutex";

            // Try to create a new mutex
            appMutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // If the mutex already exists, another instance is running
                MessageBox.Show("The application is already running.", "NoHidden", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0); // Exit the current instance
            }

            base.OnStartup(e);

            // Load the previously saved language
            LocalizationManager.LoadLanguage();

            // Create and show the MainWindow
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Release the mutex when the application exits
            appMutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
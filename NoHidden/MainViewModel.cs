using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NoHidden.Managers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Windows;

namespace NoHidden
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AutoPlayManager _autoPlayManager = new();
        private readonly DriveManager _driveManager = new();

        [ObservableProperty]
        private string? appTitle;

        [ObservableProperty]
        private string? appVersion;

        [ObservableProperty]
        private string? systemInfo;

        [ObservableProperty]
        private string? autorunStatus;

        [ObservableProperty]
        private string? antivirusStatus;

        [ObservableProperty]
        private string? messagesTitle;

        [ObservableProperty]
        private string? messages;

        [ObservableProperty]
        private string? disableAutoRunText;

        [ObservableProperty]
        private string? learnMoreAboutAntiVirus;

        [ObservableProperty]
        private string? operationGroupHeader;

        [ObservableProperty]
        private ObservableCollection<string>? removableDrives;

        [ObservableProperty]
        private string? selectedDrive;

        [ObservableProperty]
        private ObservableCollection<Problem> detectedProblems = new();

        [ObservableProperty]
        private Visibility disableAutoRunButtonVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility learnMoreButtonVisibility = Visibility.Visible;

        public MainViewModel()
        {
            LoadLocalizedResources();
            CheckAutorunStatus();
            CheckAntivirusStatus();
            LoadDrives();

            // test
            DetectedProblems.Add(new Problem { Description = "Detect virus file." });
            DetectedProblems.Add(new Problem { Description = "Detect hidden folder without name." });
            DetectedProblems.Add(new Problem { Description = "Detect autorun.inf file in the root of the USB disk." });
        }

        private void LoadLocalizedResources()
        {
            AppTitle = (string)Application.Current.FindResource("AppTitle");
            AppVersion = (string)Application.Current.FindResource("AppVersion");
            SystemInfo = (string)Application.Current.FindResource("SystemInfo");
            MessagesTitle = (string)Application.Current.FindResource("Messages");
            DisableAutoRunText = (string)Application.Current.FindResource("ClickToDisableAutoRun");
            LearnMoreAboutAntiVirus = (string)Application.Current.FindResource("ClickToLearnMoreAboutAntiVirus");
            OperationGroupHeader = (string)Application.Current.FindResource("OperationsGroupHeader");
        }

        private void LoadDrives()
        {
            RemovableDrives = _driveManager.GetRemovableDrives();
        }

        private void CheckAutorunStatus()
        {
            try
            {
                var (isAutorunDisabled, isAutoPlayDisabled) = _autoPlayManager.CheckStatus();

                // Update AutorunStatus
                AutorunStatus = isAutorunDisabled
                    ? (string)Application.Current.FindResource("AutorunDisabled")
                    : (string)Application.Current.FindResource("AutorunEnabled");

                // Add AutoPlay Status
                AutorunStatus += isAutoPlayDisabled
                    ? " + " + (string)Application.Current.FindResource("AutoPlayDisabledGlobally")
                    : " + " + (string)Application.Current.FindResource("AutoPlayEnabled");

                // Update Button Visibility
                DisableAutoRunButtonVisibility = isAutorunDisabled ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                AutorunStatus = string.Format((string)Application.Current.FindResource("Error"), ex.Message);
            }
        }

        private void CheckAntivirusStatus()
        {
            try
            {
                var antivirusInfo = AntivirusDetector.GetAntivirusInfo();

                if (antivirusInfo != null)
                {
                    // Interpret the productState bitmask as needed
                    // Note: The interpretation of productState may vary between antivirus products

                    AntivirusStatus = string.Format((string)Application.Current.FindResource("AntivirusStatus"), antivirusInfo.DisplayName, $"{antivirusInfo.ProductState:X}");

                    LearnMoreButtonVisibility = Visibility.Collapsed;
                }
                else
                {
                    AntivirusStatus = (string)Application.Current.FindResource("NotInstalled");

                    LearnMoreButtonVisibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                AntivirusStatus = string.Format((string)Application.Current.FindResource("Error"), ex.Message);
            }
        }

        [RelayCommand]
        private void DisableAutorun()
        {
            try
            {
                _autoPlayManager.DisableAutorunAndAutoPlay();
                AutorunStatus = (string)Application.Current.FindResource("AutoPlayDisabledGlobally");

                // Hide the button when Autorun is disabled
                DisableAutoRunButtonVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Messages = string.Format((string)Application.Current.FindResource("Error"), ex.Message);
            }
        }

        [RelayCommand]
        private void OpenAntivirusInfo()
        {
            try
            {
                string url = "https://www.mehrdad32.ir/7042/why-antivirus-is-important-now/";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Messages = string.Format((string)Application.Current.FindResource("Error"), ex.Message);
            }
        }

        [RelayCommand]
        private void RefreshDrives()
        {
            LoadDrives();
            if (RemovableDrives!.Count == 0)
                Messages = "No removable devices has been detected!";
        }

        [RelayCommand]
        private void FixProblem(Problem problem)
        {
            // Handle fixing the problem
            // Example: Remove the problem from the list
            DetectedProblems.Remove(problem);

            // Add your logic here to resolve the specific problem
            // Example: Delete a file or perform another action
        }
    }

    public class Problem
    {
        public string? Description { get; set; }
        public string? AdditionalData { get; set; }
    }
}

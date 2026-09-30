using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoHidden.Managers;
using NoHidden.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace NoHidden;

public partial class MainViewModel : ObservableObject
{
    private readonly AutoPlayManager _autoPlayManager = new();
    private readonly DriveManager _driveManager = new();

    [ObservableProperty]
    private string appTitle = string.Empty;

    [ObservableProperty]
    private string appVersion = string.Empty;

    [ObservableProperty]
    private string systemInfo = string.Empty;

    [ObservableProperty]
    private string autorunStatus = string.Empty;

    [ObservableProperty]
    private string antivirusStatus = string.Empty;

    [ObservableProperty]
    private string messagesTitle = string.Empty;

    [ObservableProperty]
    private string messages = string.Empty;

    [ObservableProperty]
    private string disableAutoRunText = string.Empty;

    [ObservableProperty]
    private string learnMoreAboutAntiVirus = string.Empty;

    [ObservableProperty]
    private string operationGroupHeader = string.Empty;

    [ObservableProperty]
    private string selectUsbDriveText = string.Empty;

    [ObservableProperty]
    private string problemsHeader = string.Empty;

    [ObservableProperty]
    private string scanNotStartedText = string.Empty;

    [ObservableProperty]
    private string languageHeader = string.Empty;

    [ObservableProperty]
    private string refreshDrivesToolTip = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UsbDriveInfo> removableDrives = [];

    [ObservableProperty]
    private UsbDriveInfo? selectedDrive;

    [ObservableProperty]
    private Visibility disableAutoRunButtonVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility learnMoreButtonVisibility = Visibility.Visible;

    public MainViewModel()
    {
        ReloadLocalization();
        LoadDrives();
    }

    public void ReloadLocalization()
    {
        LoadLocalizedResources();
        CheckAutorunStatus();
        CheckAntivirusStatus();
    }

    private void LoadLocalizedResources()
    {
        AppTitle = Resource("AppTitle");
        AppVersion = Resource("AppVersion");
        SystemInfo = Resource("SystemInfo");
        MessagesTitle = Resource("Messages");
        DisableAutoRunText = Resource("ClickToDisableAutoRun");
        LearnMoreAboutAntiVirus = Resource("ClickToLearnMoreAboutAntiVirus");
        OperationGroupHeader = Resource("OperationsGroupHeader");
        SelectUsbDriveText = Resource("SelectUsbDrive");
        ProblemsHeader = Resource("Problems");
        ScanNotStartedText = Resource("ScanNotStarted");
        LanguageHeader = Resource("Language");
        RefreshDrivesToolTip = Resource("RefreshDrives");
    }

    private void LoadDrives()
    {
        string? previouslySelectedRoot = SelectedDrive?.RootPath;
        ObservableCollection<UsbDriveInfo> drives = _driveManager.GetRemovableDrives();

        RemovableDrives = drives;

        SelectedDrive = previouslySelectedRoot is null
            ? drives.FirstOrDefault()
            : drives.FirstOrDefault(
                drive => string.Equals(
                    drive.RootPath,
                    previouslySelectedRoot,
                    StringComparison.OrdinalIgnoreCase))
              ?? drives.FirstOrDefault();

        Messages = drives.Count == 0
            ? Resource("NoRemovableDevices")
            : string.Empty;
    }

    private void CheckAutorunStatus()
    {
        try
        {
            var (isAutorunDisabled, isAutoPlayDisabled) = _autoPlayManager.CheckStatus();

            string autorun = isAutorunDisabled
                ? Resource("AutorunDisabled")
                : Resource("AutorunEnabled");

            string autoplay = isAutoPlayDisabled
                ? Resource("AutoPlayDisabledGlobally")
                : Resource("AutoPlayEnabled");

            AutorunStatus = $"{autorun} + {autoplay}";
            DisableAutoRunButtonVisibility =
                isAutorunDisabled && isAutoPlayDisabled
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
        catch (Exception ex)
        {
            AutorunStatus = FormatError(ex);
        }
    }

    private void CheckAntivirusStatus()
    {
        try
        {
            AntivirusInfo? antivirusInfo = AntivirusDetector.GetAntivirusInfo();

            if (antivirusInfo is null)
            {
                AntivirusStatus = Resource("NotInstalled");
                LearnMoreButtonVisibility = Visibility.Visible;
                return;
            }

            AntivirusStatus = string.Format(
                Resource("AntivirusStatus"),
                antivirusInfo.DisplayName ?? Resource("UnknownAntivirus"),
                antivirusInfo.ProductState.ToString("X6"));

            LearnMoreButtonVisibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            AntivirusStatus = FormatError(ex);
            LearnMoreButtonVisibility = Visibility.Visible;
        }
    }

    [RelayCommand]
    private void DisableAutorun()
    {
        try
        {
            _autoPlayManager.DisableAutorunAndAutoPlay();
            CheckAutorunStatus();
            Messages = Resource("AutorunProtectionEnabled");
        }
        catch (Exception ex)
        {
            Messages = FormatError(ex);
        }
    }

    [RelayCommand]
    private void OpenAntivirusInfo()
    {
        try
        {
            const string url =
                "https://www.mehrdad32.ir/7042/why-antivirus-is-important-now/";

            Process.Start(
                new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
        }
        catch (Exception ex)
        {
            Messages = FormatError(ex);
        }
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        LoadDrives();
    }

    private static string Resource(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }

    private static string FormatError(Exception exception)
    {
        return string.Format(Resource("Error"), exception.Message);
    }
}

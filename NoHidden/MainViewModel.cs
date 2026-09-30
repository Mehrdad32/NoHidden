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
    private string autorunStatus = string.Empty;

    [ObservableProperty]
    private string protectionBadgeText = string.Empty;

    [ObservableProperty]
    private string protectionStatusColor = "#F59E0B";

    [ObservableProperty]
    private string antivirusStatus = string.Empty;

    [ObservableProperty]
    private string antivirusBadgeText = string.Empty;

    [ObservableProperty]
    private string antivirusStatusColor = "#64748B";

    [ObservableProperty]
    private string usbStatus = string.Empty;

    [ObservableProperty]
    private string usbDetails = string.Empty;

    [ObservableProperty]
    private string usbStatusColor = "#64748B";

    [ObservableProperty]
    private string emptyStateTitle = string.Empty;

    [ObservableProperty]
    private string emptyStateDescription = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UsbDriveInfo> removableDrives = [];

    [ObservableProperty]
    private UsbDriveInfo? selectedDrive;

    [ObservableProperty]
    private bool hasSelectedDrive;

    [ObservableProperty]
    private Visibility disableAutoRunButtonVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility learnMoreButtonVisibility = Visibility.Visible;

    public MainViewModel()
    {
        ReloadLocalization();
        LoadDrives();
        StatusMessage = Resource("ReadyStatus");
    }

    public void ReloadLocalization()
    {
        CheckAutorunStatus();
        CheckAntivirusStatus();
        UpdateUsbState();
    }

    partial void OnSelectedDriveChanged(UsbDriveInfo? value)
    {
        UpdateUsbState();
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

        UpdateUsbState();
    }

    private void UpdateUsbState()
    {
        HasSelectedDrive = SelectedDrive is not null;

        if (SelectedDrive is null)
        {
            UsbStatus = Resource("UsbNotConnected");
            UsbDetails = Resource("UsbConnectHint");
            UsbStatusColor = "#64748B";
            EmptyStateTitle = Resource("EmptyStateNoUsbTitle");
            EmptyStateDescription = Resource("EmptyStateNoUsbDescription");
            return;
        }

        UsbStatus = SelectedDrive.DisplayName;
        UsbDetails = $"{SelectedDrive.DriveFormat}  •  {SelectedDrive.CapacityText}";
        UsbStatusColor = "#3B82F6";
        EmptyStateTitle = Resource("EmptyStateReadyTitle");
        EmptyStateDescription = Resource("EmptyStateReadyDescription");
    }

    private void CheckAutorunStatus()
    {
        try
        {
            var (isAutorunDisabled, isAutoPlayDisabled) = _autoPlayManager.CheckStatus();

            if (isAutorunDisabled && isAutoPlayDisabled)
            {
                ProtectionBadgeText = Resource("Protected");
                ProtectionStatusColor = "#22C55E";
                AutorunStatus = Resource("AutorunProtectionActive");
                DisableAutoRunButtonVisibility = Visibility.Collapsed;
                return;
            }

            ProtectionBadgeText = Resource("Attention");
            ProtectionStatusColor = "#F59E0B";
            AutorunStatus = Resource("AutorunProtectionRecommended");
            DisableAutoRunButtonVisibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            ProtectionBadgeText = Resource("Unknown");
            ProtectionStatusColor = "#64748B";
            AutorunStatus = Resource("ProtectionStatusUnavailable");
            StatusMessage = FormatError(ex);
        }
    }

    private void CheckAntivirusStatus()
    {
        try
        {
            AntivirusInfo? antivirusInfo = AntivirusDetector.GetAntivirusInfo();

            if (antivirusInfo is null)
            {
                AntivirusBadgeText = Resource("NotDetected");
                AntivirusStatusColor = "#F59E0B";
                AntivirusStatus = Resource("NotInstalled");
                LearnMoreButtonVisibility = Visibility.Visible;
                return;
            }

            AntivirusBadgeText = Resource("Detected");
            AntivirusStatusColor = "#22C55E";
            AntivirusStatus = string.Format(
                Resource("AntivirusDetected"),
                antivirusInfo.DisplayName ?? Resource("UnknownAntivirus"));

            LearnMoreButtonVisibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            AntivirusBadgeText = Resource("Unknown");
            AntivirusStatusColor = "#64748B";
            AntivirusStatus = Resource("AntivirusStatusUnavailable");
            LearnMoreButtonVisibility = Visibility.Visible;
            StatusMessage = FormatError(ex);
        }
    }

    [RelayCommand]
    private void DisableAutorun()
    {
        try
        {
            _autoPlayManager.DisableAutorunAndAutoPlay();
            CheckAutorunStatus();
            StatusMessage = Resource("AutorunProtectionEnabled");
        }
        catch (Exception ex)
        {
            StatusMessage = FormatError(ex);
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
            StatusMessage = FormatError(ex);
        }
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        LoadDrives();

        StatusMessage = HasSelectedDrive
            ? Resource("UsbRefreshed")
            : Resource("NoRemovableDevices");
    }

    [RelayCommand]
    private void StartScan()
    {
        StatusMessage = Resource("ScannerComingNext");
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

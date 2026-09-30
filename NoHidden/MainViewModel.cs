using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoHidden.Managers;
using NoHidden.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace NoHidden;

public partial class MainViewModel : ObservableObject
{
    private readonly AutoPlayManager _autoPlayManager = new();
    private readonly DriveManager _driveManager = new();
    private AntivirusInfo? _currentAntivirusInfo;

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
    private FlowDirection contentFlowDirection = FlowDirection.LeftToRight;

    private string _statusMessageResourceKey = "ReadyStatus";
    private string? _statusMessageDetail;

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
    }

    public void ReloadLocalization()
    {
        ContentFlowDirection = LocalizationManager.GetFlowDirection();
        CheckAutorunStatus();
        CheckAntivirusStatus();
        UpdateUsbState();
        RefreshStatusMessage();
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
            SetErrorStatus(ex);
        }
    }

    private void CheckAntivirusStatus()
    {
        try
        {
            AntivirusInfo? antivirusInfo = AntivirusDetector.GetAntivirusInfo();
            _currentAntivirusInfo = antivirusInfo;

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
            SetErrorStatus(ex);
        }
    }

    [RelayCommand]
    private void DisableAutorun()
    {
        if (ElevationManager.IsAdministrator())
        {
            ApplyAutorunProtection();
            return;
        }

        var dialog = new AdminPermissionDialog
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() != true)
        {
            SetStatusMessage("AdminPermissionCanceled");
            return;
        }

        try
        {
            ElevationManager.RestartAsAdministratorForAutorunProtection();
            Application.Current.Shutdown();
        }
        catch (Exception ex) when (ElevationManager.IsElevationCanceled(ex))
        {
            SetStatusMessage("AdminPermissionCanceled");
        }
        catch (Exception ex)
        {
            SetErrorStatus(ex);
        }
    }

    public void ApplyAutorunProtectionFromElevatedStartup()
    {
        if (!ElevationManager.IsAdministrator())
        {
            SetStatusMessage("AdminElevationFailed");
            return;
        }

        ApplyAutorunProtection();
    }

    private void ApplyAutorunProtection()
    {
        try
        {
            _autoPlayManager.DisableAutorunAndAutoPlay();
            CheckAutorunStatus();
            SetStatusMessage("AutorunProtectionEnabled");
        }
        catch (UnauthorizedAccessException)
        {
            SetStatusMessage("AdminElevationRequired");
        }
        catch (Exception ex)
        {
            SetErrorStatus(ex);
        }
    }

    [RelayCommand]
    private void OpenAntivirusInfo()
    {
        var dialog = new AntivirusDetailsDialog(_currentAntivirusInfo)
        {
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        LoadDrives();

        SetStatusMessage(
            HasSelectedDrive
                ? "UsbRefreshed"
                : "NoRemovableDevices");
    }

    [RelayCommand]
    private void StartScan()
    {
        SetStatusMessage("ScannerComingNext");
    }

    private void SetStatusMessage(string resourceKey)
    {
        _statusMessageResourceKey = resourceKey;
        _statusMessageDetail = null;
        RefreshStatusMessage();
    }

    private void SetErrorStatus(Exception exception)
    {
        _statusMessageResourceKey = "Error";
        _statusMessageDetail = exception.Message;
        RefreshStatusMessage();
    }

    private void RefreshStatusMessage()
    {
        StatusMessage = _statusMessageResourceKey == "Error" &&
                        !string.IsNullOrWhiteSpace(_statusMessageDetail)
            ? string.Format(Resource("Error"), _statusMessageDetail)
            : Resource(_statusMessageResourceKey);
    }

    private static string Resource(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }
}

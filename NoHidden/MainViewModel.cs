using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoHidden.Managers;
using NoHidden.Models;
using NoHidden.Services;
using NoHidden.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;

namespace NoHidden;

public partial class MainViewModel : ObservableObject
{
    private readonly AutoPlayManager _autoPlayManager = new();
    private readonly DriveManager _driveManager = new();
    private readonly UsbScanner _usbScanner = new();
    private readonly FileVisibilityRepairService _visibilityRepairService = new();
    private readonly FileNeutralizationService _neutralizationService = new();
    private readonly DefenderScanService _defenderScanService = new();
    private readonly FileHashService _fileHashService = new();

    private AntivirusInfo? _currentAntivirusInfo;
    private CancellationTokenSource? _scanCancellationTokenSource;
    private ScanReport? _lastScanReport;

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

    [ObservableProperty]
    private ObservableCollection<UsbDriveInfo> removableDrives = [];

    [ObservableProperty]
    private UsbDriveInfo? selectedDrive;

    [ObservableProperty]
    private bool hasSelectedDrive;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private bool canStartScan;

    [ObservableProperty]
    private bool isDriveInteractionEnabled = true;

    [ObservableProperty]
    private string scanProgressText = string.Empty;

    [ObservableProperty]
    private string scanSummaryText = string.Empty;

    [ObservableProperty]
    private int scannedItemsCount;

    [ObservableProperty]
    private int recoverableCount;

    [ObservableProperty]
    private int highRiskCount;

    [ObservableProperty]
    private int suspiciousCount;

    [ObservableProperty]
    private Visibility scanEmptyStateVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility scanProgressVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility scanResultsVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility restoreAllVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility cancelScanVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private ObservableCollection<ScanFindingViewModel> scanFindings = [];

    [ObservableProperty]
    private Visibility disableAutoRunButtonVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility learnMoreButtonVisibility = Visibility.Visible;

    private string _statusMessageResourceKey = "ReadyStatus";
    private string? _statusMessageDetail;
    private string _emptyStateTitleResourceKey = "EmptyStateNoUsbTitle";
    private string _emptyStateDescriptionResourceKey = "EmptyStateNoUsbDescription";

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
        UpdateUsbState(updateEmptyState: false);
        RefreshEmptyStateLocalization();

        foreach (ScanFindingViewModel finding in ScanFindings)
        {
            finding.RefreshLocalization();
        }

        RefreshScanSummary();
        RefreshStatusMessage();
    }

    partial void OnSelectedDriveChanged(UsbDriveInfo? value)
    {
        UpdateUsbState();

        if (!IsScanning)
        {
            ResetScanPresentation();
        }
    }

    partial void OnIsScanningChanged(bool value)
    {
        CanStartScan = HasSelectedDrive && !value;
        IsDriveInteractionEnabled = !value;
        CancelScanVisibility = value
            ? Visibility.Visible
            : Visibility.Collapsed;
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

    private void UpdateUsbState(bool updateEmptyState = true)
    {
        HasSelectedDrive = SelectedDrive is not null;
        CanStartScan = HasSelectedDrive && !IsScanning;

        if (SelectedDrive is null)
        {
            UsbStatus = Resource("UsbNotConnected");
            UsbDetails = Resource("UsbConnectHint");
            UsbStatusColor = "#64748B";

            if (updateEmptyState)
            {
                SetEmptyState(
                    "EmptyStateNoUsbTitle",
                    "EmptyStateNoUsbDescription");
            }

            return;
        }

        UsbStatus = SelectedDrive.DisplayName;
        UsbDetails = $"{SelectedDrive.DriveFormat}  •  {SelectedDrive.CapacityText}";
        UsbStatusColor = "#3B82F6";

        if (updateEmptyState)
        {
            SetEmptyState(
                "EmptyStateReadyTitle",
                "EmptyStateReadyDescription");
        }
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

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task StartScanAsync()
    {
        UsbDriveInfo? drive = SelectedDrive;

        if (drive is null || IsScanning)
        {
            return;
        }

        ResetScanPresentation();
        IsScanning = true;
        ScanEmptyStateVisibility = Visibility.Collapsed;
        ScanProgressVisibility = Visibility.Visible;
        SetStatusMessage("ScanningStatus");

        _scanCancellationTokenSource?.Dispose();
        _scanCancellationTokenSource = new CancellationTokenSource();

        var progress = new Progress<ScanProgress>(
            value =>
            {
                ScannedItemsCount = value.ScannedItems;
                ScanProgressText = string.Format(
                    Resource("ScanningProgress"),
                    value.ScannedItems,
                    value.CurrentPath);
            });

        try
        {
            ScanReport report = await _usbScanner.ScanAsync(
                drive.RootPath,
                progress,
                _scanCancellationTokenSource.Token);

            _lastScanReport = report;
            ScannedItemsCount = report.ScannedItems;
            RecoverableCount = report.RecoverableCount;
            HighRiskCount = report.HighRiskCount;
            SuspiciousCount = report.SuspiciousCount;

            ScanFindings = new ObservableCollection<ScanFindingViewModel>(
                report.Findings
                    .OrderByDescending(finding => finding.Severity)
                    .ThenBy(finding => finding.RelativePath)
                    .Select(
                        finding => new ScanFindingViewModel(
                            finding,
                            _defenderScanService.IsAvailable)));

            ScanProgressVisibility = Visibility.Collapsed;

            if (ScanFindings.Count == 0)
            {
                SetEmptyState(
                    "ScanCleanTitle",
                    "ScanCleanDescription");
                ScanEmptyStateVisibility = Visibility.Visible;
                ScanResultsVisibility = Visibility.Collapsed;
                SetStatusMessage("ScanCompletedClean");
            }
            else
            {
                ScanEmptyStateVisibility = Visibility.Collapsed;
                ScanResultsVisibility = Visibility.Visible;
                RestoreAllVisibility = RecoverableCount > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                SetStatusMessage("ScanCompletedWithFindings");
            }

            RefreshScanSummary();
        }
        catch (OperationCanceledException)
        {
            ScanProgressVisibility = Visibility.Collapsed;
            ScanResultsVisibility = Visibility.Collapsed;
            ScanEmptyStateVisibility = Visibility.Visible;
            SetEmptyState(
                "ScanCanceledTitle",
                "ScanCanceledDescription");
            SetStatusMessage("ScanCanceledStatus");
        }
        catch (Exception ex)
        {
            ScanProgressVisibility = Visibility.Collapsed;
            ScanResultsVisibility = Visibility.Collapsed;
            ScanEmptyStateVisibility = Visibility.Visible;
            SetEmptyState(
                "ScanFailedTitle",
                "ScanFailedDescription");
            SetErrorStatus(ex);
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        _scanCancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void RestoreVisibility(ScanFindingViewModel? item)
    {
        if (item is null ||
            !item.CanRestoreVisibility ||
            SelectedDrive is null)
        {
            return;
        }

        try
        {
            _visibilityRepairService.RestoreVisibility(
                SelectedDrive.RootPath,
                item.Finding.Path);

            RemoveFinding(item);
            SetStatusMessage("VisibilityRestored");
        }
        catch (Exception ex)
        {
            SetErrorStatus(ex);
        }
    }

    [RelayCommand]
    private async Task ScanWithDefenderAsync(ScanFindingViewModel? item)
    {
        if (item is null ||
            !item.CanScanWithDefender)
        {
            return;
        }

        SetStatusMessage("DefenderScanStarting");

        try
        {
            DefenderFileScanResult result =
                await _defenderScanService.ScanFileAsync(
                    item.Finding.Path);

            SetStatusMessage(
                result.Status switch
                {
                    DefenderFileScanStatus.CleanOrNoActionRequired =>
                        "DefenderScanNoAction",

                    DefenderFileScanStatus.DetectionOrScanProblem =>
                        "DefenderScanAttention",

                    _ =>
                        "DefenderScanFailed"
                });
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

    [RelayCommand]
    private void NeutralizeFinding(ScanFindingViewModel? item)
    {
        if (item is null ||
            !item.CanNeutralize ||
            SelectedDrive is null)
        {
            return;
        }

        MessageBoxResult confirmation =
            MessageBox.Show(
                string.Format(
                    Resource("NeutralizeConfirmation"),
                    item.Path),
                Resource("NeutralizeConfirmationTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            NeutralizationResult result =
                _neutralizationService.Neutralize(
                    SelectedDrive.RootPath,
                    item.Finding.Path);

            RemoveFinding(item);

            SetFormattedStatus(
                "FileNeutralized",
                Path.GetFileName(result.NeutralizedPath));
        }
        catch (Exception ex)
        {
            SetErrorStatus(ex);
        }
    }

    [RelayCommand]
    private async Task CheckVirusTotalAsync(ScanFindingViewModel? item)
    {
        if (item is null ||
            !item.CanCheckOnline)
        {
            return;
        }

        SetStatusMessage("HashingFile");

        try
        {
            string sha256 =
                await _fileHashService.ComputeSha256Async(
                    item.Finding.Path);

            string url =
                $"https://www.virustotal.com/gui/file/{sha256}";

            Process.Start(
                new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });

            SetStatusMessage("VirusTotalOpened");
        }
        catch (Exception ex)
        {
            SetErrorStatus(ex);
        }
    }

    [RelayCommand]
    private void RestoreAllVisibleItems()
    {
        if (SelectedDrive is null)
        {
            return;
        }

        List<ScanFindingViewModel> recoverableItems =
            ScanFindings
                .Where(item => item.CanRestoreVisibility)
                .ToList();

        int restored = 0;
        int failed = 0;

        foreach (ScanFindingViewModel item in recoverableItems)
        {
            try
            {
                _visibilityRepairService.RestoreVisibility(
                    SelectedDrive.RootPath,
                    item.Finding.Path);

                ScanFindings.Remove(item);
                restored++;
            }
            catch
            {
                failed++;
            }
        }

        RecalculateFindingCounts();

        if (failed == 0)
        {
            SetStatusMessage(
                restored == 1
                    ? "VisibilityRestored"
                    : "VisibilityRestoredMultiple");
        }
        else
        {
            _statusMessageResourceKey = "VisibilityRestorePartial";
            _statusMessageDetail = $"{restored}|{failed}";
            RefreshStatusMessage();
        }
    }

    private void RemoveFinding(ScanFindingViewModel item)
    {
        ScanFindings.Remove(item);
        RecalculateFindingCounts();

        if (ScanFindings.Count == 0)
        {
            ScanResultsVisibility = Visibility.Collapsed;
            ScanEmptyStateVisibility = Visibility.Visible;
            SetEmptyState(
                "ScanCleanTitle",
                "ScanCleanDescription");
        }
    }

    private void RecalculateFindingCounts()
    {
        RecoverableCount =
            ScanFindings.Count(item => item.CanRestoreVisibility);

        HighRiskCount =
            ScanFindings.Count(
                item => item.Finding.Severity >= FindingSeverity.High);

        SuspiciousCount =
            ScanFindings.Count(
                item => item.Finding.Severity is FindingSeverity.Medium or FindingSeverity.Low);

        RestoreAllVisibility = RecoverableCount > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        RefreshScanSummary();
    }

    private void ResetScanPresentation()
    {
        _lastScanReport = null;
        ScanFindings.Clear();
        ScannedItemsCount = 0;
        RecoverableCount = 0;
        HighRiskCount = 0;
        SuspiciousCount = 0;
        ScanProgressText = string.Empty;
        ScanSummaryText = string.Empty;
        ScanProgressVisibility = Visibility.Collapsed;
        ScanResultsVisibility = Visibility.Collapsed;
        RestoreAllVisibility = Visibility.Collapsed;
        ScanEmptyStateVisibility = Visibility.Visible;
        UpdateUsbState();
    }

    private void RefreshScanSummary()
    {
        ScanSummaryText = string.Format(
            Resource("ScanSummary"),
            ScannedItemsCount,
            ScanFindings.Count,
            RecoverableCount,
            HighRiskCount);
    }

    private void SetEmptyState(
        string titleResourceKey,
        string descriptionResourceKey)
    {
        _emptyStateTitleResourceKey = titleResourceKey;
        _emptyStateDescriptionResourceKey = descriptionResourceKey;
        RefreshEmptyStateLocalization();
    }

    private void RefreshEmptyStateLocalization()
    {
        EmptyStateTitle = Resource(_emptyStateTitleResourceKey);
        EmptyStateDescription = Resource(_emptyStateDescriptionResourceKey);
    }

    private void SetStatusMessage(string resourceKey)
    {
        _statusMessageResourceKey = resourceKey;
        _statusMessageDetail = null;
        RefreshStatusMessage();
    }

    private void SetFormattedStatus(
        string resourceKey,
        string detail)
    {
        _statusMessageResourceKey = resourceKey;
        _statusMessageDetail = detail;
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
        if (_statusMessageResourceKey == "Error" &&
            !string.IsNullOrWhiteSpace(_statusMessageDetail))
        {
            StatusMessage = string.Format(
                Resource("Error"),
                _statusMessageDetail);

            return;
        }

        if (_statusMessageResourceKey == "VisibilityRestorePartial" &&
            !string.IsNullOrWhiteSpace(_statusMessageDetail))
        {
            string[] parts = _statusMessageDetail.Split('|');

            if (parts.Length == 2)
            {
                StatusMessage = string.Format(
                    Resource("VisibilityRestorePartial"),
                    parts[0],
                    parts[1]);

                return;
            }
        }

        if (_statusMessageResourceKey == "FileNeutralized" &&
            !string.IsNullOrWhiteSpace(_statusMessageDetail))
        {
            StatusMessage = string.Format(
                Resource("FileNeutralized"),
                _statusMessageDetail);

            return;
        }

        StatusMessage = Resource(_statusMessageResourceKey);
    }

    private static string Resource(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }
}

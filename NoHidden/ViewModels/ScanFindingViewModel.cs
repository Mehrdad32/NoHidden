using CommunityToolkit.Mvvm.ComponentModel;
using NoHidden.Models;
using System.IO;
using System.Windows;

namespace NoHidden.ViewModels;

public partial class ScanFindingViewModel : ObservableObject
{
    private readonly bool _defenderAvailable;

    public ScanFindingViewModel(
        ScanFinding finding,
        bool defenderAvailable)
    {
        Finding = finding;
        _defenderAvailable = defenderAvailable;
        RefreshLocalization();
    }

    public ScanFinding Finding { get; }

    public string Path => Finding.RelativePath;

    public bool CanRestoreVisibility => Finding.CanRestoreVisibility;

    public bool CanNeutralize =>
        !Finding.CanRestoreVisibility &&
        Finding.Type is not FindingType.HiddenItem &&
        File.Exists(Finding.Path);

    public bool CanScanWithDefender =>
        _defenderAvailable &&
        !Finding.CanRestoreVisibility &&
        File.Exists(Finding.Path);

    [ObservableProperty]
    private string typeText = string.Empty;

    [ObservableProperty]
    private string severityText = string.Empty;

    [ObservableProperty]
    private string severityColor = "#64748B";

    [ObservableProperty]
    private string reasonText = string.Empty;

    [ObservableProperty]
    private string restoreActionText = string.Empty;

    [ObservableProperty]
    private string defenderActionText = string.Empty;

    [ObservableProperty]
    private string neutralizeActionText = string.Empty;

    [ObservableProperty]
    private Visibility restoreActionVisibility;

    [ObservableProperty]
    private Visibility defenderActionVisibility;

    [ObservableProperty]
    private Visibility neutralizeActionVisibility;

    public void RefreshLocalization()
    {
        TypeText = Resource(GetTypeResourceKey(Finding.Type));
        SeverityText = Resource(GetSeverityResourceKey(Finding.Severity));
        SeverityColor = GetSeverityColor(Finding.Severity);
        ReasonText = Resource(Finding.ReasonKey);

        RestoreActionText = Resource("RestoreVisibility");
        DefenderActionText = Resource("ScanWithDefender");
        NeutralizeActionText = Resource("NeutralizeFile");

        RestoreActionVisibility =
            CanRestoreVisibility
                ? Visibility.Visible
                : Visibility.Collapsed;

        DefenderActionVisibility =
            CanScanWithDefender
                ? Visibility.Visible
                : Visibility.Collapsed;

        NeutralizeActionVisibility =
            CanNeutralize
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private static string GetTypeResourceKey(FindingType type)
    {
        return type switch
        {
            FindingType.HiddenItem => "FindingTypeHiddenItem",
            FindingType.Autorun => "FindingTypeAutorun",
            FindingType.SuspiciousScript => "FindingTypeSuspiciousScript",
            FindingType.SuspiciousExecutable => "FindingTypeSuspiciousExecutable",
            FindingType.DoubleExtension => "FindingTypeDoubleExtension",
            FindingType.FolderMasquerading => "FindingTypeFolderMasquerading",
            FindingType.SuspiciousShortcut => "FindingTypeSuspiciousShortcut",
            _ => "FindingTypeUnknown"
        };
    }

    private static string GetSeverityResourceKey(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Info => "SeverityInfo",
            FindingSeverity.Low => "SeverityLow",
            FindingSeverity.Medium => "SeverityMedium",
            FindingSeverity.High => "SeverityHigh",
            FindingSeverity.Critical => "SeverityCritical",
            _ => "SeverityInfo"
        };
    }

    private static string GetSeverityColor(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Info => "#60A5FA",
            FindingSeverity.Low => "#94A3B8",
            FindingSeverity.Medium => "#F59E0B",
            FindingSeverity.High => "#F97316",
            FindingSeverity.Critical => "#EF4444",
            _ => "#64748B"
        };
    }

    private static string Resource(string key)
    {
        return Application.Current.TryFindResource(key) as string ?? key;
    }
}

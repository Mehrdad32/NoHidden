namespace NoHidden.Models;

public enum FindingSeverity
{
    Info = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum FindingType
{
    HiddenItem,
    Autorun,
    SuspiciousScript,
    SuspiciousExecutable,
    DoubleExtension,
    FolderMasquerading,
    SuspiciousShortcut
}

public sealed record ScanFinding(
    FindingType Type,
    FindingSeverity Severity,
    string Path,
    string RelativePath,
    string ReasonKey,
    bool CanRestoreVisibility = false,
    string? TechnicalDetails = null);

namespace NoHidden.Models;

public sealed record ScanReport(
    string DriveRoot,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    int ScannedItems,
    IReadOnlyList<ScanFinding> Findings)
{
    public int RecoverableCount =>
        Findings.Count(finding => finding.CanRestoreVisibility);

    public int HighRiskCount =>
        Findings.Count(finding => finding.Severity >= FindingSeverity.High);

    public int SuspiciousCount =>
        Findings.Count(finding =>
            finding.Severity is FindingSeverity.Medium or FindingSeverity.Low);
}

using NoHidden.Models;
using NoHidden.Services;

string root = Path.Combine(
    Path.GetTempPath(),
    "NoHidden-Scanner-Smoke-" + Guid.NewGuid().ToString("N"));

Directory.CreateDirectory(root);

try
{
    string hiddenFile = Path.Combine(root, "private-notes.txt");
    File.WriteAllText(hiddenFile, "test");
    File.SetAttributes(
        hiddenFile,
        File.GetAttributes(hiddenFile) |
        FileAttributes.Hidden |
        FileAttributes.System);

    string disguisedExecutable = Path.Combine(root, "photo.jpg.exe");
    File.WriteAllText(disguisedExecutable, "not-a-real-executable");

    string scriptFile = Path.Combine(root, "run.bat");
    File.WriteAllText(scriptFile, "@echo off");

    string documentsFolder = Path.Combine(root, "Documents");
    Directory.CreateDirectory(documentsFolder);

    string impersonatingExecutable = Path.Combine(root, "Documents.exe");
    File.WriteAllText(impersonatingExecutable, "not-a-real-executable");

    string desktopIni = Path.Combine(root, "desktop.ini");
    File.WriteAllText(desktopIni, "[.ShellClassInfo]");
    File.SetAttributes(
        desktopIni,
        File.GetAttributes(desktopIni) |
        FileAttributes.Hidden |
        FileAttributes.System);

    string payloadFile = Path.Combine(root, "evil.vbs");
    File.WriteAllText(payloadFile, "WScript.Echo \"test\"");
    File.SetAttributes(
        payloadFile,
        File.GetAttributes(payloadFile) |
        FileAttributes.Hidden |
        FileAttributes.System);

    string autorunFile = Path.Combine(root, "autorun.inf");
    File.WriteAllText(
        autorunFile,
        "[autorun]" + Environment.NewLine +
        "open=wscript.exe evil.vbs");

    var scanner = new UsbScanner();
    ScanReport report = await scanner.ScanAsync(root);

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.HiddenItem &&
                finding.Path == hiddenFile &&
                finding.CanRestoreVisibility),
        "Hidden/System file was not detected as recoverable.");

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.DoubleExtension &&
                finding.Path == disguisedExecutable &&
                finding.Severity == FindingSeverity.High),
        "Double-extension executable was not detected as high risk.");

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.SuspiciousScript &&
                finding.Path == scriptFile),
        "Root BAT script was not detected.");

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.FolderMasquerading &&
                finding.Path == impersonatingExecutable &&
                finding.Severity == FindingSeverity.High),
        "Folder-impersonating executable was not detected as high risk.");

    Assert(
        !report.Findings.Any(
            finding =>
                finding.Type == FindingType.HiddenItem &&
                finding.Path == desktopIni),
        "desktop.ini should not be offered as recoverable user content.");

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.Autorun &&
                finding.Path == autorunFile &&
                finding.Severity == FindingSeverity.High),
        "Suspicious autorun.inf was not detected as high risk.");

    Assert(
        report.Findings.Any(
            finding =>
                finding.Type == FindingType.SuspiciousScript &&
                finding.Path == payloadFile &&
                finding.Severity == FindingSeverity.High),
        "Hidden script payload was not detected as high risk.");

    Assert(
        !report.Findings.Any(
            finding =>
                finding.Type == FindingType.HiddenItem &&
                finding.Path == payloadFile &&
                finding.CanRestoreVisibility),
        "Suspicious hidden script must never be offered as a visibility-recovery item.");

    var repairService = new FileVisibilityRepairService();
    repairService.RestoreVisibility(root, hiddenFile);

    FileAttributes repairedAttributes = File.GetAttributes(hiddenFile);

    Assert(
        !repairedAttributes.HasFlag(FileAttributes.Hidden) &&
        !repairedAttributes.HasFlag(FileAttributes.System),
        "Visibility repair did not remove Hidden/System attributes.");

    var hashService = new FileHashService();
    string sha256 = await hashService.ComputeSha256Async(hiddenFile);

    Assert(
        sha256.Length == 64 &&
        sha256.All(Uri.IsHexDigit),
        "SHA-256 service did not return a valid hexadecimal hash.");

    var neutralizationService = new FileNeutralizationService();
    NeutralizationResult neutralization =
        neutralizationService.Neutralize(
            root,
            impersonatingExecutable);

    Assert(
        !File.Exists(impersonatingExecutable),
        "Original suspicious filename still exists after neutralization.");

    Assert(
        File.Exists(neutralization.NeutralizedPath) &&
        neutralization.NeutralizedPath.EndsWith(
            ".nohidden-disabled",
            StringComparison.OrdinalIgnoreCase),
        "Neutralized file was not renamed with the safe NoHidden suffix.");

    Console.WriteLine(
        $"Scanner smoke test passed. Scanned {report.ScannedItems} items and found {report.Findings.Count} findings.");
}
finally
{
    try
    {
        foreach (string entry in Directory.EnumerateFileSystemEntries(
                     root,
                     "*",
                     SearchOption.AllDirectories))
        {
            try
            {
                File.SetAttributes(entry, FileAttributes.Normal);
            }
            catch
            {
            }
        }

        Directory.Delete(root, recursive: true);
    }
    catch
    {
    }
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

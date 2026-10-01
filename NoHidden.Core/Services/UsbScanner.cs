using NoHidden.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace NoHidden.Services;

public sealed class UsbScanner
{
    private static readonly HashSet<string> IgnoredDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "System Volume Information",
            "$RECYCLE.BIN",
            "RECYCLER",
            "FOUND.000",
            ".Spotlight-V100",
            ".Trashes"
        };

    private static readonly HashSet<string> ExecutableExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe",
            ".com",
            ".scr",
            ".pif"
        };

    private static readonly HashSet<string> ScriptExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".bat",
            ".cmd",
            ".vbs",
            ".vbe",
            ".js",
            ".jse",
            ".wsf",
            ".wsh",
            ".ps1",
            ".hta"
        };

    private static readonly HashSet<string> LureExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".ppt",
            ".pptx",
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".mp3",
            ".mp4",
            ".txt",
            ".zip",
            ".rar",
            ".7z"
        };

    private static readonly HashSet<string> SuspiciousCommandNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "cmd.exe",
            "powershell.exe",
            "pwsh.exe",
            "wscript.exe",
            "cscript.exe",
            "mshta.exe",
            "rundll32.exe"
        };

    public Task<ScanReport> ScanAsync(
        string driveRoot,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () => Scan(driveRoot, progress, cancellationToken),
            cancellationToken);
    }

    private ScanReport Scan(
        string driveRoot,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        string normalizedRoot = NormalizeRoot(driveRoot);
        var startedAt = DateTimeOffset.Now;
        var findings = new List<ScanFinding>();
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(normalizedRoot);

        int scannedItems = 0;

        while (pendingDirectories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string currentDirectory = pendingDirectories.Pop();

            string[] entries;

            try
            {
                entries = Directory.GetFileSystemEntries(currentDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (IOException)
            {
                continue;
            }

            foreach (string entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileAttributes attributes;

                try
                {
                    attributes = File.GetAttributes(entry);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }

                scannedItems++;
                progress?.Report(
                    new ScanProgress(
                        scannedItems,
                        Path.GetRelativePath(normalizedRoot, entry)));

                bool isDirectory =
                    attributes.HasFlag(FileAttributes.Directory);

                string name = Path.GetFileName(
                    entry.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar));

                if (isDirectory && IsIgnoredDirectory(name))
                {
                    continue;
                }

                bool isHiddenOrSystem =
                    attributes.HasFlag(FileAttributes.Hidden) ||
                    attributes.HasFlag(FileAttributes.System);

                if (isHiddenOrSystem &&
                    IsRecoverableHiddenItem(name, isDirectory))
                {
                    findings.Add(
                        CreateFinding(
                            FindingType.HiddenItem,
                            FindingSeverity.Info,
                            normalizedRoot,
                            entry,
                            "ReasonHiddenAttributes",
                            canRestoreVisibility: true));
                }

                if (isDirectory)
                {
                    if (!attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        pendingDirectories.Push(entry);
                    }

                    continue;
                }

                AnalyzeFile(
                    normalizedRoot,
                    entry,
                    attributes,
                    findings);
            }
        }

        if (!Directory.Exists(normalizedRoot))
        {
            throw new DirectoryNotFoundException(
                "The USB drive was disconnected before the scan completed.");
        }

        return new ScanReport(
            normalizedRoot,
            startedAt,
            DateTimeOffset.Now,
            scannedItems,
            findings);
    }

    private static void AnalyzeFile(
        string root,
        string path,
        FileAttributes attributes,
        ICollection<ScanFinding> findings)
    {
        string fileName = Path.GetFileName(path);
        string extension = Path.GetExtension(path);
        bool inRoot = IsInRoot(root, path);
        bool hiddenOrSystem =
            attributes.HasFlag(FileAttributes.Hidden) ||
            attributes.HasFlag(FileAttributes.System);

        if (string.Equals(
                fileName,
                "autorun.inf",
                StringComparison.OrdinalIgnoreCase))
        {
            AnalyzeAutorun(root, path, findings);
            return;
        }

        if (string.Equals(
                extension,
                ".lnk",
                StringComparison.OrdinalIgnoreCase))
        {
            AnalyzeShortcut(root, path, inRoot, findings);
            return;
        }

        bool isExecutable = ExecutableExtensions.Contains(extension);
        bool isScript = ScriptExtensions.Contains(extension);

        if (!isExecutable && !isScript)
        {
            return;
        }

        if (MatchesNearbyFolder(path))
        {
            findings.Add(
                CreateFinding(
                    FindingType.FolderMasquerading,
                    FindingSeverity.High,
                    root,
                    path,
                    "ReasonFolderMasquerading"));

            return;
        }

        if (HasDoubleExtension(path))
        {
            findings.Add(
                CreateFinding(
                    FindingType.DoubleExtension,
                    FindingSeverity.High,
                    root,
                    path,
                    "ReasonDoubleExtension"));

            return;
        }

        FindingSeverity severity =
            hiddenOrSystem
                ? FindingSeverity.High
                : inRoot
                    ? FindingSeverity.Medium
                    : FindingSeverity.Low;

        string reasonKey = hiddenOrSystem
            ? "ReasonHiddenDangerous"
            : isScript
                ? "ReasonSuspiciousScript"
                : "ReasonSuspiciousExecutable";

        findings.Add(
            CreateFinding(
                isScript
                    ? FindingType.SuspiciousScript
                    : FindingType.SuspiciousExecutable,
                severity,
                root,
                path,
                reasonKey));
    }

    private static void AnalyzeAutorun(
        string root,
        string path,
        ICollection<ScanFinding> findings)
    {
        string? content = TryReadSmallTextFile(path);

        if (content is null)
        {
            findings.Add(
                CreateFinding(
                    FindingType.Autorun,
                    FindingSeverity.Medium,
                    root,
                    path,
                    "ReasonAutorunPresent"));

            return;
        }

        bool suspicious =
            Regex.IsMatch(
                content,
                @"(?im)^\s*(open|shellexecute|shell\\[^=]+\\command)\s*=") &&
            (ContainsDangerousExtension(content) ||
             SuspiciousCommandNames.Any(
                 command => content.Contains(
                     command,
                     StringComparison.OrdinalIgnoreCase)));

        findings.Add(
            CreateFinding(
                FindingType.Autorun,
                suspicious
                    ? FindingSeverity.High
                    : FindingSeverity.Medium,
                root,
                path,
                suspicious
                    ? "ReasonAutorunSuspicious"
                    : "ReasonAutorunPresent"));
    }

    private static void AnalyzeShortcut(
        string root,
        string path,
        bool inRoot,
        ICollection<ScanFinding> findings)
    {
        ShortcutInfo? shortcut = ShortcutInspector.TryInspect(path);

        if (shortcut is null)
        {
            if (inRoot)
            {
                findings.Add(
                    CreateFinding(
                        FindingType.SuspiciousShortcut,
                        FindingSeverity.Medium,
                        root,
                        path,
                        "ReasonRootShortcut"));
            }

            return;
        }

        string target = shortcut.TargetPath ?? string.Empty;
        string arguments = shortcut.Arguments ?? string.Empty;
        string targetFileName = Path.GetFileName(target);

        bool launchesCommand =
            SuspiciousCommandNames.Contains(targetFileName) ||
            SuspiciousCommandNames.Any(
                command => arguments.Contains(
                    command,
                    StringComparison.OrdinalIgnoreCase));

        if (launchesCommand)
        {
            findings.Add(
                CreateFinding(
                    FindingType.SuspiciousShortcut,
                    FindingSeverity.High,
                    root,
                    path,
                    "ReasonSuspiciousShortcutCommand",
                    technicalDetails:
                        BuildShortcutDetails(shortcut)));

            return;
        }

        bool dangerousTarget =
            IsDangerousExtension(Path.GetExtension(target)) ||
            IsHiddenOrSystemFile(target);

        if (dangerousTarget)
        {
            findings.Add(
                CreateFinding(
                    FindingType.SuspiciousShortcut,
                    FindingSeverity.High,
                    root,
                    path,
                    "ReasonSuspiciousShortcutPayload",
                    technicalDetails:
                        BuildShortcutDetails(shortcut)));

            return;
        }

        if (MatchesNearbyFolder(path) || inRoot)
        {
            findings.Add(
                CreateFinding(
                    FindingType.SuspiciousShortcut,
                    inRoot
                        ? FindingSeverity.Medium
                        : FindingSeverity.Low,
                    root,
                    path,
                    "ReasonRootShortcut",
                    technicalDetails:
                        BuildShortcutDetails(shortcut)));
        }
    }

    private static string BuildShortcutDetails(
        ShortcutInfo shortcut)
    {
        return $"Target: {shortcut.TargetPath ?? "-"}; Arguments: {shortcut.Arguments ?? "-"}";
    }

    private static ScanFinding CreateFinding(
        FindingType type,
        FindingSeverity severity,
        string root,
        string path,
        string reasonKey,
        bool canRestoreVisibility = false,
        string? technicalDetails = null)
    {
        return new ScanFinding(
            type,
            severity,
            path,
            Path.GetRelativePath(root, path),
            reasonKey,
            canRestoreVisibility,
            technicalDetails);
    }

    private static string NormalizeRoot(string driveRoot)
    {
        string fullPath = Path.GetFullPath(driveRoot);

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(
                $"USB drive '{driveRoot}' is no longer available.");
        }

        return Path.EndsInDirectorySeparator(fullPath)
            ? fullPath
            : fullPath + Path.DirectorySeparatorChar;
    }

    private static bool IsRecoverableHiddenItem(
        string name,
        bool isDirectory)
    {
        if (isDirectory)
        {
            return true;
        }

        if (string.Equals(
                name,
                "autorun.inf",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                name,
                "desktop.ini",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                name,
                "thumbs.db",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string extension = Path.GetExtension(name);

        return !IsDangerousExtension(extension) &&
               !string.Equals(
                   extension,
                   ".lnk",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsIgnoredDirectory(string name)
    {
        return IgnoredDirectoryNames.Contains(name);
    }

    private static bool IsInRoot(
        string root,
        string path)
    {
        string? parent = Path.GetDirectoryName(path);

        if (parent is null)
        {
            return false;
        }

        return string.Equals(
            Path.TrimEndingDirectorySeparator(parent),
            Path.TrimEndingDirectorySeparator(root),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDangerousExtension(string extension)
    {
        return ExecutableExtensions.Contains(extension) ||
               ScriptExtensions.Contains(extension);
    }

    private static bool ContainsDangerousExtension(string value)
    {
        return ExecutableExtensions
                   .Concat(ScriptExtensions)
                   .Any(
                       extension => value.Contains(
                           extension,
                           StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasDoubleExtension(string path)
    {
        string finalExtension = Path.GetExtension(path);

        if (!IsDangerousExtension(finalExtension))
        {
            return false;
        }

        string withoutFinalExtension =
            Path.GetFileNameWithoutExtension(path);

        string previousExtension =
            Path.GetExtension(withoutFinalExtension);

        return LureExtensions.Contains(previousExtension);
    }

    private static bool MatchesNearbyFolder(string path)
    {
        string? directory = Path.GetDirectoryName(path);

        if (directory is null)
        {
            return false;
        }

        string candidateDirectory =
            Path.Combine(
                directory,
                Path.GetFileNameWithoutExtension(path));

        return Directory.Exists(candidateDirectory);
    }

    private static bool IsHiddenOrSystemFile(string target)
    {
        if (string.IsNullOrWhiteSpace(target) ||
            !File.Exists(target))
        {
            return false;
        }

        try
        {
            FileAttributes attributes = File.GetAttributes(target);

            return attributes.HasFlag(FileAttributes.Hidden) ||
                   attributes.HasFlag(FileAttributes.System);
        }
        catch
        {
            return false;
        }
    }

    private static string? TryReadSmallTextFile(string path)
    {
        try
        {
            var info = new FileInfo(path);

            if (info.Length > 1024 * 1024)
            {
                return null;
            }

            return File.ReadAllText(path);
        }
        catch
        {
            return null;
        }
    }
}

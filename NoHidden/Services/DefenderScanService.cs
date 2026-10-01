using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace NoHidden.Services;

public enum DefenderFileScanStatus
{
    CleanOrNoActionRequired,
    DetectionOrScanProblem,
    Failed
}

public sealed record DefenderFileScanResult(
    DefenderFileScanStatus Status,
    int ExitCode);

public sealed class DefenderScanService
{
    public bool IsAvailable => FindMpCmdRunPath() is not null;

    public async Task<DefenderFileScanResult> ScanFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        string? executablePath = FindMpCmdRunPath();

        if (executablePath is null)
        {
            throw new FileNotFoundException(
                "Microsoft Defender command-line scanner was not found.");
        }

        string normalizedFilePath = Path.GetFullPath(filePath);

        if (!File.Exists(normalizedFilePath))
        {
            throw new FileNotFoundException(
                "The selected file no longer exists.",
                normalizedFilePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments =
                $"-Scan -ScanType 3 -File \"{EscapeArgument(normalizedFilePath)}\" -DisableRemediation",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Path.GetDirectoryName(executablePath)
                               ?? Environment.SystemDirectory,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using Process? process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException(
                "Microsoft Defender scan could not be started.");
        }

        await process.WaitForExitAsync(cancellationToken);

        return process.ExitCode switch
        {
            0 => new DefenderFileScanResult(
                DefenderFileScanStatus.CleanOrNoActionRequired,
                process.ExitCode),

            2 => new DefenderFileScanResult(
                DefenderFileScanStatus.DetectionOrScanProblem,
                process.ExitCode),

            _ => new DefenderFileScanResult(
                DefenderFileScanStatus.Failed,
                process.ExitCode)
        };
    }

    private static string EscapeArgument(string value)
    {
        return value.Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string? FindMpCmdRunPath()
    {
        string programFiles =
            Environment.GetFolderPath(
                Environment.SpecialFolder.ProgramFiles);

        string legacyPath =
            Path.Combine(
                programFiles,
                "Windows Defender",
                "MpCmdRun.exe");

        string platformRoot =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData),
                "Microsoft",
                "Windows Defender",
                "Platform");

        try
        {
            if (Directory.Exists(platformRoot))
            {
                string? newestPlatformExecutable =
                    Directory
                        .EnumerateDirectories(platformRoot)
                        .OrderByDescending(
                            directory => directory,
                            StringComparer.OrdinalIgnoreCase)
                        .Select(
                            directory => Path.Combine(
                                directory,
                                "MpCmdRun.exe"))
                        .FirstOrDefault(File.Exists);

                if (newestPlatformExecutable is not null)
                {
                    return newestPlatformExecutable;
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Fall back to the legacy Defender location.
        }
        catch (IOException)
        {
            // Fall back to the legacy Defender location.
        }

        return File.Exists(legacyPath)
            ? legacyPath
            : null;
    }
}

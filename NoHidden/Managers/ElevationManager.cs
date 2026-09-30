using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace NoHidden.Managers;

public static class ElevationManager
{
    public const string ApplyAutorunProtectionArgument = "--apply-autorun-protection";
    public const string ParentProcessIdArgument = "--parent-pid";

    public static bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RestartAsAdministratorForAutorunProtection()
    {
        string executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException(
                "NoHidden could not determine its executable path.");

        string arguments =
            $"{ApplyAutorunProtectionArgument} {ParentProcessIdArgument} {Environment.ProcessId}";

        Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = AppContext.BaseDirectory
        });
    }

    public static bool IsElevationCanceled(Exception exception)
    {
        return exception is Win32Exception win32Exception &&
               win32Exception.NativeErrorCode == 1223;
    }

    public static int? GetParentProcessId(IReadOnlyList<string> arguments)
    {
        for (int i = 0; i < arguments.Count - 1; i++)
        {
            if (!string.Equals(
                    arguments[i],
                    ParentProcessIdArgument,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return int.TryParse(arguments[i + 1], out int processId)
                ? processId
                : null;
        }

        return null;
    }

    public static void WaitForParentProcessExit(
        int? parentProcessId,
        int timeoutMilliseconds = 10000)
    {
        if (parentProcessId is null ||
            parentProcessId <= 0 ||
            parentProcessId == Environment.ProcessId)
        {
            return;
        }

        try
        {
            using Process parent = Process.GetProcessById(parentProcessId.Value);

            if (!parent.HasExited)
            {
                parent.WaitForExit(timeoutMilliseconds);
            }
        }
        catch (ArgumentException)
        {
            // The parent process already exited.
        }
        catch (InvalidOperationException)
        {
            // The parent process is no longer available.
        }
    }
}

using Microsoft.Win32;

namespace NoHidden.Managers;

public sealed class AutoPlayManager
{
    private const string ExplorerPoliciesPath =
        @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";

    private const int AllDriveTypesMask = 0xFF;
    private const int AllDriveLettersMask = 0x03FFFFFF;

    public (bool IsAutorunDisabled, bool IsAutoPlayDisabled) CheckStatus()
    {
        int? machineDriveType = TryReadPolicyValue(
            Registry.LocalMachine,
            "NoDriveTypeAutoRun");

        int? userDriveType = TryReadPolicyValue(
            Registry.CurrentUser,
            "NoDriveTypeAutoRun");

        int? machineDriveLetters = TryReadPolicyValue(
            Registry.LocalMachine,
            "NoDriveAutoRun");

        int? userDriveLetters = TryReadPolicyValue(
            Registry.CurrentUser,
            "NoDriveAutoRun");

        bool isAutorunDisabled =
            IsMaskFullySet(machineDriveType, AllDriveTypesMask) ||
            IsMaskFullySet(userDriveType, AllDriveTypesMask);

        bool isAutoPlayDisabled =
            IsMaskFullySet(machineDriveLetters, AllDriveLettersMask) ||
            IsMaskFullySet(userDriveLetters, AllDriveLettersMask);

        return (isAutorunDisabled, isAutoPlayDisabled);
    }

    public void DisableAutorunAndAutoPlay()
    {
        if (!ElevationManager.IsAdministrator())
        {
            throw new UnauthorizedAccessException(
                "Administrator permission is required to change system-wide AutoRun policy.");
        }

        using RegistryKey key =
            Registry.LocalMachine.CreateSubKey(
                ExplorerPoliciesPath,
                writable: true)
            ?? throw new InvalidOperationException(
                "Unable to open the Windows Explorer policy registry key.");

        key.SetValue(
            "NoDriveTypeAutoRun",
            AllDriveTypesMask,
            RegistryValueKind.DWord);

        key.SetValue(
            "NoDriveAutoRun",
            AllDriveLettersMask,
            RegistryValueKind.DWord);

        key.Flush();

        var status = CheckStatus();

        if (!status.IsAutorunDisabled || !status.IsAutoPlayDisabled)
        {
            throw new InvalidOperationException(
                "Windows did not confirm the requested AutoRun policy changes.");
        }
    }

    private static int? TryReadPolicyValue(
        RegistryKey hive,
        string valueName)
    {
        try
        {
            using RegistryKey? key =
                hive.OpenSubKey(
                    ExplorerPoliciesPath,
                    writable: false);

            object? value = key?.GetValue(valueName);

            return value is null
                ? null
                : Convert.ToInt32(value);
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (System.Security.SecurityException)
        {
            return null;
        }
    }

    private static bool IsMaskFullySet(
        int? value,
        int mask)
    {
        return value is not null &&
               (value.Value & mask) == mask;
    }
}

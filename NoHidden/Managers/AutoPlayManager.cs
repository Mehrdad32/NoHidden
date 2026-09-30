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
        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(ExplorerPoliciesPath, writable: false);

        object? driveTypeValue = key?.GetValue("NoDriveTypeAutoRun");
        object? driveLetterValue = key?.GetValue("NoDriveAutoRun");

        bool isAutorunDisabled = driveTypeValue is not null &&
            (Convert.ToInt32(driveTypeValue) & AllDriveTypesMask) == AllDriveTypesMask;

        bool isAutoPlayDisabled = driveLetterValue is not null &&
            (Convert.ToInt32(driveLetterValue) & AllDriveLettersMask) == AllDriveLettersMask;

        return (isAutorunDisabled, isAutoPlayDisabled);
    }

    public void DisableAutorunAndAutoPlay()
    {
        using RegistryKey key =
            Registry.CurrentUser.CreateSubKey(ExplorerPoliciesPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open the Windows Explorer policy registry key.");

        key.SetValue("NoDriveTypeAutoRun", AllDriveTypesMask, RegistryValueKind.DWord);
        key.SetValue("NoDriveAutoRun", AllDriveLettersMask, RegistryValueKind.DWord);
    }
}

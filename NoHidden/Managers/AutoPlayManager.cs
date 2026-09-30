using Microsoft.Win32;

namespace NoHidden.Managers
{
    public class AutoPlayManager
    {
        private const string ExplorerPoliciesPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";

        /// <summary>
        /// Checks the Autorun and AutoPlay status.
        /// </summary>
        /// <returns>A tuple containing whether Autorun is disabled and whether AutoPlay is disabled.</returns>
        public (bool IsAutorunDisabled, bool IsAutoPlayDisabled) CheckStatus()
        {
            bool isAutorunDisabled = false;
            bool isAutoPlayDisabled = false;

            try
            {
                // Check Autorun (NoDriveTypeAutoRun)
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ExplorerPoliciesPath, false))
                {
                    object noDriveTypeAutoRunValue = key?.GetValue("NoDriveTypeAutoRun");
                    if (noDriveTypeAutoRunValue != null)
                    {
                        int noDriveTypeAutoRun = (int)noDriveTypeAutoRunValue;
                        isAutorunDisabled = (noDriveTypeAutoRun & 0x91) == 0x91; // Check bitmask
                    }
                }

                // Check AutoPlay (NoDriveAutoRun)
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ExplorerPoliciesPath, false))
                {
                    object noDriveAutoRunValue = key?.GetValue("NoDriveAutoRun");
                    if (noDriveAutoRunValue != null)
                    {
                        isAutoPlayDisabled = (int)noDriveAutoRunValue == 0xFF;
                    }
                }
            }
            catch
            {
                // Log or handle exceptions if needed
            }

            return (isAutorunDisabled, isAutoPlayDisabled);
        }

        /// <summary>
        /// Disables Autorun and AutoPlay for all drives.
        /// </summary>
        public void DisableAutorunAndAutoPlay()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ExplorerPoliciesPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue("NoDriveTypeAutoRun", 0x91, RegistryValueKind.DWord); // Disable Autorun
                        key.SetValue("NoDriveAutoRun", 0xFF, RegistryValueKind.DWord);   // Disable AutoPlay
                    }
                }
            }
            catch
            {
                // Log or handle exceptions if needed
                throw;
            }
        }
    }
}

using NoHidden.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace NoHidden.Managers;

public sealed class DriveManager
{
    public ObservableCollection<UsbDriveInfo> GetRemovableDrives()
    {
        var removableDrives = new ObservableCollection<UsbDriveInfo>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (drive.DriveType != DriveType.Removable || !drive.IsReady)
                {
                    continue;
                }

                removableDrives.Add(new UsbDriveInfo(
                    drive.RootDirectory.FullName,
                    drive.VolumeLabel,
                    drive.DriveFormat,
                    drive.TotalSize,
                    drive.AvailableFreeSpace));
            }
            catch (IOException)
            {
                // A removable drive can disappear while Windows is enumerating it.
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore inaccessible removable drives and continue enumerating.
            }
        }

        return removableDrives;
    }
}

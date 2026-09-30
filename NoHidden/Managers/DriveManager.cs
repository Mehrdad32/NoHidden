using System.Collections.ObjectModel;
using System.IO;

namespace NoHidden.Managers
{
    public class DriveManager
    {
        /// <summary>
        /// Gets a list of removable drives.
        /// </summary>
        /// <returns>A collection of drive names.</returns>
        public ObservableCollection<string> GetRemovableDrives()
        {
            var removableDrives = new ObservableCollection<string>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    removableDrives.Add($"{drive.Name} ({drive.VolumeLabel})");
                }
            }

            return removableDrives;
        }
    }

}

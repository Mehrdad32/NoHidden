using System.Globalization;

namespace NoHidden.Models;

public sealed record UsbDriveInfo(
    string RootPath,
    string VolumeLabel,
    string DriveFormat,
    long TotalSize,
    long AvailableFreeSpace)
{
    public string DisplayName => string.IsNullOrWhiteSpace(VolumeLabel)
        ? RootPath
        : $"{RootPath} ({VolumeLabel})";

    public string CapacityText =>
        $"{FormatBytes(AvailableFreeSpace)} free of {FormatBytes(TotalSize)}";

    public override string ToString() => DisplayName;

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int unitIndex = 0;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{value:0.#} {units[unitIndex]}");
    }
}

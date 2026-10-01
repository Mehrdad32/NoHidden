using System.IO;

namespace NoHidden.Services;

public sealed class FileVisibilityRepairService
{
    public void RestoreVisibility(
        string driveRoot,
        string targetPath)
    {
        string normalizedRoot =
            Path.GetFullPath(driveRoot);

        string normalizedTarget =
            Path.GetFullPath(targetPath);

        string rootWithSeparator =
            Path.EndsInDirectorySeparator(normalizedRoot)
                ? normalizedRoot
                : normalizedRoot + Path.DirectorySeparatorChar;

        if (!normalizedTarget.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The selected item is outside the USB drive.");
        }

        if (!File.Exists(normalizedTarget) &&
            !Directory.Exists(normalizedTarget))
        {
            throw new FileNotFoundException(
                "The selected item no longer exists.",
                normalizedTarget);
        }

        FileAttributes attributes =
            File.GetAttributes(normalizedTarget);

        FileAttributes repairedAttributes =
            attributes &
            ~FileAttributes.Hidden &
            ~FileAttributes.System;

        File.SetAttributes(
            normalizedTarget,
            repairedAttributes);
    }
}

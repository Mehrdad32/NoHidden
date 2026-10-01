using System.IO;

namespace NoHidden.Services;

public sealed record NeutralizationResult(
    string OriginalPath,
    string NeutralizedPath);

public sealed class FileNeutralizationService
{
    private const string DisabledSuffix = ".nohidden-disabled";

    public NeutralizationResult Neutralize(
        string driveRoot,
        string targetPath)
    {
        string normalizedRoot = Path.GetFullPath(driveRoot);
        string normalizedTarget = Path.GetFullPath(targetPath);

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

        if (!File.Exists(normalizedTarget))
        {
            throw new FileNotFoundException(
                "The selected file no longer exists.",
                normalizedTarget);
        }

        if (normalizedTarget.EndsWith(
                DisabledSuffix,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This file is already neutralized by NoHidden.");
        }

        string destination = normalizedTarget + DisabledSuffix;
        int suffix = 1;

        while (File.Exists(destination))
        {
            destination =
                normalizedTarget +
                DisabledSuffix +
                "." +
                suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);

            suffix++;
        }

        File.Move(normalizedTarget, destination);

        return new NeutralizationResult(
            normalizedTarget,
            destination);
    }
}

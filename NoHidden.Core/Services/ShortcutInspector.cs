using System.Runtime.InteropServices;

namespace NoHidden.Services;

public sealed record ShortcutInfo(
    string? TargetPath,
    string? Arguments,
    string? WorkingDirectory);

public static class ShortcutInspector
{
    public static ShortcutInfo? TryInspect(string shortcutPath)
    {
        object? shellObject = null;
        object? shortcutObject = null;

        try
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");

            if (shellType is null)
            {
                return null;
            }

            shellObject = Activator.CreateInstance(shellType);

            if (shellObject is null)
            {
                return null;
            }

            dynamic shell = shellObject;
            shortcutObject = shell.CreateShortcut(shortcutPath);
            dynamic shortcut = shortcutObject;

            return new ShortcutInfo(
                shortcut.TargetPath as string,
                shortcut.Arguments as string,
                shortcut.WorkingDirectory as string);
        }
        catch
        {
            return null;
        }
        finally
        {
            ReleaseComObject(shortcutObject);
            ReleaseComObject(shellObject);
        }
    }

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}

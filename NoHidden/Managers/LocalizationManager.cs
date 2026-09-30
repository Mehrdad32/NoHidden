using System.Globalization;
using System.IO;
using System.Windows;

namespace NoHidden.Managers;

public static class LocalizationManager
{
    private const string DefaultLanguage = "en-US";

    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "en-US",
            "fa-IR"
        };

    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NoHidden");

    private static readonly string LanguageFilePath =
        Path.Combine(SettingsDirectory, "language.txt");

    public static string CurrentLanguage { get; private set; } = DefaultLanguage;

    public static void LoadLanguage()
    {
        ChangeLanguage(ReadSavedLanguage(), persist: false);
    }

    public static void ChangeLanguage(string cultureName, bool persist = true)
    {
        if (!SupportedLanguages.Contains(cultureName))
        {
            cultureName = DefaultLanguage;
        }

        var culture = CultureInfo.GetCultureInfo(cultureName);
        var dictionary = new ResourceDictionary
        {
            Source = new Uri(
                $"/Resources/Strings.{cultureName}.xaml",
                UriKind.Relative)
        };

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(dictionary);

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CurrentLanguage = cultureName;

        if (persist)
        {
            SaveLanguage(cultureName);
        }
    }

    public static FlowDirection GetFlowDirection(string? cultureName = null)
    {
        string language = cultureName ?? CurrentLanguage;

        return language.StartsWith("fa", StringComparison.OrdinalIgnoreCase)
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }

    private static string ReadSavedLanguage()
    {
        try
        {
            if (!File.Exists(LanguageFilePath))
            {
                return DefaultLanguage;
            }

            string savedLanguage = File.ReadAllText(LanguageFilePath).Trim();
            return SupportedLanguages.Contains(savedLanguage)
                ? savedLanguage
                : DefaultLanguage;
        }
        catch (IOException)
        {
            return DefaultLanguage;
        }
        catch (UnauthorizedAccessException)
        {
            return DefaultLanguage;
        }
    }

    private static void SaveLanguage(string cultureName)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(LanguageFilePath, cultureName);
        }
        catch (IOException)
        {
            // Language persistence is optional; the UI should still keep working.
        }
        catch (UnauthorizedAccessException)
        {
            // Language persistence is optional; the UI should still keep working.
        }
    }
}

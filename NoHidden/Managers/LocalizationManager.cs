using System;
using System.Configuration;
using System.Globalization;
using System.Windows;

namespace NoHidden.Managers
{
    public static class LocalizationManager
    {
        private const string LanguageConfigKey = "Language";

        // Change the language dynamically
        public static void ChangeLanguage(string cultureName)
        {
            // Load the appropriate ResourceDictionary
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"/Resources/Strings.{cultureName}.xaml", UriKind.Relative)
            };

            // Replace existing ResourceDictionary
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            // Set the culture
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);

            // Save the selected language
            SaveLanguageToConfig(cultureName);

            // Set the FlowDirection
            SetFlowDirection(cultureName);

            // Notify the UI to refresh
            RefreshUI();
        }

        // Load the language on startup
        public static void LoadLanguage()
        {
            string cultureName = GetSavedLanguage();
            ChangeLanguage(cultureName);
        }

        // Save the selected language to config
        private static void SaveLanguageToConfig(string cultureName)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[LanguageConfigKey].Value = cultureName;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        // Get the saved language from config
        private static string GetSavedLanguage()
        {
            return ConfigurationManager.AppSettings[LanguageConfigKey] ?? "en-US";
        }

        // Set FlowDirection based on language
        private static void SetFlowDirection(string cultureName)
        {
            FlowDirection flowDirection = cultureName.StartsWith("fa") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            foreach (Window window in Application.Current.Windows)
            {
                window.FlowDirection = flowDirection;
            }
        }

        // Refresh the UI dynamically
        private static void RefreshUI()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext != null)
                {
                    var viewModelType = window.DataContext.GetType();
                    var newViewModel = Activator.CreateInstance(viewModelType);
                    window.DataContext = newViewModel;
                }
            }
        }
    }
}

using NoHidden.Managers;
using System.Windows;
using System.Windows.Controls;

namespace NoHidden;

public partial class MainWindow : Window
{
    private bool _isInitializingLanguageSelection;

    public MainWindow()
    {
        _isInitializingLanguageSelection = true;

        InitializeComponent();

        LanguageComboBox.SelectedValue = LocalizationManager.CurrentLanguage;

        _isInitializingLanguageSelection = false;
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializingLanguageSelection ||
            sender is not ComboBox comboBox ||
            comboBox.SelectedItem is not ComboBoxItem selectedItem ||
            selectedItem.Tag is not string cultureName)
        {
            return;
        }

        LocalizationManager.ChangeLanguage(cultureName);

        if (DataContext is MainViewModel viewModel)
        {
            viewModel.ReloadLocalization();
        }
    }

    private void Border_MouseLeftButtonDown(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border border &&
            border.TemplatedParent is ComboBox comboBox)
        {
            comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
        }
    }
}

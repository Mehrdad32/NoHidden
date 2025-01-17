using System.Windows;
using System.Windows.Controls;
using NoHidden.Managers;

namespace NoHidden
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureName = selectedItem.Tag.ToString();
                LocalizationManager.ChangeLanguage(cultureName);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.TemplatedParent is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
            }
        }
    }
}
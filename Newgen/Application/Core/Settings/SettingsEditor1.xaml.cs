using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor1.xaml
    /// </summary>
    public partial class SettingsEditor1 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor1"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor1() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:EnableAutoStartCheckBoxClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnEnableAutoStartCheckBoxClick(object sender, RoutedEventArgs e) {
            try {
                if (Settings.Current.Autostart)
                    ApplicationExtensions.SetStartWithWindows(
                        App.Current.Title,
                        Process.GetCurrentProcess().MainModule.FileName
                        );
                else
                    ApplicationExtensions.RemoveStartWithWindows(App.Current.Title);
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:EnableHKClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnEnableHKClick(object sender, RoutedEventArgs e) {
        }

        /// <summary>
        /// Handles the <see cref="E:LanguageComboBoxSelectionChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (LanguageComboBox.SelectedIndex > -1)
                Settings.Current.Language = Settings.Current.Cultures[LanguageComboBox.SelectedIndex].Name;
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                foreach (var culture in Settings.Current.Cultures) {
                    LanguageComboBox.Items.Add(new ComboBoxItem() {
                        Content = culture.NativeName
                    });
                }
                LanguageComboBox.Text = CultureInfo.GetCultureInfo(Settings.Current.Language).NativeName;

                if (Api.Settings.TimeMode == TimeMode.H12)
                    Time12HRadioButton.IsChecked = true;
                if (Api.Settings.TimeMode == TimeMode.H24)
                    Time24HRadioButton.IsChecked = true;
                LockScreenTimeTextBox.Text = Settings.Current.LockScreenTime.ToString();

                //if (!Settings.IsProMode) {
                //    EnableAutoStartCheckBox.IsEnabled = false;
                //    EnableHK.IsEnabled = false;
                //    Time12HRadioButton.IsEnabled = false;
                //    Time24HRadioButton.IsEnabled = false;
                //    LockScreenTimeTextBox.IsEnabled = false;
                //}
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:LockScreenTimeTextBoxTextChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLockScreenTimeTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            try {
                if (string.IsNullOrWhiteSpace(LockScreenTimeTextBox.Text))
                    Settings.Current.LockScreenTime = -1;
                else
                    Settings.Current.LockScreenTime = int.Parse(LockScreenTimeTextBox.Text);
            }
            catch /* Eat */ {
                LockScreenTimeTextBox.Text = Settings.Current.LockScreenTime.ToString();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Time12HRadioButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnTime12HRadioButtonClick(object sender, RoutedEventArgs e) {
            Api.Settings.TimeMode = TimeMode.H24;
        }

        /// <summary>
        /// Handles the <see cref="E:Time24HRadioButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnTime24HRadioButtonClick(object sender, RoutedEventArgs e) {
            Api.Settings.TimeMode = TimeMode.H24;
        }
    }
}
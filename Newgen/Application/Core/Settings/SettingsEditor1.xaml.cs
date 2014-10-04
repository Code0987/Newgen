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
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                foreach (var culture in Settings.Current.Cultures) {
                    LanguageComboBox.Items.Add(new ComboBoxItem() {
                        Content = culture.NativeName
                    });
                }
                LanguageComboBox.Text = CultureInfo.GetCultureInfo(Settings.Current.Language).NativeName;
                LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;

                if (Api.Settings.TimeMode == TimeMode.H12)
                    Time12HRadioButton.IsChecked = true;
                if (Api.Settings.TimeMode == TimeMode.H24)
                    Time24HRadioButton.IsChecked = true;

                LockScreenTimeSlider.Value = (double)Settings.Current.MinTileHeight;
                LockScreenTimeSlider.ValueChanged += LockScreenTimeSlider_ValueChanged;
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Locks the screen time slider_ value changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>...</remarks>
        private void LockScreenTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            try {
                Settings.Current.LockScreenTime = (int)(LockScreenTimeSlider.Value);
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the LanguageComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (LanguageComboBox.SelectedIndex > -1)
                Settings.Current.Language = Settings.Current.Cultures[LanguageComboBox.SelectedIndex].Name;
        }

        /// <summary>
        /// Handles the <see cref="E:Time12HRadioButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void Time12HRadioButton_Click(object sender, RoutedEventArgs e) {
            Api.Settings.TimeMode = TimeMode.H24;
        }

        /// <summary>
        /// Handles the <see cref="E:Time24HRadioButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void Time24HRadioButton_Click(object sender, RoutedEventArgs e) {
            Api.Settings.TimeMode = TimeMode.H24;
        }
    }
}
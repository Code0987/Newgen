using System.Windows;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsHub.xaml
    /// </summary>
    public partial class SettingsHub : HubWindow {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsHub"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsHub() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:BackButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnBackButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// Handles the <see cref="E:CancelButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnCancelButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// Handles the <see cref="E:OkButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnOkButtonClick(object sender, RoutedEventArgs e) {
            Settings.Current.Save();

            Close();
        }
    }
}
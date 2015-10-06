using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using libns.Applied.Licensing;
using NS.Web;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor0.xaml
    /// </summary>
    public partial class SettingsEditor0 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor0" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor0() {
            InitializeComponent();

            LicenseControl.LicenseManager.RequestEndpoint = new Uri(WebShared.libns_Applied_Licensing_Uri_NSApps_net);
        }

        /// <summary>
        /// Called when [license control active license identifier changed].
        /// </summary>
        /// <param name="licenseId">The license identifier.</param>
        /// <remarks>...</remarks>
        private void OnLicenseControlActiveLicenseIdChanged(Guid licenseId) {
            if (LicenseManager.Current.IsValid(id: licenseId, productId: App.Current.Guid.ToString())) {
                Settings.Current.ActiveLicenseId = licenseId;
                Settings.Current.Save();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:LicenseControlLoaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLicenseControlLoaded(object sender, RoutedEventArgs e) {
            try {
                LicenseControl.ActiveLicenseId = Settings.Current.GetAndValidateActiveLicense();
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                TextBlock_Version.Text = App.Current.Version;
                TextBlock_Copyright.Text = App.Current.Copyright;
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:TextBlock_Link_AppMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnTextBlock_Link_AppMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Api.Messenger.Send(new EMessage() {
                Key = EMessage.UrlKey,
                Value = TextBlock_Link_App.Text
            });
        }
    }
}
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using libns.Native;
using libns.Media;
using libns.Media.Imaging;

namespace Newgen.Packages.AppLink
{
    /// <summary>
    /// Class AppLinkPackageAddressBarWindow.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class AppLinkPackageAddressBarWindow : HubWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppLinkPackageAddressBarWindow"/> class.
        /// </summary>
        public AppLinkPackageAddressBarWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Windows the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e) {

            var accent = Settings.Current.ToolbarBackgroundColor;

            accent.A = 255;
            Root.Background = accent.ToBrush();
            accent.A = 128;
            Background = accent.ToBrush();

            AddressBox.Focus();
            AddressBox.CaretIndex = AddressBox.Text.Length;
        }

        /// <summary>
        /// Windows the key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                AddressBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Addresses the box key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void OnAddressBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close();
        }     
    }
}
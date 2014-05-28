using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Newgen.Packages.AppLink {

    /// <summary>
    /// Interaction logic for AppWidgetOptionsHub.xaml
    /// </summary>
    public partial class AppLinkPackageOptionsHub : HubWindow {
        private AppLinkPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeHub" /> class.
        /// </summary>
        /// <param name="package">The appwidget.</param>
        public AppLinkPackageOptionsHub(AppLinkPackage package)
            : base() {
            this.package = package;
            package.customizedSettings = package.Settings.Customize<AppLinkPackageCustomizedSettings>();

            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            TextBox_WidgetTitle.Text = ((AppLinkPackageTile)package.Tile).Title.Text;
            IconImage.Source = ((AppLinkPackageTile)package.Tile).Icon.Source;
            TextBox_WidgetArgs.Text = package.customizedSettings.Args ?? "";
        }

        /// <summary>
        /// Handles the Click event of the OkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            package.customizedSettings = package.Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
                s.Title = TextBox_WidgetTitle.Text;
                s.Args = TextBox_WidgetArgs.Text;
            });

            package.ReloadTile();

            Close();
        }

        /// <summary>
        /// Handles the Click event of the CancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the ChangeIconButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void ChangeIconButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = Api.ImageFilter;
            if (!(bool)dialog.ShowDialog())
                return;

            try {
                File.WriteAllBytes(package.Settings.CreateAbsolutePathFor(package.customizedSettings.IconPath), File.ReadAllBytes(dialog.FileName));
                package.ReloadTile();
                IconImage.Source = ((AppLinkPackageTile)package.Tile).Icon.Source;
            }
            catch (Exception) {
                MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
            }
        }
    }
}
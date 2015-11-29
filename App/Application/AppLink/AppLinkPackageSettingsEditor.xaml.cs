using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newgen.Resources;

namespace Newgen.AppLink {

    /// <summary>
    /// Interaction logic for AppLinkPackageSettingsEditor.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class AppLinkPackageSettingsEditor : UserControl {

        /// <summary>
        /// The package
        /// </summary>
        private AppLinkPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeHub" /> class.
        /// </summary>
        /// <param name="package">The appwidget.</param>
        /// <remarks>...</remarks>
        public AppLinkPackageSettingsEditor(AppLinkPackage package)
            : base() {
            this.package = package;
            package.customizedSettings = package.GetSettings().Customize<AppLinkPackageCustomizedSettings>();

            InitializeComponent();
        }

        /// <summary>
        /// Handles the TextChanged event of the ArgsTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void ArgsTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var changed = false;

            package.customizedSettings = package.GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                var tb = ((TextBox)sender);
                if (string.Compare(s.Args, tb.Text) == 0)
                    return;
                changed = true;
                s.Args = tb.Text;
            });

            if (changed)
                package.ReloadTile();
        }

        /// <summary>
        /// Handles the Click event of the ChangeIconButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void ChangeIconButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = Api.ImageFilter;
            if (!(bool)dialog.ShowDialog())
                return;

            try {
                File.WriteAllBytes(Path.Combine(package.Location, package.customizedSettings.IconPath), File.ReadAllBytes(dialog.FileName));
                package.ReloadTile();
            }
            catch (Exception) {
                MessageBox.Show(Api.MSG_ER_FEATURE, Definitions.Error);
            }
        }

        /// <summary>
        /// Ons the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
            TitleTextBox.Text = ((AppLinkPackageTile)package.Tile).Title.Text;
            ArgsTextBox.Text = package.customizedSettings.Args ?? "";
        }

        /// <summary>
        /// Handles the TextChanged event of the TitleTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var changed = false;

            package.customizedSettings = package.GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                var tb = ((TextBox)sender);
                if (string.Compare(s.Title, tb.Text) == 0)
                    return;
                changed = true;
                s.Title = tb.Text;
            });

            if (changed)
                package.ReloadTile();
        }
    }
}
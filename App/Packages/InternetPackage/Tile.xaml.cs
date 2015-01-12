using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using libns;
using libns.Threading;
using Newgen;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NodeWebkit;

namespace InternetPackage {

    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Tile : Border {

        /// <summary>
        /// The hub
        /// </summary>
        private Hub hub;
        
        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

            InitializeComponent();

            MenuItemExternalCommand.Text = package.CustomizedSettings.ExternalBrowserCommand;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Load() {
            UpdateColor();
        }

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>...</remarks>
        public void Navigate(string url) {
            if (package.CustomizedSettings.RenderingMode == RenderingMode.External) {
                try {
                    var p = new Process();
                    p.StartInfo.Arguments = url;
                    p.StartInfo.FileName = package.CustomizedSettings.ExternalBrowserCommand;
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                }
                catch (Exception ex) {
                    Api.Logger.LogError(InternetPackage.Properties.Resources.UnableToLaunchExternalBrowser, ex);
                }
            }
            else if (package.CustomizedSettings.RenderingMode == RenderingMode.NW) {
                try {
                    NW.Run(Newgen.InternalHelper.GetHomePagePath(url));
                }
                catch (Exception ex) {
                    Api.Logger.LogError(InternetPackage.Properties.Resources.UnableToRunNW, ex);
                }
            }
            else {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    if (hub != null) {
                        if (hub.IsVisible)
                            hub.Activate();
                        hub.Navigate(url);
                    }
                    else {
                        hub = new Hub(package, url);
                        hub.AllowsTransparency = false;
                        hub.ShowDialog();
                        hub.Navigate(url);
                    }
                }));
            }
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {
        }

        /// <summary>
        /// Handles the <see cref="E:MenuItemCEFClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMenuItemExternalClick(object sender, System.Windows.RoutedEventArgs e) {
            try {
                package.CustomizedSettings.RenderingMode = RenderingMode.External;
                UpdateColor();
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:MenuItemIEClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMenuItemIEClick(object sender, System.Windows.RoutedEventArgs e) {
            try {
                package.CustomizedSettings.RenderingMode = RenderingMode.IE;
                UpdateColor();
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:MenuItemNWClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMenuItemNWClick(object sender, System.Windows.RoutedEventArgs e) {
            try {
                package.CustomizedSettings.RenderingMode = RenderingMode.NW;
                UpdateColor();
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Users the control mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Navigate(package.CustomizedSettings.LastSearchLocation);
        }

        /// <summary>
        /// Updates the color.
        /// </summary>
        /// <remarks>...</remarks>
        private void UpdateColor() {
            try {
                if (package.CustomizedSettings.RenderingMode == RenderingMode.External) {
                    this.MenuItemExternal.Background = new SolidColorBrush(Colors.DarkGray);
                    this.MenuItemNW.Background = new SolidColorBrush(Colors.Transparent);
                    this.MenuItemIE.Background = new SolidColorBrush(Colors.Transparent);
                }
                else if (package.CustomizedSettings.RenderingMode == RenderingMode.NW) {
                    this.MenuItemExternal.Background = new SolidColorBrush(Colors.Transparent);
                    this.MenuItemNW.Background = new SolidColorBrush(Colors.DarkGray);
                    this.MenuItemIE.Background = new SolidColorBrush(Colors.Transparent);
                }
                else {
                    this.MenuItemExternal.Background = new SolidColorBrush(Colors.Transparent);
                    this.MenuItemNW.Background = new SolidColorBrush(Colors.Transparent);
                    this.MenuItemIE.Background = new SolidColorBrush(Colors.DarkGray);
                }
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:MenuItemExternalCommandTextChanged" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMenuItemExternalCommandTextChanged(object sender, TextChangedEventArgs e) {
            var textBox = sender as TextBox;

            if (!string.IsNullOrWhiteSpace(textBox.Text))
                if (!textBox.Text.Equals(package.CustomizedSettings.ExternalBrowserCommand, StringComparison.InvariantCultureIgnoreCase))
                    package.CustomizedSettings.ExternalBrowserCommand = textBox.Text;
        }
    }
}
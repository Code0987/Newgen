using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;

namespace Newgen.Packages.Internet {

    /// <summary>
    /// Class InternetPackageTile.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class InternetPackageTile : Border {
        
        /// <summary>
        /// The hub
        /// </summary>
        private InternetPackageInternetBrowserHub hub;

        /// <summary>
        /// The package
        /// </summary>
        private InternetPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetPackageTile"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public InternetPackageTile(InternetPackage package) {
            this.package = package;

            InitializeComponent();
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
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                if (hub != null) {
                    if (hub.IsVisible)
                        hub.Activate();
                    hub.Navigate(url);
                }
                else {
                    hub = new InternetPackageInternetBrowserHub(package, url);
                    hub.AllowsTransparency = false;
                    hub.ShowDialog();
                    hub.Navigate(url);
                }
            }));
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
        private void OnMenuItemCEFClick(object sender, System.Windows.RoutedEventArgs e) {
            try {
                package.CustomizedSettings.RenderingMode = RenderingMode.CEF;
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
                if (package.CustomizedSettings.RenderingMode == RenderingMode.CEF) {
                    this.MenuItemCEF.Background = new SolidColorBrush(Colors.DarkGray);
                    this.MenuItemIE.Background = new SolidColorBrush(Colors.Transparent);
                }
                else {
                    this.MenuItemIE.Background = new SolidColorBrush(Colors.DarkGray);
                    this.MenuItemCEF.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            catch /* Eat */ { /* Tasty ? */ }
        }
    }
}
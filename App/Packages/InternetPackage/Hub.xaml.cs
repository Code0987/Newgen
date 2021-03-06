﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using libns.Threading;
using Newgen;

namespace InternetPackage {

    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : HubWindow {

        /// <summary>
        /// The browser
        /// </summary>
        private Browser browser;

        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hub"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="address">The address.</param>
        /// <remarks>...</remarks>
        public Hub(Package package, string address)
            : base() {
            this.package = package;

            InitializeComponent();

            browser = new IEBasedBrowser(new WebBrowser());

            SearchPanel.Children.Add(browser.Provider as UIElement);

            // Configure it
            (browser.Provider as UIElement).PreviewKeyDown += OnBrowserProviderPreviewKeyDown;

            browser.add_Error(OnBrowserError);
            browser.add_LoadCompleted(OnBrowserLoadCompleted);
            browser.add_LoadError(OnBrowserLoadError);

            ThreadingExtensions.Delay(() => Navigate(address), 1500, ThreadingOptions.OnDispatcherThread);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Hub"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~Hub() {
            (browser.Provider as UIElement).PreviewKeyDown -= OnBrowserProviderPreviewKeyDown;

            browser.remove_Error(OnBrowserError);
            browser.remove_LoadCompleted(OnBrowserLoadCompleted);
            browser.remove_LoadError(OnBrowserLoadError);
        }

        /// <summary>
        /// Navigates the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <remarks>...</remarks>
        public void Navigate(string address) {
            try {
                var uri = (Uri)null;
                Uri.TryCreate(URLBox.Text, UriKind.RelativeOrAbsolute, out uri);

                // Do absolute search
                if (uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                    browser.Navigate(uri.OriginalString);
                }
                // Do relative search
                else {
                    browser.Navigate(string.Format(package.CustomizedSettings.RelativeSearchAddressFormat, URLBox.Text));
                }

                LoadProgressBar.IsIndeterminate = true;
            }
            catch /* Eat */ {
                //Api.ShowErrorMessage("Error locating web resource ! The Address Url / Uri must be absolute !");
                OnHomeButtonMouseLeftButtonUp(null, null);
            }
        }
        /// <summary>
        /// Handles the <see cref="E:BackButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnBackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                browser.Back();
            }
            catch /* Eat */ {
            }
        }

        /// <summary>
        /// Called when [browser error].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <remarks>...</remarks>
        private void OnBrowserError(object sender, dynamic e) {
            OnHomeButtonMouseLeftButtonUp(null, null);
        }

        /// <summary>
        /// Called when [browser load completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The e.</param>
        /// <remarks>...</remarks>
        private void OnBrowserLoadCompleted(object sender, Uri uri, dynamic e) {
            this.InvokeAsyncThreadSafe(() => {
                try {
                    LoadProgressBar.IsIndeterminate = false;

                    URLBox.Text = uri.OriginalString;
                    URLBox.CaretIndex = URLBox.Text.Length;
                }
                catch /* Eat */ {
                }
            });
        }

        /// <summary>
        /// Called when [browser load error].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The e.</param>
        /// <remarks>...</remarks>
        private void OnBrowserLoadError(object sender, Uri uri, dynamic e) {
            OnHomeButtonMouseLeftButtonUp(null, null);
        }

        /// <summary>
        /// Handles the <see cref="E:BrowserProviderPreviewKeyDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnBrowserProviderPreviewKeyDown(object sender, KeyEventArgs e) {
            OnPreviewKeyUp(sender, e);
        }

        /// <summary>
        /// Handles the <see cref="E:CloseButtonMouseDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnCloseButtonMouseDown(object sender, MouseButtonEventArgs e) {
            try {
                package.CustomizedSettings.LastSearchLocation = URLBox.Text;

                Close();
            }
            catch /* Eat */ {
            }
        }

        /// <summary>
        /// Handles the <see cref="E:FwButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnFwButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                browser.Forward();
            }
            catch /* Eat */ {
            }
        }

        /// <summary>
        /// Handles the <see cref="E:HomeButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnHomeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var path = Newgen.InternalHelper.GetHomePagePath(Settings.DefaultLocation); // TODO: Move resource into this project.
            browser.Navigate(path, File.ReadAllText(path));
        }

        /// <summary>
        /// Handles the <see cref="E:PreviewKeyUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnPreviewKeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
            case Key.Escape:
                OnCloseButtonMouseDown(sender, null);
                break;

            case Key.BrowserBack:
            case Key.OemBackTab:
                OnBackButtonMouseLeftButtonUp(sender, null);
                break;

            case Key.BrowserForward:
                OnFwButtonMouseLeftButtonUp(sender, null);
                break;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:RefButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnRefButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                browser.Reload();
            }
            catch /* Eat */ {
            }
        }

        /// <summary>
        /// Handles the <see cref="E:URLBoxPreviewKeyDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnURLBoxPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Navigate(URLBox.Text);
            }
        }
    }
}
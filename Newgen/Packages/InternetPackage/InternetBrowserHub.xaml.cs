using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;
using Newgen;
using libns.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace InternetPackage {

    /// <summary>
    /// Interaction logic for InternetBrowserHub.xaml
    /// </summary>
    public partial class InternetBrowserHub : HubWindow {

        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// The browser
        /// </summary>
        private Browser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetBrowserHub"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="address">The address.</param>
        /// <remarks>...</remarks>
        public InternetBrowserHub(Package package, string address)
            : base() {
            this.package = package;

            InitializeComponent();

            if (package.CustomizedSettings.RenderingMode == RenderingMode.IE) {
                browser = new IEBasedBrowser(new WebBrowser());
            }
            else {
                browser = new CefBasedBrowser(new ChromiumWebBrowser());
            }

            SearchPanel.Children.Add(browser.Provider as UIElement);

            // Configure it
            (browser.Provider as UIElement).PreviewKeyDown += OnBrowserProviderPreviewKeyDown;

            browser.Error += OnBrowserError;
            browser.LoadCompleted += OnBrowserLoadCompleted;
            browser.LoadError += OnBrowserLoadError;

            ThreadingExtensions.Delay(() => Navigate(address), 1500, ThreadingOptions.OnDispatcherThread);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="InternetBrowserHub"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~InternetBrowserHub() {
            (browser.Provider as UIElement).PreviewKeyDown -= OnBrowserProviderPreviewKeyDown;

            browser.Error -= OnBrowserError;
            browser.LoadCompleted -= OnBrowserLoadCompleted;
            browser.LoadError -= OnBrowserLoadError;
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
                    browser.Navigate(uri.OriginalString, "<p>Loading ...</p>");
                }
                // Do relative search
                else {
                    browser.Navigate(string.Format(package.CustomizedSettings.RelativeSearchAddressFormat, URLBox.Text), "<p>Loading ...</p>");
                }

                LoadProgressBar.IsIndeterminate = true;
            }
            catch /* Eat */ {
                Api.ShowErrorMessage("Error locating web resource ! The Address Url / Uri must be absolute !");
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
                    URLBox.Text = uri.OriginalString;
                    URLBox.CaretIndex = URLBox.Text.Length;

                    LoadProgressBar.IsIndeterminate = false;
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

        }

        /// <summary>
        /// Handles the <see cref="E:BrowserProviderPreviewKeyDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnBrowserProviderPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                try {
                    package.CustomizedSettings.LastSearchLocation = URLBox.Text;

                    Close();
                }
                catch /* Eat */ {
                }
            }
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
            Navigate(Settings.DefaultLocation);
        }

        /// <summary>
        /// Handles the <see cref="E:PreviewKeyUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnPreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                try {
                    package.CustomizedSettings.LastSearchLocation = URLBox.Text;

                    Close();
                }
                catch /* Eat */ {
                }
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
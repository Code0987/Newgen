using System;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using CefSharp;
using CefSharp.Wpf;
using libns.Threading;
using Newgen;

namespace Newgen.Packages.Internet {

    /// <summary>
    /// Interaction logic for InternetPackageInternetBrowserHub.xaml
    /// </summary>
    public partial class InternetPackageInternetBrowserHub : HubWindow {

        /// <summary>
        /// The browser
        /// </summary>
        private Browser browser;

        /// <summary>
        /// The home page cache
        /// </summary>
        private string homePageCache;

        /// <summary>
        /// The package
        /// </summary>
        private InternetPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetPackageInternetBrowserHub"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="address">The address.</param>
        /// <remarks>...</remarks>
        public InternetPackageInternetBrowserHub(InternetPackage package, string address)
            : base() {
            this.package = package;

            InitializeComponent();

            if (package.CustomizedSettings.RenderingMode == RenderingMode.CEF)
                try {
                    CefBasedBrowser.CefStart();
                    browser = new CefBasedBrowser(new WebView());
                }
                catch /* Eat */ {
                    browser = new IEBasedBrowser(new WebBrowser());
                }
            else {
                browser = new IEBasedBrowser(new WebBrowser());
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
        /// Finalizes an instance of the <see cref="InternetPackageInternetBrowserHub"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~InternetPackageInternetBrowserHub() {
            if (package.CustomizedSettings.RenderingMode == RenderingMode.CEF)
                CefBasedBrowser.CefStop();

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
        /// Gets the content of the home page.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        private string GetHomePageContent() {
            if (string.IsNullOrWhiteSpace(homePageCache))
                try {
                    homePageCache = File.ReadAllText("Resources/HtmlApp/HomePage.html");
                    homePageCache = homePageCache
                        .Replace("{{WelcomeMessage}}", string.Format("Hello {0} !", Environment.UserName))
                        .Replace("{{InternetStatus}}", string.Format("{0}", NetworkInterface.GetIsNetworkAvailable() ? "Type your query / url below !" : "Turn on your `internet connection` to connect with world !"))
                        ;
                }
                catch /* Eat */ { homePageCache = string.Empty; }
            return homePageCache;
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
            browser.Navigate(InternetPackageSettings.DefaultLocation, GetHomePageContent());
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
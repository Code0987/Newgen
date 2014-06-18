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

namespace InternetPackage {

    /// <summary>
    /// Interaction logic for CefInternetBrowser.xaml
    /// </summary>
    public partial class CefInternetBrowser : HubWindow {

        private Package package;

        public CefInternetBrowser(Package package, string address)
            : base() {
            this.package = package;

            InitializeComponent();

            // Configure it
            WebBrowser.PreviewKeyDown += OnWebBrowserPreviewKeyDown;

            WebBrowser.ConsoleMessage += OnWebBrowserConsoleMessage;
            WebBrowser.FrameLoadEnd += OnWebBrowserFrameLoadEnd;
            WebBrowser.LoadError += OnWebBrowserLoadError;

            ThreadingExtensions.Delay(() => Navigate(address), 1500, ThreadingOptions.OnDispatcherThread);
        }

        ~CefInternetBrowser() {
            WebBrowser.PreviewKeyDown -= OnWebBrowserPreviewKeyDown;

            WebBrowser.ConsoleMessage -= OnWebBrowserConsoleMessage;
            WebBrowser.FrameLoadEnd -= OnWebBrowserFrameLoadEnd;
            WebBrowser.LoadError -= OnWebBrowserLoadError;
        }

        public void Navigate(string address) {
            try {
                var uri = (Uri)null;
                Uri.TryCreate(URLBox.Text, UriKind.RelativeOrAbsolute, out uri);

                // Do absolute search
                if (uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                    WebBrowser.LoadHtml("<p>Loading ...</p>", uri.OriginalString);
                }
                // Do relative search
                else {
                    WebBrowser.LoadHtml("<p>Loading ...</p>", string.Format(package.CustomizedSettings.RelativeSearchAddressFormat, URLBox.Text));
                }
            }
            catch /* Eat */ {
                Api.ShowErrorMessage("Error locating web resource ! The Address Url / Uri must be absolute !");
            }
        }

        private void OnBackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                WebBrowser.BackCommand.Execute(null);
            }
            catch /* Eat */ {
            }
        }

        private void OnWebBrowserConsoleMessage(object sender, ConsoleMessageEventArgs e) {
        }

        private void OnWebBrowserFrameLoadEnd(object sender, FrameLoadEndEventArgs url) {
            this.InvokeAsyncThreadSafe(() => {
                try {
                    URLBox.Text = url.Url;
                    URLBox.CaretIndex = URLBox.Text.Length;
                }
                catch /* Eat */ {
                }
            });
        }

        private void OnWebBrowserLoadError(object sender, LoadErrorEventArgs e) {

        }

        private void OnWebBrowserPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                try {
                    package.CustomizedSettings.LastSearchLocation = URLBox.Text;

                    Close();
                }
                catch /* Eat */ {
                }
            }
        }

        private void OnCloseButtonMouseDown(object sender, MouseButtonEventArgs e) {
            try {
                package.CustomizedSettings.LastSearchLocation = URLBox.Text;

                Close();
            }
            catch /* Eat */ {
            }
        }

        private void OnFwButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                WebBrowser.ForwardCommand.Execute(null);
            }
            catch /* Eat */ {
            }
        }

        private void OnHomeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Navigate(Settings.DefaultLocation);
        }

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

        private void OnRefButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                WebBrowser.ReloadCommand.Execute(null);
            }
            catch /* Eat */ {
            }
        }

        private void OnURLBoxPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Navigate(URLBox.Text);
            }
        }
    }
}
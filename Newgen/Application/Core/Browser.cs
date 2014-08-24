using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using libns;

namespace Newgen {

    /// <summary>
    /// Class Browser.
    /// </summary>
    /// <remarks>...</remarks>
    public abstract class Browser {

        /// <summary>
        /// Occurs when [error].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<object, dynamic> Error;

        /// <summary>
        /// Occurs when [load completed].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<object, Uri, dynamic> LoadCompleted;

        /// <summary>
        /// Occurs when [load error].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<object, Uri, dynamic> LoadError;

        /// <summary>
        /// Occurs when [navigated].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<object, Uri, dynamic> Navigated;

        /// <summary>
        /// Occurs when [navigating].
        /// </summary>
        /// <remarks>...</remarks>
        public event Action<object, Uri, dynamic> Navigating;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        /// <remarks>...</remarks>
        public abstract object Provider { get; }

        /// <summary>
        /// Backs this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public abstract void Back();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public abstract void Clear();

        /// <summary>
        /// Forwards this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public abstract void Forward();

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="html">The HTML.</param>
        /// <remarks>...</remarks>
        public abstract void Navigate(string url, string html = "");

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public abstract void Reload();

        /// <summary>
        /// Handles the <see cref="E:Error" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NavigationEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        protected void OnError(object sender, dynamic e) {
            if (Error != null)
                Error(sender, e);
        }

        /// <summary>
        /// Handles the <see cref="E:LoadCompleted" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The <see cref="NavigationEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        protected void OnLoadCompleted(object sender, Uri uri, dynamic e) {
            if (LoadCompleted != null)
                LoadCompleted(sender, uri, e);
        }

        /// <summary>
        /// Handles the <see cref="E:LoadError" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The <see cref="NavigationEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        protected void OnLoadError(object sender, Uri uri, dynamic e) {
            if (LoadError != null)
                LoadError(sender, uri, e);
        }

        /// <summary>
        /// Handles the <see cref="E:Navigated" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The <see cref="NavigationEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        protected void OnNavigated(object sender, Uri uri, dynamic e) {
            if (Navigated != null)
                Navigated(sender, uri, e);
        }

        /// <summary>
        /// Handles the <see cref="E:Navigating" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        protected void OnNavigating(object sender, Uri uri, dynamic e) {
            if (Navigating != null)
                Navigating(sender, uri, e);
        }
    }
    
    /* REVIEW: Implement CEF support if possible.
    /// <summary>
    /// Class CefBasedBrowser.
    /// </summary>
    /// <remarks>...</remarks>
    public class CefBasedBrowser : Browser {

        /// <summary>
        /// The provider
        /// </summary>
        private WebView provider;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        /// <remarks>...</remarks>
        public override object Provider {
            get {
                return provider;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CefBasedBrowser"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <remarks>...</remarks>
        public CefBasedBrowser(WebView provider) {
            this.provider = provider;

            provider.ConsoleMessage += OnProviderConsoleMessage;
            provider.LoadCompleted += OnProviderLoadCompleted;
            provider.LoadError += OnProviderLoadError;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CefBasedBrowser"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~CefBasedBrowser() {
            provider.ConsoleMessage -= OnProviderConsoleMessage;
            provider.LoadCompleted -= OnProviderLoadCompleted;
            provider.LoadError -= OnProviderLoadError;
        }

        /// <summary>
        /// Cefs the initialize.
        /// </summary>
        /// <remarks>...</remarks>
        public static void CefStart() {
            // Load CEF
            var settings = new CefSettings() {
                LogFile = "CEF.log",
                LogSeverity = LogSeverity.Verbose,
                PackLoadingDisabled = true
            };
            if (!Cef.Initialize(settings)) {
                Api.Logger.LogWarning("Loading CEF falied !");
            }
        }

        /// <summary>
        /// Cefs the stop.
        /// </summary>
        /// <remarks>...</remarks>
        public static void CefStop() {
            Cef.Shutdown();
        }
        /// <summary>
        /// Backs this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Back() {
            provider.ReloadCommand.Execute(null);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Clear() {
            Navigate("about:blank");
        }

        /// <summary>
        /// Forwards this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Forward() {
            provider.ForwardCommand.Execute(null);
        }

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="html">The HTML.</param>
        /// <remarks>...</remarks>
        public override void Navigate(string url, string html = "") {
            if (string.IsNullOrWhiteSpace(html))
                provider.Load(url);
            else
                provider.LoadHtml(html, url);
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Reload() {
            provider.ReloadCommand.Execute(null);
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderConsoleMessage" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConsoleMessageEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            OnError(sender, e);
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderFrameLoadEnd" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="url">The <see cref="FrameLoadEndEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderLoadCompleted(object sender, LoadCompletedEventArgs url) {
            OnLoadCompleted(sender, new Uri(url.Url), url);
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderLoadError" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LoadErrorEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderLoadError(string failedUrl, CefErrorCode errorCode, string errorText) {
            OnLoadError(null, new Uri(failedUrl), errorText);
        }
    } 
    */

    /// <summary>
    /// Class IEBasedBrowser.
    /// </summary>
    /// <remarks>...</remarks>
    public class IEBasedBrowser : Browser {

        /// <summary>
        /// Interface IOleServiceProvider
        /// </summary>
        /// <remarks>...</remarks>
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider {

            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        /// <summary>
        /// The provider
        /// </summary>
        private WebBrowser provider;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        /// <remarks>...</remarks>
        public override object Provider {
            get {
                return provider;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IEBasedBrowser"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <remarks>...</remarks>
        public IEBasedBrowser(WebBrowser provider) {
            this.provider = provider;

            provider.LoadCompleted += OnProviderLoadCompleted;
            provider.Navigated += OnProviderNavigated;
            provider.Navigating += OnProviderNavigating;
        }

        /// <summary>
        /// Sets the silent.
        /// </summary>
        /// <param name="browser">The browser.</param>
        /// <param name="silent">if set to <c>true</c> [silent].</param>
        /// <exception cref="System.ArgumentNullException">browser</exception>
        /// <remarks>...</remarks>
        public static void SetSilent(WebBrowser browser, bool silent) {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null) {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null) {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        /// <summary>
        /// Backs this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Back() {
            provider.GoBack();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Clear() {
            Navigate("about:blank");
        }

        /// <summary>
        /// Forwards this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Forward() {
            provider.GoForward();
        }

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="html">The HTML.</param>
        /// <remarks>...</remarks>
        public override void Navigate(string url, string html = "") {
            if (string.IsNullOrWhiteSpace(html))
                provider.Navigate(url);
            else
                provider.NavigateToString(html);
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Reload() {
            provider.Refresh();
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderLoadCompleted" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.NavigationEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            OnLoadCompleted(sender, e.Uri, e);
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderNavigated" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.NavigationEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            try {
                SetSilent(Provider as WebBrowser, true);
                OnNavigated(sender, e.Uri, e);
            }
            catch /* Eat */ {
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ProviderNavigating" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.NavigatingCancelEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnProviderNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {
            OnNavigating(sender, e.Uri, e);
        }
    }
}
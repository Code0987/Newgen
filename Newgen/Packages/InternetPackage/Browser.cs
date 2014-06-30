using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using CefSharp;
using CefSharp.Wpf;
using libns;

namespace InternetPackage {

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

    public class CefBasedBrowser : Browser {
        private ChromiumWebBrowser provider;

        public override object Provider {
            get {
                return provider;
            }
        }

        public CefBasedBrowser(ChromiumWebBrowser provider) {
            this.provider = provider;

            provider.ConsoleMessage += OnProviderConsoleMessage;
            provider.FrameLoadEnd += OnProviderFrameLoadEnd;
            provider.LoadError += OnProviderLoadError;
        }

        ~CefBasedBrowser() {
            provider.ConsoleMessage -= OnProviderConsoleMessage;
            provider.FrameLoadEnd -= OnProviderFrameLoadEnd;
            provider.LoadError -= OnProviderLoadError;
        }

        public override void Back() {
            provider.ReloadCommand.Execute(null);
        }

        public override void Clear() {
            Navigate("about:blank");
        }

        public override void Forward() {
            provider.ForwardCommand.Execute(null);
        }

        public override void Navigate(string url, string html = "") {
            if (string.IsNullOrWhiteSpace(html))
                provider.Load(url);
            else
                provider.LoadHtml(html, url);
        }

        public override void Reload() {
            provider.ReloadCommand.Execute(null);
        }

        private void OnProviderConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            OnError(sender, e);
        }

        private void OnProviderFrameLoadEnd(object sender, FrameLoadEndEventArgs url) {
            OnLoadCompleted(sender, new Uri(url.Url), url);
        }

        private void OnProviderLoadError(object sender, LoadErrorEventArgs e) {
            OnLoadError(sender, new Uri(e.FailedUrl), e);
        }
    }

    public class IEBasedBrowser : Browser {

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider {

            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private WebBrowser provider;

        public override object Provider {
            get {
                return provider;
            }
        }

        public IEBasedBrowser(WebBrowser provider) {
            this.provider = provider;

            provider.LoadCompleted += OnProviderLoadCompleted;
            provider.Navigated += OnProviderNavigated;
            provider.Navigating += OnProviderNavigating;
        }

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

        public override void Back() {
            provider.GoBack();
        }

        public override void Clear() {
            Navigate("about:blank");
        }

        public override void Forward() {
            provider.GoForward();
        }

        public override void Navigate(string url, string html = "") {
            provider.Navigate(url);
        }

        public override void Reload() {
            provider.Refresh();
        }

        private void OnProviderLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            OnLoadCompleted(sender, e.Uri , e);
        }

        private void OnProviderNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            try {
                SetSilent(Provider as WebBrowser, true);
                OnNavigated(sender, e.Uri, e);
            }
            catch /* Eat */ {
            }
        }

        private void OnProviderNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {
            OnNavigating(sender, e.Uri, e);
        }
    }
}
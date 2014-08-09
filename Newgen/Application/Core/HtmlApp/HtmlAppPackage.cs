using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EdgeJs;
using libns;

namespace Newgen.Packages.HtmlApp {

    /// <summary>
    /// Newgen Html app Package (Internal/Local).
    /// </summary>
    public class HtmlAppPackage : Package {

        /// <summary>
        /// The tile page
        /// </summary>
        internal static readonly string TilePage = "Tile.html";

        /// <summary>
        /// The meta resource identifier
        /// </summary>
        internal static readonly string MetaResourceIdentifier = "Api";

        /// <summary>
        /// The tile
        /// </summary>
        internal BrowserControl tile;

        /// <summary>
        /// The server task
        /// </summary>
        private static Func<object, Task<object>> serverTask;

        /// <summary>
        /// Gets or sets the server host.
        /// </summary>
        /// <value>The server host.</value>
        /// <remarks>...</remarks>
        public static string ServerHost { get; set; }

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile { get { return tile; } }

        /// <summary>
        /// Initializes static members of the <see cref="HtmlAppPackage"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        static HtmlAppPackage() {
            ServerHost = "localhost";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        private HtmlAppPackage(string location)
            : base(location) {
        }

        /// <summary>
        /// Closes the current hub.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>
        public static async Task<object> CloseCurrentHub() {
            // TODO: Impl. this.
            return null;
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFrom(string location) {
            var package = new HtmlAppPackage(location);
            if (package.Metadata == null || string.IsNullOrWhiteSpace(package.Metadata.Id))
                throw new Exception("Not a html app package !");
            if (!File.Exists(package.Settings.CreateAbsolutePathFor(TilePage)))
                throw new Exception("Not a valid html app package !");
            return package;
        }

        /// <summary>
        /// Gets the server URI for.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetServerUriFor(string path) {
            return string.Format(
                "http://{0}/{1}",
                ServerHost,
                path
                );
        }

        /// <summary>
        /// Gets the server URI for.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetServerUriOfPackageResourceFor(string packageId, string path) {
            return GetServerUriFor(string.Join(
                "/",
                "Packages",
                packageId,
                path
                ));
        }

        /// <summary>
        /// Gets the server URI of resource for.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetServerUriOfResourceFor(string path) {
            return GetServerUriFor(string.Join(
                "/",
                "Resources",
                path
                ));
        }

        /// <summary>
        /// Gets the server URI of meta resource for.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetServerUriOfMetaResourceFor(string path) {
            return GetServerUriFor(string.Join(
                "/",
                MetaResourceIdentifier,
                path
                ));
        }

        /// <summary>
        /// Runs the server.
        /// </summary>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>
        public static async Task<object> StartServer() {
            // Find port.
            var port = 44311 /* WebTools.GetFreeTcpPort("localhost") */;
            // Set host.
            ServerHost += ":" + port.ToString();
            // Create.
            var start = Edge.Func(
                string.Format(
                "return require('{0}')",
                "./../HtmlApp/Server.js"
                ));
            // Start.
            serverTask = (Func<object, Task<object>>)await start(new {
                port = port,
                host = ServerHost,
                location = App.Current.Location.ToUriPath(),

                metaKey = MetaResourceIdentifier,

                appCloseCurrentHub = (Func<object, Task<object>>)(async (message) => {
                    return await CloseCurrentHub();
                })
            });

#if DEBUG
            Debug.WriteLine((new WebClient()).DownloadString(new Uri(GetServerUriOfMetaResourceFor("Test"))));
#endif

            return null;
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>
        public static async Task<object> StopServer() {
            // Stop.
            return await serverTask(null);
        }

        /// <summary>
        /// Gets the server URI of package resource for.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public string GetServerUriOfPackageResourceFor(string path) {
            return GetServerUriOfPackageResourceFor(Metadata.Id, path);
        }

        /// <summary>
        /// Loads from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public override void Load() {
            base.Load();

            // Load UI
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                var wb = new WebBrowser();
                var b = new IEBasedBrowser(wb);
                tile = new BrowserControl(b);

                tile.Browser.Navigate(
                    GetServerUriOfPackageResourceFor(TilePage)
                    );
            }));
        }
    }
}
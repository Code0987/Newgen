﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EdgeJs;
using libns;
using libns.Media.Imaging;

namespace Newgen.Packages.HtmlApp {

    /// <summary>
    /// Newgen Html app Package (Internal/Local).
    /// </summary>
    public class HtmlAppPackage : Package {

        /// <summary>
        /// The meta resource identifier
        /// </summary>
        internal static readonly string MetaResourceIdentifier = "Api";

        /// <summary>
        /// The customized settings
        /// </summary>
        internal HtmlAppPackageCustomizedSettings customizedSettings;

        /// <summary>
        /// The tile
        /// </summary>
        internal BrowserControl tile;

        /// <summary>
        /// The tile image
        /// </summary>
        internal Image tileImage;

        /// <summary>
        /// The current hub
        /// </summary>
        internal static HubWindow currentHub;
        /// <summary>
        /// The hub
        /// </summary>
        internal static BrowserControl hub;

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
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>It defines the vertical span of tile.</remarks>
        public override int ColumnSpan {
            get {
                return customizedSettings.ColumnSpan;
            }
        }

        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>It defines the horizontal span of tile.</remarks>
        public override int RowSpan {
            get {
                return customizedSettings.RowSpan;
            }
        }

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile {
            get {
                if (tile != null)
                    return tile;
                return tileImage;
            }
        }

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
            // Pre-load settings for html apps.
            // This prevent abnormal behavious as Row/Column span for html apps are included in settings file
            // while app expects them as compiled defaults.
            LoadSettings(true);

            // Update html app settings.
            customizedSettings = Settings.Customize<HtmlAppPackageCustomizedSettings>(s => {
            });
        }

        /// <summary>
        /// Closes the current hub.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>        
        public static async Task<object> CloseCurrentHub() {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() => {

                if (currentHub == null)
                    return;

                currentHub.Close();

                currentHub = null;

            }));

            return null;
        }

        /// <summary>
        /// Sets the current hub.
        /// </summary>
        /// <returns>Task&lt;System.Object&gt;.</returns>
        /// <remarks>...</remarks>
        public static async Task<object> SetCurrentHub(dynamic input) {
            await Application.Current.Dispatcher.BeginInvoke(new Action(async () => {

                if (currentHub != null)
                    await CloseCurrentHub();

                var wb = new WebBrowser();
                var b = new IEBasedBrowser(wb);
                hub = new BrowserControl(b);

                hub.Browser.Navigate(
                    input as string
                    );

                currentHub = new HubWindow() {
                    Content = hub
                };

                currentHub.Show();

            }));

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
            if (
                !File.Exists(package.Settings.CreateAbsolutePathFor(package.customizedSettings.TilePage))
                &&
                !File.Exists(package.Settings.CreateAbsolutePathFor(package.customizedSettings.TilePageImage))
                )
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
                }),
                appSetCurrentHub = (Func<object, Task<object>>)(async (message) => {
                    return await SetCurrentHub(message);
                })
            });

#if DEBUG
            // TEST: Detect edge issues here.
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
            try {
                return await serverTask(null);
            }
            catch { return null; }
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
                tileImage = new Image() {
                    Source = Settings.CreateAbsolutePathFor(customizedSettings.TilePageImage).ToBitmapSource(),
                    Stretch = Stretch.Fill,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                if (File.Exists(Settings.CreateAbsolutePathFor(customizedSettings.TilePage))) {
                    var wb = new WebBrowser();
                    var b = new IEBasedBrowser(wb);
                    tile = new BrowserControl(b);

                    tile.Browser.Navigate(
                        GetServerUriOfPackageResourceFor(customizedSettings.TilePage)
                        );
                }
            }));
        }
    }

    /// <summary>
    /// Class HtmlAppPackageCustomizedSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class HtmlAppPackageCustomizedSettings {

        /// <summary>
        /// The column span
        /// </summary>
        public int ColumnSpan = 2;

        /// <summary>
        /// The row span
        /// </summary>
        public int RowSpan = 2;

        /// <summary>
        /// The tile page
        /// </summary>
        public string TilePage = "Tile.html";

        /// <summary>
        /// The tile page image
        /// </summary>
        public string TilePageImage = "Tile.png";
    }
}
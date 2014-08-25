using System;
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
using Microsoft.Win32;
using NodeWebkit;

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
        /// Sets/removes the registry hacks.
        /// </summary>
        /// <remarks>...</remarks>
        public static void RegistryHacks(bool clear = false) {
            var app = new FileInfo(typeof(HtmlAppPackage).Assembly.Location).Name;

            var regkey = Registry.LocalMachine.OpenSubKey(
                "SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION",
                true
                );
            if (regkey.GetValue(app, null) == null) {
                regkey.SetValue(app, 1);
            }
            else if (clear) {
                regkey.DeleteValue(app);
            }
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

                metaKey = MetaResourceIdentifier
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

                tileImage.MouseLeftButtonUp += tileImage_MouseLeftButtonUp;

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

        /// <summary>
        /// Called whenever the package is un-loaded from user context.
        /// </summary>
        /// <remarks>Do all finalization steps here ! (e.g. saving settings)</remarks>
        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tileImage.MouseLeftButtonUp -= tileImage_MouseLeftButtonUp;
            }));

            base.Unload();
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event of the tileImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void tileImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NW.Run(GetServerUriOfPackageResourceFor(customizedSettings.HubPage));
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
        /// The hub page
        /// </summary>
        public string HubPage = "Hub.html";

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
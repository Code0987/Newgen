using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using libns;
using libns.Media.Imaging;
using Microsoft.Win32;
using NodeWebkit;
using PackageManager;

namespace Newgen.HtmlApp {

    /// <summary>
    /// Newgen Html app Package (Internal/Local).
    /// </summary>
    public class HtmlAppPackage : NewgenPackage {

        /// <summary>
        /// The meta resource identifier
        /// </summary>
        internal static readonly string MetaResourceIdentifier = "Api";

        /// <summary>
        /// The package type identifier
        /// </summary>
        internal static readonly string PackageTypeId = "NewgenHtmlApp";

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
        /// Initializes a new instance of the <see cref="NewgenPackage" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="settingsStorage">The settings storage.</param>
        /// <remarks>...</remarks>
        public HtmlAppPackage(string location, IPackageSettingsStorage settingsStorage)
            : base(location, settingsStorage) {
            // Package type marker.
            SettingsStorage.Put(this, PackageTypeId, PackageTypeId);

            // Pre-load settings for html apps.
            // This prevent abnormal behavious as Row/Column span for html apps are included in settings file
            // while app expects them as compiled defaults.
            Load();

            // Update html app settings.
            customizedSettings = GetSettings().Customize<HtmlAppPackageCustomizedSettings>(s => {
            });

            if (
                !File.Exists(Path.Combine(Location, customizedSettings.TilePage))
                &&
                !File.Exists(Path.Combine(Location, customizedSettings.TilePageImage))
            )
                throw new Exception("Not a valid html app package !");
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
        /// Gets the server URI of package resource for.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public string GetServerUriOfPackageResourceFor(string path) {
            return GetServerUriOfPackageResourceFor(this.GetId(), path);
        }

        /// <summary>
        /// Called when [start].
        /// </summary>
        /// <remarks>...</remarks>
        protected override void OnStart() {
            // Load UI
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tileImage = new Image() {
                    Source = Path.Combine(Location, customizedSettings.TilePageImage).ToBitmapSource(),
                    Stretch = Stretch.Fill,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                tileImage.MouseLeftButtonUp += tileImage_MouseLeftButtonUp;

                if (File.Exists(Path.Combine(Location, customizedSettings.TilePage))) {
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
        /// Called when [stop].
        /// </summary>
        /// <remarks>...</remarks>
        protected override void OnStop() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tileImage.MouseLeftButtonUp -= tileImage_MouseLeftButtonUp;
            }));
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
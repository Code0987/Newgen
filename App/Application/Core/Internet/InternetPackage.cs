using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using libns;
using NS.Web;

namespace Newgen.Packages.Internet {

    /// <summary>
    /// Enum RenderingMode
    /// </summary>
    /// <remarks>...</remarks>
    public enum RenderingMode {

        /// <summary>
        /// The ie
        /// </summary>
        IE = 0x0E,

        /// <summary>
        /// The nw
        /// </summary>
        NW = 0x11,

        /// <summary>
        /// The cef
        /// </summary>
        External = 0xFF
    }

    /// <summary>
    /// Class Package.
    /// </summary>
    /// <remarks>...</remarks>
    public class InternetPackage : Package {

        /// <summary>
        /// The package identifier
        /// </summary>
        internal static readonly string PackageId = "Internet";

        /// <summary>
        /// The customized settings
        /// </summary>
        internal InternetPackageSettings CustomizedSettings;

        /// <summary>
        /// The tile
        /// </summary>
        internal InternetPackageTile tile;

        /// <summary>
        /// Returns widget control
        /// </summary>
        public override FrameworkElement Tile {
            get { return tile; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetPackage" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        public InternetPackage(string location)
            : base(location) {
            // Create metadata
            Metadata = new PackageMetadata() {
                Id = PackageId,
                Version = typeof(InternetPackage).Assembly.GetTimestamp(),
                Author = Environment.UserName,
                License = string.Format("© 2014 NS, {0} for Newgen", PackageId),
                Name = PackageId,
                AuthorEMailAddress = string.Format(WebShared.Newgen_Api_AuthorEMailFormat, PackageId),
                AuthorWebsite = string.Format(WebShared.Newgen_Api_AuthorWebsiteUriFormat, PackageId),
                Description = "Internet package (Internal/Local)."
            };
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package Create() {
            return new InternetPackage(PackageManager.Current.CreateAbsolutePathFor(PackageId));
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFrom(string location) {
            var package = new InternetPackage(location);
            if (package.Metadata == null || !package.Metadata.Id.Equals(PackageId))
                throw new Exception("Not a Internet package !");
            return package;
        }

        /// <summary>
        /// Package initialization (e.g.variables initialization, reading settings, loading resources) must be here.
        /// This method calls when user clicks on widget icon in Newgen menu or at Newgen launch if widget was added earlier
        /// </summary>
        public override void Load() {
            base.Load();

            CustomizedSettings = Settings.Customize<InternetPackageSettings>(s => {
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new InternetPackageTile(this);
                tile.Load();
            }));
        }

        /// <summary>
        /// Called whenever a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        public override void OnMessageReceived(EMessage message) {
            switch (message.Key) {
            default:
                tile.Navigate(message.Value);
                break;
            }
        }

        /// <summary>
        /// Releasing resources and settings saving must be here.
        /// This method calls when user removes widget from Newgen grid or when user closes Newgen if widget was loaded earlier
        /// </summary>
        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));

            Settings.Customize(CustomizedSettings);

            base.Unload();
        }

        /// <summary>
        /// Gets the default page URL.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetHomePagePath(string url) {
            var content = File.ReadAllText(Path.Combine(App.Current.Location, "Resources/HtmlApp/HomePage-template.html"));
            try {
                content = content
                    .Replace("{{WelcomeMessage}}", string.Format("Hello {0} !", Environment.UserName))
                    .Replace("{{InternetStatus}}", string.Format("{0}", NetworkInterface.GetIsNetworkAvailable() ? "Type your query / url below !" : "Turn on your `internet connection` to connect with world !"))
                    .Replace("{{Url}}", url)
                    ;
            }
            catch /* Eat */ { /* Tasty ? */ }

            var path = Path.Combine(App.Current.Location, "Resources/HtmlApp/HomePage.html");
            File.WriteAllText(path, content);

            return path;
        }
    }

    /// <summary>
    /// Class Settings.
    /// </summary>
    /// <remarks>...</remarks>
    public class InternetPackageSettings {

        /// <summary>
        /// The default location
        /// </summary>
        internal const string DefaultLocation = "about:blank";

        /// <summary>
        /// Gets or sets the external browser command.
        /// </summary>
        /// <value>The external browser command.</value>
        /// <remarks>...</remarks>
        public string ExternalBrowserCommand { get; set; }

        /// <summary>
        /// Gets or sets the last search location.
        /// </summary>
        /// <value>The last search location.</value>
        /// <remarks>...</remarks>
        public string LastSearchLocation { get; set; }

        /// <summary>
        /// Gets or sets the relative search address format.
        /// </summary>
        /// <value>The relative search address format.</value>
        /// <remarks>...</remarks>
        public string RelativeSearchAddressFormat { get; set; }

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        /// <value>The rendering mode.</value>
        /// <remarks>...</remarks>
        public RenderingMode RenderingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetPackageSettings"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public InternetPackageSettings() {
            RelativeSearchAddressFormat = "http://www.google.com/search?q={0}";
            LastSearchLocation = InternetPackageSettings.DefaultLocation;
            RenderingMode = RenderingMode.IE;
            ExternalBrowserCommand = "Enter here ...";
        }
    }
}
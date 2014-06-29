using System;
using System.Windows;
using CefSharp;
using Newgen;
using NS.Web;

namespace InternetPackage {
    /// <summary>
    /// Class Package.
    /// </summary>
    /// <remarks>...</remarks>
    public class Package : Newgen.Packages.Package {
        /// <summary>
        /// The package identifier
        /// </summary>
        internal const string PackageId = "Internet";

        /// <summary>
        /// The customized settings
        /// </summary>
        internal Settings CustomizedSettings;

        private Tile tile;

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        public Package(string location)
            : base(location) {
            Metadata = WebShared.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Internet package for Newgen. Provides Internet Explorer & Webkit a.k.a. Chrome support into Newgen."
                );
        }

        /// <summary>
        /// Returns widget control
        /// </summary>
        public override FrameworkElement Tile {
            get { return tile; }
        }


        /// <summary>
        /// Package initialization (e.g.variables initialization, reading settings, loading resources) must be here.
        /// This method calls when user clicks on widget icon in Newgen menu or at Newgen launch if widget was added earlier
        /// </summary>
        public override void Load() {
            base.Load();

            CustomizedSettings = Settings.Customize<Settings>(s => {
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);
                tile.Load();
            }));

            // CEF
            if (Cef.Initialize(new CefSharp.CefSettings {
                PackLoadingDisabled = true,
                LogFile = Settings.CreateAbsolutePathFor("CEF.log"),
                LogSeverity = LogSeverity.Verbose
            })) {
                // Init
            }
        }

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

            Cef.Shutdown();

            base.Unload();
        }
    }
}
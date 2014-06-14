using System;
using System.Windows;
using Newgen;
using NS.Web;

namespace InternetPackage {
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Internet";

        internal Settings CustomizedSettings;

        private Tile tile;

        public Package(string location)
            : base(location) {
            Metadata = WebShared.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Internet package for Newgen. Provides Internet Explorer & Webkit a.k.a. Chrome support into Newgen."
                );
        }

        /// <summary>
        /// Path to widget icon. Return null if there is no icon.
        /// </summary>
        public override Uri IconPath {
            get { return new Uri("pack://application:,,,/" + PackageId + ";component/Resources/icon.png", UriKind.RelativeOrAbsolute); }
        }
        /// <summary>
        /// Returns widget control
        /// </summary>
        public override FrameworkElement Tile {
            get { return tile; }
        }


        /// <summary>
        /// Widget initialization (e.g.variables initialization, reading settings, loading resources) must be here.
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

            base.Unload();
        }
    }
}
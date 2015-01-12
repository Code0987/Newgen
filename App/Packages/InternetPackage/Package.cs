using System;
using System.Windows;
using Newgen;
using NS.Web;

namespace InternetPackage
{
    /// <summary>
    /// Package
    /// </summary>
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Internet";

        internal Settings CustomizedSettings;

        private Tile tile;
        
        public override FrameworkElement Tile
        {
            get { return tile; }
        }

        public Package(string location)
            : base(location) {
            Metadata = Newgen.Api.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Internet package for Newgen. Provides minimal web browser features."
                );
        }

        public override void Load() {
            base.Load();

            CustomizedSettings = Settings.Customize<Settings>(s => {
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);
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

        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));

            Settings.Customize(CustomizedSettings);

            base.Unload();
        }
    }
}
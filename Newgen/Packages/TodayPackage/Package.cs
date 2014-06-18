using System;
using System.Windows;
using NS.Web;

namespace TodayPackage
{
    /// <summary>
    /// Package
    /// </summary>
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Today";

        internal Settings CustomizedSettings;

        private Tile tile;

        public override int ColumnSpan {
            get { return 5; }
        }

        public override int RowSpan {
            get {
                return 2;
            }
        }

        public override FrameworkElement Tile
        {
            get { return tile; }
        }

        public Package(string location)
            : base(location) {
            Metadata = WebShared.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Today package for Newgen. Provides latest quotes, life hacks, top news, etc."
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

        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));

            Settings.Customize(CustomizedSettings);

            base.Unload();
        }
    }
}
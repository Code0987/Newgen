using System;
using System.Windows;
using NS.Web;

namespace BackgroundPackage
{
    /// <summary>
    /// Package
    /// </summary>
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Background";

        internal Settings CustomizedSettings;

        private Tile tile;

        public override int ColumnSpan {
            get { return 1; }
        }

        public override int RowSpan {
            get {
                return 1;
            }
        }

        public override FrameworkElement Tile
        {
            get { return tile; }
        }

        public Package(string location)
            : base(location) {
            Metadata = Newgen.Api.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Background package for Newgen. Provides advanced background suppliments like slide show, video, ..."
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
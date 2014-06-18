using System;
using System.Windows;
using Newgen;

namespace ClockPackage
{
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Clock";
        
        private Tile tile;        

        public override FrameworkElement Tile
        {
            get { return tile; }
        }

        

        public override int ColumnSpan
        {
            get { return 3; }
        }

        public override int RowSpan {
            get {
                return 2;
            }
        }

        public Package(string location)
            : base(location) {
            Metadata = WebShared.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Clock package for Newgen. Provides latest local/global time information."
                );
        }

        public override void Load() {
            base.Load();

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);
                tile.Load();
            }));
        }

        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));

            base.Unload();
        }
    }
}
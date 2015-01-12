using System;
using System.Windows;
using Newgen;
using NS.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using libns;
using libns.Media.Imaging;
using libns.Native;
using NS.Web;

namespace NotificationsPackage
{
    /// <summary>
    /// Package
    /// </summary>
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Notifications";

        internal Settings CustomizedSettings;

        private Tile tile;
        
        public override FrameworkElement Tile
        {
            get { return tile; }
        }

        /// <summary>
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>...</remarks>
        public override int ColumnSpan {
            get {
                return 2;
            }
        }

        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>...</remarks>
        public override int RowSpan {
            get {
                return 2;
            }
        }

        public Package(string location)
            : base(location) {
            Metadata = Newgen.Api.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Notifications package for Newgen. Shows notification right in font of your eyes across Newgen."
                );
        }

        public override void Load() {
            base.Load();

            CustomizedSettings = Settings.Customize<Settings>(s => {
            });

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);

                App.Current.AddNotificationManager(tile.nm);
                Api.Logger.Add(tile.logger);
            }));
        }
        
        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                App.Current.RemoveNotificationManager(tile.nm);
                Api.Logger.Remove(tile.logger);
            }));

            Settings.Customize(CustomizedSettings);

            base.Unload();
        }
    }
}
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

namespace Newgen.Packages.Notifications {

    /// <summary>
    /// Newgen Notifications Package (Internal/Local).
    /// </summary>
    public class NotificationsPackage : Package {
        internal NotificationsPackageTile tile;

        internal static readonly string PackageId = "Notifications";

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile { get { return tile; } }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsPackage" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        private NotificationsPackage(string location)
            : base(location) {
            // Create metadata
            Metadata = new PackageMetadata() {
                Id = PackageId,
                Version = typeof(NotificationsPackage).Assembly.GetTimestamp(),
                Author = Environment.UserName,
                License = string.Format("© 2014 NS, {0} for Newgen", PackageId),
                Name = PackageId,
                AuthorEMailAddress = string.Format("Newgen+Support+{0}", PackageId),
                AuthorWebsite = string.Format("{0}/Apps/NewgenStore?Package={1}", WebShared.MainSiteUri, PackageId),
                Description = "Notification package (Internal/Local) shows notification right infont of your eyes across Newgen."
            };
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFrom(string location) {
            var package = new NotificationsPackage(location);
            if (!package.Metadata.Id.Equals(PackageId))
                throw new Exception("Not a notifications package !");
            return package;
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package Create() {
            return new NotificationsPackage(PackageManager.Current.CreateAbsolutePathFor(PackageId));
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Load() {
            base.Load();

            // Load UI
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new NotificationsPackageTile();

                App.Current.AddNotificationManager(tile.nm);
            }));
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                App.Current.RemoveNotificationManager(tile.nm);
            }));

            base.Unload();
        }
    }
}
﻿using System;
using System.Windows;
using Newgen;
using NS.Web;

namespace DesktopPackage
{
    /// <summary>
    /// Class Package.
    /// </summary>
    /// <remarks>...</remarks>
    public class Package : Newgen.Packages.Package {
        /// <summary>
        /// The package identifier
        /// </summary>
        internal const string PackageId = "Desktop";

        /// <summary>
        /// The tile
        /// </summary>
        private Tile tile;

        /// <summary>
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>It defines the vertical span of tile.</remarks>
        public override int ColumnSpan {
            get { return 2; }
        }

        /// <summary>
        /// Gets the package tile control.
        /// </summary>
        /// <value>The tile.</value>
        /// <remarks>Return a valid XAML control for display.</remarks>
        public override FrameworkElement Tile {
            get { return tile; }
        }

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
                "Desktop package for Newgen. Allows you to quickly switch to real windows desktop."
                );
        }
        /// <summary>
        /// Called whenever the package is loaded into user context.
        /// </summary>
        /// <remarks>Do all loading steps here ! (e.g. loading settings, preparing UI)</remarks>
        public override void Load() {
            base.Load();

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);
                tile.Load();
            }));

        }

        /// <summary>
        /// Called whenever the package is un-loaded from user context.
        /// </summary>
        /// <remarks>Do all finalization steps here ! (e.g. saving settings)</remarks>
        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));
            
            base.Unload();
        }
    }
}
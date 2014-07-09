using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using libns;
using libns.Media.Imaging;
using libns.Native;

namespace Newgen.Packages.HtmlApp {

    /// <summary>
    /// Newgen Html app Package (Internal/Local).
    /// </summary>
    /// <remarks>
    /// <![CDATA[
    ///     Html App Package
    ///     ---- --- -------
    ///     
    ///     Structure
    ///     ---------
    ///     
    ///     Root 
    ///     |
    ///     \   Tile.html
    ///     
    /// 
    ///     Javascript Api
    ///     ---------- ---
    ///     
    ///     $Newgen. or $.
    ///         
    ///     Protocol for resources -
    ///         $://<Package Id>/...
    ///     
    /// ]]>
    /// </remarks>
    public class HtmlAppPackage : Package {
        internal BrowserControl tile;

        internal static readonly string TilePage = "Tile.html";

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile { get { return tile; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        private HtmlAppPackage(string location)
            : base(location) {
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFrom(string location) {
            var package = new HtmlAppPackage(location);
            if (package.Metadata == null || !string.IsNullOrWhiteSpace(package.Metadata.Id))
                throw new Exception("Not a html app package !");
            if (!File.Exists(package.Settings.CreateAbsolutePathFor(TilePage)))
                throw new Exception("Not a valid html app package !");
            return package;
        }

        /// <summary>
        /// Loads from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public override void Load() {
            base.Load();

            // Load UI
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                var wb = new WebBrowser();
                var b = new IEBasedBrowser(wb);
                tile = new BrowserControl(b);                               
                
                tile.Browser.Navigate(Settings.CreateAbsolutePathFor(TilePage));
            }));
        }
    }
}
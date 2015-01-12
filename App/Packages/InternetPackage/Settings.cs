using System;
using System.Collections.Generic;
using Newgen;

namespace InternetPackage {

    /// <summary>
    /// Enum RenderingMode
    /// </summary>
    /// <remarks>...</remarks>
    public enum RenderingMode {

        /// <summary>
        /// The ie
        /// </summary>
        IE = 0x0E,

        /// <summary>
        /// The nw
        /// </summary>
        NW = 0x11,

        /// <summary>
        /// The cef
        /// </summary>
        External = 0xFF
    }

    /// <summary>
    /// Settings
    /// </summary>
    public class Settings {

        /// <summary>
        /// The default location
        /// </summary>
        internal const string DefaultLocation = "about:blank";

        /// <summary>
        /// Gets or sets the external browser command.
        /// </summary>
        /// <value>The external browser command.</value>
        /// <remarks>...</remarks>
        public string ExternalBrowserCommand { get; set; }

        /// <summary>
        /// Gets or sets the last search location.
        /// </summary>
        /// <value>The last search location.</value>
        /// <remarks>...</remarks>
        public string LastSearchLocation { get; set; }

        /// <summary>
        /// Gets or sets the relative search address format.
        /// </summary>
        /// <value>The relative search address format.</value>
        /// <remarks>...</remarks>
        public string RelativeSearchAddressFormat { get; set; }

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        /// <value>The rendering mode.</value>
        /// <remarks>...</remarks>
        public RenderingMode RenderingMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetPackageSettings"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public Settings() {
            RelativeSearchAddressFormat = "http://www.google.com/search?q={0}";
            LastSearchLocation = DefaultLocation;
            RenderingMode = RenderingMode.IE;
            ExternalBrowserCommand = "Enter here ...";
        }
    }
}

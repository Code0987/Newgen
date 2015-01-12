using System;
using System.Collections.Generic;
using Newgen;

namespace BackgroundPackage {
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings {

        /// <summary>
        /// Gets or sets the slide show images.
        /// </summary>
        /// <value>The slide show images.</value>
        /// <remarks>...</remarks>
        public List<string> SlideShowImages { get; set; }

        /// <summary>
        /// Gets or sets the slide show time.
        /// </summary>
        /// <value>The slide show time.</value>
        /// <remarks>...</remarks>
        public int SlideShowTime { get; set; }

        /// <summary>
        /// Initializes settings.
        /// </summary>
        public Settings() {
            SlideShowImages = new List<string>();
            SlideShowTime = 30;
        }
    }
}

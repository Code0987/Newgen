using System;
using System.Collections.Generic;
using Newgen.Base;

namespace Quotes
{
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings : XmlSerializable
    {
        public List<string> QuotesList { get; set; }

        public DateTime LastQuoteDownloadTime { get; set; }

        /// <summary>
        /// Initializes settings.
        /// </summary>
        public Settings()
        {
            this.QuotesList = new List<string>();
            this.LastQuoteDownloadTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        }
    }
}
using System;
using System.Collections.Generic;
using Newgen;

namespace TodayPackage {
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings {
        internal const char ListItemPartsDelimiter = '@';

        public List<string> Feeds { get; set; }

        public int FeedsInterval { get; set; }

        public List<string> List { get; set; }

        public int ListInterval { get; set; }

        /// <summary>
        /// Initializes settings.
        /// </summary>
        public Settings() {
            Feeds = new List<string>() {
                "https://news.google.com/?output=rss",
                "http://feeds.feedburner.com/homeinsteaders/KhCJ",
                "http://feeds.feedburner.com/quotationspage/qotd",
                "http://www.lifehack.org/feed",
                "http://lifehacksdaily.com/feed/"
            };
            List = new List<string>() { };
            FeedsInterval = 35 * 60 * 1000;
            ListInterval = 3 * 60 * 1000;
        }
    }
}

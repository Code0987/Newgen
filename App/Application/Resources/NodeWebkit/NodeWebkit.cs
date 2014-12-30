using System;
using System.Diagnostics;
using System.IO;
using libns;
using Newgen;
using Newgen.Resources;

namespace NodeWebkit {

    /// <summary>
    /// Class NodeWebkit.
    /// </summary>
    /// <remarks>...</remarks>
    public class NW {

        /// <summary>
        /// The location
        /// </summary>
        private static string location;

        /// <summary>
        /// Gets the current process.
        /// </summary>
        /// <value>The current process.</value>
        /// <remarks>...</remarks>
        public static Process CurrentProcess { get; internal set; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        /// <remarks>...</remarks>
        public static string Location {
            get {
                if (string.IsNullOrWhiteSpace(location))
                    location = Path.Combine(App.Current.Location, "Resources\\NodeWebkit");

                return location;
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="NW"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        static NW() {
            try {
                var package_json_content = File.ReadAllText(Path.Combine(Location, "package-template.json"));

                package_json_content = package_json_content
                    .Replace("{{Width}}", Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth).ToString())
                    .Replace("{{Height}}", Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight).ToString())
                    ;

                File.WriteAllText(Path.Combine(Location, "package.json"), package_json_content);
            }
            catch /* Eat */ { /* Tasty ? */ }

            try {
                var content = File.ReadAllText(Path.Combine(Location, "index-template.html"));

                content = content
                    .Replace("{{NWRunHelpTitle}}", Definitions.NWRunHelpTitle)
                    .Replace("{{NWRunHelpContent}}", Definitions.NWRunHelpContent)
                    ;

                File.WriteAllText(Path.Combine(Location, "index.html"), content);
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Runs the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>...</remarks>
        public static void Run(string url = null) {
            try {
                if (CurrentProcess != null) {
                    if (!CurrentProcess.HasExited)
                        CurrentProcess.Kill();
                    CurrentProcess = null;
                }
            }
            catch /* Eat */ { /* Tasty ? */ }

            try {
                var p = new Process();
                if (string.IsNullOrWhiteSpace(url))
                    p.StartInfo.Arguments = string.Format("--data-path=\"{0}\"", Api.CacheRoot);
                else
                    p.StartInfo.Arguments = string.Format("--data-path=\"{0}\" --url=\"{1}\"", Api.CacheRoot, url);
                p.StartInfo.FileName = Path.Combine(Location, "nw.exe");
                p.StartInfo.UseShellExecute = true;
                p.Start();

                App.Current.ShowNotification(new Notification(Definitions.NWRunHelpTitle, Definitions.NWRunHelpContent));

                CurrentProcess = p;
            }
            catch /* Eat */ { /* Tasty ? */ }
        }
    }
}
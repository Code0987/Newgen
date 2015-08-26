using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using libns;
using libns.Applied;
using libns.Media.Imaging;
using libns.Native;
using libns.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Packages;
using NS.Web;

namespace Newgen {

    /// <summary>
    /// Class InternalHelper.
    /// </summary>
    /// <remarks>...</remarks>
    public static class InternalHelper {

        /// <summary>
        /// The startup time
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now;

        /// <summary>
        /// The feeds aggregator
        /// </summary>
        public static FeedsAggregator FeedsAggregator;

#if !DEBUG

        /// <summary>
        /// The tracker
        /// </summary>
        internal static ExtendedTracker tracker;

#endif

        /// <summary>
        /// Initializes static members of the <see cref="InternalHelper"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        static InternalHelper() {
            FeedsAggregator = new FeedsAggregator(
                App.Current.Guid,
                new FileDataStore() { BaseDirectory = Api.CacheRoot },
                new List<Uri>() { GetUpdatesUrlFor("Feeds.RSS20.xml") }
                );
#if DEBUG
            FeedsAggregator.ExceptionOccurred += (o, e) => { throw e; };
#endif

#if !DEBUG

            // Tracker
            tracker = new ExtendedTracker(
                NS.Web.WebShared.GoogleAnalytics_NSApps_net_Id,
                NS.Web.WebShared.GoogleAnalytics_NSApps_net_Domain
                );

#endif
        }

        /// <summary>
        /// Fors the each HWND.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="action">The action.</param>
        /// <remarks>...</remarks>
        public static void ForEachHWND(this Window w, Action<IntPtr, string> action) {
            WinAPI.ForEachVisibleWindow(
                ((HwndSource)HwndSource.FromVisual(w)).Handle,
                (current, text) => {
                    if (string.IsNullOrWhiteSpace(text))
                        return;

                    try {
                        action(current, text);
                    }
                    catch /* Eat */ { }
                });
        }

        /// <summary>
        /// Gets all icons.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Tuple&lt;ImageSource, System.Int32&gt;[].</returns>
        /// <remarks>...</remarks>
        public static Tuple<ImageSource, int>[] GetAllIcons(this string path) {
            var icons = new List<Tuple<ImageSource, int>>();
            var count = 0;
            while (true) {
                var icon = WinAPI.ShellThumbnail.ExtractIconFromExe(path, true, count);

                if (icon == null)
                    break;

                icons.Add(Tuple.Create(
                    icon.ToBitmap().ToBitmapSource() as ImageSource,
                    count
                    ));

                count++;
            }
            return icons.ToArray();
        }

        /// <summary>
        /// Gets the package logo.
        /// </summary>
        /// <param name="feedItem">The feed item.</param>
        /// <returns>BitmapImage.</returns>
        /// <remarks>...</remarks>
        public static BitmapImage GetPackageLogo(this SyndicationItem feedItem) {
            if (feedItem != null) {
                var logoLink = feedItem.Links.FirstOrDefault(f => f.MediaType != null && f.MediaType.Contains("image/png"));
                if (logoLink != null)
                    return new BitmapImage(InternalHelper.GetUpdatesUrlFor(logoLink.Uri.OriginalString)); // TODO: Enable cache
            }
            return new BitmapImage(new Uri("/Newgen.Core;component/Resources/NWP_Icon.ico", UriKind.Relative));
        }

        /// <summary>
        /// Gets the thumbnail.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>ImageSource.</returns>
        /// <remarks>...</remarks>
        public static ImageSource GetThumbnail(this string path) {
            var source = default(BitmapSource);
            try {
                // For Vista +
                if (ShellObject.IsPlatformSupported) {
                    // For file icon/thumbnail.
                    if (File.Exists(path))
                        try {
                            source = ShellFile.FromFilePath(path).Thumbnail.LargeBitmapSource;
                        }
                        catch {
                            source = ShellFile.FromFilePath(path).Thumbnail.BitmapSource;
                        }

                    // For folder icon/thumbnail.
                    else if (Directory.Exists(path))
                        try {
                            source = ShellObject.FromParsingName(path).Thumbnail.LargeBitmapSource;
                        }
                        catch {
                            source = ShellObject.FromParsingName(path).Thumbnail.BitmapSource;
                        }
                }

                // For XP
                if (source == null) {
                    var bmp = default(Bitmap);

                    //// First check if thumbnail is available.
                    //if(File.Exists(path))
                    //    try
                    //    {
                    //        bmp = (new ShellThumbnail()
                    //        {
                    //            DesiredSize = new System.Drawing.Size(128, 128)
                    //        })
                    //        .GetThumbnail(path);
                    //    }
                    //    catch
                    //    {
                    //        bmp = null;
                    //    }

                    // If no thumbnail is available, check for large size icon, then for small.
                    if (bmp == null) {
                        try {
                            bmp = (Bitmap)WinAPI.ShellThumbnail.GetLargeFileIcon(path).ToBitmap();
                        }
                        catch {
                            bmp = (Bitmap)WinAPI.ShellThumbnail.GetSmallFileIcon(path).ToBitmap();
                        }
                    }
                    source = (bmp).ToBitmapSource();
                }
            }
            catch {
                source = new BitmapImage(new Uri("/Newgen.Core;component/Resources/Default.png", UriKind.Relative));
            }

            return source;
        }

        /// <summary>
        /// Gets the updates URL for.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Uri.</returns>
        /// <remarks>...</remarks>
        public static Uri GetUpdatesUrlFor(string key) {
            return new Uri(string.Format("{0}/Cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/14/{1}", WebShared.DataSiteUri, key));
        }

        /* 
        // REVIEW: Review
        /// <summary>
        /// Packages the feed item to metadata.
        /// </summary>
        /// <param name="feedItem">The feed item.</param>
        /// <returns>PackageMetadata.</returns>
        /// <remarks>...</remarks>
        public static PackageMetadata PackageFeedItemToMetadata(this SyndicationItem feedItem) {
            return new PackageMetadata() {
                Id = feedItem.Id,
                Name = feedItem.Title.Text,
                Description = (feedItem.Summary == null ? string.Empty : feedItem.Summary.Text).Trim(),
                Version = feedItem.PublishDate.UtcDateTime,
                Author = feedItem.Authors.Select(f => f.Uri).Aggregate((f, g) => string.Join(" ", f, g)),
                AuthorWebsite = "",//TODO: Fix
                AuthorEMailAddress = "",//TODO: Fix
                License = feedItem.Content == null ? string.Empty : feedItem.Content.ToString()//TODO: Fix
            };
        }
        */

        /// <summary>
        /// Sends the analytics.
        /// </summary>
        /// <returns>Task.</returns>
        /// <remarks>...</remarks>
        public static async Task SendAnalytics() { // TODO: Replace with universal GA library.
            var uri = new Uri(Newgen.Resources.Definitions.Link_App);
            var cat = Newgen.Resources.Definitions.Text_App;

            try {
                
#if !DEBUG

                await tracker
                    .TrackEventAsync(cat, "Packages", "Loaded", PackageManager.Current.Packages.Count)
                    .ConfigureIgnoreExceptions();
                await tracker
                    .TrackEventAsync(cat, "Packages", "Installed", PackageManager.Current.Packages.Count)
                    .ConfigureIgnoreExceptions();
                await tracker
                    .TrackEventAsync(cat, "IsLicensePresent", Settings.IsProMode ? "Yes" : "No", 1)
                    .ConfigureIgnoreExceptions();

#endif
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Fixes the windows taskbar.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void FixWindowsTaskbar() {
            try {
                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                (App.Screen).Width = SystemParameters.PrimaryScreenWidth;

                if (Settings.Current.ShowTaskbarAlways) {
                    (App.Screen).Height = SystemParameters.WorkArea.Height;
                    (App.Screen).Top = SystemParameters.WorkArea.Top;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else if (Settings.Current.ShowTaskbar) {
                    (App.Screen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.Screen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else {
                    (App.Screen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.Screen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Hide);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Hide);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Gets the default page URL.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetHomePagePath(string url) {
            var content = File.ReadAllText(Path.Combine(App.Current.Location, "Resources/HtmlApp/HomePage-template.html"));
            try {
                content = content
                    .Replace("{{WelcomeMessage}}", string.Format("Hello {0} !", Environment.UserName))
                    .Replace("{{InternetStatus}}", string.Format("{0}", NetworkInterface.GetIsNetworkAvailable() ? "Type your query / url below !" : "Turn on your `internet connection` to connect with world !"))
                    .Replace("{{Url}}", url)
                    ;
            }
            catch /* Eat */ { /* Tasty ? */ }

            var path = Path.Combine(App.Current.Location, "Resources/HtmlApp/HomePage.html");
            File.WriteAllText(path, content);

            return path;
        }
    }
}
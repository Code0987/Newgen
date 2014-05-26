using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using libns;
using libns.Media.Imaging;
using libns.Native;
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

        /// <summary>
        /// Initializes static members of the <see cref="InternalHelper"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        static InternalHelper() {
            FeedsAggregator = new FeedsAggregator(
                App.Current.Guid,
                new FileDataStore() { BaseDirectory = E.CacheRoot },
                new List<Uri>() { GetUpdatesUrlFor("Feeds.RSS20.xml") }
                );
#if DEBUG
            FeedsAggregator.ExceptionOccurred += (o, e) => { throw e; };
#endif
        }

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
            return new BitmapImage(new Uri("/Resources/NWP_Icon.ico", UriKind.Relative));
        }

        /// <summary>
        /// Gets the updates URL for.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Uri.</returns>
        /// <remarks>...</remarks>
        public static Uri GetUpdatesUrlFor(string key) {
            return new Uri(string.Format
(
#if DEBUG
"{0}/Cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/14/{1}"
#else
"{0}/Cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/14/{1}"
#endif
,
WebShared.DataSiteUri,
key
));
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
                source = new BitmapImage(new Uri("/Resources/Default.png", UriKind.Relative));
            }

            return source;
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
    }
}
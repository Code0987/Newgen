using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using libns.Communication.IPC;

namespace Newgen
{

    /// <summary>
    /// Class EMessage.
    /// </summary>
    /// <remarks>...</remarks>
    [Serializable]
    public class EMessage {
        /// <summary>
        /// All key
        /// </summary>
        public const string AllKey = "[*]";
        
        /// <summary>
        /// The internet key
        /// </summary>
        public const string InternetKey = "Internet";

        /// <summary>
        /// The URL key
        /// </summary>
        public const string UrlKey = "[Url]";

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        /// <remarks>...</remarks>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>...</remarks>
        public string Value { get; set; }
    }

    /// <summary>
    /// Newgen's runtime shared data
    /// </summary>
    public static class E
    {
        /// <summary>
        /// Message for network related problems
        /// </summary>
        public const string MSG_NE = "Oops !\n\nSomething went wrong ! We can't get information from server. Please report it to NS or try again next time.";

        /// <summary>
        /// Message for asking user confirmation
        /// </summary>
        public const string MSG_QA_INSTALLWIDGET = "Do you want to install this package ?";

        /// <summary>
        /// Message for error on any feature
        /// </summary>
        public const string MSG_ER_FEATURE = "ERROR !\n\nCannot use this feature now. General error, try next time.";

        /// <summary>
        /// Internal.
        /// </summary>
        public const string MSG_ER_SRVER = "ERROR !\n\nAn error occurred while activating Newgen Local Server.\nPlease report this problem.\nThis will cause many features of Newgen to function incorrectly.";

        /// <summary>
        /// Tiles size / scale factor
        /// </summary>
        public const double TilesSizeFactor = 1d;

        /// <summary>
        /// Minimum size of any tile
        /// </summary>
        public const double MinTilesSize = 150d;

        /// <summary>
        /// Image formats supported by Newgen
        /// </summary>
        public const string ImageFilter = "Image files|*.png;*.jpg;*.jpeg";

        /// <summary>
        /// Image formats supported by Newgen
        /// </summary>
        public const string AnyFilter = "Anything|*.*";

        /// <summary>
        /// The HTML package mark
        /// </summary>
        public static readonly string HTMLWidgetMetadataFile = "HTMLWidget.xml";

        /// <summary>
        /// Gets the root.
        /// </summary>
        public static string Root { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        /// <summary>
        /// Gets the packages root.
        /// </summary>
        public static string PackagesRoot { get { return Root + "\\Packages\\"; } }

        /// <summary>
        /// Gets the cache root.
        /// </summary>
        public static string CacheRoot { get { return Root + "\\Cache\\"; } }

        /// <summary>
        /// Gets the config.
        /// </summary>
        public static string Config { get { return CacheRoot + "Settings.settings"; } }

        /// <summary>
        /// Gets the bg image.
        /// </summary>
        public static string BgImage { get { return CacheRoot + "BgImage.png"; } }

        /// <summary>
        /// Gets the extern data.
        /// </summary>
        public static string ExternData { get { return CacheRoot + "Shared.data"; } }

        /// <summary>
        /// Gets the user image.
        /// </summary>
        public static string UserImage { get { return CacheRoot + "UserThumb.png"; } }

        /// <summary>
        /// Gets or sets the rows count.
        /// </summary>
        /// <value>
        /// The rows count.
        /// </value>
        public static int RowsCount { get; set; }

        /// <summary>
        /// Gets or sets the columns count.
        /// </summary>
        /// <value>
        /// The columns count.
        /// </value>
        public static int ColumnsCount { get; set; }

        /// <summary>
        /// Gets or sets the margin.
        /// </summary>
        /// <value>
        /// The margin.
        /// </value>
        public static Thickness Margin { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public static string Language { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public static Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the animation time precision.
        /// </summary>
        /// <value>
        /// The animation time precision.
        /// </value>
        public static int AnimationTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the tile spacing.
        /// </summary>
        /// <value>
        /// The tile spacing.
        /// </value>
        public static int TileSpacing { get; set; }

        /// <summary>
        /// Gets or sets the min height of the tile.
        /// </summary>
        public static double MinTileHeight { get; set; }

        /// <summary>
        /// Gets or sets the min width of the tile.
        /// </summary>
        public static double MinTileWidth { get; set; }

        /// <summary>
        /// Gets or sets the objects.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public static Dictionary<string, object> Objects { get; set; }

        /// <summary>
        /// Occurs when [hub opening].
        /// </summary>
        public static event Action HubOpening;

        /// <summary>
        /// Occurs when [hub closing].
        /// </summary>
        public static event Action HubClosing;

        /// <summary>
        /// Gets or sets the messenger.
        /// </summary>
        /// <value>The messenger.</value>
        /// <remarks>...</remarks>
        public static SimpleWindowsMessaging<EMessage> Messenger { get; set; }
        
        /// <summary>
        /// Initializes the <see cref="E"/> class.
        /// </summary>
        static E()
        {
            Messenger = new SimpleWindowsMessaging<EMessage>();

            Margin = new Thickness(0, 0, 0, 0);

            AnimationTimePrecision = 1200;

            Objects = new Dictionary<string, object>();
        }

        /// <summary>
        /// Init.
        /// </summary>
        public static void Init()
        {
            if(!Directory.Exists(PackagesRoot))
                Directory.CreateDirectory(PackagesRoot);
            if(!Directory.Exists(CacheRoot))
                Directory.CreateDirectory(CacheRoot);
        }

        /// <summary>
        /// Calls the event.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void CallEvent(string name)
        {
            switch(name)
            {
                case "HubOpening":
                    HubOpening.Invoke();
                    break;

                case "HubClosing":
                    HubClosing.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Creates the settings path for package.
        /// </summary>
        /// <param name="packagename">The packagename.</param>
        /// <returns>The String.</returns>
        /// <remarks>...</remarks>
        public static string CreateSettingsPathForWidget(string packagename)
        {
            return CacheRoot + packagename + ".settings";
        }

        /// <summary>
        /// Creates the content of the absolute path for package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string CreateAbsolutePathForPackageContent(string packageId, string relativePath = "") {
            return PackagesRoot + packageId + "\\" + relativePath;
        }

        /// <summary>
        /// Add or the update data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddorUpdateData(string key, object value)
        {
            if(Objects.ContainsKey(key))
            {
                Objects[key] = value;
            }
            else { Objects.Add(key, value); }
        }

        /// <summary>
        /// Removes the data.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void RemoveData(string key)
        {
            if(Objects.ContainsKey(key))
            {
                Objects.Remove(key);
            }
        }

        /// <summary>
        /// Gets the shared local data.
        /// </summary>
        /// <param name="packagename">The packagename.</param>
        /// <returns>System.String.</returns>
        public static string GetSharedLocalData(string packagename)
        {
            if(!File.Exists(PackagesRoot + packagename + "\\Shared.data"))
                return string.Empty;

            string content = File.ReadAllText(string.Format("{0}{1}\\Shared.data", PackagesRoot, packagename));

            return content;
        }

        /// <summary>
        /// Gets the shared local data.
        /// </summary>
        /// <param name="packagename">The packagename.</param>
        /// <param name="data">The data.</param>
        public static void SetSharedLocalData(string packagename, string data)
        {
            File.WriteAllText(PackagesRoot + packagename + "\\Shared.data", data);
        }

        /// <summary>
        /// Clears the shared local data.
        /// </summary>
        /// <param name="packagename">The packagename.</param>
        /// <param name="data">The data.</param>
        public static void ClearSharedLocalData(string packagename)
        {
            if(File.Exists(PackagesRoot + packagename + "\\Shared.data"))
                File.Delete(PackagesRoot + packagename + "\\Shared.data");
        }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static BitmapSource GetBitmap(string path)
        {
            if(!File.Exists(path))
                return null;
            var ms = new MemoryStream();
            var bi = new BitmapImage();
            try
            {
                var bytArray = File.ReadAllBytes(path);
                ms.Write(bytArray, 0, bytArray.Length);
                ms.Position = 0;
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch
            {
                return null;
            }
            finally
            {
                ms.Dispose();
            }
        }

        private static List<MediaPlayer> mediaplayers = new List<MediaPlayer>();

        /// <summary>
        /// Plays the specified media source.
        /// </summary>
        /// <param name="source">The source.</param>
        public static void Play(Uri source)
        {
            try
            {
                var mp = new MediaPlayer()
                {
                    Volume = 1.0
                };
                mediaplayers.Add(mp);
                mp.MediaEnded += (s, e) => { try { mediaplayers.Remove(mp); } catch { } };
                mp.MediaFailed += (s, e) => { try { mediaplayers.Remove(mp); } catch { } };
                mp.MediaOpened += (s, e) => { mp.Play(); };
                mp.Open(source);
            }
            catch { }
        }
    }
}
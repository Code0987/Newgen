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
    public static class Api
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
        /// Initializes the <see cref="Api"/> class.
        /// </summary>
        static Api()
        {
            Messenger = new SimpleWindowsMessaging<EMessage>();

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
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowErrorMessage(string message) {
            MessageBox.Show(message, "// Newgen / : Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// Shows the info message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowInfoMessage(string message) {
            MessageBox.Show(message, "// Newgen / : Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows the QA message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static MessageBoxResult ShowQAMessage(string message) {
            return MessageBox.Show(message, "// Newgen / : ?", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
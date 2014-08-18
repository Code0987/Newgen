using System;
using System.IO;
using System.Windows;
using libns;
using libns.Communication.IPC;

namespace Newgen {

    /// <summary>
    /// Enum TimeMode
    /// </summary>
    /// <remarks>...</remarks>
    public enum TimeMode {

        /// <summary>
        /// The H12
        /// </summary>
        H12 = 12,

        /// <summary>
        /// The H24
        /// </summary>
        H24 = 24
    }

    /// <summary>
    /// Newgen's runtime shared data
    /// </summary>
    public static class Api {

        /// <summary>
        /// Occurs when [hub closing].
        /// </summary>
        public static event Action HubClosing;

        /// <summary>
        /// Occurs when [hub opening].
        /// </summary>
        public static event Action HubOpening;

        /// <summary>
        /// Image formats supported by Newgen
        /// </summary>
        public const string AnyFilter = "Anything|*.*";

        /// <summary>
        /// Image formats supported by Newgen
        /// </summary>
        public const string ImageFilter = "Images|*.png;*.jpg;*.jpeg";

        /// <summary>
        /// The logger name
        /// </summary>
        public const string LoggerName = "Api";

        /// <summary>
        /// Message for error on any feature
        /// </summary>
        public const string MSG_ER_FEATURE = "ERROR !\n\nCannot use this feature now. General error, try next time.";

        /// <summary>
        /// Internal.
        /// </summary>
        public const string MSG_ER_SRVER = "ERROR !\n\nAn error occurred while activating Newgen Local Server.\nPlease report this problem.\nThis will cause many features of Newgen to function incorrectly.";

        /// <summary>
        /// Message for network related problems
        /// </summary>
        public const string MSG_NE = "Oops !\n\nSomething went wrong ! We can't get information from server. Please report it to NS or try again next time.";

        /// <summary>
        /// Message for asking user confirmation
        /// </summary>
        public const string MSG_QA_INSTALLPACKAGE = "Do you want to install this package ?";

        /// <summary>
        /// The API settings file
        /// </summary>
        private const string FilePath = "Api.settings";

        /// <summary>
        /// The settings
        /// </summary>
        private static ApiSettings settings;

        /// <summary>
        /// Gets the cache root.
        /// </summary>
        public static string CacheRoot {
            get {
                return Path.Combine(ResourcesRoot, "[Cache]");
            }
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <value>The host.</value>
        /// <remarks>...</remarks>
        public static ExtendedApplication Host { get; internal set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        /// <remarks>...</remarks>
        public static Logger Logger { get; private set; }

        /// <summary>
        /// Gets or sets the messenger.
        /// </summary>
        /// <value>The messenger.</value>
        /// <remarks>...</remarks>
        public static SimpleWindowsMessaging<EMessage> Messenger { get; set; }

        /// <summary>
        /// Gets the packages root.
        /// </summary>
        public static string PackagesRoot {
            get {
                return Path.Combine(Host.Location, "Packages");
            }
        }

        /// <summary>
        /// Gets the cache root.
        /// </summary>
        public static string ResourcesRoot {
            get {
                return Path.Combine(Host.Location, "Resources");
            }
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        /// <exception cref="System.InvalidOperationException">Loading api settings failed.</exception>
        /// <remarks>...</remarks>
        public static ApiSettings Settings {
            get {
                if (settings == null) {
                    try {
                        settings = Path.Combine(ResourcesRoot, FilePath).LoadJavascriptFromFile<ApiSettings>();
                        if (settings == null)
                            throw new InvalidOperationException("Loading api settings failed.");
                    }
                    catch { settings = new ApiSettings(); }
                }
                return settings;
            }
        }
        
        /// <summary>
        /// Calls the hub closing.
        /// </summary>
        /// <remarks>...</remarks>
        public static void CallHubClosing() {
            HubClosing.Invoke();
            Api.Logger.LogInformation("Api call on event [HubClosing] from [Api.CallHubClosing]");
        }

        /// <summary>
        /// Calls the hub opening.
        /// </summary>
        /// <remarks>...</remarks>
        public static void CallHubOpening() {
            HubOpening.Invoke();
            Api.Logger.LogInformation("Api call on event [HubOpening] from [Api.CallHubOpening]");
        }

        /// <summary>
        /// Called when [pre finalization].
        /// </summary>
        /// <remarks>...</remarks>
        public static void OnPreFinalization() {
            try {
                settings.SaveJavascriptToFile(Path.Combine(ResourcesRoot, FilePath));
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Called when [pre initialization].
        /// </summary>
        /// <param name="host">The host.</param>
        /// <remarks>...</remarks>
        public static void OnPreInitialization(ExtendedApplication host) {
            Host = host;

            Logger = Logger.CreateFileLogger(LoggerName, CacheRoot);

            Messenger = new SimpleWindowsMessaging<EMessage>();

            Host.Exit += (s, e) => Logger.Close();

            if (!Directory.Exists(PackagesRoot))
                Directory.CreateDirectory(PackagesRoot);

            if (!Directory.Exists(ResourcesRoot))
                Directory.CreateDirectory(ResourcesRoot);

            if (!Directory.Exists(CacheRoot))
                Directory.CreateDirectory(CacheRoot);
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

    /// <summary>
    /// Class ApiSettings.
    /// </summary>
    /// <remarks>...</remarks>
    [Serializable]
    public class ApiSettings {

        /// <summary>
        /// Gets or sets the time mode.
        /// </summary>
        /// <value>The time mode.</value>
        /// <remarks>...</remarks>
        public TimeMode TimeMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSettings"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public ApiSettings() {
            TimeMode = TimeMode.H12;
        }
    }

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
        /// The notifications key
        /// </summary>
        public const string NotificationKey = "[Notification]";

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
}
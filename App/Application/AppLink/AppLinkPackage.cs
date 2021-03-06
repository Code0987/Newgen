﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using libns;
using libns.Applied;
using libns.Media.Imaging;
using libns.Native;
using Newgen.Packages;
using PackageManager;

namespace Newgen.AppLink {

    /// <summary>
    /// Newgen App Link Package (Internal/Local).
    /// </summary>
    public class AppLinkPackage : NewgenPackage {

        /// <summary>
        /// The package type identifier
        /// </summary>
        internal static readonly string PackageTypeId = "NewgenAppLink";

        /// <summary>
        /// The browser
        /// </summary>
        internal System.Windows.Forms.WebBrowser browser;

        /// <summary>
        /// The customized settings
        /// </summary>
        internal AppLinkPackageCustomizedSettings customizedSettings;

        /// <summary>
        /// The tile
        /// </summary>
        internal AppLinkPackageTile tile;

        /// <summary>
        /// The package identifier prefix
        /// </summary>
        private static readonly string PackageIdPrefix = "App Link";

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile { get { return tile; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLinkPackage" /> class.
        /// </summary>
        /// <param name="linkPath">The link path.</param>
        /// <param name="location">The location.</param>
        /// <param name="settingsStorage">The settings storage.</param>
        /// <remarks>...</remarks>
        public AppLinkPackage(string linkPath, string location, IPackageSettingsStorage settingsStorage)
            : base(location, settingsStorage) {
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            // Package type marker.
            SettingsStorage.Put(this, PackageTypeId, PackageTypeId);

            // Pre-load settings for app link apps.
            // This prevent abnormal behavious as Row/Column span for html apps are included in settings file
            // while app expects them as compiled defaults.
            Load();

            // Create settings
            customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                s.LinkPath = linkPath;
                s.IconPath = Path.Combine(Location, "Icon.png");
                s.ScreenshotPath = Path.Combine(Location, "Screenshot.png");
            });

            // Create icons and texts
            Uri uri;
            if (Uri.TryCreate(customizedSettings.LinkPath, UriKind.RelativeOrAbsolute, out uri)
                &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ) {
                // For url
                customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                    s.IsUrl = true;
                });

                browser = new System.Windows.Forms.WebBrowser();
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.DocumentCompleted += OnbrowserDocumentCompleted;
                browser.Width = (int)SystemParameters.WorkArea.Width;
                browser.Height = (int)SystemParameters.WorkArea.Height;
                browser.Navigate(customizedSettings.LinkPath);
            }
            else {
                // For file / folder

                // Check shell
                if (!File.Exists(customizedSettings.LinkPath) & !Directory.Exists(customizedSettings.LinkPath))
                    return;

                // Check Directory
                if (Directory.Exists(customizedSettings.LinkPath))
                    customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                        s.IsFolder = true;
                    });

                // Create texts
                customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                    if (customizedSettings.IsFolder)
                        s.Title = new DirectoryInfo(s.LinkPath).Name;
                    else
                        s.Title = FileVersionInfo.GetVersionInfo(s.LinkPath).FileDescription ?? new FileInfo(s.LinkPath).Name;
                });

                // Create images
                var icon = InternalHelper.GetThumbnail(customizedSettings.LinkPath) as BitmapSource;
                icon.ToFile(new PngBitmapEncoder(), Path.Combine(Location, customizedSettings.IconPath));

                // Create tile color
                GetSettings().ObjectData[TileControl.TileBgColorKey] = icon.CalculateAverageColor(0xFF).ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewgenPackage" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="settingsStorage">The settings storage.</param>
        /// <remarks>...</remarks>
        public AppLinkPackage(string location, IPackageSettingsStorage settingsStorage)
            : base(location, settingsStorage) {
        }

        /// <summary>
        /// Gets the package identifier.
        /// </summary>
        /// <param name="linkPath">The link path.</param>
        /// <returns>System.String.</returns>
        /// <remarks>...</remarks>
        public static string GetPackageId(string linkPath) {
            return string.Format("{0} ({1})", PackageIdPrefix, linkPath.ToSafeFileName());
        }

        /// <summary>
        /// Loads the texts.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Data not set, may be damaged or the entry is new.
        /// </exception>
        /// <remarks>...</remarks>
        internal void ReloadTile() {
            customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>();

            tile.Title.Text = customizedSettings.Title;
            tile.Icon.Source = Path.Combine(Location, customizedSettings.IconPath).ToBitmapSource();
            tile.Screenshot.Source = Path.Combine(Location, customizedSettings.ScreenshotPath).ToBitmapSource();
        }

        /// <summary>
        /// Called when [start].
        /// </summary>
        /// <remarks>...</remarks>
        protected override void OnStart() {
            customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>();

            // Fix logo
            if (!Settings.OfType<NewgenPackageLogoSettings>().Any())
                Settings.Add(new NewgenPackageLogoSettings(this));
            Settings.OfType<NewgenPackageLogoSettings>().First().Value = Path.Combine(Location, customizedSettings.IconPath);

            // Load UI
            Application.Current.Dispatcher.Invoke(new Action(() => {
                tile = new AppLinkPackageTile();

                tile.MouseLeftButtonUp += (f, g) => {
                    try {
                        if (customizedSettings.IsUrl) {
                            Api.Messenger.Send(new EMessage() {
                                Key = EMessage.UrlKey,
                                Value = customizedSettings.LinkPath
                            });
                        }
                        else {
                            foreach (var process in Process.GetProcesses())
                                if (process.StartInfo.FileName.Equals(customizedSettings.LinkPath)) {
                                    WinAPI.SetForegroundWindow(process.MainWindowHandle);
                                    return;
                                }
                            var p = new Process();
                            p.StartInfo.Arguments = customizedSettings.Args;
                            p.StartInfo.FileName = customizedSettings.LinkPath;
                            p.StartInfo.UseShellExecute = true;
                            p.Start();
                        }
                    }
                    catch { }
                };

                tile.ContextMenu = new ContextMenu();
                var mi_options = new MenuItem();
                mi_options.Header = new AppLinkPackageSettingsEditor(this);
                tile.ContextMenu.Items.Add(mi_options);

                // Re load tile
                ReloadTile();
            }));
        }

        /// <summary>
        /// Called when [stop].
        /// </summary>
        /// <remarks>...</remarks>
        protected override void OnStop() {
            if (browser != null && !browser.IsDisposed)
                browser.Dispose();
        }

        /// <summary>
        /// Handles the <see cref="E:DocumentCompleted" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.WebBrowserDocumentCompletedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnbrowserDocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) {
            if (browser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
                return;
            // Detach
            browser.DocumentCompleted -= OnbrowserDocumentCompleted;

            // Container image
            var bitmap = new System.Drawing.Bitmap(browser.Width, browser.Height);
            try {
                // Save screenshot
                browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, browser.Width, browser.Height));
                bitmap.Save(Path.Combine(Location, customizedSettings.ScreenshotPath), System.Drawing.Imaging.ImageFormat.Png);
                // Save title
                customizedSettings = GetSettings().Customize<AppLinkPackageCustomizedSettings>(s => {
                    s.Title = browser.DocumentTitle;
                });
            }
            catch /* Eat */ {
            }
            finally {
                bitmap.Dispose();
                browser.Dispose();
            }
            // Next
            try {
                // Save icon
                bitmap = new System.Drawing.Bitmap(customizedSettings.LinkPath.GetFavicon().ToMemoryStream());
                bitmap.Save(Path.Combine(Location, customizedSettings.IconPath), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch /* Eat */ {
            }
            finally {
                bitmap.Dispose();
            }
            // Finally load !
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                ReloadTile();
            }));
        }
    }

    /// <summary>
    /// Class AppLinkPackageCustomizedSettings.
    /// </summary>
    /// <remarks>...</remarks>
    public class AppLinkPackageCustomizedSettings {

        /// <summary>
        /// The arguments
        /// </summary>
        public string Args;

        /// <summary>
        /// The icon path
        /// </summary>
        public string IconPath;

        /// <summary>
        /// The is folder
        /// </summary>
        public bool IsFolder;

        /// <summary>
        /// The is network location
        /// </summary>
        public bool IsUrl;

        /// <summary>
        /// The link path
        /// </summary>
        public string LinkPath;

        /// <summary>
        /// The screenshot path
        /// </summary>
        public string ScreenshotPath;

        /// <summary>
        /// The title
        /// </summary>
        public string Title;
    }
}
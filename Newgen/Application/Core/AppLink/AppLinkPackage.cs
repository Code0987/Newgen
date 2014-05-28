using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using libns;
using libns.Media.Imaging;
using libns.Native;

namespace Newgen.Packages.AppLink {

    /// <summary>
    /// Newgen App Link Package (Internal/Local).
    /// </summary>
    public class AppLinkPackage : Package {
        internal AppLinkPackageCustomizedSettings customizedSettings;
        internal AppLinkPackageTile tile;
        internal System.Windows.Forms.WebBrowser browser;

        private static readonly string PackageIdPrefix = "App Link";

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement Tile { get { return tile; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLinkPackage" /> class.
        /// </summary>
        /// <param name="linkPath">The link path.</param>
        /// <remarks>...</remarks>
        private AppLinkPackage(string linkPath, string location)
            : this(location) {
            // Create metadata
            Metadata = new PackageMetadata() {
                Id = GetPackageId(linkPath),
                Version = DateTime.Now,
                Author = Environment.UserName,
                License = "Public Domain",
                Name = GetPackageId(linkPath)
            };

            // Create settings
            customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
                s.LinkPath = linkPath;
                s.IconPath = Settings.CreateAbsolutePathFor("Icon.png");
                s.ScreenshotPath = Settings.CreateAbsolutePathFor("Screenshot.png");
            });


            // Create icons and texts
            Uri uri;
            if (Uri.TryCreate(customizedSettings.LinkPath, UriKind.RelativeOrAbsolute, out uri)
                &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ) {
                // For url
                customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
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
                    customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
                        s.IsFolder = true;
                    });

                // Create texts
                customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
                    if (customizedSettings.IsFolder)
                        s.Title = new DirectoryInfo(s.LinkPath).Name;
                    else
                        s.Title = FileVersionInfo.GetVersionInfo(s.LinkPath).FileDescription ?? new FileInfo(s.LinkPath).Name;
                });

                // Create images
                var icon = InternalHelper.GetThumbnail(customizedSettings.LinkPath) as BitmapSource;
                icon.ToFile(new PngBitmapEncoder(), Settings.CreateAbsolutePathFor(customizedSettings.IconPath));

                // Create tile color
                Settings.ObjectData[TileControl.TileBgColorKey] = icon.CalculateAverageColor().ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        private AppLinkPackage(string location)
            : base(location) {
        }

        /// <summary>
        /// Creates for.
        /// </summary>
        /// <param name="linkPath">The link path.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFor(string linkPath) {
            return new AppLinkPackage(linkPath, PackageManager.Current.CreateAbsolutePathFor(GetPackageId(linkPath)));
        }

        /// <summary>
        /// Creates from.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Package.</returns>
        /// <remarks>...</remarks>
        public static Package CreateFrom(string location) {
            var package = new AppLinkPackage(location);
            if (!package.Metadata.Id.StartsWith(PackageIdPrefix))
                throw new Exception("Not a app link package !");
            return package;
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
        /// Loads from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public override void Load() {
            base.Load();
            // Load UI
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
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
                mi_options.Header = "Options";
                mi_options.Click += (f, g) => {
                    (new AppLinkPackageOptionsHub(this)).ShowDialog();
                };
                tile.ContextMenu.Items.Add(mi_options);


                // Re load tile
                ReloadTile();
            }));
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public override void Unload() {
            if (browser != null && !browser.IsDisposed)
                browser.Dispose();

            base.Unload();
        }

        /// <summary>
        /// Loads the texts.
        /// </summary>
        /// <exception cref="System.Exception">
        /// Data not set, may be damaged or the entry is new.
        /// </exception>
        /// <remarks>...</remarks>
        internal void ReloadTile() {
            customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>();

            tile.Title.Text = customizedSettings.Title;
            tile.Icon.Source = Settings.CreateAbsolutePathFor(customizedSettings.IconPath).ToBitmapSource();
            tile.Screenshot.Source = Settings.CreateAbsolutePathFor(customizedSettings.ScreenshotPath).ToBitmapSource();
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
                bitmap.Save(Settings.CreateAbsolutePathFor(customizedSettings.ScreenshotPath), System.Drawing.Imaging.ImageFormat.Png);
                // Save title
                customizedSettings = Settings.Customize<AppLinkPackageCustomizedSettings>(s => {
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
                bitmap.Save(Settings.CreateAbsolutePathFor(customizedSettings.IconPath), System.Drawing.Imaging.ImageFormat.Png);
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
        /// The screenshot path
        /// </summary>
        public string ScreenshotPath;

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
        /// The title
        /// </summary>
        public string Title;
    }
}
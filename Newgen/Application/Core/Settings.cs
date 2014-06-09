// ***********************************************************************
// Assembly : Newgen Author : Code0987 Created : 05-14-2014
//
// <copyright file="Settings.cs" company="NS">Copyright (c) NS. All rights reserved.</copyright>
// <summary>
// </summary>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using iFramework.Security.Licensing;
using libns;
using libns.UI;
using Newgen.Resources;

/// <summary>
/// The Newgen namespace.
/// </summary>
/// <remarks>...</remarks>
namespace Newgen {

    /// <summary>
    /// Class Settings.
    /// </summary>
    /// <remarks>...</remarks>
    public class Settings {

        /// <summary>
        /// The cultures
        /// </summary>
        internal List<CultureInfo> Cultures = typeof(Definitions).GetSupportedCultures().ToList();

        /// <summary>
        /// The current
        /// </summary>
        private static Settings current;

        /// <summary>
        /// Gets or sets the active license identifier.
        /// </summary>
        /// <value>The active license identifier.</value>
        /// <remarks>...</remarks>
        public Guid ActiveLicenseId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Settings" /> is autostart.
        /// </summary>
        /// <value><c>true</c> if autostart; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool Autostart { get; set; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        /// <remarks>...</remarks>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable hotkeys].
        /// </summary>
        /// <value><c>true</c> if [enable hotkeys]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool EnableHotkeys { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tiles lock enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is tiles lock enabled; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool IsTilesLockEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is user tile enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is user tile enabled; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool IsUserTileEnabled { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        /// <remarks>...</remarks>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the lock screen time.
        /// </summary>
        /// <value>The lock screen time.</value>
        /// <remarks>...</remarks>
        public int LockScreenTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum height of the tile.
        /// </summary>
        /// <value>The minimum height of the tile.</value>
        /// <remarks>...</remarks>
        public double MinTileHeight { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of the tile.
        /// </summary>
        /// <value>The minimum width of the tile.</value>
        /// <remarks>...</remarks>
        public double MinTileWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [provide usage data].
        /// </summary>
        /// <value><c>true</c> if [provide usage data]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool ProvideUsageData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show startbar always].
        /// </summary>
        /// <value><c>true</c> if [show startbar always]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool ShowStartbarAlways { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show taskbar].
        /// </summary>
        /// <value><c>true</c> if [show taskbar]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool ShowTaskbar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show taskbar always].
        /// </summary>
        /// <value><c>true</c> if [show taskbar always]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool ShowTaskbarAlways { get; set; }

        /// <summary>
        /// Gets or sets the slide show images.
        /// </summary>
        /// <value>The slide show images.</value>
        /// <remarks>...</remarks>
        public List<string> SlideShowImages { get; set; }

        /// <summary>
        /// Gets or sets the slide show time.
        /// </summary>
        /// <value>The slide show time.</value>
        /// <remarks>...</remarks>
        public int SlideShowTime { get; set; }

        /// <summary>
        /// Gets or sets the start text.
        /// </summary>
        /// <value>The start text.</value>
        /// <remarks>...</remarks>
        public string StartText { get; set; }

        /// <summary>
        /// Gets or sets the task bar process exclusion list.
        /// </summary>
        /// <value>The task bar process exclusion list.</value>
        /// <remarks>...</remarks>
        public List<string> TaskBarProcessExclusionList { get; set; }

        /// <summary>
        /// Gets or sets the tile screen groups.
        /// </summary>
        /// <value>The tile screen groups.</value>
        /// <remarks>...</remarks>
        public List<TileControlsGroupBarSettings> TileScreenGroups { get; set; }

        /// <summary>
        /// Gets or sets the tile spacing.
        /// </summary>
        /// <value>The tile spacing.</value>
        /// <remarks>...</remarks>
        public int TileSpacing { get; set; }

        /// <summary>
        /// Gets or sets the time mode.
        /// </summary>
        /// <value>The time mode.</value>
        /// <remarks>...</remarks>
        public int TimeMode { get; set; }

        /// <summary>
        /// Gets or sets the color of the toolbar background.
        /// </summary>
        /// <value>The color of the toolbar background.</value>
        /// <remarks>...</remarks>
        public Color ToolbarBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use bg image].
        /// </summary>
        /// <value><c>true</c> if [use bg image]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool UseBgImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use thumbails bar].
        /// </summary>
        /// <value><c>true</c> if [use thumbails bar]; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool UseThumbailsBar { get; set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>The current.</value>
        /// <exception cref="System.InvalidOperationException">Loading settings failed.</exception>
        /// <remarks>...</remarks>
        internal static Settings Current {
            get {
                if (current == null) {
                    // 1. Check settings first
                    try {
                        current = Api.Config.LoadJavascriptFromFile<Settings>();
                        if (current == null)
                            throw new InvalidOperationException("Loading settings failed.");
                    }
                    catch { current = new Settings(); }
                    // 2. Load license
                    try {
                        current.GetAndValidateActiveLicense();
                    }
                    catch { }
                    // 3. Update settings
                    current.Update();
                }

                return current;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is pro mode.
        /// </summary>
        /// <value><c>true</c> if this instance is pro mode; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        [XmlIgnore]
        internal static bool IsProMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public Settings() {
            ActiveLicenseId = Guid.Empty;

            TileScreenGroups = new List<TileControlsGroupBarSettings>();
            SlideShowImages = new List<string>();

            BackgroundColor = Color.FromRgb(55, 55, 55);
            ToolbarBackgroundColor = Color.FromRgb(35, 135, 200);

            Language = CultureInfo.CurrentUICulture.Name;
            ProvideUsageData = true;
            StartText = Definitions.Text_DefaultTilesScreenTitle;

            MinTileWidth = 90.0;
            MinTileHeight = 90.0;

            TimeMode = 1;
            LockScreenTime = -1;
            TileSpacing = 8;
            SlideShowTime = 30;

            Autostart = true;
            IsUserTileEnabled = true;
            UseBgImage = true;
            ShowTaskbarAlways = false;
            ShowTaskbar = true;
            EnableHotkeys = false;
            UseThumbailsBar = true;
            IsTilesLockEnabled = false;
            ShowStartbarAlways = false;

            TaskBarProcessExclusionList = new List<string>();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Save() {
            this.SaveJavascriptToFile(Api.Config);
        }

        /// <summary>
        /// Gets the and validate active license.
        /// </summary>
        /// <returns>Guid.</returns>
        /// <remarks>...</remarks>
        internal Guid GetAndValidateActiveLicense() {
            try {
                IsProMode =
#if DEBUG
 true
#else
 (ClientManager.Current.IsActive(ActiveLicenseId))
 &&
 (ClientManager.Current.IsValid(ActiveLicenseId, App.Current.Guid))
#endif
;
            }
            catch { }
            return ActiveLicenseId;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <remarks>...</remarks>
        internal void Update() {
            // Update

            if (!IsProMode) {
                SlideShowImages.Clear();

                BackgroundColor = Color.FromRgb(55, 55, 55);
                ToolbarBackgroundColor = Color.FromRgb(35, 135, 200);

                StartText = Definitions.Text_DefaultTilesScreenTitle;

                TimeMode = 1;
                LockScreenTime = -1;
                TileSpacing = 8;
                SlideShowTime = 30;

                IsUserTileEnabled = true;
                UseBgImage = true;
                ShowTaskbarAlways = false;
                ShowTaskbar = true;
                UseThumbailsBar = true;
                IsTilesLockEnabled = false;
                ShowStartbarAlways = false;
            }

            // Update environment

            Definitions.Culture = Language.ResetCulture();
        }
    }

    /// <summary>
    /// Class SettingsBinding.
    /// </summary>
    /// <remarks>...</remarks>
    public class SettingsBinding : Binding {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBinding" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsBinding() {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Data.Binding" /> class
        /// with an initial path.
        /// </summary>
        /// <param name="path">
        /// The initial <see cref="P:System.Windows.Data.Binding.Path" /> for the binding.
        /// </param>
        /// <remarks>...</remarks>
        public SettingsBinding(string path)
            : base(path) {
            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>...</remarks>
        private void Initialize() {
            Source = Settings.Current;
            Mode = BindingMode.TwoWay;
        }
    }

    /// <summary>
    /// Class TileControlsGroupBarSettings.
    /// </summary>
    /// <remarks>...</remarks>
    [Serializable]
    [XmlRoot("tsg")]
    public class TileControlsGroupBarSettings {

        /// <summary>
        /// The column
        /// </summary>
        [XmlAttribute(AttributeName = "c", DataType = "int")]
        public int Column;

        /// <summary>
        /// The identifier
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id;

        /// <summary>
        /// The title
        /// </summary>
        [XmlAttribute(AttributeName = "t")]
        public string Title;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileControlsGroupBarSettings" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public TileControlsGroupBarSettings() {
            Id = Guid.NewGuid();
            Title = "";
            Column = 6;
        }
    }

    /// <summary>
    /// Class TaskBarProcessExclusionData.
    /// </summary>
    /// <remarks>...</remarks>
    internal class TaskBarProcessExclusionData {

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        /// <remarks>...</remarks>
        public BitmapSource Icon { get; set; }

        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        /// <value>The name of the process.</value>
        /// <remarks>...</remarks>
        public string ProcessName { get; set; }
    }
}
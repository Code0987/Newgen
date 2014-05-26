using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Xml.Serialization;
using libns;
using iFramework.Security.Licensing;
using Newgen.Resources;
using libns.UI;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Newgen {

    public class Settings {
        private static Settings current;

        internal List<CultureInfo> Cultures = typeof(Definitions).GetSupportedCultures().ToList();

        public Guid ActiveLicenseId { get; set; }

        public bool Autostart { get; set; }

        public Color BackgroundColor { get; set; }

        public bool EnableHotkeys { get; set; }
        
        public bool IsUserTileEnabled { get; set; }

        public bool IsTilesLockEnabled { get; set; }

        public string Language { get; set; }
        
        public int LockScreenTime { get; set; }

        public double MinTileHeight { get; set; }

        public double MinTileWidth { get; set; }

        public bool ProvideUsageData { get; set; }

        public bool ShowStartbarAlways { get; set; }

        public bool ShowTaskbar { get; set; }

        public bool ShowTaskbarAlways { get; set; }

        public List<string> SlideShowImages { get; set; }

        public int SlideShowTime { get; set; }

        public string StartText { get; set; }

        public List<TileControlsGroupBarSettings> TileScreenGroups { get; set; }

        public int TileSpacing { get; set; }

        public int TimeMode { get; set; }

        public Color ToolbarBackgroundColor { get; set; }

        public bool UseBgImage { get; set; }

        public bool UseThumbailsBar { get; set; }

        public List<string> TaskBarProcessExclusionList { get; set; }

        internal static Settings Current {
            get {
                if (current == null) {
                    // 1. Check settings first
                    try {
                        current = E.Config.LoadJavascriptFromFile<Settings>();
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

        [XmlIgnore]
        internal static bool IsProMode { get; set; }

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

        public Settings() {
            ActiveLicenseId = Guid.Empty;

            TileScreenGroups = new List<TileControlsGroupBarSettings>();
            SlideShowImages = new List<string>();

            BackgroundColor = Color.FromRgb(55, 55, 55);
            ToolbarBackgroundColor = Color.FromRgb(35, 135, 200);

            Language = CultureInfo.CurrentUICulture.Name;
            ProvideUsageData = true;
            StartText = Definitions.Text_DefaultTilesScreenTitle;

            MinTileWidth = 180.0;
            MinTileHeight = 180.0;

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

        //public void Save(string path) {
        //    LoadedWidgets.Clear();
        //    foreach (var w in App.Screen.TileControls) {
        //        if (w.WidgetProxy.Path != null && (w.WidgetProxy.Path.Contains(@"\") || w.WidgetProxy.Path.Contains(@"/")))
        //            w.WidgetProxy.Path = w.WidgetProxy.Path.Replace(E.PackagesRoot, "");
        //        LoadedWidgets.Add(w.WidgetProxy);
        //    }
        //    base.Save(path);
        //}

        public void Save() {
            this.SaveJavascriptToFile(E.Config);
        }

        internal void Update() {
            // Update

            if (!IsProMode) {
                //try {
                //    var legacyWidgets = LoadedWidgets.Where(f => f.Path != null && f.Path.EndsWith(".dll"));
                //    var toRemove = legacyWidgets.Count() > 6 ? legacyWidgets.Take(legacyWidgets.Count() - 7) : null;
                //    if (toRemove != null)
                //        foreach (var widget in toRemove)
                //            LoadedWidgets.Remove(widget);
                //}
                //catch { }
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

            E.Language = Language;
            E.TileSpacing = TileSpacing;
            E.MinTileWidth = MinTileWidth;
            E.MinTileHeight = MinTileHeight;
            E.BackgroundColor = BackgroundColor;
        }
    }

    public class SettingsBinding : Binding {
        public SettingsBinding() {
            Initialize();
        }

        public SettingsBinding(string path)
            : base(path) {
            Initialize();
        }

        private void Initialize() {
            Source = Settings.Current;
            Mode = BindingMode.TwoWay;
        }
    }
    
    [Serializable]
    [XmlRoot("tsg")]
    public class TileControlsGroupBarSettings {

        [XmlAttribute(AttributeName = "c", DataType = "int")]
        public int Column;

        [XmlAttribute(AttributeName = "id")]
        public Guid Id;

        [XmlAttribute(AttributeName = "t")]
        public string Title;

        public TileControlsGroupBarSettings() {
            Id = Guid.NewGuid();
            Title = "";
            Column = 6;
        }
    }

    internal class TaskBarProcessExclusionData {
        public BitmapSource Icon { get; set; }

        public string ProcessName { get; set; }
    }

}
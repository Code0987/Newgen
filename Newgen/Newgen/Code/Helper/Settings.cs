using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using Newgen.Base;

namespace Newgen
{
    public class Settings : XmlSerializable
    {
        public bool IsFirstStart { get; set; }

        public bool UseBgImage { get; set; }

        public bool ShowTaskbarAlways { get; set; }

        public bool ShowTaskbar { get; set; }

        public bool EnableHotkeys { get; set; }

        public bool UseThumbailsBar { get; set; }

        public bool ShowStartbarAlways { get; set; }

        public bool DisableStartBarClock { get; set; }

        public bool EnableOutOfBoxExperience { get; set; }

        public bool Autostart { get; set; }

        public bool IsUserTileEnabled { get; set; }

        public bool IsWidgetsLockEnabled { get; set; }

        public int TimeMode { get; set; }

        public int LockScreenTime { get; set; }

        public int TileSpacing { get; set; }

        public int SlideShowTime { get; set; }

        public double MinTileHeight { get; set; }

        public double MinTileWidth { get; set; }

        public string Language { get; set; }

        public bool ProvideUsageData { get; set; }

        [XmlIgnore]
        public DateTime ProvideUsageDataLastSentOn { get; set; }

        public string StartText { get; set; }

        public string FBAuthData { get; set; }

        public Color BackgroundColor { get; set; }

        public Color ToolbarBackgroundColor { get; set; }

        public List<string> SlideShowImages { get; set; }

        public List<TileScreenWidgetInfo> LoadedWidgets { get; set; }

        public List<TileScreenGroup> TileScreenGroups { get; set; }

        public Settings()
        {
            LoadedWidgets = new List<TileScreenWidgetInfo>();
            TileScreenGroups = new List<TileScreenGroup>();
            SlideShowImages = new List<string>();

            BackgroundColor = Color.FromRgb(55, 55, 55);
            ToolbarBackgroundColor = Color.FromRgb(35, 135, 200);

            Language = CultureInfo.CurrentUICulture.Name;
            ProvideUsageData = true;
            ProvideUsageDataLastSentOn = DateTime.Now.Subtract(TimeSpan.FromHours(1d));
            StartText = Resources.Resources.Text_DefaultTilesScreenTitle;
            FBAuthData = string.Empty;

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
            EnableOutOfBoxExperience = false;
            IsWidgetsLockEnabled = false;
            ShowStartbarAlways = false;
            DisableStartBarClock = false;
            IsFirstStart = true;
        }

        internal void Update()
        {
            if(!App.IsProMode)
            {
                try
                {
                    var legacyWidgets = LoadedWidgets.Where(f => f.Path != null && f.Path.EndsWith(".dll"));
                    var toRemove = legacyWidgets.Count() > 6 ? legacyWidgets.Take(legacyWidgets.Count() - 7) : null;
                    if(toRemove != null)
                        foreach(var widget in toRemove)
                            LoadedWidgets.Remove(widget);
                }
                catch { }
                SlideShowImages.Clear();

                BackgroundColor = Color.FromRgb(55, 55, 55);
                ToolbarBackgroundColor = Color.FromRgb(35, 135, 200);

                StartText = Resources.Resources.Text_DefaultTilesScreenTitle;
                FBAuthData = string.Empty;

                TimeMode = 1;
                LockScreenTime = -1;
                TileSpacing = 8;
                SlideShowTime = 30;

                IsUserTileEnabled = true;
                UseBgImage = true;
                ShowTaskbarAlways = false;
                ShowTaskbar = true;
                UseThumbailsBar = true;
                EnableOutOfBoxExperience = false;
                IsWidgetsLockEnabled = false;
                ShowStartbarAlways = false;
                DisableStartBarClock = false;
            }
        }
    }

    [Serializable]
    [XmlRoot("tswi")]
    public class TileScreenWidgetInfo
    {
        [XmlAttribute(AttributeName = "n")]
        public string Name;

        [XmlAttribute(AttributeName = "p")]
        public string Path;

        [XmlAttribute(AttributeName = "id")]
        public string Id;

        [XmlAttribute(AttributeName = "c", DataType = "int")]
        public int Column;

        [XmlAttribute(AttributeName = "r", DataType = "int")]
        public int Row;

        [XmlAttribute(AttributeName = "d")]
        public string Data = "";
    }

    [Serializable]
    [XmlRoot("tsg")]
    public class TileScreenGroup
    {
        [XmlAttribute(AttributeName = "id")]
        public Guid Id;

        [XmlAttribute(AttributeName = "t")]
        public string Title;

        [XmlAttribute(AttributeName = "c", DataType = "int")]
        public int Column;

        public TileScreenGroup()
        {
            Id = Guid.NewGuid();
            Title = "";
            Column = 6;
        }
    }
}
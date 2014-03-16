using System;
using System.Windows;
using Newgen.Base;

namespace Internet
{
    /// <summary>
    /// Provides information about widget for Newgen and allows Newgen determine that this .dll file is a widget
    /// </summary>
    public class Widget : Newgen.Base.NewgenWidget
    {
        internal const string WIDGET_NAME = "Internet";

        internal static Settings Settings;
        internal static string SettingsFile = E.CreateSettingsPathForWidget(WIDGET_NAME);

        /// <summary>
        /// Name of the Widget, required to display in Newgen.
        /// </summary>
        public override string Name
        {
            get { return WIDGET_NAME; }
        }

        /// <summary>
        /// Path to widget icon. Return null if there is no icon.
        /// </summary>
        public override Uri IconPath
        {
            get { return new Uri("/" + WIDGET_NAME + ";component/Resources/Icon.png", UriKind.Relative); }
        }
        
        private WidgetControl widgetControl;
        
        /// <summary>
        /// Returns widget control
        /// </summary>
        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        /// <summary>
        /// Returns number of horizontal cells occupied by widget. Should be 1 or 2 (more is possible but not recommended)
        /// </summary>
        public override int ColumnSpan
        {
            get { return 1; }
        }

        /// <summary>
        /// Widget initialization (e.g.variables initialization, reading settings, loading resources) must be here.
        /// This method calls when user clicks on widget icon in Newgen menu or at Newgen launch if widget was added earlier
        /// </summary>
        public override void Load()
        {
            //read settings, remove if you don't need settings
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), SettingsFile) ?? new Settings();

            widgetControl = new WidgetControl();
            widgetControl.Load();
        }

        /// <summary>
        /// Releasing resources and settings saving must be here.
        /// This method calls when user removes widget from Newgen grid or when user closes Newgen if widget was loaded earlier
        /// </summary>
        public override void Unload()
        {
            //write settings, remove if you don't need settings
            Settings.Save(SettingsFile);

            widgetControl.Unload();
        }

        public override void HandleMessage(string message)
        {
            switch (message)
            {
                default:
                    widgetControl.Navigate(message);
                    break;
            }
        }
    }
}
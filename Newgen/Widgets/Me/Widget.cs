using System;
using System.Windows;
using Newgen.Base;

namespace Me
{
    public class Widget : NewgenWidget
    {
        internal const string WIDGET_NAME = "Me";

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
        
        private MeWidget widgetControl;
        

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), Widget.SettingsFile) ?? new Settings();
            widgetControl = new MeWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(Widget.SettingsFile);
            widgetControl.Unload();
        }
    }
}
using System;
using System.Windows;
using Newgen.Base;

namespace Desktop
{
    public class Widget : NewgenWidget
    {
        private DesktopWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Desktop"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Desktop;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new DesktopWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            //widgetControl.Unload();
        }
    }
}
using System;
using System.Windows;
using Newgen.Base;

namespace CPUMonitor
{
    public class Widget : NewgenWidget
    {
        private CPUMonitorWidget widgetControl;

        //public static Settings Settings;

        public override string Name
        {
            get { return "CPUMonitor"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/CPUMonitor;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            widgetControl = new CPUMonitorWidget();
        }

        public override void Unload()
        {
            try
            {
                widgetControl.OnRemoved();
            }
            catch { }
        }
    }
}
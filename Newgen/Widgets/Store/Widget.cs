using System;
using System.IO;
using System.Windows;
using Newgen.Base;

namespace Store
{
    public class Widget : Newgen.Base.NewgenWidget
    {
        private WidgetControl widgetControl;

        public const string WidgetsBase = "http://data.nsapps.net/cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/12/widgets/";

        public override string Name
        {
            get { return "Store"; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Store;component/Resources/icon.png", UriKind.Relative); }
        }

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
            widgetfolder = E.WidgetsRoot + Name + "\\";
            widgetControl = new WidgetControl();
            widgetControl.Load();
        }

        public override void Unload()
        {
            widgetControl.Unload();
        }

        internal static string widgetfolder = null;

        internal static bool IsWidgetInstalled(string name)
        {
            try
            {
                if (Directory.Exists(widgetfolder) && File.Exists(E.WidgetsRoot + name + "\\Widget.xml")) { return true; }
                else { return false; }
            }
            catch { return false; }
        }

        internal static bool IsWidgetUpdateAvailable(string name, string version)
        {
            try
            {
                string widgetfolder = E.WidgetsRoot + name;
                if (Directory.Exists(widgetfolder) && File.Exists(E.WidgetsRoot + name + "\\Widget.xml"))
                {
                    WidgetInfo info = null;
                    bool available = false;

                    try { info = (WidgetInfo)XmlSerializable.Load(typeof(WidgetInfo), E.WidgetsRoot + name + "\\Widget.xml"); }
                    catch { return false; }

                    if (double.Parse(info.Version) < double.Parse(version))
                    {
                        available = true;
                    }

                    return available;
                }
                else { return false; }
            }
            catch { return false; }
        }

        public override void HandleMessage(string message)
        {
            switch (message)
            {
                case "WidgetInstalled":
                    MessageBox.Show("Widget Installed / Updated. Please restart Newgen to apply new updates.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "WidgetRemoved":
                    MessageBox.Show("Widget Removed. Please restart Newgen to apply changes.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    break;
            }
        }
    }
}
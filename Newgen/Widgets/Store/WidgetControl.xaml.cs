using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Newgen.Base;

namespace Store
{
    public partial class WidgetControl : UserControl
    {
        private HubWindow hub;
        private HubControl hubContent;

        public WidgetControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            Helper.Delay(() => { CheckForUpdates(); }, 2000);
        }

        public void Unload()
        {
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.Topmost = false;
            hub.AllowsTransparency = false;
            hubContent = new HubControl();
            hubContent.Closing += HubContentClosing;
            hub.Content = hubContent;
            hub.ShowDialog();
        }

        private void HubContentClosing(object sender, System.EventArgs e)
        {
            hub.Close();
        }

        private void CheckForUpdates()
        {
            ThreadStart start = delegate()
            {
                try
                {
                    XElement xml = null;
                    int count = 0;

                    xml = XElement.Load(Widget.WidgetsBase + "meta.xml");

                    if(!Directory.Exists(Widget.widgetfolder + "$[Cache]"))
                        Directory.CreateDirectory(Widget.widgetfolder + "$[Cache]");

                    try
                    {
                        System.Threading.Tasks.Parallel.ForEach<XElement>(
                            xml.Descendants("Widget"),
                            (element, e) =>
                            {
                                Thread.Sleep(1500);

                                string id = "";
                                try
                                {
                                    id=element.Attribute("Id").Value;
                                }
                                catch { id=element.Attribute("ID").Value; }
                                string name = element.Attribute("Name").Value;
                                string version = element.Attribute("Version").Value;

                                if(Widget.IsWidgetUpdateAvailable(name, version))
                                    count++;
                            }
                            );
                    }
                    catch { }

                    Application.Current.Dispatcher.BeginInvoke(new Action(() => { try { Text_Info.Text = count.ToString(); } catch { } }));
                }
                catch { }
            };
            new Thread(start).Start();
        }
    }
}
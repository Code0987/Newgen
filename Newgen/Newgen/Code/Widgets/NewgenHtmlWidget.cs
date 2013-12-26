using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Xml.Linq;
using Newgen.Base;
using WebKit;

namespace Newgen.Core
{
    public class NewgenHtmlWidget : NewgenWidget, IDisposable
    {
        private string rootpath;
        private string contentpath;
        private string contentfbimgpath;
        private string hubContentPath;
        private string optionshubcontentpath;
        private HubWindow hub;
        private WebKitBrowser tilebroswer;
        private Image tilebroswerview;
        private System.Windows.Threading.DispatcherTimer loop;

        private string name;

        public override string Name { get { return name; } }

        private Grid widgetcontrol;

        public override System.Windows.FrameworkElement WidgetControl { get { return widgetcontrol; } }

        private string iconpath;

        public override System.Uri IconPath { get { return new Uri(this.iconpath); } }

        private int colspan;

        public override int ColumnSpan { get { return colspan; } }

        public NewgenHtmlWidget(string path)
        {
            this.rootpath = path;

            if (!File.Exists(this.rootpath + "\\Widget.Definition.xml"))
            {
                throw new FileNotFoundException(@"HTML Widget definition file ~\Widget.Definition.xml not found.");
            }

            try
            {
                XElement xml = XElement.Load(this.rootpath + "\\Widget.Definition.xml");

                try { name = xml.Element("Name").Value; }
                catch { throw new Exception("Error while allocating widget 'Name'."); }

                try { this.colspan = int.Parse(xml.Element("CSpan").Value); }
                catch { throw new Exception("Error while allocating widget 'CSpan'."); }

                try { this.contentpath = xml.Element("Content").Value; }
                catch { throw new Exception("Error while allocating widget 'Content'."); }

                try { this.contentfbimgpath = xml.Element("Content.FallbackImage").Value; }
                catch { throw new Exception("Error while allocating widget 'Content.Fallback'."); }

                if (!File.Exists(this.rootpath + "\\" + this.contentpath))
                { throw new FileNotFoundException("Content file not found."); }

                if (xml.Element("Icon") != null) { this.iconpath = this.rootpath + "\\" + xml.Element("Icon").Value; }

                if (xml.Element("Options") != null)
                {
                    this.optionshubcontentpath = xml.Element("Options").Value;
                    if (!File.Exists(this.rootpath + "\\" + this.optionshubcontentpath))
                    { throw new FileNotFoundException("Options content file not found."); }
                }
                if (xml.Element("Hub") != null)
                {
                    hubContentPath = xml.Element("Hub").Value;
                    if (!File.Exists(this.rootpath + "\\" + this.hubContentPath))
                    { throw new FileNotFoundException("Hub content file not found."); }
                }
            }
            catch (Exception ex)
            {
                WidgetInfo widgetinfo = null;
                try { widgetinfo = ((WidgetInfo)XmlSerializable.Load(typeof(WidgetInfo), this.rootpath + "\\Widget.xml")); }
                catch { }
                if (widgetinfo == null) widgetinfo = new WidgetInfo();

                HelperMethods.ShowErrorMessage(
                    "Fatal error while loading a HTML widget.\nPlease contact widget provider to get more information.\n\n" +
                    "Widget Name: \t" + widgetinfo.Name + "\n" +
                    "Widget ID: \t" + widgetinfo.ID + "\n" +
                    "Widget Author: \t" + widgetinfo.Author + "\n" +
                    "Widget Website: \t" + widgetinfo.AuthorWeb + "\n" +
                    "Widget Error: \t" + ex.Message);

                throw ex;
            }
        }

        public override void Load()
        {
            this.widgetcontrol = new Grid();
            this.widgetcontrol.Background = new SolidColorBrush(Colors.Black);
            this.widgetcontrol.Children.Add(new Image()
            {
                Stretch = Stretch.Fill,
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(this.rootpath + "\\" + this.contentfbimgpath))
            });
            this.widgetcontrol.Children.Add(new System.Windows.Shapes.Rectangle()
            {
                Stroke = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#71FFFFFF")),
                StrokeThickness = 1
            });
            this.tilebroswerview = new Image()
            {
                Stretch = Stretch.Fill,
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(this.rootpath + "\\" + this.contentfbimgpath))
            };
            this.widgetcontrol.Children.Add(this.tilebroswerview);

            this.tilebroswer = new WebKitBrowser();
            this.tilebroswer.Navigate(this.GetWidgetUrl(this.contentpath));

            this.widgetcontrol.Children.Add(new System.Windows.Shapes.Rectangle()
            {
                Stroke = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#71FFFFFF")),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
            });

            if (this.widgetcontrol.ContextMenu == null) this.widgetcontrol.ContextMenu = new ContextMenu();

            if (!string.IsNullOrWhiteSpace(this.optionshubcontentpath))
            {
                MenuItem mi_settings = new MenuItem();
                mi_settings.Header = "Settings / Options";
                mi_settings.Click += new System.Windows.RoutedEventHandler((oo, aa) => { this.OpenHub(this.optionshubcontentpath); });
                this.widgetcontrol.ContextMenu.Items.Add(mi_settings);
            }

            loop = Helper.RunFor(() => this.UpdateTile(), -1, 1000);

            this.widgetcontrol.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler((oo, aa) =>
            {
                this.OpenHub(this.hubContentPath);
            });
        }

        public override void Unload()
        {
            if (this.loop != null) this.loop.Stop();
            this.loop = null;
            this.hub = null;
            this.tilebroswer.Dispose();
            this.tilebroswerview = null;
            this.widgetcontrol = null;
        }

        private void UpdateTile()
        {
            try
            {
                this.tilebroswer.Width = (int)this.widgetcontrol.ActualWidth;
                this.tilebroswer.Height = (int)this.widgetcontrol.ActualHeight;
                System.Drawing.Bitmap fbc = new System.Drawing.Bitmap((int)this.tilebroswer.Width, (int)this.tilebroswer.Height);
                this.tilebroswer.DrawToBitmap(fbc, new System.Drawing.Rectangle(0, 0, (int)this.tilebroswer.Width, (int)this.tilebroswer.Height));

                this.tilebroswerview.Source = HelperMethods.CreateBitmapSourceFromBitmap(fbc);
            }
            catch { }
        }

        private void OpenHub(string hubfile)
        {
            this.hub = new HubWindow();
            WebKitBrowser hubbroswer = new WebKitBrowser();
            WindowsFormsHost host = new WindowsFormsHost();
            this.hub.Content = host;
            host.Child = hubbroswer;

            hubbroswer.Navigate(this.GetWidgetUrl(hubfile));

            if (E.Language == "he-IL" || E.Language == "ar-SA") { hub.FlowDirection = System.Windows.FlowDirection.RightToLeft; }
            else { hub.FlowDirection = System.Windows.FlowDirection.LeftToRight; }

            hubbroswer.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(
                (oo, aa) =>
                {
                    try
                    {
                        if (aa.Url.OriginalString.Contains("$Newgen::CloseHub"))
                        {
                            aa.Cancel = true;
                            this.hub.Close();
                        }
                    }
                    catch { }
                });

            this.hub.ShowDialog();

            this.hub.Closed += new EventHandler((oo, aa) =>
            {
                hubbroswer.Dispose();

                this.hub = null;
            });
        }

        private string GetWidgetUrl(string file)
        {
            return string.Format("http://{0}/Widget:{1};{2}", App.LocalServer.CurrentAddress, this.name, file);
        }

        public void Dispose()
        {
            tilebroswer.Dispose();
        }
    }
}
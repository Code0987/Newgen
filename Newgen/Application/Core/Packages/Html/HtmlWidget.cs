using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Newgen.Base;

namespace Newgen.Core {

    
                //// Get html package
                //filePaths = Directory.GetFiles(packageFolder, E.HTMLWidgetMetadataFile, SearchOption.TopDirectoryOnly);
                //foreach (var filePath in filePaths) {
                //    var w = (new PackageProxy()).Initialize(new DirectoryInfo(packageFolder).Name, filePath, TileCellType.Html);
                //    if (w.HasErrors)
                //        continue;
                //    Packages.Add(w);
                //    break; // Only one widgets per package !
                //}

    public class NewgenHtmlWidget : Package, IDisposable {
        private WebBrowser broswer;
        private int colspan;
        private string contentpath;
        private HubWindow hub;
        private string hubContentPath;
        private string iconpath;
        private string name;
        private string optionshubcontentpath;
        private string rootpath;
        private Grid widgetcontrol;

        public override int ColumnSpan { get { return colspan; } }

        public override Uri IconPath { get { return new Uri(this.iconpath); } }

        public override string Id { get { return name; } }

        public override FrameworkElement Tile { get { return widgetcontrol; } }

        public NewgenHtmlWidget(string path) {
            rootpath = path;

            var metadataFile = rootpath + "\\" + E.HTMLWidgetMetadataFile;
            if (!File.Exists(metadataFile)) {
                throw new FileNotFoundException("HTML Widget definition file not found.");
            }

            try {
                var xml = XElement.Load(metadataFile);

                try {
                    name = xml.Element("Name").Value;
                }
                catch {
                    throw new Exception("Error while allocating widget 'Name'.");
                }

                try {
                    this.colspan = int.Parse(xml.Element("CSpan").Value);
                }
                catch {
                    throw new Exception("Error while allocating widget 'CSpan'.");
                }

                try {
                    this.contentpath = xml.Element("Content").Value;
                }
                catch {
                    throw new Exception("Error while allocating widget 'Content'.");
                }

                if (!File.Exists(rootpath + "\\" + contentpath)) {
                    throw new FileNotFoundException("Content file not found.");
                }

                if (xml.Element("Icon") != null) {
                    iconpath = rootpath + "\\" + xml.Element("Icon").Value;
                }

                if (xml.Element("Options") != null) {
                    this.optionshubcontentpath = xml.Element("Options").Value;
                    if (!File.Exists(rootpath + "\\" + optionshubcontentpath)) {
                        throw new FileNotFoundException("Options content file not found.");
                    }
                }
                if (xml.Element("Hub") != null) {
                    hubContentPath = xml.Element("Hub").Value;
                    if (!File.Exists(rootpath + "\\" + hubContentPath)) {
                        throw new FileNotFoundException("Hub content file not found.");
                    }
                }
            }
            catch (Exception ex) {
                var widgetinfo = (PackageMetadata)null;
                try {
                    widgetinfo = ((PackageMetadata)XmlSerializable.Load(typeof(PackageMetadata), this.rootpath + "\\PackageMetadata.xml"));
                }
                catch { }
                if (widgetinfo == null)
                    widgetinfo = new PackageMetadata();

                Helper.ShowErrorMessage(
                    "Fatal error while loading a HTML widget.\nPlease contact widget provider to get more information.\n\n" +
                    "Name: \t" + widgetinfo.Name + "\n" +
                    "Id: \t" + widgetinfo.Id + "\n" +
                    "Author: \t" + widgetinfo.Author + "\n" +
                    "Website: \t" + widgetinfo.AuthorWebsite + "\n" +
                    "Error: \t" + ex.Message);

                throw ex;
            }
        }

        public void Dispose() {
            broswer.Dispose();
        }

        public override void Load(dynamic proxy) {
            var widgetProxy = (PackageProxy)proxy;

            this.widgetcontrol = new Grid();
            this.widgetcontrol.Background = new SolidColorBrush(Colors.Black);

            this.broswer = new WebBrowser();
            this.broswer.Navigate(this.GetWidgetUrl(this.contentpath));
            this.widgetcontrol.Children.Add(this.broswer);

            this.widgetcontrol.Children.Add(new System.Windows.Shapes.Rectangle() {
                Stroke = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#71FFFFFF")),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
            });

            if (this.widgetcontrol.ContextMenu == null)
                this.widgetcontrol.ContextMenu = new ContextMenu();

            if (!string.IsNullOrWhiteSpace(this.optionshubcontentpath)) {
                MenuItem mi_settings = new MenuItem();
                mi_settings.Header = "Settings / Options";
                mi_settings.Click += new System.Windows.RoutedEventHandler((oo, aa) => { this.OpenHub(this.optionshubcontentpath); });
                this.widgetcontrol.ContextMenu.Items.Add(mi_settings);
            }

            this.widgetcontrol.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler((oo, aa) => {
                this.OpenHub(this.hubContentPath);
            });
        }

        public override void Unload(dynamic proxy) {
            var widgetProxy = (PackageProxy)proxy;

            hub = null;
            broswer.Dispose();
            broswer = null;
            widgetcontrol = null;
        }

        private string GetWidgetUrl(string file) {
            return string.Format("http://{0}/Widget:{1};{2}", PackageServer.Current.Address, name, file);
        }

        private void OpenHub(string hubfile) {
            hub = new HubWindow();
            var hubbroswer = new WebBrowser();
            hub.Content = hubbroswer;

            hubbroswer.Navigate(this.GetWidgetUrl(hubfile));

            // HACK: Language fix
            if (E.Language == "he-IL" || E.Language == "ar-SA")
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;

            hubbroswer.Navigating += (object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) => {
                if (e.Uri.OriginalString.Contains("$Newgen::CloseHub")) {
                    e.Cancel = true;
                    hub.Close();
                }
            };

            hub.ShowDialog();

            hub.Closed += new EventHandler((oo, aa) => {
                hubbroswer.Dispose();
                hub = null;
            });
        }
    }
}
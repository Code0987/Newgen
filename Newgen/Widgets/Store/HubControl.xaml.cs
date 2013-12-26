using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Newgen.Base;

namespace Store
{
    /// <summary>
    /// Interaction logic for HubControl.xaml
    /// </summary>
    public partial class HubControl : UserControl
    {
        public event EventHandler Closing;

        private HubWindow wis;

        public HubControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.Delay(() => { GetWidgets(); }, 2000);
        }

        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(wis != null && wis.IsVisible)
            {
                wis.Activate();
                return;
            }
            StoreItem control = (StoreItem)sender;
            WidgetItem wi = new WidgetItem
            {
                ID = control.ID,
                Title = control.Title,
                Author = control.Author,
                AuthorWeb = control.AuthorWeb,
                Description = control.Description,
                Version = control.Version,
                Icon = control.Icon,
                WidgetURL = control.WidgetURL
            };

            wis = new HubWindow()
           {
               Topmost = false,
               AllowsTransparency = false,
               Content = wi
           };
            wi.Closing += new EventHandler(wi_Closing);
            wis.ShowDialog();
        }

        private void wi_Closing(object sender, System.EventArgs e)
        {
            try
            {
                wis.Close();
            }
            catch { }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if(Closing != null)
                Closing(this, EventArgs.Empty);
        }

        private void GetWidgets()
        {
            ThreadStart start = delegate()
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
               {
                   ProgressBar.IsIndeterminate = true;
                   XElement xml = null;

                   try
                   {
                       Task.Factory.StartNew(() => xml = XElement.Load(Widget.WidgetsBase + "meta.xml")).Wait();
                   }
                   catch
                   {
                       MessageBox.Show(E.MSG_NE, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                   }

                   if(!Directory.Exists(Widget.widgetfolder + "$[Cache]"))
                       Directory.CreateDirectory(Widget.widgetfolder + "$[Cache]");

                   try
                   {
                       System.Threading.Tasks.Parallel.ForEach<XElement>(
                           xml.Descendants("Widget"),
                           (element, e) =>
                           {
                               Thread.Sleep(1500);

                               Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                   {
                                       string id = "";
                                       try
                                       {
                                           id=element.Attribute("Id").Value;
                                       }
                                       catch { id=element.Attribute("ID").Value; }
                                       string name = element.Attribute("Name").Value;
                                       string author = element.Attribute("Author").Value;
                                       string authorweb = element.Attribute("AuthorWeb").Value;
                                       string description = element.Attribute("Description").Value;
                                       string version = element.Attribute("Version").Value;

                                       string iconpath = Widget.widgetfolder + "$[Cache]\\Icon_" + name + ".png";

                                       WebClient client = new WebClient();

                                       StoreItem control = new StoreItem
                                       {
                                           ID = id,
                                           Title = name,
                                           Author = author,
                                           AuthorWeb = authorweb,
                                           Description = description,
                                           Version = version,
                                           WidgetURL = Widget.WidgetsBase + name + ".nwp"
                                       };

                                       client.DownloadFileCompleted += (a, b) =>
                                       {
                                           try { if(b.Error != null)control.Icon = new BitmapImage(new Uri(iconpath)); }
                                           catch { }
                                       };

                                       if(!File.Exists(iconpath))
                                       {
                                           client.DownloadFileAsync(new Uri(Widget.WidgetsBase + name + ".png"), iconpath);
                                       }
                                       else
                                       {
                                           try { control.Icon = new BitmapImage(new Uri(iconpath)); }
                                           catch { }
                                       }

                                       control.MouseLeftButtonUp += new MouseButtonEventHandler(this.ItemMouseLeftButtonUp);
                                       if(Widget.IsWidgetInstalled(control.Title))
                                       {
                                           InstalledWidgets.Children.Add(control);
                                           if(Widget.IsWidgetUpdateAvailable(control.Title, control.Version))
                                           {
                                               StoreItem updateitem = new StoreItem
                                               {
                                                   ID = control.ID,
                                                   Title = control.Title,
                                                   Author = control.Author,
                                                   AuthorWeb = control.AuthorWeb,
                                                   Description = control.Description,
                                                   Version = control.Version,
                                                   WidgetURL = Widget.WidgetsBase + control.Title + ".nwp"
                                               };
                                               client.DownloadFileCompleted += (a, b) =>
                                               {
                                                   if(b.Error != null)
                                                       updateitem.Icon = new BitmapImage(new Uri(iconpath));
                                               };
                                               client.DownloadFileAsync(new Uri(Widget.WidgetsBase + name + ".png"), iconpath);
                                               updateitem.MouseLeftButtonUp += new MouseButtonEventHandler(this.ItemMouseLeftButtonUp);
                                               WidgetUpdates.Children.Add(updateitem);
                                           }
                                       }
                                       else
                                       {
                                           AvailableWidgets.Children.Add(control);
                                       }
                                   }), DispatcherPriority.Background, null);
                           }
                           );
                   }
                   catch
                   {
                       MessageBox.Show(E.MSG_NE, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                   }
                   ProgressBar.IsIndeterminate = false;
               }), DispatcherPriority.Background, null);
            };
            new Thread(start).Start();
        }
    }
}
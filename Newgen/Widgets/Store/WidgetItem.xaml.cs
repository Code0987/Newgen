using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newgen.Base;

namespace Store
{
    /// <summary>
    /// Interaction logic for WidgetInfo.xaml
    /// </summary>
    public partial class WidgetItem : UserControl
    {
        public event EventHandler Closing;

        public string WidgetURL { get; set; }

        public string ID
        {
            get
            {
                return this.IDTextBlock.Text;
            }
            set
            {
                this.IDTextBlock.Text = "ID : " + value;
            }
        }

        public string Title
        {
            get
            {
                return this.TitleTextBlock.Text;
            }
            set
            {
                this.TitleTextBlock.Text = value;
            }
        }

        public string Author
        {
            get
            {
                return this.AuthorTextBlock.Text;
            }
            set
            {
                this.AuthorTextBlock.Text = "Author : " + value;
            }
        }

        public string AuthorWeb { get; set; }

        public string Description
        {
            get
            {
                return this.DesTextBlock.Text;
            }
            set
            {
                this.DesTextBlock.Text = value;
            }
        }

        private string version;

        public string Version
        {
            get
            {
                return this.VersionTextBlock.Text;
            }
            set
            {
                this.VersionTextBlock.Text = "Version : " + value;
                this.version = value;
            }
        }

        public string Size
        {
            get
            {
                return this.SizeTextBlock.Text;
            }
            set
            {
                this.SizeTextBlock.Text = value;
            }
        }

        private string work = "install";

        public ImageSource Icon
        {
            get
            {
                return this.IconImage.Source;
            }
            set
            {
                this.IconImage.Source = value;
            }
        }

        public WidgetItem()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;

            if (!Widget.IsWidgetInstalled(Title))
            {
                Btn_Fn.Content = "install"; work = "install";
            }
            else if (Widget.IsWidgetInstalled(Title))
            {
                Btn_Fn.Content = "remove"; work = "remove";
            }
            if (Widget.IsWidgetUpdateAvailable(Title, version))
            {
                Btn_Fn.Content = "update"; work = "update";
            }

            Helper.Delay(() =>
            {
                ThreadStart start = delegate()
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                       {
                           try
                           {
                               switch (work)
                               {
                                   case "install":
                                   case "update":
                                       try
                                       {
                                           ProgressBar.IsIndeterminate = true;
                                           System.Net.WebResponse resp = null;
                                           System.Net.WebRequest req = System.Net.HttpWebRequest.Create(this.WidgetURL);
                                           req.Method = "HEAD";
                                           resp = req.GetResponse();
                                           int ContentLength;
                                           if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                                           {
                                               if (ContentLength >= 1048576) { this.Size = ("Size : " + ((ContentLength / 1048576).ToString("0.00")) + " mb"); }
                                               else { this.Size = ("Size : " + ((ContentLength / 1024).ToString("0.00")) + " kb"); }
                                           }
                                           else { this.Size = ("Size : !ERROR"); }
                                       }
                                       catch { this.Size = ("Size : !ERROR"); }
                                       ProgressBar.IsIndeterminate = false;
                                       break;
                                   case "remove":
                                       {
                                           try
                                           {
                                               ProgressBar.IsIndeterminate = true;

                                               long ContentLength = 0;
                                               string[] files = Directory.GetFiles(Newgen.Base.E.WidgetsRoot + Title, "*", SearchOption.AllDirectories);
                                               foreach (string file in files) ContentLength += new FileInfo(file).Length;

                                               if (ContentLength >= 1048576)
                                               {
                                                   SizeTextBlock.Text = "Size : " + ((ContentLength / 1048576).ToString("0.00")) + " mb";
                                               }
                                               else
                                               {
                                                   SizeTextBlock.Text = "Size : " + ((ContentLength / 1024).ToString("0.00")) + " kb";
                                               }

                                               ProgressBar.IsIndeterminate = false;
                                           }
                                           catch { }
                                       }
                                       break;
                                   default:
                                       break;
                               }
                           }
                           catch
                           {
                               MessageBox.Show(E.MSG_NE, "// Newgen / : Error", MessageBoxButton.OK, MessageBoxImage.Error);
                           }
                       }), DispatcherPriority.Background, null);
                    };
                new Thread(start).Start();
            }, 1500);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Closing != null)
                Closing(this, EventArgs.Empty);
        }

        private void AuthorTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen((AuthorWeb.Contains("mailto:") ? "Link.Mail" : "URL"), AuthorWeb);
        }

        private void Btn_Fn_Click(object sender, RoutedEventArgs e)
        {
            switch (work)
            {
                case "install":
                case "update":
                    {
                        ThreadStart start = delegate()
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                           {
                               Btn_Fn.IsEnabled = false;
                               ProgressBar.IsIndeterminate = true;
                               WebClient client = new WebClient();
                               string tempdown = Path.GetTempFileName();

                               client.DownloadFileCompleted += (a, b) =>
                               {
                                   try
                                   {
                                       Newgen.Base.MessagingHelper.SendMessageToNewgen("InstallWidget", tempdown + "," + Title);
                                       Btn_Fn.IsEnabled = true;
                                       Btn_Fn.Content = "installed";
                                       Btn_Fn.IsEnabled = false;
                                       ProgressBar.IsIndeterminate = false;
                                       StatusImage.Source = new BitmapImage(new Uri("/Store;component/Resources/check.png", UriKind.Relative));
                                   }
                                   catch { StatusImage.Source = new BitmapImage(new Uri("/Store;component/Resources/error.png", UriKind.Relative)); }
                               };

                               client.DownloadProgressChanged += (a, b) =>
                               {
                                   ProgressBar.IsIndeterminate = false;
                                   ProgressBar.Value = b.ProgressPercentage;
                               };

                               try { client.DownloadFileAsync(new Uri(WidgetURL), tempdown); }
                               catch { StatusImage.Source = new BitmapImage(new Uri("/Store;component/Resources/error.png", UriKind.Relative)); }
                           }), DispatcherPriority.Background, null);
                        };
                        new Thread(start).Start();
                    }
                    break;
                case "remove":
                    ThreadStart startremove = delegate()
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                       {
                           try
                           {
                               Btn_Fn.IsEnabled = false;
                               ProgressBar.IsIndeterminate = true;
                               Newgen.Base.MessagingHelper.SendMessageToNewgen("RemoveWidget", Title);
                               Thread.Sleep(500);
                               Btn_Fn.IsEnabled = true;
                               Btn_Fn.Content = "removed";
                               Btn_Fn.IsEnabled = false;
                               ProgressBar.IsIndeterminate = false;
                               StatusImage.Source = new BitmapImage(new Uri("/Store;component/Resources/check.png", UriKind.Relative));
                           }
                           catch { StatusImage.Source = new BitmapImage(new Uri("/Store;component/Resources/error.png", UriKind.Relative)); }
                       }), DispatcherPriority.Background, null);
                    };
                    new Thread(startremove).Start();
                    break;
                default:
                    break;
            }
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Controls;

namespace Newgen.Core
{
    public class NewgenFriendWidget : NewgenWidget, IDisposable
    {
        private NewgenFriendWidgetControl control;
        private WebClient webClient;
        private string id;

        public override string Name
        {
            get { return null; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return control; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string id, string name, int seed)
        {
            this.id = id;
            control = new NewgenFriendWidgetControl(seed);
            control.UserName.Text = name;
            control.MouseLeftButtonUp += ControlMouseLeftButtonUp;
            control.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f92d6"));

            if (!File.Exists(E.CacheRoot + id + ".png"))
            {
                webClient = new WebClient();
                webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=large", id)), E.CacheRoot + id + ".png");
            }
            else
            {
                control.UserPic.Source = new BitmapImage(new Uri(E.CacheRoot + id + ".png"));
            }
        }

        private void ControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hub = new NewgenFriendWidgetHub(id);
            hub.ShowDialog();
        }

        public override void Unload()
        {
            control.MouseLeftButtonUp -= ControlMouseLeftButtonUp;
        }

        private void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            webClient.DownloadFileCompleted -= WebClientDownloadFileCompleted;
            if (e.Error != null)
                return;
            control.UserPic.Source = new BitmapImage(new Uri(E.CacheRoot + id + ".png"));
        }

        public void Dispose()
        {
            webClient.Dispose();
        }
    }
}
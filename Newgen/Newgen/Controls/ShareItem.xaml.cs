using System;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Core;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for ShareItem.xaml
    /// </summary>
    public partial class ShareItem : UserControl, IDisposable
    {
        public bool MousePressed;
        private WebClient webClient;

        public ShareItem()
        {
            InitializeComponent();
        }

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value)
                {
                    Title.Foreground = Brushes.Gray;
                    Image.Opacity = 0.4;
                }
                else
                {
                    Title.Foreground = Brushes.White;
                    Image.Opacity = 1;
                }
                isChecked = value;
            }
        }

        private Friend friend;

        public Friend Friend
        {
            get { return friend; }
            set
            {
                friend = value;
                if (File.Exists(E.CacheRoot + friend.Id + "_s.png"))
                    Image.Source = new BitmapImage(new Uri(E.CacheRoot + friend.Id + "_s.png"));
                else
                {
                    webClient = new WebClient();
                    webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=square", value.Id)), E.CacheRoot + value.Id + "_s.png");
                }
                //Image.Source = new BitmapImage(new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=square", value.Id)));
                Title.Text = value.Name;
            }
        }

        private void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            webClient.DownloadFileCompleted -= new System.ComponentModel.AsyncCompletedEventHandler(WebClientDownloadFileCompleted);
            webClient.Dispose();
            Image.Source = new BitmapImage(new Uri(E.CacheRoot + friend.Id + "_s.png"));
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            MousePressed = true;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
            if (!IsChecked)
                IsChecked = true;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!MousePressed)
                return;
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }

        public void Dispose()
        {
            webClient.Dispose();
        }
    }
}
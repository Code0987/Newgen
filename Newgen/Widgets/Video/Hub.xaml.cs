using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newgen.Base;

namespace Video
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        private bool isplaying = false;

        public event EventHandler Close;

        private List<Category> categories;

        public Hub()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Delay(new Action(() =>
            {
                categories = Widget.Settings.GetVideos().Item1;

                foreach(var category in categories)
                {
                    var control = new VideoCategoryControl();
                    control.Initialize(category);
                    VideosPanel.Children.Add(control);
                }

                VideoPlayer_Player.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(VideoPlayer_Player_MediaFailed);
                VideoPlayer_Player.MediaEnded += new RoutedEventHandler(VideoPlayer_Player_MediaEnded);
                VideoPlayer_Player.MediaOpened += new RoutedEventHandler(VideoPlayer_Player_MediaOpened);
            }), 500);
        }

        private void VideoPlayer_Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/pause.png", UriKind.Relative));
            isplaying = true;
        }

        private void VideoPlayer_Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                isplaying = false;
                VideoPlayer_Player.Play();
                isplaying = true;
            }
            catch { }
        }

        private void VideoPlayer_Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            isplaying = false;
            MessageBox.Show("An error occurred. Cannot play Video !", "// Newgen / : Error", MessageBoxButton.YesNo);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close(this, EventArgs.Empty);
        }

        internal void PlayVideo(Uri source)
        {
            try
            {
                VideoPlayer.Visibility = Visibility.Visible;
                VideoPlayer_Player.Source = source;
                VideoPlayer_Player.Play();
                isplaying = true;
            }
            catch { }
        }

        private void VideoPlayerBackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Visibility = Visibility.Collapsed;
                VideoPlayer_Player.Stop();
                VideoPlayer_Player.Source = null;
            }
            catch { }
        }

        private void VideoPlayer_PnPButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(VideoPlayer_Player.CanPause && isplaying)
                {
                    VideoPlayer_Player.Pause();
                    VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/play.png", UriKind.Relative));
                    isplaying = false;
                    return;
                }
                else if(VideoPlayer_Player.CanPause && !isplaying)
                {
                    VideoPlayer_Player.Play();
                    VideoPlayer_PnPButton.Source = new BitmapImage(new Uri("/Video;component/Resources/pause.png", UriKind.Relative));
                    isplaying = true;
                    return;
                }
            }
            catch { }
        }

        private void VideoPlayer_BkButton_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            try
            {
                for(int i = 0; i < categories.Count; i++)
                {
                    for(int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if(VideoPlayer_Player.Source.OriginalString == categories[i].Files[io])
                        {
                            PlayVideo(new Uri(categories[i].Files[(io - 1 == -1) ? 0 : io - 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }

        private void VideoPlayer_NextButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for(int i = 0; i < categories.Count; i++)
                {
                    for(int io = 0; io < categories[i].Files.Count; io++)
                    {
                        if(VideoPlayer_Player.Source.OriginalString == categories[i].Files[io])
                        {
                            PlayVideo(new Uri(categories[i].Files[(io + 1 == categories[i].Files.Count) ? io : io + 1], UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }
    }
}
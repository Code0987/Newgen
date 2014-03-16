using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newgen.Base;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for PicturesWidget.xaml
    /// </summary>
    public partial class PicturesWidget : UserControl
    {
        private List<string> pictures = new List<string>();
        private Random random;
        private DispatcherTimer timer;
        private HubWindow hub;
        private Hub hubContent;

        public PicturesWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            try
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Tick += new EventHandler(TimerTick);
                timer.Start();

                pictures = Widget.Settings.GetImages(20).Item2;

                random = new Random(Environment.TickCount);
                if(pictures.Count > 0)
                    LoadPicture(pictures[random.Next(0, pictures.Count - 1)], Picture);
            }
            catch { }
        }

        private void LoadPicture(string path, Image image)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();

                bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);

                bi.DecodePixelWidth = (int)E.MinTileWidth * 2;
                bi.EndInit();

                image.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    try
                    {
                        image.Source = bi;
                    }
                    catch
                    {
                    }
                });
            }
            catch { }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                if(pictures.Count <= 0)
                    return;
                LoadPicture(pictures[random.Next(0, pictures.Count - 1)], PictureBg);

                var s = (Storyboard)Resources["SwitchPictureAnim"];
                s.Begin();
            }
            catch { }
        }

        private void SwitchAnimationCompleted(object sender, EventArgs e)
        {
            try
            {
                Picture.Source = PictureBg.Source;
                PictureBg.Source = null;
            }
            catch { }
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.AllowsTransparency = true;
            hubContent = new Hub();
            hub.Content = hubContent;
            hubContent.Close += HubContentClose;

            if(E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hub.ShowDialog();
        }

        private void HubContentClose(object sender, EventArgs e)
        {
            hubContent.Close -= HubContentClose;
            hub.Close();
        }

        private void MenuItem_AddLookupFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            Widget.Settings.LookupDirectories.Add(dlg.SelectedPath);
        }

        private void MenuItem_ClearLookupFolders_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Widget.Settings.LookupDirectories.Clear();
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newgen.Base;

namespace Socialite
{
    /// <summary>
    /// Interaction logic for SocialiteWidget.xaml
    /// </summary>
    public partial class SocialiteWidget : UserControl
    {
        private Random r;
        private DispatcherTimer tileAnimTimer;
        private DispatcherTimer updateModeTimer;
        public static string[] Files;
        private HubWindow hub;
        private Hub hubContent;

        public SocialiteWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            r = new Random(Environment.TickCount);
            Files = Directory.GetFiles(E.CacheRoot, "*_s.png");
            for (var i = 0; i < 9; i++)
            {
                var item = new UserPicControl();
                if (Files.Length > 0)
                {
                    var file = Files[r.Next(0, Files.Count())];
                    //item.UserPic.Source = new BitmapImage(new Uri(file));
                    item.Source = file;
                    item.Order = r.Next(0, 9);
                    item.Mode = r.Next(0, 4);
                }
                else
                    item.Mode = r.Next(0, 2);
                Host.Children.Add(item);
            }

            updateModeTimer = new DispatcherTimer();
            updateModeTimer.Interval = TimeSpan.FromMilliseconds(500);
            updateModeTimer.Tick += UpdateModeTimerTick;

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(10);
            tileAnimTimer.Tick += TileAnimTimerTick;

            tileAnimTimer.Start();
        }

        private int scanCounter = 5; //used to rescan cache folder with avatars but not very often
        private void TileAnimTimerTick(object sender, EventArgs e)
        {
            scanCounter--;
            if (scanCounter <= 0)
            {
                scanCounter = 5;
                Files = Directory.GetFiles(E.CacheRoot, "*_s.png");
            }
            updateModeTimer.Start();
        }

        private int updateCount;

        private void UpdateModeTimerTick(object sender, EventArgs e)
        {
            foreach (UserPicControl item in Host.Children)
            {
                if (Files.Length > 0)
                {
                    var file = Files[r.Next(0, Files.Count())];
                    item.UpdateMode(r.Next(0, 4), file);
                }
                else
                    item.UpdateMode(r.Next(0, 2), null);
            }
            updateCount++;
            if (updateCount > 2)
            {
                updateModeTimer.Stop();
                updateCount = 0;
            }
        }

        public void Unload()
        {
            tileAnimTimer.Stop();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.AllowsTransparency = true;
            hub.Topmost = false;
            hub.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            hubContent = new Hub();
            hubContent.Close += HubContentClose;
            hub.Content = hubContent;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hub.Topmost = false;
            hub.ShowDialog();
        }

        private void HubContentClose(object sender, EventArgs e)
        {
            hubContent.Close -= HubContentClose;
            hub.Close();
        }
    }
}
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using libns.Native;

namespace Newgen.Windows
{
    public partial class LockScreen : UserControl
    {
        public event EventHandler Unlocked;
        private DispatcherTimer timer;
        private DispatcherTimer udt;
        private bool ison = false;

        public LockScreen()
        {
            InitializeComponent();
        }

        private string wallpaperPath;

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            UpdateWallpaper();
            udt = Helper.RunFor(new Action(UpdateWallpaper), -1, 2000);
            udt.Stop();
            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Month.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
            Time.Text = DateTime.Now.ToShortTimeString();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(TimerTick);
            timer.Stop();

            this.Visibility = Visibility.Collapsed;

            if (Settings.Current.LockScreenTime != -1)
            {
                Helper.RunFor(new Action(() =>
                {
                    if ((int)WinAPI.GetIdleTime() >= (Settings.Current.LockScreenTime * 60 * 1000))
                    {
                        Lock();
                    }
                }), -1, 2000);
            }
        }

        private void UpdateWallpaper()
        {
            LockScreenBg.Source = null;

            if (!File.Exists(wallpaperPath))
            { return; }

            LockScreenBg.Source = E.GetBitmap(wallpaperPath);

            var power = System.Windows.Forms.SystemInformation.PowerStatus;
            if (power.BatteryChargeStatus == System.Windows.Forms.BatteryChargeStatus.NoSystemBattery)
                BatteryIcon.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                BatteryIcon.Visibility = System.Windows.Visibility.Visible;
                var iconNumber = (int)(power.BatteryLifePercent * 10) + 1;
                if (iconNumber >= 10)
                    BatteryIcon.Source = new BitmapImage(new Uri("/Resources/Icons/batt10.png", UriKind.Relative));
                else
                    BatteryIcon.Source = new BitmapImage(new Uri(string.Format("/Resources/batt{0}.png", iconNumber), UriKind.Relative));
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToShortTimeString();
        }

        private double mouseY;

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            mouseY = e.GetPosition((IInputElement)this.Parent).Y;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mouseY = 0;
            if (Translate.Y == 0)
            {
                var s = (Storyboard)Resources["JumpAnim"];

                s.Begin();
                return;
            }

            if (Translate.Y > -120)
            {
                var s = (Storyboard)Resources["MoveBackAnim"];

                s.Begin();
                return;
            }
            Unlock();
        }

        private void UserControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != this || e.LeftButton == MouseButtonState.Released)
                return;
            var y = e.GetPosition((IInputElement)this.Parent).Y;
            if (y >= mouseY)
                return;
            Translate.Y = y - mouseY;
        }

        private void UnlockAnimCompleted(object sender, EventArgs e)
        {
            if (Unlocked != null)
                Unlocked(this, EventArgs.Empty);
        }

        private void UserControlUnloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void MoveBackAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }

        public void Lock()
        {
            if (!ison)
            {
                ison = true;
                this.Visibility = Visibility.Visible;
                udt.Start();
                timer.Start();
                Translate.Y = -SystemParameters.PrimaryScreenHeight;
                var s = (Storyboard)Resources["MoveBackAnim"];
                s.Begin();
            }
        }

        public void Unlock()
        {
            ison = false;
            var s = (Storyboard)Resources["UnlockAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenHeight;
            s.Begin();
            timer.Stop();
            udt.Stop();
        }

        private void JumpAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }
    }
}
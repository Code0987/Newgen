using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Newgen.Base;

namespace Me
{
    /// <summary>
    /// Interaction logic for MeWidget.xaml
    /// </summary>
    public partial class MeWidget : UserControl
    {
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        public static extern int ExitWindowsEx(int uFlags, int dwReason);

        private const byte UP = 2;
        private const byte CTRL = 17;
        private const byte ESC = 27;

        private DispatcherTimer tileAnimTimer;

        public MeWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            Helper.Animate(OptionsTranslate, TranslateTransform.YProperty, 250, 0, this.Options.Height);
            Helper.Animate(Options, OpacityProperty, 250, 0);

            if (File.Exists(E.UserImage))
            {
                LoadUserPicFromCache();
            }
            else if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
            {
                File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.UserImage, true);
                LoadUserPicFromCache();
            }

            UserPic.Width = E.MinTileWidth;
            UserPic.Height = E.MinTileHeight;
            UserName.Text = Environment.UserName;

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(14);
            tileAnimTimer.Tick += TileAnimTimerTick;

            tileAnimTimer.Start();
        }

        private void LoadUserPicFromCache()
        {
            if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
            { File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.UserImage, true); }
            BitmapSource bs = E.GetBitmap(E.UserImage);
            UserPic.Source = bs == null ? UserPic.Source : bs;
        }

        public void Unload()
        {
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void UserControlDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void UserControlDrop(object sender, DragEventArgs e)
        {
            var file = from x in ((string[])e.Data.GetData(DataFormats.FileDrop, true))
                       where x.EndsWith(".png") || x.EndsWith(".jpg")
                       select x;
            var path = file.First();

            File.Copy(path, E.UserImage, true);
            LoadUserPicFromCache();
        }

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount >= 3)
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                if (!(bool)dialog.ShowDialog())
                    return;
                try
                {
                    File.Copy(dialog.FileName, E.UserImage, true);
                    LoadUserPicFromCache();
                    Newgen.Base.MessagingHelper.SendMessageToNewgen("Update", "UserInfo");
                }
                catch (Exception)
                {
                    MessageBox.Show("Problem with user account image.", "Error");
                }
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Helper.Animate(OptionsTranslate, TranslateTransform.YProperty, 250, 0.0);
            Helper.Animate(Options, OpacityProperty, 250, 1);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Helper.Animate(OptionsTranslate, TranslateTransform.YProperty, 250, 0.0, this.Options.Height);
            Helper.Animate(Options, OpacityProperty, 250, 0);
        }

        private void Button_Logoff_Click(object sender, RoutedEventArgs e)
        {
            try { ExitWindowsEx(0, 0); }
            catch { }
        }

        private void Button_Shutdown_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("shutdown.exe", "-s -f"); }
            catch { }
        }

        private void Button_Restart_Click(object sender, RoutedEventArgs e)
        {
            try { System.Diagnostics.Process.Start("shutdown.exe", "-r -f"); }
            catch { }
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Function depreciated due to stack imbalance
        }
    }
}
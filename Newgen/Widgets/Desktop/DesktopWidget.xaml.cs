using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newgen.Base;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for DesktopWidget.xaml
    /// </summary>
    public partial class DesktopWidget : UserControl
    {
        public DesktopWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            wallpaperPath = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            UpdateWallpaper();
            Helper.RunFor(new Action(UpdateWallpaper), -1, 2000);
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var desktopWindow = new DesktopWindow();
            desktopWindow.DesktopImage.Source = DesktopPreview.Source;
            desktopWindow.Show();
        }

        private string wallpaperPath;

        private void UpdateWallpaper()
        {
            try
            {
                DesktopPreview.Source = null;

                if (!File.Exists(wallpaperPath))
                { return; }

                DesktopPreview.Source = Newgen.Base.E.GetBitmap(wallpaperPath);
            }
            catch
            {
            }
        }
    }
}
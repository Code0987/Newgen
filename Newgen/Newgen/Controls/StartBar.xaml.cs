using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Newgen.Base;
using Newgen.Controls;
using Newgen.Hubs;
using Newgen.Native;

namespace Newgen.Windows
{
    public partial class StartBar : ToolbarWindow
    {
        private LeftClock leftClock = null;
        private DateTime mouseClickTimestamp = DateTime.Now;

        public StartBar()
            : base()
        {
            this.Location = ToolbarLocation.Right;
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.StartButton.IconImage.ImageFailed += (a, b) =>
            {
                this.StartButton.Icon = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("/Newgen;component/Resources/start_icon.png", UriKind.Relative)
                    );
            };
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach(var path in droppedFilePaths)
                    if(File.Exists(path))
                        App.WidgetManager.LoadWidget(App.WidgetManager.CreateWidget(path));
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if(!this.IsOpened)
                    this.OpenToolbar();
                else
                    this.CloseToolbar();
            }
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource == this.LayoutRoot ||
               e.OriginalSource == this.TouchDecorator ||
               e.OriginalSource == this.ContentDecorator ||
               e.OriginalSource == this.Toolbar)
            {
                if(!this.IsOpened)
                    this.OpenToolbar();
                else
                    this.CloseToolbar();
            }
        }

        private void SearchButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CloseToolbar();
            Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", "http://www.google.com/search?q=");
        }

        private void ShareButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CloseToolbar();
            (new ShareHub()).ShowDialog();
        }

        private void StartButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount >= 2)
            {
                App.StartScreen.Hide();
                this.CloseToolbar();
            }
            else
            {
                foreach(var window in App.Current.Windows)
                {
                    if(window.GetType() == typeof(HubWindow) ||
                       window.GetType() == typeof(SwitchAppPreview)
                      )
                        continue;
                    ((Window)window).Activate();
                    ((Window)window).Show();
                }
                foreach(WidgetControl c in App.StartScreen.runningWidgets)
                    Helper.Animate(c, OpacityProperty, 250, 0, 1);
            }
        }

        private void DevicesButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CloseToolbar();
            if(Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                Process.Start("explorer.exe", "shell:::{A8A91A66-3A7D-4424-8D24-04E180695C7A}");
            else
                Process.Start("explorer.exe", "e,::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        }

        private void SettingsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CloseToolbar();
            App.ShowOptions();
        }

        private void MenuItem_PinWeb_Click(object sender, RoutedEventArgs e)
        {
            this.CloseToolbar();
            AddressBarWindow bar = new AddressBarWindow();
            bar.ShowDialog();
            if(string.IsNullOrEmpty(bar.AddressBox.Text))
                return;
            App.WidgetManager.LoadWidget(App.WidgetManager.CreateWidget(bar.AddressBox.Text));
        }

        private void MenuItem_PinApp_Click(object sender, RoutedEventArgs e)
        {
            this.CloseToolbar();
            var dlg = new OpenFileDialog();
            dlg.Filter = "Any file|*.*";
            if(!(bool)dlg.ShowDialog())
                return;
            App.WidgetManager.LoadWidget(App.WidgetManager.CreateWidget(dlg.FileName));
        }

        private void MenuItem_PinDir_Click(object sender, RoutedEventArgs e)
        {
            this.CloseToolbar();
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            App.WidgetManager.LoadWidget(App.WidgetManager.CreateWidget(dlg.SelectedPath));
        }

        private void MenuItem_Tiles_Click(object sender, RoutedEventArgs e)
        {
            this.CloseToolbar();
            TilesToolbarWindow tiles = new TilesToolbarWindow();
            tiles.Show();
            tiles.Open();
        }

        private void MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            WinAPI.FlushMemory();
        }

        private void MenuItem_SS_Click(object sender, RoutedEventArgs e)
        {
            App.SaveSettings();
        }

        private void MenuItem_Help_Click(object sender, RoutedEventArgs e)
        {
            this.CloseToolbar();
            (new NewgenHelp()).ShowDialog();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Close();
        }

        public override void OpenToolbar()
        {
            if(!App.IsProMode)
            {
                this.ShareButton.Visibility = Visibility.Collapsed;
                this.DevicesButton.Visibility = Visibility.Collapsed;
                this.ShareButton.Visibility = Visibility.Collapsed;

                this.MenuItem_PinWeb.Visibility = Visibility.Collapsed;
                this.MenuItem_SS.Visibility = Visibility.Collapsed;
            }

            base.OpenToolbar();

            if(!App.Settings.DisableStartBarClock)
            {
                this.leftClock = new LeftClock();
                this.leftClock.Show();
            }

            var anim_userTile = App.StartScreen.UserTile.Resources["LeftAnim"] as Storyboard;
            ((DoubleAnimation)anim_userTile.Children[0]).To = -this.Width;
            ((DoubleAnimation)anim_userTile.Children[0]).AccelerationRatio = 0.7;
            ((DoubleAnimation)anim_userTile.Children[0]).DecelerationRatio = 0.3;
            anim_userTile.Begin();

            for(int i = 0; i < this.Toolbar.Children.Count; i++)
            {
                try
                {
                    ToolbarItem item = null;
                    int order = 1;
                    if(i < this.Toolbar.Children.Count / 2)
                    {
                        item = ((ToolbarItem)this.Toolbar.Children[i]);
                        order = this.Toolbar.Children.Count - i;
                    }
                    else
                    {
                        item = ((ToolbarItem)this.Toolbar.Children[i]);
                        order = 1;
                    }

                    item.Translate.X = 100.0;

                    DoubleAnimation doubleAnimation1 = new DoubleAnimation()
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(350)),
                        BeginTime = new TimeSpan?(
                            TimeSpan.FromMilliseconds((double)(order*40+10))
                            ),
                        AccelerationRatio = 0.3,
                        DecelerationRatio = 0.3,
                        FillBehavior = FillBehavior.Stop
                    };
                    doubleAnimation1.Completed += (a, b) =>
                    {
                        item.Translate.X = 0;
                    };
                    item.Translate.BeginAnimation(TranslateTransform.XProperty, doubleAnimation1);
                    Helper.Animate(item, OpacityProperty, 350, 0, 1);
                }
                catch { }
            }
        }

        public override void CloseToolbar()
        {
            if(App.Settings.ShowStartbarAlways)
                return;

            base.CloseToolbar();

            if(this.leftClock != null)
            {
                Helper.Animate(this.leftClock, OpacityProperty, 200, 0);
                Helper.Delay(new Action(() =>
                {
                    if(this.leftClock != null)
                        this.leftClock.Close();
                    this.leftClock = null;
                }), 200);
            }

            var anim_userTile = App.StartScreen.UserTile.Resources["LeftAnim"] as Storyboard;
            ((DoubleAnimation)anim_userTile.Children[0]).To = 0;
            ((DoubleAnimation)anim_userTile.Children[0]).AccelerationRatio = 0.3;
            ((DoubleAnimation)anim_userTile.Children[0]).DecelerationRatio = 0.7;
            anim_userTile.Begin();
        }
    }
}
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Newgen.Base;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for ThumbnailControl.xaml
    /// </summary>
    public partial class ThumbnailControl : UserControl
    {
        private string file;
        private bool isPressed;

        public ThumbnailControl()
        {
            InitializeComponent();
        }

        public void Initialize(string file)
        {
            this.file = file;
            ThreadStart threadStarter = delegate
                                            {
                                                var shellFile = ShellFile.FromFilePath(file);
                                                this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate
                                                {
                                                    ThumbnailImage.Source = shellFile.Thumbnail.BitmapSource;
                                                    /*if (!string.IsNullOrEmpty(shellFile.Properties.System.Title.Value))
                                                        Title.Text = shellFile.Properties.System.Title.Value;
                                                    Duration.Text = shellFile.Properties.System.Media.Duration.FormatForDisplay(PropertyDescriptionFormatOptions.ShortTime);*/
                                                });
                                                shellFile.Dispose();
                                            };
            var thread = new Thread(threadStarter);
            thread.Start();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            isPressed = true;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            isPressed = false;
            //var p = new ProcessStartInfo("explorer.exe");
            //p.Arguments = "/select, " + file;
            //p.WindowStyle = ProcessWindowStyle.Maximized;
            //Process.Start(p);

            try
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(HubWindow))
                    {
                        if (((HubWindow)window).Content.GetType() == typeof(Hub))
                        {
                            HubWindow w = (HubWindow)window;
                            Hub c = (Hub)w.Content;
                            c.OpenImg(new Uri(file, UriKind.Absolute));
                        }
                    }
                }
            }
            catch { }
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (isPressed)
            {
                var s = Resources["MouseUpAnim"] as Storyboard;
                s.Begin();
            }
        }
    }
}
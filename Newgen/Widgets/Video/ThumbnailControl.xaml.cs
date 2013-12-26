using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Newgen.Base;

namespace Video
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
            Title.Text = System.IO.Path.GetFileNameWithoutExtension(file);
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
            ThreadStart threadStarter = delegate
            {
                var shellFile = ShellFile.FromFilePath(file);
                this.Dispatcher.Invoke(DispatcherPriority.SystemIdle, (Action)delegate
                {
                    if (!string.IsNullOrEmpty(shellFile.Properties.System.Title.Value))
                        Title.Text = shellFile.Properties.System.Title.Value;
                    Duration.Text = shellFile.Properties.System.Media.Duration.FormatForDisplay(PropertyDescriptionFormatOptions.ShortTime);
                });
                shellFile.Dispose();
            };
            var thread = new Thread(threadStarter);
            thread.Start();
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
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Windows Media Player\\wmplayer.exe"))
            {
                //Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Windows Media Player\\wmplayer.exe", "/fullscreen \"" + file + "\"");

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
                                c.PlayVideo(new Uri(file, UriKind.Absolute));
                            }
                        }
                    }
                }
                catch { }
            }
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
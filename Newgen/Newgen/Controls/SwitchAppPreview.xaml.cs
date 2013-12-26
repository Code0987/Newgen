using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Newgen.Controls;
using Newgen.Native;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for SwitchAppPreview.xaml
    /// </summary>
    public partial class SwitchAppPreview : Window
    {
        public bool IsMulti { get; set; }

        public SwitchAppPreview()
        {
            InitializeComponent();
            WinAPI.RemoveFromDWM(this);
        }

        WinAPI.Margins margins = new WinAPI.Margins();

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(handle);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
            margins.cxLeftWidth = -1;
            margins.cxRightWidth = -1;
            margins.cyTopHeight = -1;
            margins.cyBottomHeight = -1;
            WinAPI.DwmExtendFrameIntoClientArea(handle, ref margins);
            WinAPI.RemoveWindowIcon(handle);

            Focus();
            Activate();

            if (!IsMulti)
            {
                Root.Margin = new Thickness(0);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ThumbsList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Switch();
        }

        private void ThumbsList_MouseMove(object sender, MouseEventArgs e)
        {
            Update();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        public void PreviewWindow(IntPtr window)
        {
            var text = WinAPI.GetText(window);

            TextTitle.Text = text;
            var thumb = new Thumbnail();
            thumb.Width = 200;
            thumb.Height = 120;
            thumb.Margin = IsMulti ? new Thickness(2) : new Thickness(00);
            thumb.Source = window;

            ThumbsList.Children.Add(thumb);

            Refresh();
        }

        public void Update()
        {
            var c = Mouse.GetPosition(ThumbsList);
            foreach (Thumbnail thumb in ThumbsList.Children.OfType<Thumbnail>())
            {
                var transform = thumb.TransformToVisual(ThumbsList);
                Point p = transform.Transform(new Point(0, 0));
                if (c.Y > p.Y && c.Y < p.Y + thumb.Height && c.X > p.X && c.X < p.X + thumb.Width)
                {
                    TextTitle.Text = WinAPI.GetText(thumb.Source);
                }
            }
        }

        public void Refresh()
        {
            foreach (Thumbnail thumb in ThumbsList.Children.OfType<Thumbnail>())
            {
                thumb.Width++;
                thumb.Width--;
                thumb.Height++;
                thumb.Height--;

                Width++;
                Width--;
                Height++;
                Height--;
            }
        }

        public void Switch()
        {
            var mouse = Mouse.GetPosition(ThumbsList);

            if (IsMulti)
            {
                foreach (Thumbnail thumb in ThumbsList.Children.OfType<Thumbnail>())
                {
                    var transform = thumb.TransformToVisual(ThumbsList);
                    Point p = transform.Transform(new Point(0, 0));
                    if (mouse.Y > p.Y && mouse.Y < p.Y + thumb.Height && mouse.X > p.X && mouse.X < p.X + thumb.Width)
                    {
                        WinAPI.SwitchToThisWindow(thumb.Source, true);

                        this.Close();
                        break;
                    }
                }
            }

            foreach (Thumbnail thumb in ThumbsList.Children.OfType<Thumbnail>())
            {
                WinAPI.SwitchToThisWindow(thumb.Source, true);

                this.Close();

                break;
            }
        }

        public void Release()
        {
            ThumbsList.Children.Clear();
        }
    }
}
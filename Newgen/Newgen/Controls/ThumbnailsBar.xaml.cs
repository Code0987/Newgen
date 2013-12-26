using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newgen.Controls;
using Newgen.Native;

namespace Newgen.Windows
{
    public partial class ThumbnailsBar : ToolbarWindow
    {
        public ThumbnailsBar()
            : base()
        {
            this.Location = ToolbarLocation.Left;
            this.InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.Width++;
            this.Width--;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.CloseToolbar();
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Topmost = true;
            if(!this.IsOpened)
            {
                this.OpenToolbar();
            }
        }

        private void InitializeThumbnails()
        {
            try
            {
                IntPtr handle = ((System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this)).Handle;
                IntPtr current = WinAPI.GetWindow(handle, WinAPI.GetWindowCmd.First);

                do
                {
                    int GWL_STYLE = -16;
                    uint normalWnd = 0x10000000 | 0x00800000 | 0x00080000;
                    uint popupWnd = 0x10000000 | 0x80000000 | 0x00080000;
                    var windowLong = WinAPI.GetWindowLong(current, GWL_STYLE);
                    var text = WinAPI.GetText(current);
                    if(((normalWnd & windowLong) == normalWnd || (popupWnd & windowLong) == popupWnd) && !string.IsNullOrEmpty(text))
                    {
                        var t = new TextBlock();
                        t.HorizontalAlignment = HorizontalAlignment.Center;
                        t.TextAlignment = TextAlignment.Center;
                        t.TextWrapping = TextWrapping.Wrap;
                        t.TextTrimming = TextTrimming.CharacterEllipsis;
                        t.MaxHeight = 46;
                        t.FontSize = 16;
                        t.Margin = new Thickness(0, 10, 0, 0);
                        t.Foreground = Brushes.White;
                        t.Text = text;
                        ThumbsList.Children.Add(t);

                        var thumb = new Thumbnail();
                        thumb.Width = 120;
                        thumb.Height = 120;
                        thumb.Source = current;
                        ThumbsList.Children.Add(thumb);
                    }

                    current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);

                    if(current == handle)
                        current = WinAPI.GetWindow(current, WinAPI.GetWindowCmd.Next);
                }
                while(current != IntPtr.Zero);

                this.Width++;
                this.Width--;
            }
            catch { }
        }

        private void ThumbsListMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var c = e.GetPosition(ThumbsList);
            foreach(Thumbnail thumb in ThumbsList.Children.OfType<Thumbnail>())
            {
                var transform = thumb.TransformToVisual(ThumbsList);
                Point p = transform.Transform(new Point(0, 0));
                if(c.Y > p.Y && c.Y < p.Y + thumb.Height && c.X > p.X && c.X < p.X + thumb.Width)
                {
                    if(WinAPI.IsIconic(thumb.Source))
                        WinAPI.ShowWindow(thumb.Source, WinAPI.WindowShowStyle.Restore);
                    else
                        WinAPI.ShowWindow(thumb.Source, WinAPI.WindowShowStyle.Show);
                    WinAPI.SetForegroundWindow(thumb.Source);
                    break;
                }
            }
        }

        public override void OpenToolbar()
        {
            base.OpenToolbar();
            this.InitializeThumbnails();
            foreach(var thumb in ThumbsList.Children.OfType<Thumbnail>())
            {
                thumb.Width++;
                thumb.Width--;
                thumb.Height++;
                thumb.Height--;
            }
            Width++;
            Width--;
            Height++;
            Height--;
        }

        public override void CloseToolbar()
        {
            base.CloseToolbar();
            this.ThumbsList.Children.Clear();
        }
    }
}
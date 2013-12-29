using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Native;
using Image = System.Windows.Controls.Image;

namespace Newgen.Core
{
    /// <summary>
    /// Newgen App Widget.
    /// </summary>
    public class NewgenAppWidget : NewgenWidget
    {
        internal string fileorfolder;
        internal bool isfolder;
        internal string args;
        internal Grid widgetcontrol;
        internal Image icon;
        internal string cachedImage;
        internal TextBlock title;
        private MenuItem mi_options;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name { get { return null; } }

        /// <summary>
        /// Gets the widget control.
        /// </summary>
        public override FrameworkElement WidgetControl { get { return widgetcontrol; } }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        public override Uri IconPath { get { return null; } }

        /// <summary>
        /// Gets the column span.
        /// </summary>
        public override int ColumnSpan { get { return 1; } }

        /// <summary>
        /// Loads from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public override void Load(string path)
        {
            if(!File.Exists(path) & !Directory.Exists(path))
                return;
            if(Directory.Exists(path))
                isfolder = true;

            this.fileorfolder = path;

            this.widgetcontrol = new Grid();

            this.icon = new Image();
            this.icon.MinWidth = 64;
            this.icon.MinHeight = 64;

            BitmapSource source = null;
            try
            {
                cachedImage = E.CacheRoot + HelperMethods.ConvertToSafeFileName(fileorfolder) + ".png";
                if (File.Exists(cachedImage))
                    source = E.GetBitmap(cachedImage);
                else
                {
                    try
                    {
                        source = IconExtractor.GetThumbnail(fileorfolder) as BitmapSource;
                        var fs = File.Create(cachedImage);
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(source));
                        encoder.Save(fs);
                        fs.Dispose();
                    }
                    catch { source = new BitmapImage(new Uri("/Newgen;component/Resources/default_icon.png", UriKind.Relative)); }
                }

                this.widgetcontrol.Background = new SolidColorBrush(CalcAverageColor(source));
            }
            catch { }

            this.icon.Source = source;
            this.icon.VerticalAlignment = VerticalAlignment.Center;
            this.icon.HorizontalAlignment = HorizontalAlignment.Center;
            this.icon.Margin = new Thickness(18);
            this.widgetcontrol.Children.Add(icon);

            this.title = new TextBlock();
            this.title.Foreground = System.Windows.Media.Brushes.White;
            this.title.Margin = new Thickness(8);
            this.title.VerticalAlignment = VerticalAlignment.Bottom;
            this.title.HorizontalAlignment = HorizontalAlignment.Left;

            string[] data = HelperMethods.GetDataArray(this.fileorfolder);

            try
            {
                if(data.Length > 1 && data[1] != null && data[2] != null)
                {
                    this.title.Text = data[1];
                    this.args = data[2];
                }
                else
                    throw new Exception("Data not set, may be damaged or the entry is new.");
            }
            catch
            {
                if(this.isfolder)
                    this.title.Text = new DirectoryInfo(fileorfolder).Name;
                else
                    this.title.Text = FileVersionInfo.GetVersionInfo(fileorfolder).FileDescription ?? new FileInfo(fileorfolder).Name;

                HelperMethods.SetDataArray(this.fileorfolder, this.title.Text, 1);
                HelperMethods.SetDataArray(this.fileorfolder, this.args, 2);
            }

            this.title.FontSize = 12.5;
            this.title.FontWeight = FontWeights.Light;
            this.title.TextWrapping = TextWrapping.WrapWithOverflow;
            this.title.TextTrimming = TextTrimming.CharacterEllipsis;
            this.widgetcontrol.ToolTip = this.title.Text;
            this.widgetcontrol.Children.Add(title);

            this.widgetcontrol.MouseLeftButtonUp += WidgetControl_MouseLeftButtonUp;

            if(!this.isfolder)
            {
                this.widgetcontrol.ContextMenu = new ContextMenu();
                this.mi_options = new MenuItem();
                this.mi_options.Header = "Options";
                this.mi_options.Click += new RoutedEventHandler(MenuItem_Options_Click);
                this.widgetcontrol.ContextMenu.Items.Add(mi_options);
            }
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public override void Unload()
        {
            if(this.mi_options != null)
                this.mi_options.Click -= new RoutedEventHandler(MenuItem_Options_Click);

            this.widgetcontrol.MouseLeftButtonUp -= this.WidgetControl_MouseLeftButtonUp;
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event of the WidgetControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void WidgetControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                foreach(Process proc in Process.GetProcesses())
                    if(proc.StartInfo.FileName == fileorfolder)
                    {
                        WinAPI.SetForegroundWindow(proc.MainWindowHandle);
                        return;
                    }

                Process p = new Process();
                p.StartInfo.Arguments = this.args;
                p.StartInfo.FileName = this.fileorfolder;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            }
            catch { }
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_Options control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MenuItem_Options_Click(object sender, RoutedEventArgs e)
        {
            (new Hubs.AppWidgetOptionsHub(this)).ShowDialog();
        }

        private static System.Windows.Media.Color CalcAverageColor(BitmapSource image)
        {
            var c = CalcAverageColor(BitmapSourceToBitmap(image));
            return System.Windows.Media.Color.FromArgb(Math.Max(c.A, (byte)100), c.R, c.G, c.B);
        }

        private static System.Drawing.Color CalcAverageColor(Bitmap image)
        {
            var bmp = new Bitmap(1, 1);
            var orig = image;
            using(var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(orig, new System.Drawing.Rectangle(0, 0, 1, 1));
            }
            var pixel = bmp.GetPixel(0, 0);
            orig.Dispose();
            bmp.Dispose();
            return pixel;
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] bits = new byte[height * stride];
            bitmapSource.CopyPixels(bits, stride, 0);
            unsafe
            {
                fixed(byte* pBits = bits)
                {
                    IntPtr ptr = new IntPtr(pBits);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, ptr);
                    return bitmap;
                }
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace Newgen.Core
{
    public class NewgenWebPreviewWidget : NewgenWidget, IDisposable
    {
        private Image previewControl;
        private System.Windows.Forms.WebBrowser browser;
        private string url;
        private string file;

        public override string Name
        {
            get { return string.Empty; }
        }

        public override System.Windows.FrameworkElement WidgetControl
        {
            get { return previewControl; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string path)
        {
            url = path;
            previewControl = new Image();
            previewControl.Stretch = Stretch.UniformToFill;
            previewControl.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(PreviewControlMouseLeftButtonUp);
            RenderOptions.SetBitmapScalingMode(previewControl, BitmapScalingMode.HighQuality);
            previewControl.HorizontalAlignment = HorizontalAlignment.Left;

            file = HelperMethods.ConvertToSafeFileName(path) + ".png";
            if (File.Exists(E.CacheRoot + file))
            {
                var bi = new BitmapImage();

                bi.BeginInit();

                bi.CacheOption = BitmapCacheOption.OnLoad;

                bi.UriSource = new Uri(E.CacheRoot + file);

                bi.EndInit();
                previewControl.Source = bi;
            }
            else
            {
                browser = new WebBrowser();
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.DocumentCompleted += BrowserDocumentCompleted;
                browser.Width = 1024;
                browser.Height = 768;
                browser.Navigate(path);
            }
        }

        public override void Refresh()
        {
            browser = new WebBrowser();
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentCompleted += BrowserDocumentCompleted;
            browser.Width = 1024;
            browser.Height = 768;
            browser.Navigate(url);
        }

        private void PreviewControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", url);
        }

        private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.ReadyState != WebBrowserReadyState.Complete)
                return;
            browser.DocumentCompleted -= BrowserDocumentCompleted;
            var bitmap = new Bitmap(browser.Width, browser.Height);
            browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, browser.Width, browser.Height));
            try
            {
                previewControl.Source = null;
                if (File.Exists(E.CacheRoot + file))
                    File.Delete(E.CacheRoot + file);
                bitmap.Save(E.CacheRoot + file, ImageFormat.Png);
            }
            catch { }
            if (File.Exists(E.CacheRoot + file))
            {
                var bi = new BitmapImage();

                bi.BeginInit();

                bi.CacheOption = BitmapCacheOption.OnLoad;

                bi.UriSource = new Uri(E.CacheRoot + file);

                bi.EndInit();
                previewControl.Source = bi;
            }
            bitmap.Dispose();
            browser.Dispose();
        }

        public override void Unload()
        {
            if (browser != null && !browser.IsDisposed)
                browser.Dispose();

            previewControl.MouseLeftButtonUp -= PreviewControlMouseLeftButtonUp;
        }

        public void Dispose()
        {
            browser.Dispose();
        }
    }
}
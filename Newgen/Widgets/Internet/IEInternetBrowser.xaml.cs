using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Newgen.Base;

namespace Internet
{
    /// <summary>
    /// Interaction logic for IEInternetBrowser.xaml
    /// </summary>
    public partial class IEInternetBrowser : HubWindow
    {
        private string lurl;

        public IEInternetBrowser(string url)
        {
            InitializeComponent();

            this.lurl = url;
        }

        private bool IsHubActive { get; set; }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            {
                IsHubActive = true;
                this.Left = -SystemParameters.PrimaryScreenWidth;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                DoubleAnimation leftanimation = new DoubleAnimation()
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    BeginTime = TimeSpan.FromMilliseconds(1),
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.7,
                };
                this.BeginAnimation(LeftProperty, leftanimation);
                Helper.Animate(this, OpacityProperty, 10, 0, 1, 0.3, 0.7);
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(string.IsNullOrEmpty(this.lurl) || string.IsNullOrWhiteSpace(this.lurl)))
                {
                    Widget.Settings.LastSearchURL = this.lurl;
                }
                Control_URLBox.Text = Widget.Settings.LastSearchURL;
            }
            catch
            {
                Widget.Settings.LastSearchURL = "http://www.bing.com/?scope=web";
                Control_URLBox.Text = Widget.Settings.LastSearchURL;
            }

            try { Browser.Navigate(Control_URLBox.Text); }
            catch { }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Widget.Settings.LastSearchURL = Control_URLBox.Text;
            Widget.Settings.Save(Widget.SettingsFile);

            Close();
        }

        private void Control_BackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Browser.GoBack();
            }
            catch
            {
            }
        }

        private void Control_RefButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Browser.Refresh();
            }
            catch
            {
            }
        }

        private void Control_FwButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Browser.GoForward();
            }
            catch
            {
            }
        }

        private void Control_URLBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (Control_URLBox.Text.StartsWith("http://")) { Browser.Navigate(Control_URLBox.Text); }
                    else if (Control_URLBox.Text.StartsWith("www.")) { Browser.Navigate("http://" + Control_URLBox.Text); }
                    else { Browser.Navigate("http://www.bing.com/search?q=" + Control_URLBox.Text); }
                }
                catch
                {
                    MessageBox.Show("Error locating URI ! The Address URI must be absolute eg 'http://www.bing.com/'", "// Newgen / : Error", MessageBoxButton.OK);
                }
            }
        }

        private void Control_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                try
                {
                    Widget.Settings.LastSearchURL = Control_URLBox.Text;
                    Widget.Settings.Save(Widget.SettingsFile);

                    Close();
                }
                catch
                {
                }
            }
        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                SetSilent(Browser, true); // make it silent
                Control_URLBox.Text = Browser.Source.OriginalString;
                Control_URLBox.CaretIndex = Control_URLBox.Text.Length;
            }
            catch
            {
            }
        }

        private void Browser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                //Prog.IsIndeterminate = true;
                Control_URLBox.Text = Browser.Source.OriginalString ?? "";
                Control_URLBox.CaretIndex = Control_URLBox.Text.Length;
            }
            catch
            {
            }
        }

        private void Control_HomeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Browser.Navigate("http://www.bing.com/");
            }
            catch
            {
            }
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Control_URLBox.Text = Browser.Source.OriginalString ?? "";
                Control_URLBox.CaretIndex = Control_URLBox.Text.Length;
            }
            catch
            {
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsHubActive)
            {
                e.Cancel = true;
                DoubleAnimation leftanimation = new DoubleAnimation()
                {
                    To = -this.ActualWidth,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    BeginTime = TimeSpan.FromMilliseconds(1),
                    AccelerationRatio = 0.7,
                    DecelerationRatio = 0,
                };
                leftanimation.Completed += (a, b) =>
                {
                    Left = -this.ActualWidth;

                    leftanimation = null;
                    Helper.Delay(new Action(() =>
                    {
                        IsHubActive = false; Topmost = false; Hide(); Close();
                    }), 1);
                };
                this.BeginAnimation(LeftProperty, leftanimation);
            }
        }

        public void Navigate(string url)
        {
            try
            {
                Browser.Navigate(url);
            }
            catch
            {
            }
        }

        private void Control_CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Widget.Settings.LastSearchURL = Control_URLBox.Text;
                Widget.Settings.Save(Widget.SettingsFile);

                Close();
            }
            catch
            {
            }
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
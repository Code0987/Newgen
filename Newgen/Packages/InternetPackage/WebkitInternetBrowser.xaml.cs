﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using libns.Media.Animation;
using libns.Threading;
using Newgen;

namespace InternetPackage
{
    /// <summary>
    /// Interaction logic for WebkitInternetBrowser.xaml
    /// </summary>
    public partial class WebkitInternetBrowser : HubWindow {
        private Package package;

        private string lurl;

        public WebkitInternetBrowser(Package package, string url):base() {
            this.package = package;

            InitializeComponent();

            this.Browser.LocalStorageEnabled = true;
            this.Browser.WebBrowserContextMenuEnabled = true;
            this.Browser.LocalStorageDatabaseDirectory = Api.CacheRoot;
            this.Browser.CookieAcceptPolicy = WebKit.CookieAcceptPolicy.Always;
            
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
                AnimationExtensions.Animate(this, OpacityProperty, 10, 0, 1, 0.3, 0.7);
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(string.IsNullOrEmpty(this.lurl) || string.IsNullOrWhiteSpace(this.lurl)))
                {
                    package.CustomizedSettings.LastSearchURL = this.lurl;
                }
                Control_URLBox.Text = package.CustomizedSettings.LastSearchURL;
            }
            catch
            {
                package.CustomizedSettings.LastSearchURL = "http://www.google.com/?scope=web";
                Control_URLBox.Text = package.CustomizedSettings.LastSearchURL;
            }

            try { Browser.Navigate(Control_URLBox.Text); }
            catch { }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            package.CustomizedSettings.LastSearchURL = Control_URLBox.Text;

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
                    else { Browser.Navigate("http://www.google.com/search?q=" + Control_URLBox.Text); }
                }
                catch
                {
                    MessageBox.Show("Error locating URI ! The Address URI must be absolute eg 'http://www.google.com/'", "// Newgen / : Error", MessageBoxButton.OK);
                }
            }
        }

        private void Control_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                try
                {
                    package.CustomizedSettings.LastSearchURL = Control_URLBox.Text;

                    Close();
                }
                catch
                {
                }
            }
        }

        private void Control_HomeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Browser.Navigate("http://www.google.com/");
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
                    ThreadingExtensions.LazyInvokeThreadSafe(new Action(() =>
                    {
                        IsHubActive = false; Topmost = false; Close();
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
                package.CustomizedSettings.LastSearchURL = Control_URLBox.Text;

                Close();
            }
            catch
            {
            }
        }

        private void Browser_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                try
                {
                    package.CustomizedSettings.LastSearchURL = Control_URLBox.Text;

                    Close();
                }
                catch
                {
                }
            }
        }

        private void Browser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            try
            {
                Control_URLBox.Text = e.Url.OriginalString;
                Control_URLBox.CaretIndex = Control_URLBox.Text.Length;
            }
            catch { }
        }

        private void Browser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            try
            {
                Control_URLBox.Text = e.Url.OriginalString;
                Control_URLBox.CaretIndex = Control_URLBox.Text.Length;
            }
            catch { }
        }
    }
}
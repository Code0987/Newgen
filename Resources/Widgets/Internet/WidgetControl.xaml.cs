using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Newgen.Base;

namespace Internet
{
    /// <summary>
    /// Interaction logic for NewgenWidget.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        //let's add a hub. You can remove this variables if you don't want to make a hub for this widget
        private object hub; //a window with hub

        public WidgetControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //this is used to make widget use same language as Newgen. If you are not planning to add localization support you can remove this lines
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            //place here all initializations
            UpdateColor();
        }

        public void Unload()
        {
            //release resources here
        }

        public void Navigate(string url)
        {
            hub = null;

            if (hub != null)
            {
                if (hub.GetType() == typeof(IEInternetBrowser))
                {
                    IEInternetBrowser browser = (IEInternetBrowser)hub;
                    if (browser.IsVisible) { browser.Activate(); }
                    browser.Navigate(url);
                }
                else
                {
                    WebkitInternetBrowser browser = (WebkitInternetBrowser)hub;
                    if (browser.IsVisible) { browser.Activate(); }
                    browser.Navigate(url);
                }
            }
            else
            {
                if (Widget.Settings.RenderingMode == RenderingMode.IE)
                {
                    hub = new IEInternetBrowser(url);
                    IEInternetBrowser browser = (IEInternetBrowser)hub;
                    browser = new IEInternetBrowser(url);
                    browser.AllowsTransparency = false;
                    browser.ShowDialog();
                    browser.Navigate(url);
                }
                else
                {
                    hub = new WebkitInternetBrowser(url);
                    WebkitInternetBrowser browser = (WebkitInternetBrowser)hub;
                    browser = new WebkitInternetBrowser(url);
                    browser.AllowsTransparency = false;
                    browser.ShowDialog();
                    browser.Navigate(url);
                }
            }
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Navigate(Widget.Settings.LastSearchURL);
        }

        private void CMRM_IE_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Widget.Settings.RenderingMode = RenderingMode.IE;
                UpdateColor();
            }
            catch { }
        }

        private void CMRM_WK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Widget.Settings.RenderingMode = RenderingMode.Webkit;
                UpdateColor();
            }
            catch { }
        }

        private void UpdateColor()
        {
            try
            {
                if (Widget.Settings.RenderingMode == RenderingMode.Webkit)
                {
                    this.CMRM_WK.Background = new SolidColorBrush(Colors.DarkGray);
                    this.CMRM_IE.Background = new SolidColorBrush(Colors.Transparent);
                }

                else
                {
                    this.CMRM_IE.Background = new SolidColorBrush(Colors.DarkGray);
                    this.CMRM_WK.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            catch { }
        }

        private static void CopyTo(DirectoryInfo source, string destDirectory)
        {
            try
            {
                DirectoryInfo target = new DirectoryInfo(destDirectory);
                if (!target.Exists) target.Create();
                foreach (FileInfo file in source.GetFiles())
                {
                    try
                    {
                        file.CopyTo(Path.Combine(target.FullName, file.Name), true);
                    }
                    catch { }
                }
                foreach (DirectoryInfo directory in source.GetDirectories())
                {
                    try
                    {
                        CopyTo(directory, Path.Combine(target.FullName, directory.Name));
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
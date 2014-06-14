using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Newgen;

namespace InternetPackage
{
    public partial class Tile : Border
    {
        private Package package;

        //let's add a hub. You can remove this variables if you don't want to make a hub for this widget
        private object hub; //a window with hub

        public Tile(Package package) {
            this.package = package;

            InitializeComponent();
        }

        public void Load()
        {
            //place here all initializations
            UpdateColor();
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
                if (package.CustomizedSettings.RenderingMode == RenderingMode.IE)
                {
                    hub = new IEInternetBrowser(package, url);
                    IEInternetBrowser browser = (IEInternetBrowser)hub;
                    browser.AllowsTransparency = false;
                    browser.ShowDialog();
                    browser.Navigate(url);
                }
                else
                {
                    hub = new WebkitInternetBrowser(package, url);
                    WebkitInternetBrowser browser = (WebkitInternetBrowser)hub;
                    browser.AllowsTransparency = false;
                    browser.ShowDialog();
                    browser.Navigate(url);
                }
            }
        }

public void Unload()
        {
            //release resources here
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

private void CMRM_IE_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                package.CustomizedSettings.RenderingMode = RenderingMode.IE;
                UpdateColor();
            }
            catch { }
        }

private void CMRM_WK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                package.CustomizedSettings.RenderingMode = RenderingMode.Webkit;
                UpdateColor();
            }
            catch { }
        }

private void UpdateColor()
{
            try
            {
                if (package.CustomizedSettings.RenderingMode == RenderingMode.Webkit)
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

private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Navigate(package.CustomizedSettings.LastSearchURL);
        }
    }
}
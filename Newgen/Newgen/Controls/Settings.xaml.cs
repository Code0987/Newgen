using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;
using Newgen.Base;
using Newgen.Core;
using Newgen.Native;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly List<string> langCodes = new List<string>();
        private bool restartRequired;
        private bool isnolicense = false;
        private bool isupdatemode = false;

        public Settings(bool nlic = false, bool supd = false)
        {
            InitializeComponent();
            this.isnolicense = nlic;
            this.isupdatemode = supd;
        }

        private bool IsHubActive { get; set; }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            {
                this.IsHubActive = true;
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

            HelperMethods.RunMethodAsyncThreadSafe(() =>
            {
                if(this.isnolicense)
                {
                    this.Tab_UI.Visibility = Visibility.Collapsed;
                    this.Tab_Widgets.Visibility = Visibility.Collapsed;
                    this.Tab_General.Visibility = Visibility.Collapsed;
                    this.Tab_OOBE.Visibility = Visibility.Collapsed;
                    this.Tab_Updates.Visibility = Visibility.Collapsed;

                    this.Tabs.SelectedItem = this.Tab_About;
                }

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                BuildTag.Text = version.ToString();

                LanguageComboBox.Items.Add(new ComboBoxItem() { Content = CultureInfo.GetCultureInfo("en-US").NativeName });
                langCodes.Add("en-US");
                var langs = from x in Directory.GetDirectories(E.Root) where x.Contains("-") select System.IO.Path.GetFileNameWithoutExtension(x);
                foreach(var l in langs)
                {
                    try
                    {
                        var c = CultureInfo.GetCultureInfo(l);
                        langCodes.Add(c.Name);
                        LanguageComboBox.Items.Add(new ComboBoxItem() { Content = c.NativeName });
                    }
                    catch { }
                }

                LanguageComboBox.Text = CultureInfo.GetCultureInfo(App.Settings.Language).NativeName;
                CheckBox_PUD.IsChecked = App.Settings.ProvideUsageData;
                if(!this.isnolicense)
                {
                    EnableUserTile.IsChecked = App.Settings.IsUserTileEnabled;
                    NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                    BgColorAlpha.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BgColorAlpha_ValueChanged);
                    BgColorAlpha.Value = (double)(int)E.BackgroundColor.A;
                    EnableBgImage.IsChecked = App.Settings.UseBgImage;
                    EnableStartBarAlways.IsChecked = App.Settings.ShowStartbarAlways;
                    EnableAutoStartCheckBox.IsChecked = App.Settings.Autostart = HelperMethods.GetAutoStart();
                    EnableShowTaskbarCheckBox.IsChecked = App.Settings.ShowTaskbar;
                    EnableShowTaskbarACheckBox.IsChecked = App.Settings.ShowTaskbarAlways;
                    DisableStartBarClock.IsChecked = App.Settings.DisableStartBarClock;
                    EnableHK.IsChecked = App.Settings.EnableHotkeys;
                    EnableUseTBCheckBox.IsChecked = App.Settings.UseThumbailsBar;
                    EnableOOBE.IsChecked = App.Settings.EnableOutOfBoxExperience;
                    EnableWidgetLock.IsChecked = App.Settings.IsWidgetsLockEnabled;
                    if(App.Settings.SlideShowImages.Count > 1)
                        Enable_OOBE_SS.IsChecked = true;
                    ValStartScreenSSTimeFrame.Text = App.Settings.SlideShowTime.ToString();
                    ValStartScreenSSTimeFrame.TextChanged += new TextChangedEventHandler(ValStartScreenSSTimeFrame_TextChanged);
                    if(App.Settings.TimeMode == 1)
                        Time24HRadioButton.IsChecked = true;
                    try
                    {
                        StartBarBgColor.Fill = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);
                    }
                    catch
                    {
                    }

                    double tilesheight = (App.StartScreen).WidgetsContainer.ActualHeight - (20);
                    double rh = ((tilesheight - E.TileSpacing * 2) / 3);

                    TilesSizeScale.Maximum = (double)rh;
                    TilesSizeScale.Minimum = (double)E.MinTilesSize;
                    TilesSizeScale.Value = (double)E.MinTileHeight;
                    TilesSizeScale.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSizeScale_ValueChanged);
                    ValTilesSpacing.Text = App.Settings.TileSpacing.ToString();
                    ValLockTime.Text = App.Settings.LockScreenTime.ToString();
                    ValTilesSpacing.TextChanged += new TextChangedEventHandler(ValTilesSpacing_TextChanged);
                    ValStartScreenTitle.Text = App.Settings.StartText;
                    ValStartScreenTitle.TextChanged += new TextChangedEventHandler(ValStartScreenTitle_TextChanged);

                    this.CheckBoxClick(this.EnableBgImage, new RoutedEventArgs());
                    this.CheckBoxClick(this.Enable_OOBE_SS, new RoutedEventArgs());

                    if(!App.IsProMode)
                    {
                        this.Tab_UI.Visibility = Visibility.Collapsed;
                        this.Tab_Widgets.Visibility = Visibility.Collapsed;
                        this.Tab_OOBE.Visibility = Visibility.Collapsed;

                        this.Tabs.SelectedItem = this.Tab_About;
                    }
                }
            });
            Helper.Animate(this, OpacityProperty, 500, 0, 1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(IsHubActive)
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
                        IsHubActive = false;
                        Topmost = false;
                        Hide();
                        Close();
                    }), 1);
                };
                this.BeginAnimation(LeftProperty, leftanimation);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(restartRequired)
            {
                App.Restart();
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplySettings()
        {
            App.Settings.IsUserTileEnabled = (bool)EnableUserTile.IsChecked;
            App.Settings.UseBgImage = (bool)EnableBgImage.IsChecked;
            App.Settings.Autostart = (bool)EnableAutoStartCheckBox.IsChecked;
            App.Settings.ShowTaskbar = (bool)EnableShowTaskbarCheckBox.IsChecked;
            App.Settings.ShowTaskbarAlways = (bool)EnableShowTaskbarACheckBox.IsChecked;
            App.Settings.ShowStartbarAlways = (bool)EnableStartBarAlways.IsChecked;
            App.Settings.DisableStartBarClock = (bool)DisableStartBarClock.IsChecked;
            App.Settings.EnableHotkeys = (bool)EnableHK.IsChecked;
            App.Settings.IsWidgetsLockEnabled = (bool)EnableWidgetLock.IsChecked;

            if(Time24HRadioButton.IsChecked == true)
                App.Settings.TimeMode = 1;
            else
                App.Settings.TimeMode = 0;

            try
            {
                HelperMethods.SetAutoStart(false);
            }
            catch { }

            try
            {
                HelperMethods.SetAutoStart(App.Settings.Autostart);
            }
            catch { }

            var lastLang = App.Settings.Language;
            if(LanguageComboBox.SelectedIndex >= 0)
                App.Settings.Language = langCodes[LanguageComboBox.SelectedIndex];
            if(!restartRequired)
                restartRequired = lastLang != App.Settings.Language;

            try
            {
                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                (App.StartScreen).Width = SystemParameters.PrimaryScreenWidth;

                if(App.Settings.ShowTaskbarAlways)
                {
                    (App.StartScreen).Height = SystemParameters.WorkArea.Height;
                    (App.StartScreen).Top = SystemParameters.WorkArea.Top;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                }
                else if(App.Settings.ShowTaskbar)
                {
                    (App.StartScreen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.StartScreen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                }
                else
                {
                    (App.StartScreen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.StartScreen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Hide);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Hide);
                }
            }
            catch { }
            if(App.Settings.UseThumbailsBar != (bool)EnableUseTBCheckBox.IsChecked)
            {
                restartRequired = true;
            }
            App.Settings.UseThumbailsBar = (bool)EnableUseTBCheckBox.IsChecked;

            if(App.Settings.EnableOutOfBoxExperience != (bool)EnableOOBE.IsChecked)
            {
                restartRequired = true;
            }
            App.Settings.EnableOutOfBoxExperience = (bool)EnableOOBE.IsChecked;

            try
            {
                if(App.Settings.SlideShowImages.Count < 2 || !(bool)Enable_OOBE_SS.IsChecked)
                {
                    App.Settings.SlideShowImages.Clear();
                }
            }

            catch(Exception)
            {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
            }

            App.SaveSettings();

            // Update UserTile

            try
            {
                var window = App.StartScreen;
                window.LoadUserTileInfo();
            }
            catch { }

            Helper.Animate(this, OpacityProperty, 250, 0);
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", this.SiteLink1.Text);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", "https://mail.google.com/mail/?shva=1#compose");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Do you want to reset Newgen settings (This will not remove your widgets, but a restart is required) ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try { if(File.Exists(E.Config)) File.Delete(E.Config); }
                catch { }
            }
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if(sender == this.EnableBgImage)
            {
                if((bool)this.EnableBgImage.IsChecked)
                {
                    this.TextBgImg.Visibility = Visibility.Visible;
                    this.ChangeBgImg.Visibility = Visibility.Visible;
                    this.NewgenBgColor.Visibility = Visibility.Collapsed;
                    this.TextBgColor.Visibility = Visibility.Collapsed;
                    this.ChangeBgColorButton.Visibility = Visibility.Collapsed;
                    this.TextBgTrans.Visibility = Visibility.Collapsed;
                    this.BgColorAlpha.Visibility = Visibility.Collapsed;
                    Helper.Animate(this.TextBgImg, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0);
                    Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0);
                    Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0);
                    Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0);
                    Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0);
                }
                else
                {
                    Helper.Animate(this.TextBgImg, OpacityProperty, 250, 0);
                    Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0);
                    Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0, 1);
                    Helper.Delay(new Action(() =>
                    {
                        this.TextBgImg.Visibility = Visibility.Collapsed;
                        this.ChangeBgImg.Visibility = Visibility.Collapsed;
                        this.NewgenBgColor.Visibility = Visibility.Visible;
                        this.TextBgColor.Visibility = Visibility.Visible;
                        this.ChangeBgColorButton.Visibility = Visibility.Visible;
                        this.TextBgTrans.Visibility = Visibility.Visible;
                        this.BgColorAlpha.Visibility = Visibility.Visible;
                    }), 200);
                }
            }
            if(sender == this.Enable_OOBE_SS)
            {
                if((bool)this.Enable_OOBE_SS.IsChecked)
                {
                    this.OOBE_SS.Visibility = Visibility.Visible;
                    Helper.Animate(this.OOBE_SS, OpacityProperty, 250, 0, 1);
                }
                else
                {
                    Helper.Animate(this.OOBE_SS, OpacityProperty, 250, 0);
                    Helper.Delay(new Action(() =>
                    {
                        this.OOBE_SS.Visibility = Visibility.Collapsed;
                    }), 200);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if(c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromArgb(E.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                App.Settings.BackgroundColor = color;
                E.BackgroundColor = color;
                NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                BgColorAlpha.Value = (double)(int)color.A;

                var window = App.StartScreen;
                window.Background = new SolidColorBrush(E.BackgroundColor);
            }
        }

        private void ValLockTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int anInteger;
                anInteger = Convert.ToInt32(ValLockTime.Text);
                anInteger = int.Parse(ValLockTime.Text);
                if(string.IsNullOrEmpty(ValLockTime.Text) || string.IsNullOrWhiteSpace(ValLockTime.Text)) { App.Settings.LockScreenTime = -1; }
                else { App.Settings.LockScreenTime = Convert.ToInt32(ValLockTime.Text); }
            }
            catch
            {
                ValLockTime.Text = App.Settings.LockScreenTime.ToString();
            }
        }

        private void BgColorAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color c = App.Settings.BackgroundColor;
            var color = Color.FromArgb((byte)e.NewValue, c.R, c.G, c.B);
            App.Settings.BackgroundColor = color;
            E.BackgroundColor = color;
            NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);

            var window = App.StartScreen;
            window.Background = new SolidColorBrush(E.BackgroundColor);
        }

        private void TilesSizeScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App.Settings.MinTileHeight = TilesSizeScale.Value;

            App.Settings.MinTileWidth = App.Settings.MinTileHeight * E.TilesSizeFactor;
            E.MinTileWidth = App.Settings.MinTileWidth;
            E.MinTileHeight = App.Settings.MinTileHeight;
        }

        private void ChangeBgImgClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = E.ImageFilter;
            if(!(bool)dialog.ShowDialog())
                return;

            var window = App.StartScreen;
            try
            {
                if(!File.Exists(E.BgImage))
                    File.Create(E.BgImage);

                byte[] bytArray = File.ReadAllBytes(dialog.FileName);
                File.WriteAllBytes(E.BgImage, bytArray);

                MemoryStream ms = new MemoryStream();
                BitmapImage bi = new BitmapImage();

                ms.Write(bytArray, 0, bytArray.Length);
                ms.Position = 0;
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                window.Background = new ImageBrush(bi);
                App.Settings.UseBgImage = true;
            }
            catch(Exception)
            {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
                App.Settings.UseBgImage = false;
            }
        }

        private void ValTilesSpacing_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(ValTilesSpacing.Text) || string.IsNullOrWhiteSpace(ValTilesSpacing.Text))
                {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValTilesSpacing.Text);
                anInteger = int.Parse(ValTilesSpacing.Text);
                bool valid = anInteger > 0;
                if(valid)
                {
                    App.Settings.TileSpacing = anInteger;
                    E.TileSpacing = App.Settings.TileSpacing;
                }
            }
            catch
            {
                ValTilesSpacing.Text = App.Settings.LockScreenTime.ToString();
            }
        }

        private void ChangeStartBarBgColorButton_Click(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if(c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);
                App.Settings.ToolbarBackgroundColor = color;
                StartBarBgColor.Fill = new SolidColorBrush(color);
            }
        }

        private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToWidget("Newgen Team Widget", "OpenHub");
            Topmost = false;
        }

        private void ChangeSSImgs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                dialog.Multiselect = true;
                dialog.CheckPathExists = true;
                dialog.CheckFileExists = true;
                if(!(bool)dialog.ShowDialog())
                    return;

                var window = App.StartScreen;
                try
                {
                    App.Settings.SlideShowImages.Clear();
                    foreach(string fn in dialog.FileNames)
                    {
                        App.Settings.SlideShowImages.Add(fn);
                    }
                    restartRequired = true;
                }

                catch(Exception)
                {
                    MessageBox.Show(E.MSG_ER_FEATURE, "Error");
                }
            }

            catch { }
        }

        private void ValStartScreenSSTimeFrame_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(ValStartScreenSSTimeFrame.Text) || string.IsNullOrWhiteSpace(ValStartScreenSSTimeFrame.Text))
                {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValStartScreenSSTimeFrame.Text);
                anInteger = int.Parse(ValStartScreenSSTimeFrame.Text);
                App.Settings.SlideShowTime = Convert.ToInt32(ValStartScreenSSTimeFrame.Text);

                //restartRequired = true;
            }
            catch
            {
                //restartRequired = true;
                ValStartScreenSSTimeFrame.Text = App.Settings.SlideShowTime.ToString();
            }
        }

        private void ValStartScreenTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                App.Settings.StartText = ValStartScreenTitle.Text;
                var window = App.StartScreen;
                window.LoadUISettings();
            }
            catch
            {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
            }
        }

        private void CheckBox_PUD_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.ProvideUsageData = (bool)this.CheckBox_PUD.IsChecked;
        }

        #region Licensing

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Content_LicBox.Children.Clear();
                var licc = new iFramework.Security.Licensing.UI.LicenseControl();
                licc.LicenseManager = iFramework.Security.Licensing.LicenseManager.Current;
                this.Content_LicBox.Children.Add(licc);
            }
            catch { }
        }

        #endregion Licensing

        #region Updates

        private const string UpdatesBase = "http://data.nsapps.net/cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/12/updates/";

        private string UpdateFile = "Newgen.exe";
        private string UpdateVersion = "0.0.0.0";
        private string UpdateReleaseDate = "0-0-0";
        private string UpdateNotes = "-";

        internal static bool IsUpdateAvailable()
        {
            var xml = (XElement)null;
            var uv = "0.0.0.0";
            try
            {
                xml = XElement.Load(UpdatesBase + "meta.xml");
                foreach(XElement element in xml.Elements())
                    if(element.Name == "Version") { uv = element.Value; continue; }
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var uversion = Version.Parse(uv);
                if(version < uversion)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(UpdateButton.Content.ToString() == Newgen.Resources.Resources.OptionsCFU)
                {
                    ProgressBar.IsIndeterminate = true;
                    XElement xml = null;
                    try
                    {
                        xml = XElement.Load(UpdatesBase + "meta.xml");

                        foreach(XElement element in xml.Elements())
                        {
                            if(element.Name == "Version") { UpdateVersion = element.Value; continue; }
                            if(element.Name == "File") { UpdateFile = element.Value; continue; }
                            if(element.Name == "ReleaseDate") { UpdateReleaseDate = element.Value; continue; }
                            if(element.Name == "Notes") { UpdateNotes = element.Value; continue; }
                        }

                        var version = Assembly.GetExecutingAssembly().GetName().Version;
                        var uversion = Version.Parse(UpdateVersion);

                        if(version < uversion)
                        {
                            VersionTextBlock.Text = "Version : " + UpdateVersion;
                            RDTextBlock.Text = "Release Date : " + UpdateReleaseDate;
                            RnTextBlock.Text = "Release Notes : \n\n" + UpdateNotes;
                            UpdatesInfo.Visibility = Visibility.Visible;
                            ProgressBar.IsIndeterminate = true;

                            int ContentLength;
                            string size = "0.00 kb";

                            HelperMethods.RunMethodAsync(() =>
                            {
                                System.Net.WebRequest req = System.Net.HttpWebRequest.Create(UpdatesBase + UpdateFile);
                                req.Method = "HEAD";
                                System.Net.WebResponse resp = req.GetResponse();
                                if(int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                                {
                                    if(ContentLength >= 1048576)
                                        size = "Size : " + ((ContentLength / 1048576).ToString("0.00")) + " mb";
                                    else
                                        size = "Size : " + ((ContentLength / 1024).ToString("0.00")) + " kb";
                                }
                            }).Wait();

                            SizeTextBlock.Text = size;
                            ProgressBar.IsIndeterminate = false;
                            UpdateButton.Content = Newgen.Resources.Resources.OptionsULV;
                        }
                        else
                        {
                            UpdateButton.Content = Newgen.Resources.Resources.OptionsUNA;
                        }
                    }
                    catch { HelperMethods.ShowErrorMessage(E.MSG_NE); }

                    return;
                }

                if(UpdateButton.Content.ToString() == Newgen.Resources.Resources.OptionsULV)
                {
                    try
                    {
                        UpdateButton.IsEnabled = false;
                        ProgressBar.IsIndeterminate = true;

                        WebClient client = new WebClient();
                        string url = UpdatesBase + UpdateFile;
                        string path = Path.GetTempPath();
                        string tempdown = Path.Combine(path, Path.GetFileName(url));

                        client.DownloadFileCompleted += (a, b) =>
                        {
                            try
                            {
                                UpdateButton.Content = Newgen.Resources.Resources.OptionsUNA;
                                UpdateButton.IsEnabled = false;
                                if(HelperMethods.ShowQAMessage("Do you want copy of downloaded update file, in case the update installation failed ?\n\n(Note: File will be copied to your desktop.)").HasFlag(MessageBoxResult.Yes))
                                {
                                    FileInfo fi = new FileInfo(tempdown);
                                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.DoNotVerify);
                                    HelperMethods.RunMethodAsync(() => File.Copy(tempdown, desktop + "\\" + fi.Name)).Wait();
                                }
                                Process.Start(tempdown);
                                System.Threading.Thread.Sleep(500);
                                App.Current.Shutdown(98);
                            }
                            catch { File.Delete(tempdown); }
                        };

                        client.DownloadProgressChanged += (a, b) =>
                        {
                            ProgressBar.IsIndeterminate = false;
                            ProgressBar.Value = b.ProgressPercentage;
                        };
                        client.DownloadFileAsync(new Uri(url), tempdown);
                    }
                    catch { HelperMethods.ShowErrorMessage(E.MSG_NE); }

                    ProgressBar.IsIndeterminate = false;
                }
                return;
            }
            catch { }
        }

        #endregion Updates
    }
}
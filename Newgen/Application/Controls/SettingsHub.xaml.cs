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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using iFramework.Security.Licensing;
using iFramework.Security.Licensing.UI;
using libns;
using libns.Native;
using libns.Threading;
using libns.UI;
using Microsoft.Win32;
using Newgen.Resources;

namespace Newgen.Windows {

    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class SettingsHub : Window {
        private bool restartRequired;
        private bool isnolicense = false;
        private bool isupdatemode = false;

        public SettingsHub(bool nlic = false, bool supd = false) {
            InitializeComponent();
            this.isnolicense = nlic;
            this.isupdatemode = supd;
        }

        private bool IsHubActive { get; set; }

        private void WindowSourceInitialized(object sender, EventArgs e) {
            {
                this.IsHubActive = true;
                this.Left = -SystemParameters.PrimaryScreenWidth;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                DoubleAnimation leftanimation = new DoubleAnimation() {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    BeginTime = TimeSpan.FromMilliseconds(1),
                    AccelerationRatio = 0.3,
                    DecelerationRatio = 0.7,
                };
                this.BeginAnimation(LeftProperty, leftanimation);
                Helper.Animate(this, OpacityProperty, 10, 0, 1, 0.3, 0.7);
            }

            if (this.isnolicense) {
                this.Tab_UI.Visibility = Visibility.Collapsed;
                this.Tab_Tiles.Visibility = Visibility.Collapsed;
                this.Tab_General.Visibility = Visibility.Collapsed;
                this.Tab_Updates.Visibility = Visibility.Collapsed;

                this.Tabs.SelectedItem = this.Tab_About;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = version.ToString();

            foreach (var culture in Settings.Current.Cultures) {
                LanguageComboBox.Items.Add(new ComboBoxItem() { Content = culture.NativeName });
            }
            LanguageComboBox.Text = CultureInfo.GetCultureInfo(Settings.Current.Language).NativeName;

            CheckBox_PUD.IsChecked = Settings.Current.ProvideUsageData;
            if (!this.isnolicense) {
                EnableUserTile.IsChecked = Settings.Current.IsUserTileEnabled;
                NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                BgColorAlpha.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BgColorAlpha_ValueChanged);
                BgColorAlpha.Value = (double)(int)E.BackgroundColor.A;
                EnableBgImage.IsChecked = Settings.Current.UseBgImage;
                EnableStartBarAlways.IsChecked = Settings.Current.ShowStartbarAlways;
                EnableAutoStartCheckBox.IsChecked = Settings.Current.Autostart = ApplicationExtensions.IsStartWithWindowsEnabled(App.Current.Title);
                EnableShowTaskbarCheckBox.IsChecked = Settings.Current.ShowTaskbar;
                EnableShowTaskbarACheckBox.IsChecked = Settings.Current.ShowTaskbarAlways;
                EnableHK.IsChecked = Settings.Current.EnableHotkeys;
                EnableUseTBCheckBox.IsChecked = Settings.Current.UseThumbailsBar;
                EnableWidgetLock.IsChecked = Settings.Current.IsTilesLockEnabled;
                if (Settings.Current.SlideShowImages.Count > 1)
                    Enable_OOBE_SS.IsChecked = true;
                ValStartScreenSSTimeFrame.Text = Settings.Current.SlideShowTime.ToString();
                ValStartScreenSSTimeFrame.TextChanged += new TextChangedEventHandler(ValStartScreenSSTimeFrame_TextChanged);
                if (Settings.Current.TimeMode == 1)
                    Time24HRadioButton.IsChecked = true;
                try {
                    StartBarBgColor.Fill = new SolidColorBrush(Settings.Current.ToolbarBackgroundColor);
                }
                catch {
                }

                double tilesheight = (App.Screen).TilesControl.ActualHeight - (20);
                double rh = ((tilesheight - E.TileSpacing * 2) / 3);

                TilesSizeScale.Maximum = (double)rh;
                TilesSizeScale.Minimum = (double)E.MinTilesSize;
                TilesSizeScale.Value = (double)E.MinTileHeight;
                TilesSizeScale.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSizeScale_ValueChanged);
                ValTilesSpacing.Text = Settings.Current.TileSpacing.ToString();
                ValLockTime.Text = Settings.Current.LockScreenTime.ToString();
                ValTilesSpacing.TextChanged += new TextChangedEventHandler(ValTilesSpacing_TextChanged);
                ValStartScreenTitle.Text = Settings.Current.StartText;
                ValStartScreenTitle.TextChanged += new TextChangedEventHandler(ValStartScreenTitle_TextChanged);

                this.CheckBoxClick(this.EnableBgImage, new RoutedEventArgs());
                this.CheckBoxClick(this.Enable_OOBE_SS, new RoutedEventArgs());

                if (!Settings.IsProMode) {
                    this.Tab_UI.Visibility = Visibility.Collapsed;
                    this.Tab_Tiles.Visibility = Visibility.Collapsed;

                    this.Tabs.SelectedItem = this.Tab_About;
                }
            }

            Helper.Animate(this, OpacityProperty, 500, 0, 1);

            UpdateTaskBarPEXL();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (IsHubActive) {
                e.Cancel = true;
                DoubleAnimation leftanimation = new DoubleAnimation() {
                    To = -this.ActualWidth,
                    Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                    BeginTime = TimeSpan.FromMilliseconds(1),
                    AccelerationRatio = 0.7,
                    DecelerationRatio = 0,
                };
                leftanimation.Completed += (a, b) => {
                    Left = -this.ActualWidth;

                    leftanimation = null;
                    Helper.Delay(new Action(() => {
                        IsHubActive = false;
                        Topmost = false;
                        Hide();
                        Close();
                    }), 1);
                };
                this.BeginAnimation(LeftProperty, leftanimation);
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            if (restartRequired) {
                App.Restart();
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e) {
            ApplySettings();
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void ApplySettings() {
            Settings.Current.IsUserTileEnabled = (bool)EnableUserTile.IsChecked;
            Settings.Current.UseBgImage = (bool)EnableBgImage.IsChecked;
            Settings.Current.Autostart = (bool)EnableAutoStartCheckBox.IsChecked;
            Settings.Current.ShowTaskbar = (bool)EnableShowTaskbarCheckBox.IsChecked;
            Settings.Current.ShowTaskbarAlways = (bool)EnableShowTaskbarACheckBox.IsChecked;
            Settings.Current.ShowStartbarAlways = (bool)EnableStartBarAlways.IsChecked;
            Settings.Current.EnableHotkeys = (bool)EnableHK.IsChecked;
            Settings.Current.IsTilesLockEnabled = (bool)EnableWidgetLock.IsChecked;

            if (Time24HRadioButton.IsChecked == true)
                Settings.Current.TimeMode = 1;
            else
                Settings.Current.TimeMode = 0;

            try {
                if (Settings.Current.Autostart)
                    ApplicationExtensions.SetStartWithWindows(App.Current.Title, Process.GetCurrentProcess().MainModule.FileName);
                else
                    ApplicationExtensions.RemoveStartWithWindows(App.Current.Title);
            }
            catch { }

            var lastLang = Settings.Current.Language;

            if (LanguageComboBox.SelectedIndex > -1)
                Settings.Current.Language = Settings.Current.Cultures[LanguageComboBox.SelectedIndex].Name;

            if (!restartRequired)
                restartRequired = lastLang != Settings.Current.Language;

            try {
                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                (App.Screen).Width = SystemParameters.PrimaryScreenWidth;

                if (Settings.Current.ShowTaskbarAlways) {
                    (App.Screen).Height = SystemParameters.WorkArea.Height;
                    (App.Screen).Top = SystemParameters.WorkArea.Top;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else if (Settings.Current.ShowTaskbar) {
                    (App.Screen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.Screen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
                }
                else {
                    (App.Screen).Height = SystemParameters.PrimaryScreenHeight;
                    (App.Screen).Top = 0;
                    WinAPI.ShowWindow(taskbar, WindowShowStyle.Hide);
                    WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Hide);
                }
            }
            catch { }
            if (Settings.Current.UseThumbailsBar != (bool)EnableUseTBCheckBox.IsChecked) {
                restartRequired = true;
            }
            Settings.Current.UseThumbailsBar = (bool)EnableUseTBCheckBox.IsChecked;

            try {
                if (Settings.Current.SlideShowImages.Count < 2 || !(bool)Enable_OOBE_SS.IsChecked) {
                    Settings.Current.SlideShowImages.Clear();
                }
            }
            catch (Exception) {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
            }

            Settings.Current.Save();

            // Update UserBadgeControl

            try {
                var window = App.Screen;
                window.UserBadgeControl.Reload();
            }
            catch { }

            Helper.Animate(this, OpacityProperty, 250, 0);
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            E.Messenger.Send(new EMessage() {
                Key = EMessage.UrlKey,
                Value = SiteLink1.Text
            });
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e) {
            E.Messenger.Send(new EMessage() {
                Key = EMessage.UrlKey,
                Value = "https://mail.google.com/mail/?shva=1#compose"
            });
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Do you want to reset Newgen settings (This will not remove your widgets, but a restart is required) ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                try { if (File.Exists(E.Config)) File.Delete(E.Config); }
                catch { }
            }
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e) {
            if (sender == this.EnableBgImage) {
                if ((bool)this.EnableBgImage.IsChecked) {
                    this.ChangeBgImg.Visibility = Visibility.Visible;
                    this.NewgenBgColor.Visibility = Visibility.Collapsed;
                    this.TextBgColor.Visibility = Visibility.Collapsed;
                    this.ChangeBgColorButton.Visibility = Visibility.Collapsed;
                    this.TextBgTrans.Visibility = Visibility.Collapsed;
                    this.BgColorAlpha.Visibility = Visibility.Collapsed;
                    Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0);
                    Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0);
                    Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0);
                    Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0);
                    Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0);
                }
                else {
                    Helper.Animate(this.ChangeBgImg, OpacityProperty, 250, 0);
                    Helper.Animate(this.NewgenBgColor, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.TextBgColor, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.TextBgTrans, OpacityProperty, 250, 0, 1);
                    Helper.Animate(this.BgColorAlpha, OpacityProperty, 250, 0, 1);
                    Helper.Delay(new Action(() => {
                        this.ChangeBgImg.Visibility = Visibility.Collapsed;
                        this.NewgenBgColor.Visibility = Visibility.Visible;
                        this.TextBgColor.Visibility = Visibility.Visible;
                        this.ChangeBgColorButton.Visibility = Visibility.Visible;
                        this.TextBgTrans.Visibility = Visibility.Visible;
                        this.BgColorAlpha.Visibility = Visibility.Visible;
                    }), 200);
                }
            }
            if (sender == this.Enable_OOBE_SS) {
                if ((bool)this.Enable_OOBE_SS.IsChecked) {
                    this.OOBE_SS.Visibility = Visibility.Visible;
                    Helper.Animate(this.OOBE_SS, OpacityProperty, 250, 0, 1);
                }
                else {
                    Helper.Animate(this.OOBE_SS, OpacityProperty, 250, 0);
                    Helper.Delay(new Action(() => {
                        this.OOBE_SS.Visibility = Visibility.Collapsed;
                    }), 200);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e) {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var color = Color.FromArgb(E.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                Settings.Current.BackgroundColor = color;
                E.BackgroundColor = color;
                NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                BgColorAlpha.Value = (double)(int)color.A;

                var window = App.Screen;
                window.Background = new SolidColorBrush(E.BackgroundColor);
            }
        }

        private void ValLockTime_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                int anInteger;
                anInteger = Convert.ToInt32(ValLockTime.Text);
                anInteger = int.Parse(ValLockTime.Text);
                if (string.IsNullOrEmpty(ValLockTime.Text) || string.IsNullOrWhiteSpace(ValLockTime.Text)) { Settings.Current.LockScreenTime = -1; }
                else { Settings.Current.LockScreenTime = Convert.ToInt32(ValLockTime.Text); }
            }
            catch {
                ValLockTime.Text = Settings.Current.LockScreenTime.ToString();
            }
        }

        private void BgColorAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color c = Settings.Current.BackgroundColor;
            var color = Color.FromArgb((byte)e.NewValue, c.R, c.G, c.B);
            Settings.Current.BackgroundColor = color;
            E.BackgroundColor = color;
            NewgenBgColor.Fill = new SolidColorBrush(E.BackgroundColor);

            var window = App.Screen;
            window.Background = new SolidColorBrush(E.BackgroundColor);
        }

        private void TilesSizeScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Settings.Current.MinTileHeight = TilesSizeScale.Value;

            Settings.Current.MinTileWidth = Settings.Current.MinTileHeight * E.TilesSizeFactor;
            E.MinTileWidth = Settings.Current.MinTileWidth;
            E.MinTileHeight = Settings.Current.MinTileHeight;
        }

        private void ChangeBgImgClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = E.ImageFilter;
            if (!(bool)dialog.ShowDialog())
                return;

            var window = App.Screen;
            try {
                if (!File.Exists(E.BgImage))
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
                Settings.Current.UseBgImage = true;
            }
            catch (Exception) {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
                Settings.Current.UseBgImage = false;
            }
        }

        private void ValTilesSpacing_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                if (string.IsNullOrEmpty(ValTilesSpacing.Text) || string.IsNullOrWhiteSpace(ValTilesSpacing.Text)) {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValTilesSpacing.Text);
                anInteger = int.Parse(ValTilesSpacing.Text);
                bool valid = anInteger > 0;
                if (valid) {
                    Settings.Current.TileSpacing = anInteger;
                    E.TileSpacing = Settings.Current.TileSpacing;
                }
            }
            catch {
                ValTilesSpacing.Text = Settings.Current.LockScreenTime.ToString();
            }
        }

        private void ChangeStartBarBgColorButton_Click(object sender, RoutedEventArgs e) {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var color = Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);
                Settings.Current.ToolbarBackgroundColor = color;
                StartBarBgColor.Fill = new SolidColorBrush(color);
            }
        }

        private void ChangeSSImgs_Click(object sender, RoutedEventArgs e) {
            try {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                dialog.Multiselect = true;
                dialog.CheckPathExists = true;
                dialog.CheckFileExists = true;
                if (!(bool)dialog.ShowDialog())
                    return;

                var window = App.Screen;
                try {
                    Settings.Current.SlideShowImages.Clear();
                    foreach (string fn in dialog.FileNames) {
                        Settings.Current.SlideShowImages.Add(fn);
                    }
                    restartRequired = true;
                }
                catch (Exception) {
                    MessageBox.Show(E.MSG_ER_FEATURE, "Error");
                }
            }
            catch { }
        }

        private void ValStartScreenSSTimeFrame_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                if (string.IsNullOrEmpty(ValStartScreenSSTimeFrame.Text) || string.IsNullOrWhiteSpace(ValStartScreenSSTimeFrame.Text)) {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValStartScreenSSTimeFrame.Text);
                anInteger = int.Parse(ValStartScreenSSTimeFrame.Text);
                Settings.Current.SlideShowTime = Convert.ToInt32(ValStartScreenSSTimeFrame.Text);

                //restartRequired = true;
            }
            catch {

                //restartRequired = true;
                ValStartScreenSSTimeFrame.Text = Settings.Current.SlideShowTime.ToString();
            }
        }

        private void ValStartScreenTitle_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                Settings.Current.StartText = ValStartScreenTitle.Text;
            }
            catch {
                MessageBox.Show(E.MSG_ER_FEATURE, "Error");
            }
        }

        private void CheckBox_PUD_Click(object sender, RoutedEventArgs e) {
            Settings.Current.ProvideUsageData = (bool)this.CheckBox_PUD.IsChecked;
        }

        private bool isLoadingTaskBarDatadone = false;

        private void UpdateTaskBarPEXL() {
            try {

                //TODO:StartSystem.tbtimer.Stop();

                ListBox_ItemsToExclude.Items.Clear();

                WinAPI.ForEachVisibleWindow(
    ((HwndSource)HwndSource.FromVisual(this)).Handle,
    (current, text) => {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var fip = new FileInfo(WinAPI.GetProcessPath(current));
        ListBox_ItemsToExclude.Items.Add(new TaskBarProcessExclusionData() {
            ProcessName = fip.Name,
            Icon = InternalHelper.GetThumbnail(WinAPI.GetProcessPath(current)) as BitmapSource
        });
    });

                var addeditems = ListBox_ItemsToExclude.Items.OfType<TaskBarProcessExclusionData>().ToList();

                foreach (var item in Settings.Current.TaskBarProcessExclusionList) {
                    try {
                        var existcount = 0;
                        foreach (var item2 in addeditems) {
                            if (item2.ProcessName == item) { existcount++; ListBox_ItemsToExclude.SelectedItems.Add(item2); }
                        }
                        if (existcount <= 0) {
                            var data = new TaskBarProcessExclusionData() {
                                ProcessName = item
                            };
                            ListBox_ItemsToExclude.Items.Add(data);
                            ListBox_ItemsToExclude.SelectedItems.Add(data);
                        }
                    }
                    catch { }
                }

                isLoadingTaskBarDatadone = true;
            }
            catch { }
        }

        private void ListBox_ItemsToExclude_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (isLoadingTaskBarDatadone) {

                //SavePEXL();
            }
        }

        private void SavePEXL() {
            try {
                Settings.Current.TaskBarProcessExclusionList.Clear();

                foreach (var item in ListBox_ItemsToExclude.SelectedItems.OfType<TaskBarProcessExclusionData>()) {
                    Settings.Current.TaskBarProcessExclusionList.Add(item.ProcessName);
                }
            }
            catch { }

            //try {
            //    foreach (string item in Settings.Current.TaskBarProcessExclusionList) {
            //        foreach (Window wnd in App.Current.Windows) {
            //            if (wnd is StartSystem) {
            //                List<StartBarItem> items = ((StartSystem)wnd).Icons.Children.OfType<StartBarItem>().ToList();

            //                foreach (StartBarItem item2 in items) {
            //                    if (WinAPI.GetProcessPath(item2.Handles[0]).Contains(item))
            //                        ((StartSystem)wnd).RemoveIcon(item2);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch { }
        }

        private void Button_AddPEXL_Click(object sender, RoutedEventArgs e) {

            //try {
            //    var dialog = new OpenFileDialog();
            //    dialog.Filter = "Executable Files|*.exe";
            //    if (!(bool)dialog.ShowDialog())
            //        return;

            // FileInfo fip = new FileInfo(dialog.FileName);

            // TaskBarProcessExclusionData data = new TaskBarProcessExclusionData() { ProcessName =
            // fip.Name, Icon = IconExtractor.GetIcon(dialog.FileName) };

            // ListBox_ItemsToExclude.Items.Add(data);

            //    ListBox_ItemsToExclude.SelectedItems.Add(data);
            //}
            //catch (Exception) {
            //    Helper.ShowErrorMessage("Cannot process your request.");
            //}
        }

        #region Licensing

        private void LicenseControlInstance_Loaded(object sender, RoutedEventArgs e) {
            try {
                LicenseControlInstance.ActiveLicenseId = Settings.Current.GetAndValidateActiveLicense();
            }
            catch { }
        }

        private void LicenseControlInstance_ActiveLicenseIdChanged(Guid licenseId) {
            if (ClientManager.Current.IsValid(licenseId, App.Current.Guid) && ClientManager.Current.IsActive(licenseId)) {
                Settings.Current.ActiveLicenseId = licenseId;
                Settings.Current.Save();
            }
        }

        #endregion Licensing

        #region Updates

        private const string UpdatesBase = "http://data.nsapps.net/cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/12/updates/";

        private string UpdateFile = "Newgen.exe";
        private string UpdateVersion = "0.0.0.0";
        private string UpdateReleaseDate = "0-0-0";
        private string UpdateNotes = "-";

        internal static bool IsUpdateAvailable() {
            var xml = (XElement)null;
            var uv = "0.0.0.0";
            try {
                xml = XElement.Load(UpdatesBase + "meta.xml");
                foreach (XElement element in xml.Elements())
                    if (element.Name == "Version") { uv = element.Value; continue; }
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var uversion = Version.Parse(uv);
                if (version < uversion)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            try {
                if (UpdateButton.Content.ToString() == Definitions.OptionsCFU) {
                    ProgressBar.IsIndeterminate = true;
                    XElement xml = null;
                    try {
                        xml = XElement.Load(UpdatesBase + "meta.xml");

                        foreach (XElement element in xml.Elements()) {
                            if (element.Name == "Version") { UpdateVersion = element.Value; continue; }
                            if (element.Name == "File") { UpdateFile = element.Value; continue; }
                            if (element.Name == "ReleaseDate") { UpdateReleaseDate = element.Value; continue; }
                            if (element.Name == "Notes") { UpdateNotes = element.Value; continue; }
                        }

                        var version = Assembly.GetExecutingAssembly().GetName().Version;
                        var uversion = Version.Parse(UpdateVersion);

                        if (version < uversion) {
                            VersionTextBlock.Text = "Version : " + UpdateVersion;
                            RDTextBlock.Text = "Release Date : " + UpdateReleaseDate;
                            RnTextBlock.Text = "Release Notes : \n\n" + UpdateNotes;
                            UpdatesInfo.Visibility = Visibility.Visible;
                            ProgressBar.IsIndeterminate = true;

                            int ContentLength;
                            string size = "0.00 kb";

                            this.InvokeAsync(() => {
                                System.Net.WebRequest req = System.Net.HttpWebRequest.Create(UpdatesBase + UpdateFile);
                                req.Method = "HEAD";
                                System.Net.WebResponse resp = req.GetResponse();
                                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength)) {
                                    if (ContentLength >= 1048576)
                                        size = "Size : " + ((ContentLength / 1048576).ToString("0.00")) + " mb";
                                    else
                                        size = "Size : " + ((ContentLength / 1024).ToString("0.00")) + " kb";
                                }
                            }).Wait();

                            SizeTextBlock.Text = size;
                            ProgressBar.IsIndeterminate = false;
                            UpdateButton.Content = Definitions.OptionsULV;
                        }
                        else {
                            UpdateButton.Content = Definitions.OptionsUNA;
                        }
                    }
                    catch { Helper.ShowErrorMessage(E.MSG_NE); }

                    return;
                }

                if (UpdateButton.Content.ToString() == Definitions.OptionsULV) {
                    try {
                        UpdateButton.IsEnabled = false;
                        ProgressBar.IsIndeterminate = true;

                        WebClient client = new WebClient();
                        string url = UpdatesBase + UpdateFile;
                        string path = Path.GetTempPath();
                        string tempdown = Path.Combine(path, Path.GetFileName(url));

                        client.DownloadFileCompleted += (a, b) => {
                            try {
                                UpdateButton.Content = Definitions.OptionsUNA;
                                UpdateButton.IsEnabled = false;
                                if (Helper.ShowQAMessage("Do you want copy of downloaded update file, in case the update installation failed ?\n\n(Note: File will be copied to your desktop.)").HasFlag(MessageBoxResult.Yes)) {
                                    FileInfo fi = new FileInfo(tempdown);
                                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.DoNotVerify);
                                    this.InvokeAsync(() => File.Copy(tempdown, desktop + "\\" + fi.Name)).Wait();
                                }
                                Process.Start(tempdown);
                                System.Threading.Thread.Sleep(500);
                                App.Current.Shutdown(98);
                            }
                            catch { File.Delete(tempdown); }
                        };

                        client.DownloadProgressChanged += (a, b) => {
                            ProgressBar.IsIndeterminate = false;
                            ProgressBar.Value = b.ProgressPercentage;
                        };
                        client.DownloadFileAsync(new Uri(url), tempdown);
                    }
                    catch { Helper.ShowErrorMessage(E.MSG_NE); }

                    ProgressBar.IsIndeterminate = false;
                }
                return;
            }
            catch { }
        }

        #endregion Updates

        internal static void ShowHub(bool nlic = false, bool isupdatemode = false) {
            var window = new SettingsHub(nlic, isupdatemode);
            E.CallEvent("HubOpening");
            window.ShowDialog();
            E.CallEvent("HubClosing");
        }
    }
}
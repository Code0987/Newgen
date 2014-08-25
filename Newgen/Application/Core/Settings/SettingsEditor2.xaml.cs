using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iFramework.Security.Licensing;
using libns.Media.Animation;
using libns.Threading;
using Microsoft.Win32;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor2.xaml
    /// </summary>
    public partial class SettingsEditor2 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor2"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor2() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                EnableUserTile.IsChecked = Settings.Current.IsUserTileEnabled;
                NewgenBgColor.Fill = new SolidColorBrush(Settings.Current.BackgroundColor);
                BgColorAlpha.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BgColorAlpha_ValueChanged);
                BgColorAlpha.Value = (double)(int)Settings.Current.BackgroundColor.A;
                EnableBgImage.IsChecked = Settings.Current.UseBgImage;
                EnableStartBarAlways.IsChecked = Settings.Current.ShowStartbarAlways;
                EnableShowTaskbarCheckBox.IsChecked = Settings.Current.ShowTaskbar;
                EnableShowTaskbarACheckBox.IsChecked = Settings.Current.ShowTaskbarAlways;
                EnableUseTBCheckBox.IsChecked = Settings.Current.UseThumbailsBar;
                
                if (Settings.Current.SlideShowImages.Count > 1)
                    Enable_OOBE_SS.IsChecked = true;
                ValStartScreenSSTimeFrame.Text = Settings.Current.SlideShowTime.ToString();
                ValStartScreenSSTimeFrame.TextChanged += new TextChangedEventHandler(ValStartScreenSSTimeFrame_TextChanged);
                try {
                    StartBarBgColor.Fill = new SolidColorBrush(Settings.Current.ToolbarBackgroundColor);
                }
                catch {
                }

                ValStartScreenTitle.Text = Settings.Current.StartText;
                ValStartScreenTitle.TextChanged += new TextChangedEventHandler(ValStartScreenTitle_TextChanged);

                this.CheckBoxClick(this.EnableBgImage, new RoutedEventArgs());
                this.CheckBoxClick(this.Enable_OOBE_SS, new RoutedEventArgs());

            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        private void SaveSettings() {
            Settings.Current.IsUserTileEnabled = (bool)EnableUserTile.IsChecked;
            Settings.Current.UseBgImage = (bool)EnableBgImage.IsChecked;
            Settings.Current.ShowTaskbar = (bool)EnableShowTaskbarCheckBox.IsChecked;
            Settings.Current.ShowTaskbarAlways = (bool)EnableShowTaskbarACheckBox.IsChecked;
            Settings.Current.ShowStartbarAlways = (bool)EnableStartBarAlways.IsChecked;

            if (Settings.Current.UseThumbailsBar != (bool)EnableUseTBCheckBox.IsChecked) {
                //TODO:restartRequired = true;
            }
            Settings.Current.UseThumbailsBar = (bool)EnableUseTBCheckBox.IsChecked;

            try {
                if (Settings.Current.SlideShowImages.Count < 2 || !(bool)Enable_OOBE_SS.IsChecked) {
                    Settings.Current.SlideShowImages.Clear();
                }
            }
            catch (Exception) {
                MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
            }

        }


        private void ChangeBgImgClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = Api.ImageFilter;
            if (!(bool)dialog.ShowDialog())
                return;

            var window = App.Screen;
            try {
                if (!File.Exists(Settings.Current.StartScreenBackgroundImage))
                    File.Create(Settings.Current.StartScreenBackgroundImage);

                byte[] bytArray = File.ReadAllBytes(dialog.FileName);
                File.WriteAllBytes(Settings.Current.StartScreenBackgroundImage, bytArray);

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
                MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
                Settings.Current.UseBgImage = false;
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
                dialog.Filter = Api.ImageFilter;
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
                    //TODO:restartRequired = true;
                }
                catch (Exception) {
                    MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
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
                MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
            }
        }
        private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e) {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var color = Color.FromArgb(Settings.Current.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                Settings.Current.BackgroundColor = color;
                Settings.Current.BackgroundColor = color;
                NewgenBgColor.Fill = new SolidColorBrush(Settings.Current.BackgroundColor);
                BgColorAlpha.Value = (double)(int)color.A;

                var window = App.Screen;
                window.Background = new SolidColorBrush(Settings.Current.BackgroundColor);
            }
        }

        private void BgColorAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Color c = Settings.Current.BackgroundColor;
            var color = Color.FromArgb((byte)e.NewValue, c.R, c.G, c.B);
            Settings.Current.BackgroundColor = color;
            Settings.Current.BackgroundColor = color;
            NewgenBgColor.Fill = new SolidColorBrush(Settings.Current.BackgroundColor);

            var window = App.Screen;
            window.Background = new SolidColorBrush(Settings.Current.BackgroundColor);
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
                    AnimationExtensions.Animate(this.ChangeBgImg, OpacityProperty, 250, 0, 1);
                    AnimationExtensions.Animate(this.NewgenBgColor, OpacityProperty, 250, 0);
                    AnimationExtensions.Animate(this.TextBgColor, OpacityProperty, 250, 0);
                    AnimationExtensions.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0);
                    AnimationExtensions.Animate(this.TextBgTrans, OpacityProperty, 250, 0);
                    AnimationExtensions.Animate(this.BgColorAlpha, OpacityProperty, 250, 0);
                }
                else {
                    AnimationExtensions.Animate(this.ChangeBgImg, OpacityProperty, 250, 0);
                    AnimationExtensions.Animate(this.NewgenBgColor, OpacityProperty, 250, 0, 1);
                    AnimationExtensions.Animate(this.TextBgColor, OpacityProperty, 250, 0, 1);
                    AnimationExtensions.Animate(this.ChangeBgColorButton, OpacityProperty, 250, 0, 1);
                    AnimationExtensions.Animate(this.TextBgTrans, OpacityProperty, 250, 0, 1);
                    AnimationExtensions.Animate(this.BgColorAlpha, OpacityProperty, 250, 0, 1);
                    ThreadingExtensions.LazyInvokeThreadSafe(new Action(() => {
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
                    AnimationExtensions.Animate(this.OOBE_SS, OpacityProperty, 250, 0, 1);
                }
                else {
                    AnimationExtensions.Animate(this.OOBE_SS, OpacityProperty, 250, 0);
                    ThreadingExtensions.LazyInvokeThreadSafe(new Action(() => {
                        this.OOBE_SS.Visibility = Visibility.Collapsed;
                    }), 200);
                }
            }
        }

    }
}
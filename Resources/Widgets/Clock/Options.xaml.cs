using System;
using System.Windows;
using System.Windows.Input;
using Newgen.Base;

namespace Clock
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : HubWindow
    {
        public event EventHandler UpdateSettings;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            if (Widget.Settings.TimeMode == 1)
                Time24HRadioButton.IsChecked = true;
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

        private void ApplyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", "http://nsapps.net/Apps/Newgen/");
        }

        private void ApplySettings()
        {
            if (Time24HRadioButton.IsChecked == true)
                Widget.Settings.TimeMode = 1;
            else
                Widget.Settings.TimeMode = 0;

            Widget.Settings.Save(Widget.SettingsFile);
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
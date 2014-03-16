using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newgen.Base;

namespace Calendar
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

            UsernameBox.Text = Widget.Settings.Username;
            PassBox.Password = Widget.Settings.Password;
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
            if ((UsernameBox.Text != Widget.Settings.Username) && (Widget.Settings.Username != null))
                resetMsgCount();

            Widget.Settings.Username = UsernameBox.Text;
            Widget.Settings.Password = PassBox.Password;

            Widget.Settings.Save(Widget.SettingsFile);
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void PasswordBoxTextChanged(object sender, RoutedEventArgs e)
        {
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void resetMsgCount()
        {
            Widget.Settings.LastMsgCount = -1;
        }
    }
}
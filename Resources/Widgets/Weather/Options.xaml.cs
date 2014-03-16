using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newgen.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : HubWindow
    {
        public event EventHandler UpdateSettings;
        private List<LocationData> locations;
        private LocationData currentLocation;

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

            ShowFeelsLikeCheckBox.IsChecked = Widget.Settings.ShowFeelsLike;
            if (Widget.Settings.TempScale == TemperatureScale.Celsius)
                CelsiusRadioButton.IsChecked = true;

            WeatherIntervalSlider.Value = Widget.Settings.RefreshInterval;
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
            if (currentLocation != null)
            {
                Widget.Settings.LocationCode = currentLocation.Code;
            }
            Widget.Settings.ShowFeelsLike = (bool)ShowFeelsLikeCheckBox.IsChecked;
            Widget.Settings.TempScale = (bool)CelsiusRadioButton.IsChecked ? TemperatureScale.Celsius : TemperatureScale.Fahrenheit;

            Widget.Settings.Save(Widget.SettingsFile);
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (SearchBox.Foreground == Brushes.Gray)
            {
                SearchBox.Text = "";
                SearchBox.FontStyle = FontStyles.Normal;
                SearchBox.Foreground = Brushes.Black;
            }
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text.Length > 2)
            {
                SearchPopup.IsOpen = true;
                SearchResultBox.Items.Clear();
                var query = SearchBox.Text;
                ThreadStart threadStarter =
                    () => GetLocations(query);
                var thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                //SearchButton.Source = new BitmapImage(new Uri("/Resources/Weather/SearchCancelIcon.png", UriKind.Relative));
            }
        }

        private void SearchBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = Properties.Resources.OptionsSearchBox;
                    SearchBox.FontStyle = FontStyles.Italic;
                    SearchBox.Foreground = Brushes.Gray;
                }
            }
            else
            {
                if (SearchBox.Text == Properties.Resources.OptionsSearchBox)
                {
                    SearchBox.Text = "";
                    SearchBox.FontStyle = FontStyles.Normal;
                    SearchBox.Foreground = Brushes.Black;
                }
            }
        }

        private void SearchResultBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultBox.SelectedIndex == -1)
                return;
            if (SearchResultBox.SelectedIndex > locations.Count)
                currentLocation = locations[locations.Count - 1];
            else
                currentLocation = locations[SearchResultBox.SelectedIndex];
            SearchBox.Text = currentLocation.City;
            SearchPopup.IsOpen = false;
            SearchResultBox.Items.Clear();
        }

        private void GetLocations(string query)
        {
            locations = WeatherWidget.WeatherProvider.GetLocations(query, CultureInfo.GetCultureInfo(E.Language));
            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    SearchResultBox.Dispatcher.Invoke((Action)delegate
                    {
                        SearchResultBox.Items.Add(location);
                    });
                }
            }
            else
            {
                SearchPopup.Dispatcher.Invoke((Action)delegate
                {
                    SearchPopup.IsOpen = false;
                });
            }
        }

        private void WeatherIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WeatherIntervalSlider.Value < 60)
            {
                WeatherIntervalValueTextBlock.Text = WeatherIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (WeatherIntervalSlider.Value == 60)
            {
                WeatherIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else
            {
                WeatherIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(WeatherIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(WeatherIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
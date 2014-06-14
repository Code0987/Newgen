using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newgen;

namespace WeatherPackage {
    /// <summary>
    /// Interaction logic for OptionsHub.xaml
    /// </summary>
    public partial class OptionsHub : HubWindow {
        public event EventHandler UpdateSettings;

        private LocationData currentLocation;
        private List<LocationData> locations;
        private Package package;
        public OptionsHub(Package package)
            : base() {
            this.package = package;

            InitializeComponent();
        }
        
        private void ApplySettings() {
            if (currentLocation != null) {
                package.WeatherOptions.LocationCode = currentLocation.Code;
            }

            package.WeatherOptions.ShowFeelsLike = (bool)ShowFeelsLikeCheckBox.IsChecked;
            package.WeatherOptions.TempScale = (bool)CelsiusRadioButton.IsChecked ? TemperatureScale.Celsius : TemperatureScale.Fahrenheit;

            if (UpdateSettings != null) {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void GetLocations(string query) {
            locations = WeatherProvider.GetLocations(query, Thread.CurrentThread.CurrentCulture);
            if (locations != null && locations.Count > 0) {
                foreach (var location in locations) {
                    SearchResultBox.Dispatcher.Invoke((Action)delegate {
                        SearchResultBox.Items.Add(location);
                    });
                }
            }
            else {
                SearchPopup.Dispatcher.Invoke((Action)delegate {
                    SearchPopup.IsOpen = false;
                });
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e) {
            ApplySettings();

            Close();
        }

        private void OnSearchBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue == false) {
                if (string.IsNullOrEmpty(SearchBox.Text)) {
                    SearchBox.Text = Properties.Resources.OptionsSearchBox;
                    SearchBox.FontStyle = FontStyles.Italic;
                    SearchBox.Foreground = Brushes.Gray;
                }
            }
            else {
                if (SearchBox.Text == Properties.Resources.OptionsSearchBox) {
                    SearchBox.Text = "";
                    SearchBox.FontStyle = FontStyles.Normal;
                    SearchBox.Foreground = Brushes.Black;
                }
            }
        }

        private void OnSearchBoxKeyDown(object sender, KeyEventArgs e) {
            if (SearchBox.Foreground == Brushes.Gray) {
                SearchBox.Text = "";
                SearchBox.FontStyle = FontStyles.Normal;
                SearchBox.Foreground = Brushes.Black;
            }
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text.Length > 2) {
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

        private void OnSearchResultBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
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

        private void OnSourceInitialized(object sender, EventArgs e) {
            ShowFeelsLikeCheckBox.IsChecked = package.WeatherOptions.ShowFeelsLike;

            if (package.WeatherOptions.TempScale == TemperatureScale.Celsius)
                CelsiusRadioButton.IsChecked = true;

            WeatherIntervalSlider.Value = package.WeatherOptions.RefreshInterval;
        }
        private void OnWeatherIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (WeatherIntervalSlider.Value < 60) {
                WeatherIntervalValueTextBlock.Text = WeatherIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (WeatherIntervalSlider.Value == 60) {
                WeatherIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else {
                WeatherIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(WeatherIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(WeatherIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
        }
    }
}
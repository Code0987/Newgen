using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newgen;

namespace WeatherPackage {
    public partial class Tile : Border {
        private Package package;

        private OptionsHub optionsHub;
        private DispatcherTimer weatherTimer;

        public Tile(Package package) {
            this.package = package;

            InitializeComponent();
        }

        public void Load() {
            UpdateUI();

            weatherTimer = new DispatcherTimer();
            weatherTimer.Interval = TimeSpan.FromMinutes(package.WeatherOptions.RefreshInterval);
            weatherTimer.Tick += OnweatherTimerTick;
            weatherTimer.Start();
            
            if (!string.IsNullOrEmpty(package.WeatherOptions.LocationCode))
                Refresh();
        }

        public void Unload() {
            weatherTimer.Tick -= OnweatherTimerTick;
            weatherTimer.Stop();
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            (new Hub(package)).ShowDialog();
        }

        private void OnOptionsItemClick(object sender, RoutedEventArgs e) {
            if (optionsHub != null && optionsHub.IsVisible) {
                optionsHub.Activate();
                return;
            }

            optionsHub = new OptionsHub(package);
            optionsHub.UpdateSettings += OnOptionsWindowUpdateSettings;

            optionsHub.ShowDialog();
        }

        private void OnRefreshItemClick(object sender, RoutedEventArgs e) {
            Refresh();
        }
        
        private void OnweatherTimerTick(object sender, EventArgs e) {
            Refresh();
        }

        private void OnOptionsWindowUpdateSettings(object sender, EventArgs e) {
            optionsHub.UpdateSettings -= OnOptionsWindowUpdateSettings;

            Refresh();

            weatherTimer.Stop();
            weatherTimer.Interval = TimeSpan.FromMinutes(package.WeatherOptions.RefreshInterval);
            weatherTimer.Start();
        }

        private void Refresh() {
            ThreadStart threadStarter = () => {
                try {
                    var wr = WeatherProvider.GetWeatherReport(
                        Thread.CurrentThread.CurrentCulture,
                        package.CurrentWeather.Location, 
                        package.WeatherOptions.TempScale
                        );
                    if (wr != null) {
                        package.CurrentWeather = wr;
                        Dispatcher.BeginInvoke((Action)UpdateUI);
                    }
                }
                catch /* Eat */ { }
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void UpdateUI() {
            Temperture.Text = package.CurrentWeather.Temperature + "° " + package.CurrentWeather.Curent.Text;
            
            if (package.CurrentWeather.ForecastList.Count < 3)
                return;

            FirstDay.Text = package.CurrentWeather.ForecastList[0].Day + " → " + package.CurrentWeather.ForecastList[0].HighTemperature + "° " + package.CurrentWeather.ForecastList[0].Text;
            SecondDay.Text = package.CurrentWeather.ForecastList[1].Day + " → " + package.CurrentWeather.ForecastList[1].HighTemperature + "° " + package.CurrentWeather.ForecastList[1].Text;
            ThirdDay.Text = package.CurrentWeather.ForecastList[2].Day + " → " + package.CurrentWeather.ForecastList[2].HighTemperature + "° " + package.CurrentWeather.ForecastList[2].Text;
            
            WeatherIcon.Source = new BitmapImage(new Uri(string.Format("/WeatherPackage;Component/Resources/weather_{0}.png", package.CurrentWeather.Curent.SkyCode), UriKind.Relative));
            
            Location.Text = package.CurrentWeather.Location.City;
        }
    }
}
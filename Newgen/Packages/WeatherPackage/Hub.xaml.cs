using System;
using System.Windows.Media.Imaging;
using Newgen;

namespace WeatherPackage {
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : HubWindow {
        private Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Hub"/> class.
        /// </summary>
        public Hub(Package package)
            : base() {
            this.package = package;

            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnBackButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            this.TextBox_CurrentLocation.Text = package.CurrentWeather.Location.City;

            this.TextBox_CurrentTemp.Text = package.CurrentWeather.Curent.HighTemperature + "°";
            this.TextBox_CurrentLowTemp.Text = package.CurrentWeather.Curent.LowTemperature + "°";

            this.TextBox_CurrentSky.Text = package.CurrentWeather.Curent.Text;
            this.TextBox_CurrentFeelsLike.Text = string.Concat(new object[] { "Feels like ", package.CurrentWeather.FeelsLike, "°" });

            this.TextBox_CurrentHumidity.Text = package.CurrentWeather.Curent.Humidity + "% Humidity";
            this.TextBox_CurrentPrecip.Text = package.CurrentWeather.Curent.Precip + "% Chances of Rain";

            for (int i = 0; i < package.CurrentWeather.ForecastList.Count; i++) {
                ((ForecastItem)this.ForecastItems.Children[i]).Day.Text =
                    DateTime.Now.AddDays((double)i).ToString("dddd") + " " + DateTime.Now.AddDays((double)i).Day;
                ((ForecastItem)this.ForecastItems.Children[i]).WeatherIcon.Source =
                    new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png", package.CurrentWeather.ForecastList[i].SkyCode), UriKind.Relative));
                ((ForecastItem)this.ForecastItems.Children[i]).Temperature.Text =
                    package.CurrentWeather.ForecastList[i].HighTemperature + "°";
                ((ForecastItem)this.ForecastItems.Children[i]).Sky.Text =
                    package.CurrentWeather.ForecastList[i].Text;
                ((ForecastItem)this.ForecastItems.Children[i]).Precipitation.Text =
                    package.CurrentWeather.ForecastList[i].Precip + "% Chances of Rain";
                ((ForecastItem)this.ForecastItems.Children[i]).LowTemperature.Text = package.CurrentWeather.ForecastList[i].LowTemperature + "° Low";
            }
        }
    }
}
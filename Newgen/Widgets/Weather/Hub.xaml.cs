using System;
using System.Windows.Media.Imaging;
using Newgen.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : HubWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hub"/> class.
        /// </summary>
        public Hub()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TextBox_CurrentLocation.Text = WeatherWidget.CurrentWeather.Location.City;

            this.TextBox_CurrentTemp.Text = WeatherWidget.CurrentWeather.Curent.HighTemperature + "°";
            this.TextBox_CurrentLowTemp.Text = WeatherWidget.CurrentWeather.Curent.LowTemperature + "°";

            this.TextBox_CurrentSky.Text = WeatherWidget.CurrentWeather.Curent.Text;
            this.TextBox_CurrentFeelsLike.Text = string.Concat(new object[] { "Feels like ", WeatherWidget.CurrentWeather.FeelsLike, "°" });

            this.TextBox_CurrentHumidity.Text = WeatherWidget.CurrentWeather.Curent.Humidity + "% Humidity";
            this.TextBox_CurrentPrecip.Text = WeatherWidget.CurrentWeather.Curent.Precip + "% Chances of Rain";

            for(int i = 0; i < WeatherWidget.CurrentWeather.ForecastList.Count; i++)
            {
                ((ForecastItem)this.ForecastItems.Children[i]).Day.Text =
                    DateTime.Now.AddDays((double)i).ToString("dddd") + " " + DateTime.Now.AddDays((double)i).Day;
                ((ForecastItem)this.ForecastItems.Children[i]).WeatherIcon.Source =
                    new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png", WeatherWidget.CurrentWeather.ForecastList[i].SkyCode), UriKind.Relative));
                ((ForecastItem)this.ForecastItems.Children[i]).Temperature.Text =
                    WeatherWidget.CurrentWeather.ForecastList[i].HighTemperature + "°";
                ((ForecastItem)this.ForecastItems.Children[i]).Sky.Text =
                    WeatherWidget.CurrentWeather.ForecastList[i].Text;
                ((ForecastItem)this.ForecastItems.Children[i]).Precipitation.Text =
                    WeatherWidget.CurrentWeather.ForecastList[i].Precip + "% Chances of Rain";
                ((ForecastItem)this.ForecastItems.Children[i]).LowTemperature.Text = WeatherWidget.CurrentWeather.ForecastList[i].LowTemperature + "° Low";
            }
        }
    }
}
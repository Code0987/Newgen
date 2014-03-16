using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newgen.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for WeatherWidget.xaml
    /// </summary>
    public partial class WeatherWidget : UserControl
    {
        internal static WeatherProvider WeatherProvider;
        internal static WeatherData CurrentWeather;
        private LocationData currentLocation;
        private Options optionsWindow;
        private DispatcherTimer weatherTimer;
        private DispatcherTimer tileAnimTimer;

        public WeatherWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            WeatherProvider = new WeatherProvider();
            CurrentWeather = (WeatherData)XmlSerializable.Load(typeof(WeatherData), Widget.DataFile) ?? new WeatherData();
            currentLocation = new LocationData();
            currentLocation.Code = Widget.Settings.LocationCode;
            UpdateWeatherUI();

            weatherTimer = new DispatcherTimer();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Tick += WeatherTimerTick;
            weatherTimer.Start();

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(15);
            tileAnimTimer.Tick += TileAnimTimerTick;

            tileAnimTimer.Start();

            if(!string.IsNullOrEmpty(Widget.Settings.LocationCode))
                RefreshWeather();
        }

        private void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void WeatherTimerTick(object sender, EventArgs e)
        {
            RefreshWeather();
        }

        private void RefreshWeather()
        {
            ThreadStart threadStarter = () =>
            {
                try
                {

                    var w = WeatherProvider.GetWeatherReport(CultureInfo.GetCultureInfo(E.Language), currentLocation, Widget.Settings.TempScale);
                    if(w != null)
                    {
                        CurrentWeather = w;
                        this.Dispatcher.BeginInvoke((Action)UpdateWeatherUI);
                    }
                }
                catch { }
                //currentWeather.Save(E.WidgetsRoot + "\\Weather\\Weather.data");
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void UpdateWeatherUI()
        {
            Temperture.Text = CurrentWeather.Temperature + "° " + CurrentWeather.Curent.Text;
            if(CurrentWeather.ForecastList.Count < 3)
                return;
            FirstDay.Text = CurrentWeather.ForecastList[0].Day + ": " + CurrentWeather.ForecastList[0].HighTemperature + "° " + CurrentWeather.ForecastList[0].Text;
            SecondDay.Text = CurrentWeather.ForecastList[1].Day + ": " + CurrentWeather.ForecastList[1].HighTemperature + "° " + CurrentWeather.ForecastList[1].Text;
            ThirdDay.Text = CurrentWeather.ForecastList[2].Day + ": " + CurrentWeather.ForecastList[2].HighTemperature + "° " + CurrentWeather.ForecastList[2].Text;
            WeatherIcon.Source = new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png", CurrentWeather.Curent.SkyCode), UriKind.Relative));
            Location.Text = CurrentWeather.Location.City;
        }

        public void Unload()
        {
            CurrentWeather.Save(Widget.DataFile);
            weatherTimer.Tick -= WeatherTimerTick;
            weatherTimer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if(optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if(E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsWindow.ShowDialog();
        }

        private void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;
            currentLocation.Code = Widget.Settings.LocationCode;
            RefreshWeather();

            weatherTimer.Stop();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Start();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            RefreshWeather();
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (new Hub()).ShowDialog();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WeatherPackage {
    public enum TemperatureScale {
        Null = -1,
        Fahrenheit = 0,
        Celsius = 1
    }

    public class LocationData {
        /// <summary>
        /// location code
        /// </summary>
        public string Code;

        /// <summary>
        /// Location name
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Country name
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Latitude
        /// </summary>
        public double Lat;
        /// <summary>
        /// Longtitude
        /// </summary>
        public double Lon;
    }

    public class ForecastData {
        public string Day { get; set; }

        public int HighTemperature { get; set; }

        public int LowTemperature { get; set; }

        public string Text { get; set; }

        public int SkyCode { get; set; }

        public int Humidity { get; set; }

        public int Precip { get; set; }

        public string Url { get; set; }

        public ForecastData() {
            SkyCode = 1;
        }
    }

    public class WeatherData {
        public LocationData Location;
        public int Temperature = 0;
        public int FeelsLike;
        public ForecastData Curent;
        public List<ForecastData> ForecastList;

        public WeatherData() {
            Location = new LocationData();
            Curent = new ForecastData();
            ForecastList = new List<ForecastData>();
        }
    }

    public class WeatherSettings {
        public WeatherSettings() {
            TempScale = TemperatureScale.Celsius;
            RefreshInterval = 20;
            ShowFeelsLike = false;
        }

        public string LocationCode { get; set; }

        public double RefreshInterval { get; set; }

        public TemperatureScale TempScale { get; set; }

        public bool ShowFeelsLike { get; set; }
    }


    public class WeatherProvider {
        private const string RequestForLocation = "http://weather.msn.com/find.aspx?weasearchstr={0}&culture={1}";
        private const string RequestForCelsius = "http://weather.service.msn.com/data.aspx?culture={0}&wealocations={1}&weadegreetype=C";
        private const string RequestForFahrenheit = "http://weather.service.msn.com/data.aspx?culture={0}&wealocations={1}&weadegreetype=F";
        private const string ForecastUrlForCelsius = "http://weather.msn.com/local.aspx?wealocations={0}&weadegreetype=C";
        private const string ForecastUrlForFahrenheit = "http://weather.msn.com/local.aspx?wealocations={0}&weadegreetype=F";

        public static List<LocationData> GetLocations(string query, CultureInfo culture) {
            var result = new List<LocationData>();
            XElement xml;

            try {
                xml = XElement.Load(string.Format(RequestForLocation, query, culture.Name));
            }
            catch (Exception) {
                return null;
            }

            var resultTempsScale = TemperatureScale.Null;

            foreach (var el in xml.Elements("weather")) {
                var l = new LocationData();
                var locString = el.Attribute("weatherlocationname").Value;
                if (locString.Contains(",")) {
                    l.City = locString.Substring(0, locString.IndexOf(","));
                    l.Country = locString.Substring(locString.IndexOf(",") + 2);
                }
                else
                    l.City = locString;
                l.Code = el.Attribute("weatherlocationcode").Value;
                if (resultTempsScale == TemperatureScale.Null) {
                    var scale = el.Attribute("degreetype").Value;
                    if (scale.ToLower() == "c")
                        resultTempsScale = TemperatureScale.Celsius;
                    else
                        resultTempsScale = TemperatureScale.Fahrenheit;
                }
                result.Add(l);
            }

            return result;
        }

        public static WeatherData GetWeatherReport(CultureInfo culture, LocationData location, TemperatureScale tempScale) {
            var url = string.Format(tempScale == TemperatureScale.Celsius ? RequestForCelsius : RequestForFahrenheit, culture.Name, location.Code);

            XDocument doc;

            try {
                doc = XDocument.Load(url);
            }
            catch (Exception) {
                return null;
            }

            var weather = from x in doc.Descendants("weather")
                          let xElement = x.Element("current")
                          where xElement != null
                          select new {
                              feelslike = xElement.Attribute("feelslike").Value,
                              windspeed = xElement.Attribute("windspeed").Value,
                              humidity = xElement.Attribute("humidity").Value,
                              temp = xElement.Attribute("temperature").Value,
                              text = xElement.Attribute("skytext").Value,
                              skycode = xElement.Attribute("skycode").Value,
                              windstring = xElement.Attribute("winddisplay").Value
                          };

            var result = new WeatherData();
            var currentWeather = weather.FirstOrDefault();
            if (currentWeather == null)
                return null;

            var t = 0;
            int.TryParse(currentWeather.temp, out t);
            result.Temperature = t;

            int.TryParse(currentWeather.feelslike, out t);
            result.FeelsLike = t;

            result.Curent = new ForecastData() {
                Text = currentWeather.text,
                //SkyCode = GetWeatherPic(Convert.ToInt32(currentWeather.skycode), 4, 22)
            };

            result.Location = location;

            if (doc.Descendants("weather").FirstOrDefault() == null)
                return null;

            var locString = doc.Descendants("weather").FirstOrDefault().Attribute("weatherlocationname").Value;
            if (locString.Contains(",")) {
                result.Location.City = locString.Substring(0, locString.IndexOf(","));
                result.Location.Country = locString.Substring(locString.IndexOf(",") + 2);
            }
            else {
                result.Location.City = locString;
            }

            //parse coordinates
            var coords = from x in doc.Descendants("weather")
                         select new {
                             lon = x.Attribute("long").Value,
                             lat = x.Attribute("lat").Value
                         };

            double lat, lon = double.MinValue;

            double.TryParse(coords.FirstOrDefault().lat, NumberStyles.Float, CultureInfo.GetCultureInfo("en-US").NumberFormat, out lat);
            double.TryParse(coords.FirstOrDefault().lon, NumberStyles.Float, CultureInfo.GetCultureInfo("en-US").NumberFormat, out lon);

            result.Location.Lat = lat;
            result.Location.Lon = lon;

            var sunrise = DateTime.Today.AddHours(4);
            var sunset = DateTime.Today.AddHours(22);

            result.Curent.SkyCode = GetWeatherPic(Convert.ToInt32(currentWeather.skycode));
            result.Curent.Humidity = int.Parse(currentWeather.humidity);

            //parse forecast
            var forecastUrl = string.Format(tempScale == TemperatureScale.Celsius ? ForecastUrlForCelsius : ForecastUrlForFahrenheit, location.Code);

            var f = new List<ForecastData>();

            foreach (var day in doc.Descendants("forecast")) {
                f.Add(new ForecastData());
                var temp = 0;
                int.TryParse(day.Attribute("high").Value, out temp);
                f[f.Count - 1].HighTemperature = temp;
                int.TryParse(day.Attribute("low").Value, out temp);
                f[f.Count - 1].LowTemperature = temp;
                f[f.Count - 1].Text = day.Attribute("skytextday").Value;
                f[f.Count - 1].SkyCode = GetWeatherPic(Convert.ToInt32(day.Attribute("skycodeday").Value));
                f[f.Count - 1].Humidity = -1;
                try { f[f.Count - 1].Precip = int.Parse(day.Attribute("precip").Value); }
                catch { f[f.Count - 1].Precip = -1; }
                f[f.Count - 1].Url = forecastUrl;
                f[f.Count - 1].Day = day.Attribute("day").Value;
            }

            if (f.Count > 0) {
                result.Curent.HighTemperature = f[0].HighTemperature;
                result.Curent.LowTemperature = f[0].LowTemperature;
                result.Curent.Precip = f[0].Precip;
            }

            result.ForecastList = f;

            return result;
        }

        //simple get weather pic method which returns icon associated with given skycode
        public static int GetWeatherPic(int skycode) {
            switch (skycode) {
            case 26:
                return 2;
            case 27:
                return 3;
            case 28:
                return 6;
            case 35:
            case 39:
                return 12;
            case 45:
            case 46:
                return 8;
            case 19:
            case 20:
            case 21:
            case 22:
                return 11;
            case 29:
            case 30:
                return 3;
            case 33:
                return 6;
            case 5:
            case 13:
            case 14:
            case 15:
            case 16:
                return 22;
            case 18:
            case 25:
            case 41:
            case 42:
            case 43:
                return 25;
            case 1:
            case 2:
            case 3:
            case 4:
            case 37:
            case 38:
            case 47:
                return 15;
            case 31:
            case 32:
            case 34:
            case 36:
            case 44:
                return 1;
            case 23:
            case 24:
                return 32;
            case 9:
            case 10:
            case 11:
            case 12:
            case 40:
                return 18;
            case 6:
            case 7:
            case 8:
            case 17:
                return 15;
            default:
                return 1;
            }
        }
    }
}

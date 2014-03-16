using System.Collections.Generic;
using Newgen.Base;

namespace Weather
{
    public class WeatherData : XmlSerializable
    {
        public LocationData Location; //name of the current location
        public int Temperature = 0; //curent temperature
        public int FeelsLike; //feels like temperature
        public ForecastData Curent; //curent forecast
        public List<ForecastData> ForecastList;

        public WeatherData()
        {
            Location = new LocationData();
            Curent = new ForecastData();
            ForecastList = new List<ForecastData>();
        }
    }
}
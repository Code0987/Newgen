using Newgen.Base;

namespace Weather
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            TempScale = TemperatureScale.Celsius;
            RefreshInterval = 20;
            ShowFeelsLike = false;
        }

        public string LocationCode { get; set; }

        public double RefreshInterval { get; set; }

        public TemperatureScale TempScale { get; set; }

        public bool ShowFeelsLike { get; set; }
    }
}
namespace InternetPackage
{
    public enum RenderingMode {
        IE,
        CEF
    }

    public class Settings 
    {
        internal const string DefaultLocation = "about:credits";

        public string RelativeSearchAddressFormat { get; set; }

        public string LastSearchLocation { get; set; }

        public RenderingMode RenderingMode { get; set; }

        public Settings()
        {
            RelativeSearchAddressFormat = "http://www.google.com/search?q={0}";
            LastSearchLocation = "http://www.bing.com";
            RenderingMode = RenderingMode.CEF;
        }
    }
}
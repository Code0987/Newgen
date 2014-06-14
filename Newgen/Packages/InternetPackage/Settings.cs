namespace InternetPackage
{
    public enum RenderingMode {
        IE,
        Webkit
    }

    public class Settings 
    {
        public string LastSearchURL { get; set; }

        public RenderingMode RenderingMode { get; set; }

        public Settings()
        {
            LastSearchURL = "";
            RenderingMode = RenderingMode.IE;
        }
    }
}
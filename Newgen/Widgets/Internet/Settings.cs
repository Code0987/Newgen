namespace Internet
{
    using Newgen.Base;

    public class Settings : XmlSerializable
    {
        public Settings()
        {
            LastSearchURL = "";
            RenderingMode = Internet.RenderingMode.IE;
        }

        public string LastSearchURL { get; set; }

        public RenderingMode RenderingMode { get; set; }
    }

    public enum RenderingMode
    {
        IE,
        Webkit
    }
}
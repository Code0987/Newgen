using Newgen.Base;

namespace Twitter
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            RefreshInterval = 80; //seconds
        }

        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }

        public long LastTweetId { get; set; }

        public int RefreshInterval { get; set; }
    }
}
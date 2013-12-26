using Newgen.Base;

namespace Socialite
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
        }

        public string AuthData { get; set; }
    }
}
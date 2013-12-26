using Newgen.Base;

namespace Me
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
        }

        public string PicturePath { get; set; }
    }
}
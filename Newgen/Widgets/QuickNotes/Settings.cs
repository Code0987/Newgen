using Newgen.Base;

namespace QuickNotes
{
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings : XmlSerializable
    {
        public string NotesData { get; set; }

        /// <summary>
        /// Initializes settings.
        /// </summary>
        public Settings()
        {
        }
    }
}
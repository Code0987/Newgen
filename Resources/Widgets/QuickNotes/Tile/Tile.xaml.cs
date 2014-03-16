using System.Windows.Controls;
using Newgen.Base;

namespace QuickNotes
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        public Tile()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //! Localization : Make widget use same language as Newgen. If you are not planning to add localization support you can remove this lines.
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! }

            //! Initialize all your objects here.
            this.TextBox_Notes.Text = Widget.Settings.NotesData ?? "";
        }

        public void Unload()
        {
            //! Free resources here to save memory.
            Widget.Settings.Save(Widget.SettingsFile);
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox_Notes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void TextBox_Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(this.TextBox_Notes.Text))
                this.TextBox_Notes.Text = QuickNotes.Resources.Resources.Text_Help1;
            else
            {
                Widget.Settings.NotesData = this.TextBox_Notes.Text;
            }
        }
    }
}
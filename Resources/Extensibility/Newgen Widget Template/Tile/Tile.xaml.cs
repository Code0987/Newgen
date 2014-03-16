using System.Windows.Controls;
using Newgen.Base;
using TemplateWidget.Hubs;

namespace TemplateWidget
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        private Hub hub;

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
        }

        public void Unload()
        {
            //! Free resources here to save memory.
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //! If HUB is already loaded so just bring it to the front.
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            //! Initialize HUB
            hub = new Hub();
            hub.Topmost = false; //! Set to false if HUB respects LockScreen of Newgen.
            hub.AllowsTransparency = true; //! Set to false if widget contains "WebBrowser".
            hub.ShowDialog(); //! Finally Display HUB.
        }
    }
}
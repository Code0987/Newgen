using System.Windows.Controls;
using Newgen.Base;

namespace NewgenWidget
{
    /// <summary>
    /// Interaction logic for WidgetControl.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        private HubWindow hub;
        private HubControl hubContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetControl"/>.
        /// </summary>
        public WidgetControl()
        {
            InitializeComponent();
        }

        public void Load()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language); //! }
        }

        public void Unload()
        {
        }

        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InvokeHub();
        }

        internal void InvokeHub()
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.Topmost = false; //! Set to false if HUB respects LockScreen of Newgen.
            hub.AllowsTransparency = true; //! Set to false if widget contains "WebBrowser".
            hubContent = new HubControl(); //! HUB Content.
            hubContent.Closing += HubContentClosing; //! Subscribe to Closing event.
            hubContent.Width = hub.Width;
            hubContent.Height = hub.Height;
            hub.Content = hubContent; //! Set HUB Content
            hub.ShowDialog(); //! Finally Display HUB.
        }

        private void HubContentClosing(object sender, System.EventArgs e)
        {
            hub.Close();
        }
    }
}
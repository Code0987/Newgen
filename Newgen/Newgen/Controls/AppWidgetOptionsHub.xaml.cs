using Newgen.Base;
using Newgen.Core;

namespace Newgen.Hubs
{
    /// <summary>
    /// Interaction logic for AppWidgetOptionsHub.xaml
    /// </summary>
    public partial class AppWidgetOptionsHub : HubWindow
    {
        private NewgenAppWidget appwidget;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeHub"/> class.
        /// </summary>
        /// <param name="appwidget">The appwidget.</param>
        public AppWidgetOptionsHub(NewgenAppWidget appwidget)
            : base()
        {
            this.appwidget = appwidget;

            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TextBox_WidgetTitle.Text = this.appwidget.title.Text;
            this.IconImage.Source = this.appwidget.icon.Source;
            try { this.TextBox_WidgetArgs.Text = this.appwidget.args ?? ""; }
            catch { }
        }

        /// <summary>
        /// Handles the Click event of the OkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SaveData();

            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the CancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveData()
        {
            try
            {
                HelperMethods.SetDataArray(this.appwidget.fileorfolder, this.TextBox_WidgetTitle.Text, 1);
                HelperMethods.SetDataArray(this.appwidget.fileorfolder, this.TextBox_WidgetArgs.Text, 2);
            }
            catch { }

            this.appwidget.title.Text = this.TextBox_WidgetTitle.Text;
            this.appwidget.args = this.TextBox_WidgetArgs.Text;
        }
    }
}
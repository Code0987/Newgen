using Newgen.Base;

namespace ControlPanel
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : HubWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hub"/> class.
        /// </summary>
        public Hub() : base() { this.InitializeComponent(); }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace NewgenWidget
{
    /// <summary>
    /// Interaction logic for HubControl.xaml
    /// </summary>
    public partial class HubControl : UserControl
    {
        public event EventHandler Closing;

        /// <summary>
        /// Initializes a new instance of the <see cref="HubControl"/> class.
        /// </summary>
        public HubControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Closing != null) { Closing(this, EventArgs.Empty); }
        }

        // Supp.

        private void Link_Site_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen(
                "URL", "http://nsapps.net/Apps/Newgen/");
        }

        private void Link_WWIKI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Newgen.Base.MessagingHelper.SendMessageToNewgen(
                "URL", "http://nsapps.net/Apps/Newgen/APIDoc.html");
        }
    }
}
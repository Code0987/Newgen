using System.ServiceModel.Syndication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using libns;

namespace Newgen.Packages.Notifications {

    /// <summary>
    /// Interaction logic for NotificationsPackageTileItem.xaml
    /// </summary>
    public partial class NotificationsPackageTileItem : UserControl {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsPackageTileItem" /> class.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <remarks>...</remarks>
        public NotificationsPackageTileItem(Notification n) {
            InitializeComponent();

            DataContext = n;
        }

        /// <summary>
        /// Handles the <see cref="E:MouseDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            var n = DataContext as Notification;
            if (n != null)
                return;
            var m = n.Other as Action;
            if (m != null)
                m();
        }
    }
}

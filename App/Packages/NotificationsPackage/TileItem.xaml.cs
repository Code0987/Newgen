using System.ServiceModel.Syndication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using libns;
using System.Windows.Media;

namespace NotificationsPackage {

    /// <summary>
    /// Interaction logic for TileItem.xaml
    /// </summary>
    public partial class TileItem : UserControl {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileItem" /> class.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <remarks>...</remarks>
        public TileItem(Notification n) {
            InitializeComponent();

            DataContext = n;

            if (n.Title.Contains("Error"))
                Background = new SolidColorBrush(Color.FromArgb(0x3F, 0xFF, 0x54, 0x54));
            if (n.Title.Contains("Warning"))
                Background = new SolidColorBrush(Color.FromArgb(0x3F, 0xFF, 0xD0, 0x52));
            if (n.Title.Contains("Debug"))
                Background = new SolidColorBrush(Color.FromArgb(0x3F, 0xB0, 0x52, 0xFF));
            if (n.Title.Contains("Information"))
                Background = new SolidColorBrush(Color.FromArgb(0x3F, 0x40, 0xAF, 0xFB));
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
            var m = n[typeof(Action)] as Action;
            if (m != null)
                m();
        }
    }
}

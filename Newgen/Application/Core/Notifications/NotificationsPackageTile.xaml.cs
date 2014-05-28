using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns;
using libns.Threading;

namespace Newgen.Packages.Notifications {
    /// <summary>
    /// Interaction logic for NotificationsPackageTile.xaml
    /// </summary>
    public partial class NotificationsPackageTile : Border {
        /// <summary>
        /// The nm
        /// </summary>
        internal DynamicNotificationManager nm;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsPackageTile"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public NotificationsPackageTile() {
            InitializeComponent();

            nm = new DynamicNotificationManager(
                OnNotificationLogic,
                NotificationProviderType.Custom,
                OnNotificationCompleteLogic
                );
        }

        /// <summary>
        /// Called when [notification logic].
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="n">The n.</param>
        /// <remarks>...</remarks>
        protected void OnNotificationLogic(DynamicNotificationManager manager, Notification n) {
            this.InvokeAsyncThreadSafe(() => {
                var item = new NotificationsPackageTileItem(n);
                n.Other = item;
                ItemsContainer.Children.Add(item);
                ItemsScrollViewer.ScrollToBottom();
            });
        }

        /// <summary>
        /// Called when [notification complete logic].
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="n">The n.</param>
        /// <remarks>...</remarks>
        protected void OnNotificationCompleteLogic(DynamicNotificationManager manager, Notification n) {
            this.InvokeAsyncThreadSafe(() => {
                var item = n.Other as NotificationsPackageTileItem;
                if (item != null)
                    ItemsContainer.Children.Remove(item);
            });
        }
    }
}

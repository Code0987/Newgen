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
        internal DynamicLog logger;

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

            logger = new DynamicLog(OnLoggerLog);
        }

        /// <summary>
        /// Called when [logger log].
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="args">The arguments.</param>
        /// <remarks>...</remarks>
        protected void OnLoggerLog(DynamicLog logger, LogLevel level, object message, System.Exception exception, object[] args) {
            nm.ShowNotification(new Notification(level.ToString(), message.ToString(), 1000 * 30));
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
                n[typeof(NotificationsPackageTileItem)] = item;
                ItemsContainer.Children.Insert(0, item);
                ItemsScrollViewer.ScrollToTop();
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
                var item = n[typeof(NotificationsPackageTileItem)] as NotificationsPackageTileItem;
                if (item != null)
                    ItemsContainer.Children.Remove(item);
            });
        }
    }
}

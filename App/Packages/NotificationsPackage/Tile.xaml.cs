using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using libns;
using libns.Threading;
using Newgen;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NodeWebkit;

namespace NotificationsPackage {

    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Tile : Border {
        /// <summary>
        /// The nm
        /// </summary>
        internal DynamicNotificationManager nm;
        internal DynamicLog logger;        
        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

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
                var item = new TileItem(n);
                n[typeof(TileItem)] = item;
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
                var item = n[typeof(TileItem)] as TileItem;
                if (item != null)
                    ItemsContainer.Children.Remove(item);
            });
        }
    }
}
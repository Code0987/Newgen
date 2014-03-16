using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newgen;
using Newgen.Controls;
using libns.Threading;

namespace Newgen.Hubs {

    /// <summary>
    /// Interaction logic for StoreHub.xaml
    /// </summary>
    public partial class StoreHub : HubWindow {

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeHub"/> class.
        /// </summary>
        /// <param name="appwidget">The appwidget.</param>
        public StoreHub()
            : base() {

            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {

            this.InvokeAsync(SyncContent);

        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Shows the hub.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void ShowHub() {
            var window = new StoreHub();
            E.CallEvent("HubOpening");
            window.ShowDialog();
            E.CallEvent("HubClosing");
        }

        /// <summary>
        /// Synchronizes the content.
        /// </summary>
        /// <remarks>...</remarks>
        private void SyncContent() {
            var fa = InternalHelper.FeedsAggregator;

            fa.Get();

            OnDataReady();
        }

        /// <summary>
        /// Called when [data ready].
        /// </summary>
        /// <remarks>...</remarks>
        private void OnDataReady() {
            var fa = InternalHelper.FeedsAggregator;

            foreach (var feed in fa.CachedFeeds)
                foreach (var item in feed.Items) {
                    var method = new Action(() => {
                        ItemsContainer.Children.Add(new StoreItem(item));
                    });
                    this.InvokeAsyncThreadSafe(method);
                }
        }

    }
}
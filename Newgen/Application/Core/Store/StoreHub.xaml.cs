using System;
using System.IO;
using System.Windows;
using libns.Threading;
using Microsoft.Win32;
using Newgen;
using Newgen.Packages;

namespace Newgen {

    /// <summary>
    /// Interaction logic for StoreHub.xaml
    /// </summary>
    public partial class StoreHub : HubWindow {

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeHub" /> class.
        /// </summary>
        /// <param name="appwidget">The appwidget.</param>
        public StoreHub()
            : base() {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the hub.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void ShowHub() {
            var window = new StoreHub();
            Api.CallEvent("HubOpening");
            window.ShowDialog();
            Api.CallEvent("HubClosing");
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void OnBackButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the Loaded event of the HubWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void HubWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            this.InvokeAsync(SyncContent);
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
                        ItemsContainerForNewPackages.Children.Add(new StoreItem(item));
                    });
                    this.InvokeAsyncThreadSafe(method);
                }
        }

        /// <summary>
        /// Called when [local data ready].
        /// </summary>
        /// <remarks>...</remarks>
        private void OnLocalDataReady() {
            foreach (var package in PackageManager.Current.Packages) {
                var method = new Action(() => {
                    ItemsContainerForLocalPackages.Children.Add(new StoreItem(package.Metadata));
                });
                this.InvokeAsyncThreadSafe(method);
            }
        }

        /// <summary>
        /// Synchronizes the content.
        /// </summary>
        /// <remarks>...</remarks>
        private void SyncContent() {
            var fa = InternalHelper.FeedsAggregator;

            fa.Get();

            OnLocalDataReady();
            OnDataReady();
        }
    }
}
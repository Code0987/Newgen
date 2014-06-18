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

namespace TodayPackage {

    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Tile : Border {

        /// <summary>
        /// The current text
        /// </summary>
        private string[] currentTextParts;

        /// <summary>
        /// The timer
        /// </summary>
        private System.Threading.Timer feedsTimer;

        /// <summary>
        /// The list timer
        /// </summary>
        private DispatcherTimer listTimer;

        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// The random
        /// </summary>
        private Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

            random = new Random();

            InitializeComponent();

            SettingsEditor.DataContext = package.CustomizedSettings;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Load() {
            listTimer = ThreadingExtensions.LazyInvokeThreadSafe(UpdateList, 100);
            feedsTimer = ThreadingExtensions.LazyInvoke(UpdateFeeds, 7500);
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {
            if (listTimer != null)
                listTimer.Stop();
            if (feedsTimer != null)
                feedsTimer.Dispose();
        }

        /// <summary>
        /// Handles the <see cref="E:ChangeContentMenuItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnChangeContentMenuItemClick(object sender, RoutedEventArgs e) {
            if (listTimer != null)
                listTimer.Stop();
            UpdateList();
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (currentTextParts != null && currentTextParts.Length > 2 && !string.IsNullOrWhiteSpace(currentTextParts[2]))
                Api.Messenger.Send(new EMessage() { Key = EMessage.UrlKey, Value = currentTextParts[2] });
        }

        /// <summary>
        /// Updates the feeds.
        /// </summary>
        /// <remarks>...</remarks>
        private void UpdateFeeds() {
            try {
                foreach (var feed in (new FeedsAggregator(
                            Guid.NewGuid(),
                            null,
                            package.CustomizedSettings.Feeds.Select(f => new Uri(f)).ToList()
                            )
                            .Get()
                            .CachedFeeds
                            )) {
                    foreach (var feedItem in feed.Items) {
                        try {
                            var text = string.Format(
                                "{2}{0}{1}{0}{3}",
                                Settings.ListItemPartsDelimiter,
                                feedItem.Title.Text.Trim(),
                                feedItem.Summary.Text.Replace("\"", "").Trim(),
                                feedItem.Links.Count > 0 ? feedItem.Links[0].Uri.OriginalString : string.Empty
                                );
                            if (!package.CustomizedSettings.List.Contains(text))
                                package.CustomizedSettings.List.Add(text);
                        }
                        catch /* Eat */ { }
                    }
                }
            }
            catch /* Eat */ { }

            feedsTimer = ThreadingExtensions.LazyInvoke(UpdateFeeds, package.CustomizedSettings.FeedsInterval);
        }

        /// <summary>
        /// Updates the list.
        /// </summary>
        /// <remarks>...</remarks>
        private void UpdateList() {
            try {
                currentTextParts = (package.CustomizedSettings.List.Count > 0
                    ? package.CustomizedSettings.List[random.Next(package.CustomizedSettings.List.Count)]
                    : string.Empty)
                    .Split(Settings.ListItemPartsDelimiter);

                (Content.Resources["TransitionAnimationOff"] as Storyboard).Begin();
                
                ThreadingExtensions.LazyInvokeThreadSafe(
                    () => {
                        TextBlock_Quote.Text = currentTextParts[0];
                        TextBlock_Author.Text = currentTextParts[1];
                        (Content.Resources["TransitionAnimationOn"] as Storyboard).Begin();
                    },
                    700);
            }
            catch /* Eat */ { }

            listTimer = ThreadingExtensions.LazyInvokeThreadSafe(UpdateList, package.CustomizedSettings.ListInterval);
        }
    }
}
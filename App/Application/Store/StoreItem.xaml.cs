using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Newgen;
using PackageManager;

namespace Newgen {

    /// <summary>
    /// Interaction logic for StoreItem.xaml
    /// </summary>
    public partial class StoreItem : UserControl {
        internal readonly SyndicationItem FeedItem;
        internal readonly Package Package;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItem" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="feedItem">The feed item.</param>
        /// <remarks>...</remarks>
        public StoreItem(Package package, SyndicationItem feedItem) {
            Package = package;
            FeedItem = feedItem;

            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItem"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public StoreItem() {
            InitializeComponent();
        }

        /// <summary>
        /// Loads the store item from feed item.
        /// </summary>
        /// <remarks>...</remarks>
        private void LoadStoreItemFromFeedItem() {
        }

        /// <summary>
        /// Loads the store item from package.
        /// </summary>
        /// <remarks>...</remarks>
        private void LoadStoreItemFromPackage() {
            var pkg = Package as NewgenPackage;
            if (pkg != null)
                IconImage.Source = pkg.GetLogo();

            IdText.Text = Package.GetId();

            VersionText.Text = Package.GetVersion();

            if (string.IsNullOrWhiteSpace(VersionText.Text))
                VersionText.Visibility = System.Windows.Visibility.Collapsed;
            else
                VersionText.Visibility = System.Windows.Visibility.Visible;

            try {
                var ps = Package.Settings.OfType<AuthorsSettings>().FirstOrDefault();

                if (ps != null) {
                    if (ps.Authors != null)
                        AuthorText.Text = string.Join(Environment.NewLine, ps.Authors.Select(f => string.Format("{0}\t-\t{1}", f.Key, f.Value)));
                    else
                        AuthorText.Text = ps.Value;
                }
                else
                    AuthorText.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(AuthorText.Text))
                    AuthorText.Visibility = System.Windows.Visibility.Collapsed;
                else
                    AuthorText.Visibility = System.Windows.Visibility.Visible;
            }
            catch /* Eat */ { /* Tasty ? */ }
            try {
                var ps = Package.Settings.OfType<DescriptionSettings>().FirstOrDefault();

                if (ps != null)
                    DescriptionText.Text = ps.Value;
                else
                    DescriptionText.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(DescriptionText.Text))
                    DescriptionText.Visibility = System.Windows.Visibility.Collapsed;
                else
                    DescriptionText.Visibility = System.Windows.Visibility.Visible;
            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        /// <summary>
        /// Handles the <see cref="E:EnDisableButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnEnableDisableButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            var pkg = Package as NewgenPackage;
            if (pkg == null)
                return;

            pkg.ToggleEnabled();
        }

        /// <summary>
        /// Handles the <see cref="E:InstallButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnInstallUnInstallButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            
            //PackageManager.Current.GetPackage(PackageMetadata, f => {
            //});
        }

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
            if (Package != null)
                LoadStoreItemFromPackage();

            if (FeedItem != null)
                LoadStoreItemFromFeedItem();

            //// Load other
            //if (FeedItem == null) { // Local package
            //    IconImage.Source = PackageMetadata.GetLogo().GetAsFrozen() as BitmapSource;

            //    InstallButton.IsEnabled = false;
            //    InstallButton.Visibility = System.Windows.Visibility.Collapsed;
            //    UnInstallButton.IsEnabled = false;
            //    UnInstallButton.Visibility = System.Windows.Visibility.Collapsed;
            //}
            //else { // Remote package
            //    IconImage.Source = FeedItem.GetPackageLogo();

            //    var isAd = false;
            //    var isProgramUpdate = false;
            //    var isPackage = false;

            //    isProgramUpdate =
            //        FeedItem.Categories.Any(f => f.Name.Equals("Program"))
            //        &&
            //        FeedItem.Categories.Any(f => f.Name.Equals("Update"))
            //        ;
            //    isPackage =
            //        FeedItem.Categories.Any(f => f.Name.Equals("Package"))
            //        ;

            //    var getFile = FeedItem.Links.FirstOrDefault(f => f.MediaType != null && f.MediaType.Contains("application/octet-stream"));
            //    var getFileUri = (Uri)null;
            //    if (getFile != null)
            //        getFileUri = InternalHelper.GetUpdatesUrlFor(getFile.Uri.OriginalString);

            //    isAd = (
            //        !isProgramUpdate && !isPackage
            //        ||
            //        isProgramUpdate && isPackage // Invalid config, so just classify as ad
            //        )
            //        &&
            //        getFileUri != null
            //        ;

            //    if (isAd) {
            //        InstallButton.IsEnabled = false;
            //        InstallButton.Visibility = System.Windows.Visibility.Collapsed;
            //        UnInstallButton.IsEnabled = false;
            //        UnInstallButton.Visibility = System.Windows.Visibility.Collapsed;
            //        EnDisableButton.IsEnabled = false;
            //        EnDisableButton.Visibility = System.Windows.Visibility.Collapsed;
            //    }
            //}
        }

        /// <summary>
        /// Users the control mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var storyboard = Resources["MouseDownAnimation"] as Storyboard;
            if (storyboard != null)
                storyboard.Begin();
        }

        /// <summary>
        /// Users the control mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var storyboard = Resources["MouseUpAnimation"] as Storyboard;
            if (storyboard != null)
                storyboard.Begin();
        }
    }
}
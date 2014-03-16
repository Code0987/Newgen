using System.ServiceModel.Syndication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using Newgen.Packages;

namespace Newgen.Controls {

    /// <summary>
    /// Interaction logic for StoreItem.xaml
    /// </summary>
    public partial class StoreItem : UserControl {
        private SyndicationItem feedItem;

        /// <summary>
        /// Gets or sets the feed item.
        /// </summary>
        /// <value>The feed item.</value>
        /// <remarks>...</remarks>
        public SyndicationItem FeedItem {
            get { return feedItem; }
            set { feedItem = value; }
        }

        private PackageMetadata packageMetadata;

        /// <summary>
        /// Gets the package metadata.
        /// </summary>
        /// <value>The package metadata.</value>
        /// <remarks>...</remarks>
        public PackageMetadata PackageMetadata {
            get {
                if (packageMetadata == null)
                    packageMetadata = InternalHelper.PackageFeedItemToMetadata(feedItem);
                return packageMetadata;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItem" /> class.
        /// </summary>
        /// <param name="feedItem">The feed item.</param>
        /// <remarks>...</remarks>
        public StoreItem(SyndicationItem feedItem) {
            this.feedItem = feedItem;

            InitializeComponent();

            IconImage.Source = FeedItem.GetPackageLogo();
            DataContext = PackageMetadata;
        }

        /// <summary>
        /// Users the control mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            (base.Resources["MouseDownAnim"] as Storyboard).Begin();
        }

        /// <summary>
        /// Users the control mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            (base.Resources["MouseUpAnim"] as Storyboard).Begin();
        }

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {

            var isAd = false;
            var isProgramUpdate = false;
            var isPackage = false;

            isProgramUpdate =
                FeedItem.Categories.Any(f => f.Name.Equals("Program"))
                &&
                FeedItem.Categories.Any(f => f.Name.Equals("Update"))
                ;
            isPackage =
                FeedItem.Categories.Any(f => f.Name.Equals("Package"))
                ;

            var getFile = FeedItem.Links.FirstOrDefault(f => f.MediaType != null && f.MediaType.Contains("application/octet-stream"));
            var getFileUri = (Uri)null;
            if (getFile != null)
                getFileUri = InternalHelper.GetUpdatesUrlFor(getFile.Uri.OriginalString);

            isAd = (
                !isProgramUpdate && !isPackage
                ||
                isProgramUpdate && isPackage // Invalid config, so just classify as ad
                )
                &&
                getFileUri != null
                ;

            if (isAd) {
                InstallButton.IsEnabled = false;
                InstallButton.Visibility = System.Windows.Visibility.Hidden;
                UnInstallButton.IsEnabled = false;
                UnInstallButton.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}

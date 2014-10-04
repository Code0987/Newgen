using System.ServiceModel.Syndication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using Newgen.Packages;

namespace Newgen {

    /// <summary>
    /// Interaction logic for StoreItem.xaml
    /// </summary>
    public partial class StoreItem : UserControl {
        internal readonly SyndicationItem FeedItem;
        internal readonly PackageMetadata PackageMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItem" /> class.
        /// </summary>
        /// <param name="feedItem">The feed item.</param>
        /// <remarks>...</remarks>
        public StoreItem(SyndicationItem feedItem) {
            FeedItem = feedItem;
            PackageMetadata = InternalHelper.PackageFeedItemToMetadata(feedItem);

            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreItem"/> class.
        /// </summary>
        /// <param name="packageMetadata">The package metadata.</param>
        /// <remarks>...</remarks>
        public StoreItem(PackageMetadata packageMetadata) {
            PackageMetadata = packageMetadata;

            InitializeComponent();
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
            // Load UI
            DataContext = PackageMetadata;

            // Load other
            if (FeedItem == null) { // Local package
                IconImage.Source = PackageMetadata.GetLogo().GetAsFrozen() as BitmapSource;

                InstallButton.IsEnabled = false;
                InstallButton.Visibility = System.Windows.Visibility.Collapsed;
                UnInstallButton.IsEnabled = false;
                UnInstallButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else { // Remote package

                IconImage.Source = FeedItem.GetPackageLogo();

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
                    InstallButton.Visibility = System.Windows.Visibility.Collapsed;
                    UnInstallButton.IsEnabled = false;
                    UnInstallButton.Visibility = System.Windows.Visibility.Collapsed;
                    EnDisableButton.IsEnabled = false;
                    EnDisableButton.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
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
            (Resources["MouseDownAnimation"] as Storyboard).Begin();
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
            (Resources["MouseUpAnimation"] as Storyboard).Begin();
        }

        /// <summary>
        /// Handles the <see cref="E:InstallButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnInstallButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            PackageManager.Current.GetPackage(PackageMetadata, f => { 
                
            });
        }

        /// <summary>
        /// Handles the <see cref="E:UnInstallButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnUnInstallButtonClick(object sender, System.Windows.RoutedEventArgs e) {

        }

        /// <summary>
        /// Handles the <see cref="E:EnDisableButtonClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnEnDisableButtonClick(object sender, System.Windows.RoutedEventArgs e) {
            PackageManager.Current.ToggleEnabled(PackageManager.Current.Get(PackageMetadata.Id));
        }
    }
}

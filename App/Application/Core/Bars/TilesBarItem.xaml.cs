using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newgen.Packages;

namespace Newgen {

    /// <summary>
    /// Class TilesBarItem.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TilesBarItem : UserControl {

        /// <summary>
        /// The package
        /// </summary>
        internal readonly Package Package;

        /// <summary>
        /// The order
        /// </summary>
        private int order = 0;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>The order.</value>
        /// <remarks>...</remarks>
        public int Order {
            get {
                return order;
            }

            set {
                order = value;
                ((Storyboard)Resources["LoadAnimation"]).BeginTime = TimeSpan.FromMilliseconds(50 + 50 * value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesBarItem" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public TilesBarItem(Package package) {
            Package = package;

            InitializeComponent();

            Opacity = 0;

            TitleTextBlock.Text = Package.Metadata.Name ?? Package.Metadata.Id;
            var source = IconImage.ImageSource;
            try {
                source = package.Metadata.GetLogo(package.Settings.Location).GetAsFrozen() as BitmapImage;
            }
            catch /* Eat */ { }
            if (source != null && source != IconImage.ImageSource)
                IconImage.ImageSource = source;
        }

        /// <summary>
        /// Called when [toolbar closed].
        /// </summary>
        /// <remarks>...</remarks>
        public void OnToolbarClosed() {
            Opacity = 0;
        }

        /// <summary>
        /// Called when [toolbar opened].
        /// </summary>
        /// <remarks>...</remarks>
        public void OnToolbarOpened() {
            ((Storyboard)Resources["LoadAnimation"]).Begin();
        }

        /// <summary>
        /// Handles the <see cref="E:LoadAnimationCompleted" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoadAnimationCompleted(object sender, EventArgs e) {
            Opacity = 1;
        }
    }
}
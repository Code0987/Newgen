using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Newgen {

    /// <summary>
    /// Class StartBarItem.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class StartBarItem : UserControl {

        /// <summary>
        /// The order
        /// </summary>
        private int order = 0;

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        /// <remarks>...</remarks>
        public Brush Icon {
            get { return IconImage.Background; }
            set { IconImage.Background = value; }
        }

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
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        /// <remarks>...</remarks>
        public string Title {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartBarItem" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public StartBarItem() {
            InitializeComponent();

            Opacity = 0;
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

        /// <summary>
        /// Called when [toolbar opened].
        /// </summary>
        /// <remarks>...</remarks>
        public void OnToolbarOpened() {
            ((Storyboard)Resources["LoadAnimation"]).Begin();
        }

        /// <summary>
        /// Called when [toolbar closed].
        /// </summary>
        /// <remarks>...</remarks>
        public void OnToolbarClosed() {
            Opacity = 0;
        }
    }
}
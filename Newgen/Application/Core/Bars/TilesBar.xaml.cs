using System;
using System.Linq;
using System.Windows.Input;
using libns.Threading;
using Newgen.Packages;

namespace Newgen {

    /// <summary>
    /// Class TilesBar.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TilesBar : ToolbarWindow {

        /// <summary>
        /// The mouseclicktimestamp
        /// </summary>
        private DateTime mouseclicktimestamp = DateTime.Now;

        /// <summary>
        /// The mouse x
        /// </summary>
        private double mouseX, mouseY;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesBar" /> class.
        /// </summary>
        public TilesBar()
            : base() {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void CloseToolbar() {
            foreach (var item in ItemsContainer.Children.OfType<TilesBarItem>()) {
                item.OnToolbarClosed();

                item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
            }

            ThreadingExtensions.LazyInvokeThreadSafe(() => ItemsContainer.Children.Clear(), 250);

            base.CloseToolbar();
        }

        /// <summary>
        /// Opens the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void OpenToolbar() {
            var count = 0;
            foreach (var package in PackageManager.Current.Packages) {
                var item = new TilesBarItem(package) {
                    Order = ++count
                };

                item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp += ItemMouseLeftButtonUp;

                ItemsContainer.Children.Add(item);
            }

            base.OpenToolbar();

            foreach (var item in ItemsContainer.Children.OfType<TilesBarItem>())
                item.OnToolbarOpened();
        }

        /// <summary>
        /// Items the mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the
        /// event data.
        /// </param>
        private void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            mouseX = e.GetPosition(this).X;
            mouseY = e.GetPosition(this).Y;
        }

        /// <summary>
        /// Items the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the
        /// event data.
        /// </param>
        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            int timediff = (int)DateTime.Now.Subtract(mouseclicktimestamp).TotalMilliseconds;
            mouseclicktimestamp = DateTime.Now;

            if (timediff < 1200)
                return;

            if (mouseX != e.GetPosition(this).X || mouseY != e.GetPosition(this).Y)
                return;

            var package = ((TilesBarItem)sender).Package;

            PackageManager.Current.ToggleEnabled(package);
        }

        /// <summary>
        /// Windows the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the
        /// event data.
        /// </param>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Topmost = true;
            if (!IsOpened)
                OpenToolbar();
        }

        /// <summary>
        /// Windows the preview key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl)) {
                if (!IsOpened)
                    OpenToolbar();
                else
                    CloseToolbar();
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the TouchSupportPin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the
        /// event data.
        /// </param>
        private void OnTouchSupportPinMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            CloseToolbar();
        }
    }
}
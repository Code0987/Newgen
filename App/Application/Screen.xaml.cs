using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using libns.Media.Animation;
using libns.Native;
using libns.Threading;

namespace Newgen {

    /// <summary>
    /// Class Screen.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Screen : Window {

        /// <summary>
        /// The start bar
        /// </summary>
        internal StartBar StartBar;

        /// <summary>
        /// The thumbnails bar
        /// </summary>
        internal ThumbnailsBar ThumbnailsBar;

        /// <summary>
        /// The wait window
        /// </summary>
        internal WaitWindow WaitDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public Screen() {
            Api.Logger.LogInformation("STARTING screen.");

            this.InitializeComponent();

            if (Settings.Current.BackgroundColor.A == 255)
                AllowsTransparency = false;
        }

        /// <summary>
        /// Toggles the wait dialog.
        /// </summary>
        /// <param name="circles">if set to <c>true</c> [circles].</param>
        /// <param name="text">The text.</param>
        /// <remarks>...</remarks>
        internal void ToggleWaitDialog(bool circles = true, string text = null) {
            if (WaitDialog == null || !WaitDialog.IsLoaded) {
                WaitDialog = new WaitWindow();
                WaitDialog.Show();
                if (!string.IsNullOrWhiteSpace(text))
                    WaitDialog.Text.Text = text;
                WaitDialog.Start(circles);
            }
            else {
                WaitDialog.Stop();
                ThreadingExtensions.LazyInvokeThreadSafe(() => {
                    WaitDialog.Hide();
                    WaitDialog.Close();
                    WaitDialog = null;
                }, 600);
                WinAPI.FlushMemory();
            }
        }

        /// <summary>
        /// Zs the order helper.
        /// </summary>
        /// <param name="settrue">if set to <c>true</c> [settrue].</param>
        /// <remarks>...</remarks>
        internal void ZOrderHelper(bool settrue = true) {
            if (settrue) {
                if (ThumbnailsBar != null) {
                    ThumbnailsBar.Topmost = false;
                    ThumbnailsBar.Topmost = true;
                }
                if (StartBar != null) {
                    StartBar.Topmost = false;
                    StartBar.Topmost = true;
                }
            }
            else {
                if (ThumbnailsBar != null) {
                    ThumbnailsBar.Topmost = true;
                    ThumbnailsBar.Topmost = false;
                }
                if (StartBar != null) {
                    StartBar.Topmost = true;
                    StartBar.Topmost = false;
                }
            }
        }

        /// <summary>
        /// Loads the toolbars.
        /// </summary>
        /// <remarks>...</remarks>
        private void LoadToolbars() {
            StartBar = new StartBar();
            if (Settings.Current.UseThumbailsBar)
                ThumbnailsBar = new ThumbnailsBar();

            if (Settings.Current.UseThumbailsBar)
                ThumbnailsBar.OpenToolbar();
            StartBar.OpenToolbar();
            
            ThreadingExtensions.LazyInvokeThreadSafe(() => {
                if (Settings.Current.UseThumbailsBar)
                    ThumbnailsBar.CloseToolbar();
                StartBar.CloseToolbar();
            }, 250);
        }

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            Api.Messenger.RemoveListener(new WindowInteropHelper(this).Handle);
            
            // Stop all newgen packages.
            foreach (var package in App.PM.Packages.OfType<NewgenPackage>())
                package.Stop(); 
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.UserBadgeControl.Reload();
            AnimationExtensions.Animate(this.Header, OpacityProperty, 250, 1);

            LoadToolbars();

            Api.Logger.LogInformation("STARTED screen.");
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (ThumbnailsBar != null) {
                ThumbnailsBar.CloseToolbar();
            }
            if (StartBar != null) {
                StartBar.CloseToolbar();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseMove(object sender, MouseEventArgs e) {
        }

        /// <summary>
        /// Handles the <see cref="E:SourceInitialized" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnSourceInitialized(object sender, EventArgs e) {
            var handle = new WindowInteropHelper(this).Handle;

            WinAPI.RemoveFromDWM(handle);

            Api.Messenger.AddListener(handle);

            InternalHelper.FixWindowsTaskbar();

            this.Left = 0;
        }

        /// <summary>
        /// Handles the <see cref="E:TitleTextMouseUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnTitleTextMouseUp(object sender, MouseButtonEventArgs e) {
            TilesControl.ScrollToLeftEnd();
        }
    }
}
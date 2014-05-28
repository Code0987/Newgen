using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using libns.Media.Animation;
using libns.Native;
using libns.Threading;
using libns.UI;
using Newgen;

namespace Newgen {

    /// <summary>
    /// Class DWMPreviewPopup.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class DWMPreviewPopup : Window {

        /// <summary>
        /// The automatic switch on close
        /// </summary>
        private bool autoSwitchOnClose;

        /// <summary>
        /// The h WNDS
        /// </summary>
        private readonly List<IntPtr> hWnds;

        /// <summary>
        /// The towner
        /// </summary>
        private readonly Visual towner;

        /// <summary>
        /// The isclosepending
        /// </summary>
        private bool isclosepending = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DWMPreviewPopup" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="handles">The handles.</param>
        /// <param name="autoSwitchOnClose">if set to <c>true</c> [automatic switch on close].</param>
        /// <remarks>...</remarks>
        public DWMPreviewPopup(Visual owner, List<IntPtr> handles, bool autoSwitchOnClose) {
            InitializeComponent();

            hWnds = handles;
            towner = owner;
            this.autoSwitchOnClose = autoSwitchOnClose;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DWMPreviewPopup" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="handle">The handle.</param>
        /// <param name="autoSwitchOnClose">if set to <c>true</c> [automatic switch on close].</param>
        /// <remarks>...</remarks>
        public DWMPreviewPopup(Visual owner, IntPtr handle, bool autoSwitchOnClose)
            : this(owner, new List<IntPtr>() { handle }, autoSwitchOnClose) {
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
            if (Mouse.DirectlyOver == ThumbnailsContainer || (Mouse.DirectlyOver != null && this.IsAncestorOf(Mouse.DirectlyOver as UIElement))) {
                e.Cancel = true;
            }

            if (isclosepending) {
                ThreadingExtensions.LazyInvokeThreadSafe(() => {
                    isclosepending = false;
                    if (autoSwitchOnClose)
                        Switch();
                    else
                        Close();
                }, 200);

                ThumbnailsContainer.Children.Clear();

                AnimationExtensions.Animate(this, OpacityProperty, 150, 0, 0.7, 0.3);
                AnimationExtensions.Animate(this, LeftProperty, 150, Left, -Left, 0.7, 0.3);

                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            Focus();
            Activate();

            AnimationExtensions.Animate(this, OpacityProperty, 150, 1, 0.7, 0.3);
            AnimationExtensions.Animate(this, LeftProperty, 150, 2*Left, Left, 0.7, 0.3);

            var index = 1;
            foreach (var hWnd in hWnds)
                try {
                    var text = WinAPI.GetText(hWnd);
                    var dwmtc = new DWMThumbnailControl(this) {
                        ToolTip = text,
                        Width = Width - 50,
                        Height = Height - 50,
                        Source = hWnd
                    };

                    ThumbnailsContainer.Children.Add(dwmtc);
                    TextTitle.Text = text;

                    if (hWnds.Count > 1 && index > 1) {
                        Width = Width + dwmtc.Width;
                    }
                    else {
                        Width++;
                        Width--;
                    }

                    index++;
                }
                catch /* Eat */ { }

            // Initiate auto close
            if (!autoSwitchOnClose) {
                this.Dispatcher.RunFor(() => {
                    Close();
                }, -1, 1000);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeave" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeave(object sender, MouseEventArgs e) {
            Close();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                DragMove();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:SourceInitialized" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnSourceInitialized(object sender, EventArgs e) {
            var margins = new MARGINS(-1, -1, -1, -1);
            var handle = new WindowInteropHelper(this).Handle;
            var hWndSource = HwndSource.FromHwnd(handle);
            hWndSource.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

            WinAPI.DwmExtendFrameIntoClientArea(handle, ref margins);
            WinAPI.RemoveWindowIcon(handle);
            WinAPI.RemoveFromDWM(this);
        }

        /// <summary>
        /// Handles the <see cref="E:ThumbnailsContainerMouseDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnThumbnailsContainerMouseDown(object sender, MouseButtonEventArgs e) {
            var c = e.GetPosition(ThumbnailsContainer);
            foreach (var dwmtc in ThumbnailsContainer.Children.OfType<DWMThumbnailControl>()) {
                var transform = dwmtc.TransformToVisual(ThumbnailsContainer);
                var p = transform.Transform(new Point(0, 0));
                if (c.Y > p.Y && c.Y < p.Y + dwmtc.Height && c.X > p.X && c.X < p.X + dwmtc.Width) {
                    if (!WinAPI.IsIconic(dwmtc.Source))
                        WinAPI.ShowWindow(dwmtc.Source, WindowShowStyle.Minimize);
                    else
                        WinAPI.SwitchToThisWindow(dwmtc.Source, true);
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ThumbnailsContainerMouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnThumbnailsContainerMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Switch();
        }

        /// <summary>
        /// Handles the <see cref="E:ThumbnailsContainerMouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnThumbnailsContainerMouseMove(object sender, MouseEventArgs e) {
            var c = Mouse.GetPosition(ThumbnailsContainer);
            foreach (var dwmtc in ThumbnailsContainer.Children.OfType<DWMThumbnailControl>()) {
                var transform = dwmtc.TransformToVisual(ThumbnailsContainer);
                var p = transform.Transform(new Point(0, 0));
                if (c.Y > p.Y && c.Y < p.Y + dwmtc.Height && c.X > p.X && c.X < p.X + dwmtc.Width) {
                    TextTitle.Text = WinAPI.GetText(dwmtc.Source);
                }
            }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        /// <remarks>...</remarks>
        private void Refresh() {
            foreach (var dwmtc in ThumbnailsContainer.Children.OfType<DWMThumbnailControl>()) {
                dwmtc.Width++;
                dwmtc.Width--;
                dwmtc.Height++;
                dwmtc.Height--;

                Width++;
                Width--;
                Height++;
                Height--;
            }
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <remarks>...</remarks>
        private void Release() {
            ThumbnailsContainer.Children.Clear();
        }

        /// <summary>
        /// Switches this instance.
        /// </summary>
        /// <remarks>...</remarks>
        private void Switch() {
            autoSwitchOnClose = false;
            var mouse = Mouse.GetPosition(ThumbnailsContainer);
            foreach (var dwmtc in ThumbnailsContainer.Children.OfType<DWMThumbnailControl>()) {
                var transform = dwmtc.TransformToVisual(ThumbnailsContainer);
                var p = transform.Transform(new Point(0, 0));
                if (mouse.Y > p.Y && mouse.Y < p.Y + dwmtc.Height && mouse.X > p.X && mouse.X < p.X + dwmtc.Width) {
                    WinAPI.SwitchToThisWindow(dwmtc.Source, true);
                    Close();
                    break;
                }
            }
        }
    }
}
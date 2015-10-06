using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using libns.Native;
using libns.Threading;

namespace Newgen {
    /// <summary>
    /// Class TaskBarItem.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TaskBarItem : UserControl {
        /// <summary>
        /// The popup
        /// </summary>
        private DWMPreviewPopup popup;
        /// <summary>
        /// The towner
        /// </summary>
        private StartBar taskBar;

        /// <summary>
        /// Gets or sets the handles.
        /// </summary>
        /// <value>The handles.</value>
        /// <remarks>...</remarks>
        public List<IntPtr> Handles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBarItem"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="handles">The handles.</param>
        /// <remarks>...</remarks>
        public TaskBarItem(StartBar owner, List<IntPtr> handles) {
            InitializeComponent();

            Handles = handles;
            taskBar = owner;

            IconImage.Source = InternalHelper.GetThumbnail(WinAPI.GetProcessPath(handles[0]));

            if (IconImage.Source == null) {
                taskBar.RemoveIcon(this);
                return;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBarItem"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="handle">The handle.</param>
        /// <remarks>...</remarks>
        public TaskBarItem(StartBar owner, IntPtr handle)
            : this(owner, new List<IntPtr> { handle }) {
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
            foreach (var hWnd in Handles)
                try {
                    var p = WinAPI.GetProcess(hWnd);
                    p.EnableRaisingEvents = true;
                    p.Exited += new EventHandler((o, a) => {
                        try {
                            this.InvokeAsyncThreadSafe(() => {
                                taskBar.RemoveIcon(this);
                            });
                        }
                        catch /* Eat */ { }
                    });
                }
                catch /* Eat */ { }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseEnter" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseEnter(object sender, MouseEventArgs e) {
            if (popup != null && popup.IsLoaded)
                return;

            popup = new DWMPreviewPopup(this, Handles, false);
            popup.Left = ActualWidth;
            popup.Top = TransformToAncestor(taskBar).Transform(new Point()).Y - (popup.Height / 2);
            popup.Show();

            Effect = new System.Windows.Media.Effects.DropShadowEffect {
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.9
            };
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeave" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeave(object sender, MouseEventArgs e) {
            Effect = null;
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Effect = new System.Windows.Media.Effects.DropShadowEffect {
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.9
            };
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Effect = null;

            foreach (var hWnd in Handles) {
                if (!WinAPI.IsIconic(hWnd))
                    WinAPI.ShowWindow(hWnd, WindowShowStyle.Minimize);
                else
                    WinAPI.SwitchToThisWindow(hWnd, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Unloaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e) {
        }
    }
}
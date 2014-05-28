using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using libns.Media.Animation;
using libns.Native;
using libns.Threading;
using libns.UI;

namespace Newgen {

    /// <summary>
    /// Class TaskBar.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TaskBar : ToolbarWindow {
        private DWMPreviewPopup popup;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartBar" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public TaskBar()
            : base() {
            Location = ToolbarLocation.Left;
            InitializeComponent();
        }

        /// <summary>
        /// Opens the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void OpenToolbar() {
            base.OpenToolbar();
            // Load all taskbar items
            this.ForEachHWND((current, text) => {
                var items = ItemsContainer.Children.OfType<TaskBarItem>().ToList();

                if (items.Count == 0) {
                    var fip = new FileInfo(WinAPI.GetProcessPath(current));
                    if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !fip.Name.StartsWith(App.Current.Location)) {
                        AddIcon(current);
                    }
                }
                else {
                    var existcount = 0;
                    foreach (var item in items) {
                        if (item.Handles.Contains(current))
                            existcount++;
                    }
                    if (existcount <= 0) {
                        var fip = new FileInfo(WinAPI.GetProcessPath(current));
                        if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !fip.Name.StartsWith(App.Current.Location)) {
                            AddIcon(current);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Adds the icon.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <remarks>...</remarks>
        internal void AddIcon(IntPtr hWnd) {
            var hWndpath = new FileInfo(WinAPI.GetProcessPath(hWnd)).Name;
            var items = ItemsContainer.Children.OfType<TaskBarItem>().ToList();
            var isadded = false;

            foreach (var sbi in items) {
                if (WinAPI.GetProcessPath(sbi.Handles[0]).Contains(hWndpath)) {
                    sbi.Handles.Add(hWnd);
                    isadded = true;

                    AnimationExtensions.Animate(sbi, OpacityProperty, 200, 0, 1, 0.3, 0.7);
                }
            }
            if (!isadded) {
                var icon = new TaskBarItem(this, hWnd);
                ItemsContainer.Children.Add(icon);
                AnimationExtensions.Animate(icon, OpacityProperty, 200, 0, 1, 0.3, 0.7);
            }
        }

        /// <summary>
        /// Removes the icon.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <remarks>...</remarks>
        internal void RemoveIcon(TaskBarItem icon) {
            if (icon != null) {
                AnimationExtensions.Animate(icon, OpacityProperty, 200, 0, 0.3, 0.7);
                ThreadingExtensions.LazyInvokeThreadSafe(() => { ItemsContainer.Children.Remove(icon); }, 205);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeave" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeave(object sender, MouseEventArgs e) {
            CloseToolbar();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Mouse.Capture(this);

            if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed) {
                var handle = IntPtr.Zero;

                var desktopHWND = WinAPI.GetDesktopWindow();
                var child = WinAPI.GetWindow(desktopHWND, GetWindowCmd.GW_CHILD);
                while (child != IntPtr.Zero) {
                    var LHParent = WinAPI.GetWindowLongPtr(child, GWL.GWL_HWNDPARENT);
                    var LEXSTYLE = WinAPI.GetWindowLongPtr(child, GWL.GWL_EXSTYLE);
                    if (
                        WinAPI.IsWindowVisible(child) // Visible
                        && ((LHParent == IntPtr.Zero) || (LHParent == desktopHWND)) // Child of desktop
                        && (((int)LEXSTYLE & 0x00000080) == 0 || ((int)LEXSTYLE & 0x40000) != 0) // Is valid window/popup
                        )
                        handle = child;
                    child = WinAPI.GetWindow(child, GetWindowCmd.GW_HWNDNEXT);
                }

                popup = new DWMPreviewPopup(this, handle, true);
                popup.Show();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Topmost = true;
            if (!IsOpened)
                OpenToolbar();

            Mouse.Capture(null);

            foreach (var window in App.Current.Windows.OfType<Window>())
                if (window.GetType() == typeof(DWMPreviewPopup))
                    window.Close();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (!IsOpened)
                OpenToolbar();

            // Re-position popup with mouse
            if (popup != null && popup.IsLoaded && popup.WindowStartupLocation != WindowStartupLocation.CenterScreen) {
                var mp = PointToScreen(Mouse.GetPosition(this));
                popup.Top = mp.Y - (popup.ActualHeight / 2);
                popup.Left = mp.X - (popup.ActualWidth / 2);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseRightButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            // Capture mouse
            Mouse.Capture(this);
            // Get all visible windows
            var handles = new List<IntPtr>();
            this.ForEachHWND((current, text) => handles.Add(current));
            // Show popup at center of screen
            popup = new DWMPreviewPopup(this, handles, false);
            popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            popup.Show();
        }

        /// <summary>
        /// Handles the <see cref="E:PreviewMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Topmost = true;
            if (!IsOpened)
                OpenToolbar();
        }
    }
}
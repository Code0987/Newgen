using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using libns.Media.Animation;
using libns.Media.Imaging;
using libns.Native;
using libns.Threading;
using Microsoft.Win32;
using Newgen.AppLink;
using PackageManager;

namespace Newgen {

    /// <summary>
    /// Class StartBar.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class StartBar : ToolbarWindow {
        private DateTime mouseClickTimestamp = DateTime.Now;
        private DWMPreviewPopup popup;
        private DispatcherTimer popupTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartBar" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public StartBar()
            : base() {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void CloseToolbar() {
            if (Settings.Current.ShowStartbarAlways)
                return;

            base.CloseToolbar();

            var anim_userTile = App.Screen.UserBadgeControl.Resources["LeftAnimation"] as Storyboard;
            ((DoubleAnimation)anim_userTile.Children[0]).To = 0;
            ((DoubleAnimation)anim_userTile.Children[0]).AccelerationRatio = 0.3;
            ((DoubleAnimation)anim_userTile.Children[0]).DecelerationRatio = 0.7;
            anim_userTile.Begin();
        }

        /// <summary>
        /// Opens the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void OpenToolbar() {
            if (!Settings.IsProMode) {
                PinWebItem.Visibility = Visibility.Collapsed;
                SaveSettingsItem.Visibility = Visibility.Collapsed;
            }

            base.OpenToolbar();

            // Load all taskbar items
            this.ForEachHWND((current, text) => {
                var items = ProcessesContainer.Children.OfType<FrameworkElement>().ToList();

                if (items.Count == 0) {
                    var fip = new FileInfo(WinAPI.GetProcessPath(current));
                    if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !fip.Name.StartsWith(App.Current.Location)) {
                        AddProcessIconUI(current);
                    }
                }
                else {
                    var existcount = 0;
                    foreach (var item in items) {
                        if (((List<IntPtr>)item.DataContext).Contains(current))
                            existcount++;
                    }
                    if (existcount <= 0) {
                        var fip = new FileInfo(WinAPI.GetProcessPath(current));
                        if (!Settings.Current.TaskBarProcessExclusionList.Contains(fip.Name) && !fip.Name.StartsWith(App.Current.Location)) {
                            AddProcessIconUI(current);
                        }
                    }
                }
            });

            var anim_userTile = App.Screen.UserBadgeControl.Resources["LeftAnimation"] as Storyboard;
            ((DoubleAnimation)anim_userTile.Children[0]).To = -this.Width;
            ((DoubleAnimation)anim_userTile.Children[0]).AccelerationRatio = 0.7;
            ((DoubleAnimation)anim_userTile.Children[0]).DecelerationRatio = 0.3;
            anim_userTile.Begin();
        }

        /// <summary>
        /// Adds the icon.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <remarks>...</remarks>
        internal void AddProcessIconUI(IntPtr hWnd) {
            // Get path.
            var pp = WinAPI.GetProcessPath(hWnd);
            var hWndpath = new FileInfo(pp).Name;

            // Check if already added or not.
            foreach (var sbi in ProcessesContainer.Children.OfType<FrameworkElement>().ToList()) {
                if (WinAPI.GetProcessPath(((List<IntPtr>)sbi.DataContext)[0]).Contains(hWndpath)) {
                    ((List<IntPtr>)sbi.DataContext).Add(hWnd);

                    AnimationExtensions.Animate(sbi, OpacityProperty, 500, 0, 1, 0.2, 0.8);

                    return;
                }
            }

            // Get thumb.
            var thumb = InternalHelper.GetThumbnail(pp);

            if (thumb == null)
                return;
            
            // Create ui.
            var iconUIImage = new Image() { Source = thumb, Margin = new Thickness(7) };
            var iconUI = new Button() {
                DataContext = new List<IntPtr>() { hWnd },
                Content = iconUIImage,
                MinHeight = 32,
                MinWidth = 32,
                MaxHeight = 64,
                MaxWidth = 64,
                Style = App.Current.Resources["B_FX_A1_Style"] as Style
            };
            iconUI.Background = ((BitmapSource)thumb).CalculateAverageColor(0xFF).ToBrush();
            RenderOptions.SetBitmapScalingMode(iconUIImage, BitmapScalingMode.HighQuality);
            iconUI.Loaded += iconUI_Loaded;
            iconUI.PreviewMouseLeftButtonUp += iconUI_PreviewMouseLeftButtonUp;
            iconUI.MouseEnter += iconUI_MouseEnter;
            iconUI.MouseLeave += iconUI_MouseLeave;

            // Add to list.
            ProcessesContainer.Children.Add(iconUI);

            // FX.
            AnimationExtensions.Animate(iconUI, OpacityProperty, 450, 0, 1, 0.2, 0.8);
        }

        /// <summary>
        /// Removes the icon.
        /// </summary>
        /// <param name="iconUI">The icon UI.</param>
        /// <remarks>...</remarks>
        internal void RemoveProcessIconUI(FrameworkElement iconUI) {
            // Remove from list.
            if (iconUI != null) {
                AnimationExtensions.Animate(iconUI, OpacityProperty, 700, 0, 0.1, 0.9);

                ThreadingExtensions.LazyInvokeThreadSafe(() => {
                    ProcessesContainer.Children.Remove(iconUI);
                }, 600);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the iconUI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void iconUI_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            // Monitor process, and remove icon if it exits.
            foreach (var hWnd in ((List<IntPtr>)((FrameworkElement)sender).DataContext))
                try {
                    var p = WinAPI.GetProcess(hWnd);
                    p.EnableRaisingEvents = true;
                    p.Exited += new EventHandler((o, a) => {
                        try {
                            this.InvokeAsyncThreadSafe(() => {
                                RemoveProcessIconUI(this);
                            });
                        }
                        catch (Exception ex) { Api.Logger.LogWarning("Problem while process exit on icon ui.", ex); }
                    });
                }
                catch (Exception ex) { Api.Logger.LogWarning("Problem while monitoring process for exit.", ex); }
        }

        /// <summary>
        /// Handles the MouseEnter event of the iconUI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void iconUI_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            // Kill timer first.
            if (popupTimer != null) {
                popupTimer.Stop();
                popupTimer = null;
            }

            // If old popup is visible, kill it also.
            if (popup != null) {
                popup.Close();
                popup = null;
            }
            // Create new popup.            
                popup = new DWMPreviewPopup(this, ((List<IntPtr>)((FrameworkElement)sender).DataContext), false);
            
            var tp = ((FrameworkElement)sender).TransformToAncestor(this).Transform(new Point());

            popup.Left = Left - 5 - ActualWidth;
            popup.Top = tp.Y - (popup.Height / 2);

            popup.Show();
        }

        /// <summary>
        /// Handles the MouseLeave event of the iconUI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void iconUI_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            // Should not fire, but in case.
            if (popup == null || !popup.IsLoaded)
                return;

            // Close popup after 1 sec.
            popupTimer = ThreadingExtensions.LazyInvokeThreadSafe(() => {
                popup.Close();
            }, 1000);
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event of the iconUI control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void iconUI_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            foreach (var hWnd in ((List<IntPtr>)((FrameworkElement)sender).DataContext)) {
                if (!WinAPI.IsIconic(hWnd))
                    WinAPI.ShowWindow(hWnd, WindowShowStyle.Minimize);
                else
                    WinAPI.SwitchToThisWindow(hWnd, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:DragEnter" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnDragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.All;
        }

        /// <summary>
        /// Handles the <see cref="E:Drop" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                foreach (var path in e.Data.GetData(DataFormats.FileDrop, true) as string[])
                    if (File.Exists(path))
                        App.PM.LoadPackage(new AppLinkPackage(path, Path.Combine(Api.PackagesRoot, AppLinkPackage.GetPackageId(path)), App.PSS));
        }

        /// <summary>
        /// Handles the <see cref="E:ExitItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnExitItemClick(object sender, RoutedEventArgs e) {
            App.Close();
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //Mouse.Capture(this);

            //if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed) {
            //    var handle = IntPtr.Zero;

            //    var desktopHWND = WinAPI.GetDesktopWindow();
            //    var child = WinAPI.GetWindow(desktopHWND, GetWindowCmd.GW_CHILD);
            //    while (child != IntPtr.Zero) {
            //        var LHParent = WinAPI.GetWindowLongPtr(child, GWL.GWL_HWNDPARENT);
            //        var LEXSTYLE = WinAPI.GetWindowLongPtr(child, GWL.GWL_EXSTYLE);
            //        if (
            //            WinAPI.IsWindowVisible(child) // Visible
            //            && ((LHParent == IntPtr.Zero) || (LHParent == desktopHWND)) // Child of desktop
            //            && (((int)LEXSTYLE & 0x00000080) == 0 || ((int)LEXSTYLE & 0x40000) != 0) // Is valid window/popup
            //            )
            //            handle = child;
            //        child = WinAPI.GetWindow(child, GetWindowCmd.GW_HWNDNEXT);
            //    }

            //    popup = new DWMPreviewPopup(this, handle, true);
            //    popup.Show();
            //}
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

            //Mouse.Capture(null);

            //foreach (var window in App.Current.Windows.OfType<Window>())
            //    if (window.GetType() == typeof(DWMPreviewPopup))
            //        window.Close();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseMove(object sender, MouseEventArgs e) {
            //if (!IsOpened)
            //    OpenToolbar();

            //// Re-position popup with mouse
            //if (popup != null && popup.IsLoaded && popup.WindowStartupLocation != WindowStartupLocation.CenterScreen) {
            //    var mp = PointToScreen(Mouse.GetPosition(this));
            //    popup.Top = mp.Y - (popup.ActualHeight / 2);
            //    popup.Left = mp.X - (popup.ActualWidth / 2);
            //}
        }

        /// <summary>
        /// Handles the <see cref="E:MouseRightButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            //// Capture mouse
            //Mouse.Capture(this);
            //// Get all visible windows
            //var handles = new List<IntPtr>();
            //this.ForEachHWND((current, text) => handles.Add(current));
            //// Show popup at center of screen
            //popup = new DWMPreviewPopup(this, handles, false);
            //popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //popup.Show();
        }

        /// <summary>
        /// Handles the <see cref="E:PinAppItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnPinAppItemClick(object sender, RoutedEventArgs e) {
            CloseToolbar();

            var dlg = new OpenFileDialog();
            dlg.Filter = Api.AnyFilter;
            if (!(bool)dlg.ShowDialog())
                return;

            App.PM.LoadPackage(new AppLinkPackage(dlg.FileName, Path.Combine(Api.PackagesRoot, AppLinkPackage.GetPackageId(dlg.FileName)), App.PSS));
        }

        /// <summary>
        /// Handles the <see cref="E:PinDirItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnPinDirItemClick(object sender, RoutedEventArgs e) {
            CloseToolbar();

            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            App.PM.LoadPackage(new AppLinkPackage(dlg.SelectedPath, Path.Combine(Api.PackagesRoot, AppLinkPackage.GetPackageId(dlg.SelectedPath)), App.PSS));
        }

        /// <summary>
        /// Handles the <see cref="E:PinWebItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnPinWebItemClick(object sender, RoutedEventArgs e) {
            CloseToolbar();

            var bar = new AppLinkPackageAddressBarWindow();
            bar.ShowDialog();
            var uri = default(Uri);
            if (!Uri.TryCreate(bar.AddressBox.Text, UriKind.RelativeOrAbsolute, out uri) || string.IsNullOrWhiteSpace(uri.OriginalString))
                return;

            App.PM.LoadPackage(new AppLinkPackage(uri.OriginalString, Path.Combine(Api.PackagesRoot, AppLinkPackage.GetPackageId(uri.OriginalString)), App.PSS));
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
            if (e.OriginalSource == LayoutRoot ||
               e.OriginalSource == TouchDecorator ||
               e.OriginalSource == ContentDecorator) {
                if (!IsOpened)
                    OpenToolbar();
                else
                    CloseToolbar();
            }
        }

        /// <summary>
        /// Handles the <see cref="E:RefreshItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnRefreshItemClick(object sender, RoutedEventArgs e) {
            App.Screen.ToggleWaitDialog();
            ThreadingExtensions.LazyInvokeThreadSafe(() => App.Screen.ToggleWaitDialog(), 250);
            ThreadingExtensions.LazyInvoke(() => WinAPI.FlushMemory(), 300);
        }

        /// <summary>
        /// Handles the <see cref="E:SaveSettingsItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnSaveSettingsItemClick(object sender, RoutedEventArgs e) {
            Settings.Current.Save();
        }

        /// <summary>
        /// Handles the Click event of the SettingsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void SettingsButton_Click(object sender, RoutedEventArgs e) {
            CloseToolbar();

            (new SettingsHub()).Show();
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event of the StartButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void StartButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount >= 2) {
                App.Screen.Hide();
                CloseToolbar();
            }
            else {
                foreach (var window in App.Current.Windows) {
                    if (window.GetType() == typeof(HubWindow) ||
                       window.GetType() == typeof(DWMPreviewPopup)
                      )
                        continue;
                    ((Window)window).Activate();
                    ((Window)window).Show();
                }
                foreach (var tileControl in App.Screen.TilesControl.TileControls)
                    AnimationExtensions.Animate(tileControl, OpacityProperty, 250, 0, 1);
            }
        }

        /// <summary>
        /// Handles the Click event of the StoreButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void StoreButton_Click(object sender, RoutedEventArgs e) {
            CloseToolbar();

            StoreHub.ShowHub();
        }

        ///// <summary>
        ///// Handles the <see cref="E:DevicesButtonMouseLeftButtonUp" /> event.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">
        ///// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        ///// </param>
        ///// <remarks>...</remarks>
        //private void OnDevicesButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        //    CloseToolbar();

        //    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
        //        Process.Start("explorer.exe", "shell:::{A8A91A66-3A7D-4424-8D24-04E180695C7A}");
        //    else
        //        Process.Start("explorer.exe", "e,::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        //}

        ///// <summary>
        ///// Handles the <see cref="E:SearchButtonMouseLeftButtonUp" /> event.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="e">
        ///// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        ///// </param>
        ///// <remarks>...</remarks>
        //private void OnSearchButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        //    CloseToolbar();

        //    Api.Messenger.Send(new EMessage() {
        //        Key = EMessage.UrlKey,
        //        Value = "http://www.google.com/search?q="
        //    });
        //}
    }
}
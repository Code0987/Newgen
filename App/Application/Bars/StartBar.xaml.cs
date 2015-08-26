using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using libns.Media.Animation;
using libns.Native;
using libns.Threading;
using Microsoft.Win32;
using Newgen.Packages;
using Newgen.AppLink;
using PackageManager;

namespace Newgen {

    /// <summary>
    /// Class StartBar.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class StartBar : ToolbarWindow {
        private DateTime mouseClickTimestamp = DateTime.Now;

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

            foreach (var item in ItemsContainer.Children.OfType<StartBarItem>())
                item.OnToolbarClosed();

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
                DevicesButton.Visibility = Visibility.Collapsed;
                PinWebItem.Visibility = Visibility.Collapsed;
                SaveSettingsItem.Visibility = Visibility.Collapsed;
            }
            
            base.OpenToolbar();

            foreach (var item in ItemsContainer.Children.OfType<StartBarItem>())
                item.OnToolbarOpened();

            var anim_userTile = App.Screen.UserBadgeControl.Resources["LeftAnimation"] as Storyboard;
            ((DoubleAnimation)anim_userTile.Children[0]).To = -this.Width;
            ((DoubleAnimation)anim_userTile.Children[0]).AccelerationRatio = 0.7;
            ((DoubleAnimation)anim_userTile.Children[0]).DecelerationRatio = 0.3;
            anim_userTile.Begin();
        }

        /// <summary>
        /// Handles the <see cref="E:DevicesButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnDevicesButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            CloseToolbar();

            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                Process.Start("explorer.exe", "shell:::{A8A91A66-3A7D-4424-8D24-04E180695C7A}");
            else
                Process.Start("explorer.exe", "e,::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
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
            if (e.OriginalSource == LayoutRoot ||
               e.OriginalSource == TouchDecorator ||
               e.OriginalSource == ContentDecorator ||
               e.OriginalSource == ItemsContainer) {
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
        /// Handles the <see cref="E:SearchButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnSearchButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            CloseToolbar();

            Api.Messenger.Send(new EMessage() {
                Key = EMessage.UrlKey,
                Value = "http://www.google.com/search?q="
            });
        }

        /// <summary>
        /// Handles the <see cref="E:SettingsButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnSettingsButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            CloseToolbar();

            (new SettingsHub()).Show();
        }

        /// <summary>
        /// Handles the <see cref="E:StartButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnStartButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
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
        /// Handles the <see cref="E:StoreButtonMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnStoreButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            CloseToolbar();

            StoreHub.ShowHub();
        }

        /// <summary>
        /// Handles the <see cref="E:TilesItemClick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnTilesItemClick(object sender, RoutedEventArgs e) {
            CloseToolbar();

            App.Screen.TilesBar.OpenToolbar();
        }
    }
}
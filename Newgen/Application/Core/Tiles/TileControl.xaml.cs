using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using libns.Media.Imaging;
using Newgen.Resources;

namespace Newgen.Packages {

    /// <summary>
    /// Class TileControl.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TileControl : ContentControl {

        /// <summary>
        /// The tile bg color key
        /// </summary>
        public const string TileBgColorKey = "TileBgColor";

        /// <summary>
        /// The package
        /// </summary>
        public readonly Package package;

        /// <summary>
        /// The context menu
        /// </summary>
        private ContextMenu contextMenu;

        /// <summary>
        /// The order
        /// </summary>
        private int order = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mouse pressed.
        /// </summary>
        /// <value><c>true</c> if this instance is mouse pressed; otherwise, <c>false</c>.</value>
        /// <remarks>...</remarks>
        public bool IsMousePressed { get; set; }

        /// <summary>
        /// Gets or sets the tile screen order.
        /// </summary>
        /// <value>The tile screen order.</value>
        /// <remarks>...</remarks>
        public int Order {
            get {
                return order;
            }
            set {
                order = value;
                (Resources["LoadAnimation"] as Storyboard).BeginTime = TimeSpan.FromMilliseconds(700 + 100 * value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileControl" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public TileControl(Package package) {
            this.package = package;

            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileControl" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public TileControl()
            : this(null) {
        }

        /// <summary>
        /// Adjusts the dimensions.
        /// </summary>
        /// <remarks>...</remarks>
        public void AdjustDimensions() {
            Margin = new Thickness(Settings.Current.TileSpacing);

            Width =
                Settings.Current.MinTileWidth * package.ColumnSpan
                - Settings.Current.TileSpacing * 2 * package.ColumnSpan
                + (package.ColumnSpan > 1 ? (2 * Settings.Current.TileSpacing * (package.ColumnSpan - 1)) : 0)
                ;
            Height =
                Settings.Current.MinTileHeight * package.RowSpan
                - Settings.Current.TileSpacing * 2 * package.RowSpan
                + (package.RowSpan > 1 ? (2 * Settings.Current.TileSpacing * (package.RowSpan - 1)) : 0)
                ;

            Grid.SetColumnSpan(this, package.ColumnSpan);
            Grid.SetRowSpan(this, package.RowSpan);
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Load() {
            FocusManager.SetIsFocusScope(this, true);

            LayoutRoot.Child = package.Tile;

            // Math
            AdjustDimensions();

            // UI
            if (package.Settings.ObjectData.ContainsKey(TileBgColorKey))
                package.Tile.SetValue(
                    BackgroundProperty,
                    ((Color)ColorConverter.ConvertFromString(package.Settings.ObjectData[TileBgColorKey])).ToBrush()
                    );

            // Fx
            (Resources["LoadAnimation"] as Storyboard).Begin();

            // Context menu
            if (package.Tile.ContextMenu != null)
                contextMenu = package.Tile.ContextMenu;
            else
                ContextMenu = contextMenu = new ContextMenu();

            // Separator
            if (contextMenu.Items.Count != 0)
                contextMenu.Items.Add(new Separator());

            // Change tile color
            var changeTileColorItem = new MenuItem();
            changeTileColorItem.Header = Definitions.ChangeTileColor;
            changeTileColorItem.Click += new RoutedEventHandler((sender, e) => {
                var c = new System.Windows.Forms.ColorDialog();
                if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    var color = Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);
                    package.Settings.ObjectData[TileBgColorKey] = color.ToString();
                    package.Tile.SetValue(BackgroundProperty, color.ToBrush());
                }
            });
            contextMenu.Items.Add(changeTileColorItem);

            // Reset tile color
            var resetTileColorItem = new MenuItem();
            resetTileColorItem.Header = Definitions.ResetTileColor;
            resetTileColorItem.Click += new RoutedEventHandler((sender, e) => {
                if (package.Settings.ObjectData.ContainsKey(TileBgColorKey)) {
                    package.Settings.ObjectData.Remove(TileBgColorKey);
                }
            });
            contextMenu.Items.Add(resetTileColorItem);

            // Separator
            contextMenu.Items.Add(new Separator());

            // Disable
            var disableItem = new MenuItem();
            disableItem.Header = Definitions.TileContextMenuDisableItem;
            disableItem.Click += (sender, e) => {
                Unload();
                PackageManager.Current.Disable(package);
            };
            contextMenu.Items.Add(disableItem);

            // Remove
            var removeItem = new MenuItem();
            removeItem.Header = Definitions.TileContextMenuRemoveItem;
            removeItem.Click += (sender, e) => {
                Unload();
                PackageManager.Current.Remove(package.Metadata.Id);
            };
            contextMenu.Items.Add(removeItem);
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {
            LayoutRoot.Child = null;
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
        /// Handles the <see cref="E:MouseLeave" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeave(object sender, MouseEventArgs e) {
            if (!IsMousePressed)
                return;
            IsMousePressed = false;

            (Resources["MouseUpAnimation"] as Storyboard).Begin();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (!Settings.Current.IsTilesLockEnabled) {
                IsMousePressed = true;

                Keyboard.Focus(this);
                FocusManager.SetFocusedElement(this, this);
            }

            (Resources["MouseDownAnimation"] as Storyboard).Begin();
        }

        /// <summary>
        /// Handles the <see cref="E:MouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            IsMousePressed = false;

            (Resources["MouseUpAnimation"] as Storyboard).Begin();
        }
    }
}
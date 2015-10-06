using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns.Media.Animation;
using libns.Threading;
using Newgen.Packages;
using PackageManager;

namespace Newgen {

    /// <summary>
    /// Class TilesControl.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class TilesControl : DragScrollViewer {

        /// <summary>
        /// The tile controls
        /// </summary>
        public List<TileControl> TileControls = new List<TileControl>();

        /// <summary>
        /// The mouse x
        /// </summary>
        private double mouseX, mouseY;

        /// <summary>
        /// Gets or sets the tiles padding.
        /// </summary>
        /// <value>The tiles padding.</value>
        /// <remarks>...</remarks>
        public Thickness TilesPadding { get { return LayoutRoot.Margin; } set { LayoutRoot.Margin = value; } }

        /// <summary>
        /// Gets or sets the columns count.
        /// </summary>
        /// <value>The columns count.</value>
        /// <remarks>...</remarks>
        public int ColumnsCount { get; set; }

        /// <summary>
        /// Gets or sets the rows count.
        /// </summary>
        /// <value>The rows count.</value>
        /// <remarks>...</remarks>
        public int RowsCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesControl" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public TilesControl()
            : base() {
            InitializeComponent();
        }

        /// <summary>
        /// Adds the new group.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <remarks>...</remarks>
        public void AddNewGroup(TileControlsGroupBarSettings source) {
            var index = 1;
            var atcolumn = source.Column;

            for (var i = 0; i < Settings.Current.TileScreenGroups.Count; i++) {
                index = i + 1;
                if (Settings.Current.TileScreenGroups[i].Id == source.Id)
                    break;
                atcolumn += Settings.Current.TileScreenGroups[i].Column;
            }

            var g = new TileControlsGroupBarSettings();

            Settings.Current.TileScreenGroups.Insert(index, g);
            TilesControlGroupsHost.Children.Insert(index, new TileControlsGroupBar(this, g));

            PushAtColumnBy(atcolumn, g.Column);
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <remarks>...</remarks>
        public void DeleteGroup(TileControlsGroupBarSettings g) {
            if (Settings.Current.TileScreenGroups.Count <= 1)
                return;

            var index = 0;
            var atcolumn = 0;

            for (var i = 0; i < Settings.Current.TileScreenGroups.Count; i++) {
                index = i;
                atcolumn += Settings.Current.TileScreenGroups[i].Column;
                if (Settings.Current.TileScreenGroups[i].Id == g.Id)
                    break;
            }

            Settings.Current.TileScreenGroups.RemoveAt(index);
            TilesControlGroupsHost.Children.RemoveAt(index);

            Unload(atcolumn - g.Column, g.Column);
            ThreadingExtensions.LazyInvokeThreadSafe(() => PushAtColumnBy(atcolumn, -g.Column), 100.0);
        }

        /// <summary>
        /// Des the place.
        /// </summary>
        /// <param name="tileControl">The tile control.</param>
        /// <param name="alsoRemoveFromHost"></param>
        /// <remarks>...</remarks>
        public void DePlace(TileControl tileControl, bool alsoRemoveFromHost = false, bool permanently = false) {
            if (tileControl.package.GetSettings().Column == -1 || tileControl.package.GetSettings().Row == -1)
                return;

            // Remove dummy transparent rectangles to spanned rows and columns
            foreach (var element in TilesControlHost.Children.OfType<Rectangle>().Where(f => f.Tag.Equals(tileControl)).ToList())
                TilesControlHost.Children.Remove(element);

            // Remove
            if (alsoRemoveFromHost) {
                TilesControlHost.Children.Remove(tileControl);
                if (TileControls.Contains(tileControl))
                    TileControls.Remove(tileControl);
            }

            // - 1
            if (!permanently)
                return;
            tileControl.package.GetSettings().Column = -1;
            tileControl.package.GetSettings().Row = -1;
        }

        /// <summary>
        /// Gets the group start column.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>...</remarks>
        public int GetGroupStartColumn(TileControlsGroupBarSettings g) {
            var atcolumn = 0;

            for (var i = 0; i < Settings.Current.TileScreenGroups.Count; i++) {
                atcolumn += Settings.Current.TileScreenGroups[i].Column;
                if (Settings.Current.TileScreenGroups[i].Id != g.Id)
                    continue;
                break;
            }

            return atcolumn;
        }

        /// <summary>
        /// Places the tile control.
        /// </summary>
        /// <param name="tileControl">The tile control.</param>
        /// <param name="alsoAddToHost"></param>
        /// <remarks>...</remarks>
        public void Place(TileControl tileControl, bool alsoAddToHost = true) {
            // If not set, place virtually
            if (tileControl.package.GetSettings().Column == -1 || tileControl.package.GetSettings().Row == -1)
                PlaceVirtuallyAtFreeCell(tileControl);

            // Place really
            Grid.SetColumn(tileControl, tileControl.package.GetSettings().Column);
            Grid.SetRow(tileControl, tileControl.package.GetSettings().Row);

            // Add dummy transparent rectangles to spanned rows and columns
            // Vertical
#if DEBUG
            var debugBrush = (new SolidColorBrush(Color.FromArgb(64, 255, 50, 50))).GetAsFrozen() as SolidColorBrush;
#endif
            for (var cs = 0; cs < tileControl.package.ColumnSpan; cs++) {
                // Horizontal
                for (var rs = 0; rs < tileControl.package.RowSpan; rs++) {
                    // Except at 1st place
                    if (cs != 0 || rs != 0) {
                        var element = new Rectangle() {
                            Fill =
#if DEBUG
 debugBrush
#else
                            Brushes.Transparent
#endif
,
                            StrokeThickness = 0,
                            Tag = tileControl
                        };
                        Grid.SetColumn(element, tileControl.package.GetSettings().Column + cs);
                        Grid.SetRow(element, tileControl.package.GetSettings().Row + rs);
                        TilesControlHost.Children.Add(element);
                    }
                }
            }

            // Add
            if (alsoAddToHost)
                TilesControlHost.Children.Add(tileControl);
            if (!TileControls.Contains(tileControl))
                TileControls.Add(tileControl);

            // Adjust
            tileControl.AdjustDimensions();
        }

        /// <summary>
        /// Places at.
        /// </summary>
        /// <param name="tileControl">The tile control.</param>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <remarks>...</remarks>
        public void PlaceAt(TileControl tileControl, int column, int row, bool alsoAddToHost = true) {
            // Get present element
            var element = TilesControlHost
                .Children
                .Cast<UIElement>()
                .FirstOrDefault(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column);

            // Is no element present ?
            if (element == null) {
                tileControl.package.GetSettings().Column = column;
                tileControl.package.GetSettings().Row = row;
            }

            // Place
            Place(tileControl, alsoAddToHost);
        }

        /// <summary>
        /// Places at free cell.
        /// </summary>
        /// <param name="tileControl">The tile control.</param>
        /// <remarks>...</remarks>
        public void PlaceVirtuallyAtFreeCell(TileControl tileControl) {
            var row = 0;
            var column = 0;

            // Scan in row-first model Scan all columns
            for (; column < TilesControlHost.ColumnDefinitions.Count; column += tileControl.package.ColumnSpan) {
                // Scan all rows
                for (; row < TilesControlHost.RowDefinitions.Count; row += tileControl.package.RowSpan) {
                    // Get present element
                    var element = TilesControlHost
                        .Children
                        .Cast<UIElement>()
                        .FirstOrDefault(f => Grid.GetRow(f) == row && Grid.GetColumn(f) == column);

                    // Is no element present ?
                    if (element == null) {
                        // Place tile here
                        tileControl.package.GetSettings().Column = row;
                        tileControl.package.GetSettings().Row = row;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Pushes the tileControls.
        /// </summary>
        /// <param name="atColumn">The atcolumn.</param>
        /// <param name="count">The count.</param>
        public void PushAtColumnBy(int atColumn, int count) {
            foreach (var tileControl in TilesControlHost.Children.OfType<TileControl>()) {
                if (tileControl.package.GetSettings().Column >= atColumn) {
                    tileControl.package.GetSettings().Column += count;
                    Place(tileControl, alsoAddToHost: false);
                }
            }
        }

        /// <summary>
        /// Deletes the tileControls from columns.
        /// </summary>
        /// <param name="at">At.</param>
        /// <param name="count">The count.</param>
        public void Unload(int at, int count) {
            foreach (var tileControl in TilesControlHost.Children.OfType<TileControl>()) {
                var col = Grid.GetColumn(tileControl);
                var row = Grid.GetRow(tileControl);

                if (at <= col && col < (at + count)) {
                    tileControl.package.Stop();
                }
            }
        }

        /// <summary>
        /// Places the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void Place(NewgenPackage package) {
            if (package == null || package.Tile == null)
                return;

            // Create tile ui.
            var tileControl = new TileControl(package) {
                Order = TilesControlHost.Children.Count
            };

            tileControl.MouseLeftButtonDown += OnTileControlMouseLeftButtonDown;
            tileControl.MouseLeftButtonUp += OnTileControlMouseLeftButtonUp;
            tileControl.MouseMove += OnTileControlMouseMove;

            // Start tile.
            tileControl.Start();

            // Place tile.
            Place(tileControl, alsoAddToHost: true);
        }

        /// <summary>
        /// Des the place.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public void DePlace(NewgenPackage package) {
            if (package == null && package.Tile == null)
                return;

            // Find correct tile.
            var tileControl = TileControls.Find(x => x.package == package);

            if (tileControl == null)
                return;

            AnimationExtensions.Animate(tileControl, OpacityProperty, 150, 0, 0.7, 0.3);

            // Lazy-ness for animation.
            ThreadingExtensions.LazyInvokeThreadSafe(new Action(() => {
                tileControl.MouseLeftButtonDown -= OnTileControlMouseLeftButtonDown;
                tileControl.MouseLeftButtonUp -= OnTileControlMouseLeftButtonUp;
                tileControl.MouseMove -= OnTileControlMouseMove;

                // De-place tile.
                DePlace(tileControl, alsoRemoveFromHost: true, permanently: false);

                // Stop tile.
                tileControl.Stop();
            }), 180);
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            // Init matrix

            ColumnsCount = (int)Math.Round(SystemParameters.PrimaryScreenWidth * 5 / Settings.Current.MinTileWidth);
            RowsCount = (int)Math.Ceiling(ActualHeight / Settings.Current.MinTileHeight) + 1;

            // Compose grid

            TilesControlHost.RowDefinitions.Clear();
            TilesControlHost.ColumnDefinitions.Clear();

            System.Threading.Tasks.Parallel.For(
                0, ColumnsCount, new Action<int>((int i) => this.InvokeAsyncThreadSafe(() => {
                    var column = new ColumnDefinition { Width = new GridLength(Settings.Current.MinTileWidth) };
                    TilesControlHost.ColumnDefinitions.Add(column);
                })));

            System.Threading.Tasks.Parallel.For(
               0, RowsCount, new Action<int>((int i) => this.InvokeAsyncThreadSafe(() => {
                   var row = new RowDefinition { Height = new GridLength(Settings.Current.MinTileHeight) };
                   TilesControlHost.RowDefinitions.Add(row);
               })));

            // Compose groups

            TilesControlGroupsHost.Children.Clear();

            if (Settings.Current.TileScreenGroups.Count == 0)
                Settings.Current.TileScreenGroups.Add(new TileControlsGroupBarSettings() { Title = "" });

            for (var i = 0; i < Settings.Current.TileScreenGroups.Count; i++)
                TilesControlGroupsHost.Children.Add(new TileControlsGroupBar(this, Settings.Current.TileScreenGroups[i]));
        }

        /// <summary>
        /// Controls the mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnTileControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (Mouse.Captured == sender) {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                return;
            }

            ((TileControl)sender).IsMousePressed = true;

            mouseX = e.GetPosition((IInputElement)sender).X;
            mouseY = e.GetPosition((IInputElement)sender).Y;
        }

        /// <summary>
        /// Controls the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void OnTileControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            try {
                // Not captured ?
                if (Mouse.Captured != sender)
                    return;

                // Remove mouse capture
                Mouse.Capture(null);

                // Get
                var tileControl = (TileControl)sender;

                // Get drop location
                var dropLocation = tileControl.TransformToAncestor(DragCanvas).Transform(new System.Windows.Point(0, 0));

                // Attach
                PlaceAt(
                    tileControl,
                    column: (int)Math.Truncate((dropLocation.X + (Settings.Current.MinTileWidth / 8)) / Settings.Current.MinTileWidth),
                    row: (int)Math.Truncate((dropLocation.Y + (Settings.Current.MinTileHeight / 8)) / Settings.Current.MinTileHeight),
                    alsoAddToHost: false
                    );

                // Transfer
                DragCanvas.Children.Remove(tileControl);
                TilesControlHost.Children.Add(tileControl);

                // Remove lines
                TilesControlHost.ShowGridLines = false;

                // Remove mouse focus
                tileControl.IsMousePressed = false;
            }
            catch /* Eat */ { }
        }

        /// <summary>
        /// Controls the mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnTileControlMouseMove(object sender, MouseEventArgs e) {
            // Check if locked
            Self.DragEverywhere = Settings.Current.IsTilesLockEnabled;

            // Already dragging ?
            if (Self.IsDragging)
                return;

            // Get
            var tileControl = (TileControl)sender;

            // Recheck lock and mouse
            if (Settings.Current.IsTilesLockEnabled || !tileControl.IsMousePressed)
                return;
            // If already detached
            if (Mouse.Captured == sender) {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
            }

            // Detach
            if (Mouse.Captured != null ||
                (!(Math.Abs(e.GetPosition(tileControl).X - mouseX) >= 15) &&
                 !(Math.Abs(e.GetPosition(tileControl).Y - mouseY) >= 15)))
                return;
            // Capture
            Mouse.Capture(tileControl);

            // Already present
            if (DragCanvas.Children.Contains(tileControl))
                return;

            // Not detached yet, do it
            DePlace(tileControl, alsoRemoveFromHost: false, permanently: false);

            // Transfer from grid to canvas
            TilesControlHost.Children.Remove(tileControl);
            DragCanvas.Children.Add(tileControl);

            // Re-position
            Canvas.SetLeft(tileControl, e.GetPosition(DragCanvas).X - mouseX);
            Canvas.SetTop(tileControl, e.GetPosition(DragCanvas).Y - mouseY);

            // Show lines
            TilesControlHost.ShowGridLines = true;

            // Done
            e.Handled = true;
        }
    }
}
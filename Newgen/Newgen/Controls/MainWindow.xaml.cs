using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newgen.Base;
using Newgen.Controls;
using Newgen.Core;
using Newgen.Native;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal LockScreen lockscreen;
        internal List<WidgetControl> runningWidgets = new List<WidgetControl>();

        internal StartBar StartBar;
        internal SwitchBar SwitchBar;
        internal ThumbnailsBar ThumbnailsBar;
        private double mouseX, mouseY;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            if(E.BackgroundColor.A == 255)
                AllowsTransparency = false;

            Background = (new SolidColorBrush(E.BackgroundColor)).GetAsFrozen() as Brush;

            if(App.Settings.UseBgImage)
                try
                {
                    Background = new ImageBrush(E.GetBitmap(E.BgImage));
                }
                catch
                {
                    MessageBox.Show(E.MSG_ER_FEATURE, "Error");
                }
        }

        /// <summary>
        /// Adds the new group.
        /// </summary>
        /// <param name="source">The source.</param>
        public void AddNewGroup(TileScreenGroup source)
        {
            int index = 1;
            int atcolumn = source.Column;

            for(int i = 0; i < App.Settings.TileScreenGroups.Count; i++)
            {
                index = i + 1;
                if(App.Settings.TileScreenGroups[i].Id == source.Id)
                    break;
                atcolumn += App.Settings.TileScreenGroups[i].Column;
            }

            TileScreenGroup g = new TileScreenGroup();
            App.Settings.TileScreenGroups.Insert(index, g);
            this.GroupsHost.Children.Insert(index, new TilesGroupBar(this, g));

            this.PushWidgets(atcolumn, g.Column);
        }

        public void ComposeGroups()
        {
            this.GroupsHost.Children.Clear();

            if(App.Settings.TileScreenGroups.Count == 0)
            {
                App.Settings.TileScreenGroups.Add(new TileScreenGroup() { Title = "Apps" });
            }

            for(int i = 0; i < App.Settings.TileScreenGroups.Count; i++)
                this.GroupsHost.Children.Add(new TilesGroupBar(this, App.Settings.TileScreenGroups[i]));
        }

        public void ComposeMarkupGrid()
        {
            WidgetHost.RowDefinitions.Clear();
            WidgetHost.ColumnDefinitions.Clear();

            System.Threading.Tasks.Parallel.For(
                0, App.WindowManager.Matrix.ColumnsCount, new Action<int>((int i) =>
                {
                    HelperMethods.RunMethodAsyncThreadSafe(() =>
                    {
                        var column = new ColumnDefinition();
                        column.Width = new GridLength(E.MinTileWidth / 2);
                        WidgetHost.ColumnDefinitions.Add(column);
                    });
                }));

            System.Threading.Tasks.Parallel.For(
               0, App.WindowManager.Matrix.RowsCount, new Action<int>((int i) =>
               {
                   HelperMethods.RunMethodAsyncThreadSafe(() =>
                   {
                       var row = new RowDefinition();
                       row.Height = new GridLength(E.MinTileHeight);
                       WidgetHost.RowDefinitions.Add(row);
                   });
               }));
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="g">The g.</param>
        public void DeleteGroup(TileScreenGroup g)
        {
            if(App.Settings.TileScreenGroups.Count <= 1)
                return;

            int index = 0;
            int atcolumn = 0;

            for(int i = 0; i < App.Settings.TileScreenGroups.Count; i++)
            {
                index = i;
                atcolumn += App.Settings.TileScreenGroups[i].Column;
                if(App.Settings.TileScreenGroups[i].Id == g.Id)
                    break;
            }

            App.Settings.TileScreenGroups.RemoveAt(index);
            this.GroupsHost.Children.RemoveAt(index);

            this.UnLoadWidgetsFromColumns(atcolumn - g.Column, g.Column);
            Helper.Delay(() =>
            {
                this.PushWidgets(atcolumn, -g.Column);
            }, 100.0);
        }

        /// <summary>
        /// Gets the group start column.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <returns></returns>
        public int GetGroupStartColumn(TileScreenGroup g)
        {
            int atcolumn = 0;

            for(int i = 0; i < App.Settings.TileScreenGroups.Count; i++)
            {
                atcolumn += App.Settings.TileScreenGroups[i].Column;
                if(App.Settings.TileScreenGroups[i].Id == g.Id)
                    break;
            }

            return atcolumn;
        }

        public void PlaceWidget(WidgetControl widget)
        {
            var colSpan = Grid.GetColumnSpan(widget);
            TileCell cell;
            if(widget.WidgetProxy.Column == -1 || widget.WidgetProxy.Row == -1)
                cell = App.WindowManager.Matrix.GetFreeCell(colSpan);
            else
                cell = new TileCell(widget.WidgetProxy.Column, widget.WidgetProxy.Row);
            Grid.SetColumn(widget, (int)cell.Column);
            Grid.SetRow(widget, (int)cell.Row);
            Grid.SetColumnSpan(widget, colSpan);
            App.WindowManager.Matrix.ReserveSpace(cell.Column, cell.Row, colSpan);
            widget.Width = (colSpan / 2) * E.MinTileWidth - E.TileSpacing * 2;
            widget.Height = E.MinTileHeight - E.TileSpacing * 2;
        }

        /// <summary>
        /// Pushes the widgets.
        /// </summary>
        /// <param name="atcolumn">The atcolumn.</param>
        /// <param name="count">The count.</param>
        public void PushWidgets(int atcolumn, int count)
        {
            foreach(WidgetControl widget in this.WidgetHost.Children.OfType<WidgetControl>())
            {
                var col = Grid.GetColumn(widget);
                var row = Grid.GetRow(widget);
                var colspan = Grid.GetColumnSpan(widget);

                if(col >= atcolumn)
                {
                    widget.WidgetProxy.Column = col + count;
                    widget.WidgetProxy.Row = row;

                    this.PlaceWidget(widget);

                    App.WindowManager.Matrix.FreeSpace(col, row, colspan);
                }
            }
        }

        /// <summary>
        /// Deletes the widgets from columns.
        /// </summary>
        /// <param name="at">At.</param>
        /// <param name="count">The count.</param>
        public void UnLoadWidgetsFromColumns(int at, int count)
        {
            foreach(WidgetControl widget in this.WidgetHost.Children.OfType<WidgetControl>())
            {
                var col = Grid.GetColumn(widget);
                var row = Grid.GetRow(widget);
                var colspan = Grid.GetColumnSpan(widget);

                if(at <= col && col < (at + count))
                {
                    App.WidgetManager.UnloadWidget(widget.WidgetProxy);
                }
            }
        }

        internal void LoadUISettings()
        {
            Header_TitleText.Text = App.Settings.StartText;
        }

        internal void LoadUserTileInfo()
        {
            HelperMethods.RunMethodAsyncThreadSafe(() =>
            {
                if(!App.Settings.IsUserTileEnabled)
                    UserTile.Visibility = Visibility.Collapsed;
                else
                {
                    UserTile.Visibility = Visibility.Visible;
                    UserTile.Opacity = 0;
                    Helper.Animate(UserTile, OpacityProperty, 300, 1);
                    var userimagefile = System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp";
                    if(File.Exists(userimagefile))
                        File.Copy(userimagefile, E.UserImage, true);
                    Header_UserName.Text = Environment.UserName;
                    Header_UserPic.Source = E.GetBitmap(E.UserImage) ?? Header_UserPic.Source;
                }
                Helper.Animate(this.Header, OpacityProperty, 250, 1);
            });
        }

        internal void ZOrderHelper(bool settrue = true)
        {
            if(settrue)
            {
                if(ThumbnailsBar != null)
                {
                    ThumbnailsBar.Topmost = false;
                    ThumbnailsBar.Topmost = true;
                }
                else if(SwitchBar != null)
                {
                    SwitchBar.Topmost = false;
                    SwitchBar.Topmost = true;
                }
                if(StartBar != null)
                {
                    StartBar.Topmost = false;
                    StartBar.Topmost = true;
                }
            }
            else
            {
                if(ThumbnailsBar != null)
                {
                    ThumbnailsBar.Topmost = true;
                    ThumbnailsBar.Topmost = false;
                }
                else if(SwitchBar != null)
                {
                    SwitchBar.Topmost = true;
                    SwitchBar.Topmost = false;
                }
                if(StartBar != null)
                {
                    StartBar.Topmost = true;
                    StartBar.Topmost = false;
                }
            }
        }

        private void AnimateImage()
        {
            var w = (int)SystemParameters.PrimaryScreenWidth;
            var h = (int)SystemParameters.PrimaryScreenHeight;

            var image = new System.Drawing.Bitmap(w, h);
            var graphics = System.Drawing.Graphics.FromImage(image);

            graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(w, h));
            graphics.Dispose();

            BitmapSource result;
            if(image == null)
            {
                result = null;
                return;
            }
            else
            {
                result = Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }

            AnimationImage.Source = result;
            AnimationImage.Width = w;
            AnimationImage.Height = h;
            AnimationImage2.Width = w;
            AnimationImage2.Height = h;

            Helper.Animate(
                AnimationImage, Canvas.LeftProperty,
                750, 0, -SystemParameters.PrimaryScreenWidth,
                0.7, 0.3
                );

            Helper.Delay(new Action(() =>
            {
                AnimationCanvas.Visibility = Visibility.Collapsed;

                {
                    if(App.Settings.SlideShowImages.Count > 1)
                    {
                        AnimationCanvas.Visibility = Visibility.Visible;

                        AnimationImage.Source = new BitmapImage(new Uri(
                                                          App.Settings.SlideShowImages[(new Random()).Next(0, App.Settings.SlideShowImages.Count - 1)], UriKind.Absolute)
                                                          );

                        Helper.RunFor(() =>
                        {
                            try
                            {
                                AnimationImage2.Source = new BitmapImage(new Uri(
                                                                  App.Settings.SlideShowImages[(new Random()).Next(0, App.Settings.SlideShowImages.Count - 1)], UriKind.Absolute)
                                                                  );

                                Helper.Animate(
                                    AnimationImage, Canvas.LeftProperty,
                                    750, 0, -SystemParameters.PrimaryScreenWidth, null,
                                    0.7, 0.3, false, 1, null, FillBehavior.HoldEnd,
                                    (a, b) =>
                                    {
                                        try
                                        {
                                            AnimationImage.Source = AnimationImage2.Source;
                                        }
                                        catch { }
                                    }
                                    , null);
                            }
                            catch { }
                        }, -1, App.Settings.SlideShowTime * 750);
                    }
                }
            }), 1200);
        }

        private void CloseItemClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void ControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(Mouse.Captured == sender)
            {
                Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                return;
            }

            var widget = (WidgetControl)sender;
            widget.MousePressed = true;
            mouseX = e.GetPosition((IInputElement)sender).X;
            mouseY = e.GetPosition((IInputElement)sender).Y;
        }

        private void ControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(Mouse.Captured != sender)
                    return;
                this.Topmost = false;
                Mouse.Capture(null);
                var widget = (WidgetControl)sender;
                var coords = widget.TransformToAncestor(DragCanvas).Transform(new Point(0, 0));
                DragCanvas.Children.Remove(widget);
                WidgetHost.Children.Add(widget);

                var colSpan = Grid.GetColumnSpan(widget);

                var column = (int)Math.Truncate((coords.X) / E.MinTileWidth * 2);
                var row = (int)Math.Truncate((coords.Y + E.MinTileHeight / 2) / E.MinTileHeight);
                var isFree = App.WindowManager.Matrix.IsCellFree(column, row, colSpan);
                if(!isFree ||
                    (column >= App.WindowManager.Matrix.ColumnsCount || row >= App.WindowManager.Matrix.RowsCount))
                    PlaceWidget(widget);
                else
                {
                    Grid.SetColumn(widget, column);
                    Grid.SetRow(widget, row);
                    widget.Width = (colSpan / 2) * E.MinTileWidth - E.TileSpacing * 2;
                    widget.Height = E.MinTileHeight - E.TileSpacing * 2;
                    App.WindowManager.Matrix.ReserveSpace(column, row, colSpan);
                }

                WidgetHost.ShowGridLines = false;

                widget.MousePressed = false;
            }
            catch { }
        }

        private void ControlMouseMove(object sender, MouseEventArgs e)
        {
            if(App.Settings.IsWidgetsLockEnabled)
            {
                DragScroll.DragEverywhere = true;
            }
            else
            {
                DragScroll.DragEverywhere = false;
            }

            if(DragScroll.IsDragging)
                return;

            var widget = (WidgetControl)sender;

            if(!App.Settings.IsWidgetsLockEnabled && widget.MousePressed)
            {
                if(Mouse.Captured == sender)
                {
                    Canvas.SetLeft((UIElement)sender, e.GetPosition(DragCanvas).X - mouseX);
                    Canvas.SetTop((UIElement)sender, e.GetPosition(DragCanvas).Y - mouseY);
                }

                var drag = Math.Abs(e.GetPosition(widget).X - mouseX) >= 15 || Math.Abs(e.GetPosition(widget).Y - mouseY) >= 15;

                if(Mouse.Captured == null && drag)
                {
                    this.Topmost = true;
                    Mouse.Capture(widget);
                    if(DragCanvas.Children.Contains(widget))
                        return;
                    WidgetHost.Children.Remove(widget);
                    DragCanvas.Children.Add(widget);

                    Canvas.SetLeft(widget, e.GetPosition(DragCanvas).X - mouseX);
                    Canvas.SetTop(widget, e.GetPosition(DragCanvas).Y - mouseY);
                    var col = Grid.GetColumn(widget);
                    var row = Grid.GetRow(widget);
                    var colspan = Grid.GetColumnSpan(widget);

                    App.WindowManager.Matrix.FreeSpace(col, row, colspan);
                    widget.Width = E.MinTileWidth * (colspan / 2);
                    widget.Height = E.MinTileHeight;

                    WidgetHost.ShowGridLines = true;

                    widget.WidgetProxy.Column = -1;
                    widget.WidgetProxy.Row = -1;

                    e.Handled = true;
                }
            }
        }

        private void Header_Lock_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Helper.Animate(Header_Lock, OpacityProperty, 250, 0.3);
            WinAPI.LockWorkStation();
            Helper.Animate(Header_Lock, OpacityProperty, 500, 1);
        }

        private void Header_TitleText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DragScroll.ScrollToLeftEnd();
        }

        private void LoadToolbars()
        {
            StartBar = new StartBar();
            if(App.Settings.UseThumbailsBar)
                ThumbnailsBar = new ThumbnailsBar();
            else
                SwitchBar = new SwitchBar();

            if(App.Settings.UseThumbailsBar)
                ThumbnailsBar.OpenToolbar();
            else
                SwitchBar.OpenToolbar();
            StartBar.OpenToolbar();

            lockscreen = new LockScreen();
            Root.Children.Add(lockscreen);

            Helper.Delay(() =>
            {
                if(App.Settings.UseThumbailsBar)
                    ThumbnailsBar.CloseToolbar();
                else
                    SwitchBar.CloseToolbar();
                StartBar.CloseToolbar();
            }, 250);
        }

        private void LoadWidgetSystem()
        {
            var tileswidth = SystemParameters.PrimaryScreenWidth * 4;
            var c = Math.Round(tileswidth / (E.MinTileWidth / 2));
            var tilesheight = this.DragScroll.ActualHeight - (20);
            var r = (tilesheight / (E.MinTileHeight - E.TileSpacing * 2));

            App.WindowManager.Initialize((int)c, (int)r);

            this.ComposeMarkupGrid();

            this.ComposeGroups();
            
            while(!App.WidgetManager.IsLoaded)
                ;

            if(App.Settings.LoadedWidgets.Count == 0)
            {
                App.Settings.LoadedWidgets.Add(new TileScreenWidgetInfo() { Name = "Clock", Column = 0, Row = 0 });
                App.Settings.LoadedWidgets.Add(new TileScreenWidgetInfo() { Name = "Internet", Column = 0, Row = 1 });
                App.Settings.LoadedWidgets.Add(new TileScreenWidgetInfo() { Name = "Store", Column = 2, Row = 1 });
            }

            NewgenAdWidget.LoadAds();

            foreach(var lw in App.Settings.LoadedWidgets)
            {
                TileScreenWidgetInfo loadedWidget = lw;
                HelperMethods.RunMethodAsyncThreadSafe(() =>
                {
                    WidgetProxy widget;
                    if(!string.IsNullOrEmpty(loadedWidget.Name) && string.IsNullOrEmpty(loadedWidget.Id))
                    {
                        widget = App.WidgetManager.Widgets.Find(x => x.Name == loadedWidget.Name);
                        if(widget == null)
                            return;

                        widget.Row = loadedWidget.Row;
                        widget.Column = loadedWidget.Column;
                        widget.ObjectData = loadedWidget.Data;
                        App.WidgetManager.LoadWidget(widget);
                    }
                    else if(!string.IsNullOrEmpty(loadedWidget.Path))
                    {
                        if(File.Exists(E.WidgetsRoot + loadedWidget.Path))
                            widget = App.WidgetManager.CreateWidget(E.WidgetsRoot + loadedWidget.Path);
                        else
                            widget = App.WidgetManager.CreateWidget(loadedWidget.Path);

                        widget.Row = loadedWidget.Row;
                        widget.Column = loadedWidget.Column;
                        widget.ObjectData = loadedWidget.Data;
                        App.WidgetManager.LoadWidget(widget);
                    }
                    else
                    {
                        widget = App.WidgetManager.CreateGeneratedWidget(loadedWidget.Id, loadedWidget.Name);
                        widget.Row = loadedWidget.Row;
                        widget.Column = loadedWidget.Column;
                        widget.ObjectData = loadedWidget.Data;
                        App.WidgetManager.LoadWidget(widget);
                    }
                });
            }
        }

        private void MenuItem_CUT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = E.ImageFilter;
                if(!(bool)dialog.ShowDialog())
                    return;
                try
                {
                    File.Copy(dialog.FileName, E.UserImage, true);
                    Newgen.Base.MessagingHelper.SendMessageToNewgen("Update", "UserInfo");
                }
                catch(Exception)
                {
                    MessageBox.Show("Problem with user account image.", "Error");
                }
            }
            catch { }
        }

        private void MenuItem_Lock_Click(object sender, RoutedEventArgs e)
        {
            try { WinAPI.LockWorkStation(); }
            catch { }
        }

        private void MenuItem_LogOff_Click(object sender, RoutedEventArgs e)
        {
            try { WinAPI.ExitWindowsEx(0, 0); }
            catch { }
        }

        private void MenuItem_Restart_Click(object sender, RoutedEventArgs e)
        {
            try { WinAPI.ExitWindowsEx(0x00000002, 0); }
            catch { }
        }

        private void MenuItem_Shutdown_Click(object sender, RoutedEventArgs e)
        {
            try { WinAPI.ExitWindowsEx(0x00000001, 0); }
            catch { }
        }

        private void WidgetManagerWidgetLoaded(WidgetProxy widget)
        {
            var control = new WidgetControl(widget);
            control.Order = WidgetHost.Children.Count;
            control.MouseLeftButtonDown += ControlMouseLeftButtonDown;
            control.MouseLeftButtonUp += ControlMouseLeftButtonUp;
            control.MouseMove += ControlMouseMove;
            runningWidgets.Add(control);
            control.Load();
            Grid.SetColumnSpan(control, widget.WidgetComponent.ColumnSpan * 2);
            PlaceWidget(control);
            WidgetHost.Children.Add(control);
        }

        private void WidgetManagerWidgetUnloaded(WidgetProxy widget)
        {
            var control = runningWidgets.Find(x => x.WidgetProxy == widget);
            if(control == null)
                return;
            Helper.Animate(control, OpacityProperty, 150, 0, 0.7, 0.3);
            Helper.Delay(new Action(() =>
            {
                control.MouseLeftButtonDown -= ControlMouseLeftButtonDown;
                control.MouseLeftButtonUp -= ControlMouseLeftButtonUp;
                control.MouseMove -= ControlMouseMove;
                var col = Grid.GetColumn(control);
                var row = Grid.GetRow(control);
                var colspan = Grid.GetColumnSpan(control);
                WidgetHost.Children.Remove(control);
                runningWidgets.Remove(control);
                control.Unload();
                App.WindowManager.Matrix.FreeSpace(col, row, colspan);
            }), 180);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                Newgen.Base.MessagingHelper.RemoveListener(handle);
            }
            catch { }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadUISettings();

            this.AnimateImage();

            this.LoadUserTileInfo();

            App.WidgetManager.WidgetLoaded += WidgetManagerWidgetLoaded;
            App.WidgetManager.WidgetUnloaded += WidgetManagerWidgetUnloaded;

            Helper.Delay(() => this.LoadToolbars(), 250);
            Helper.Delay(() => this.LoadWidgetSystem(), 2500);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(ThumbnailsBar != null) { ThumbnailsBar.CloseToolbar(); }
            else if(SwitchBar != null) { SwitchBar.CloseToolbar(); }
            if(StartBar != null) { StartBar.CloseToolbar(); }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;

            WinAPI.RemoveFromDWM(handle);

            try { Newgen.Base.MessagingHelper.AddListener(handle); }
            catch { }

            try
            {
                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                Width = SystemParameters.PrimaryScreenWidth;

                if(App.Settings.ShowTaskbarAlways)
                {
                    Height = SystemParameters.WorkArea.Height;
                    Top = SystemParameters.WorkArea.Top;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                }
                else if(App.Settings.ShowTaskbar)
                {
                    Height = SystemParameters.PrimaryScreenHeight;
                    Top = 0;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                }
                else
                {
                    Height = SystemParameters.PrimaryScreenHeight;
                    Top = 0;
                    WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Hide);
                    WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Hide);
                }
            }
            catch { }

            this.Left = 0;
        }
    }
}
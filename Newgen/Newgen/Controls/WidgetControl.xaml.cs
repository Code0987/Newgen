using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Newgen.Base;
using Newgen.Core;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for NewgenTile.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        public readonly WidgetProxy WidgetProxy;
        private int order = 0;
        public bool MousePressed;
        private ContextMenu contextMenu;
        private MenuItem removeItem;
        private MenuItem refreshItem;

        public int Order
        {
            get { return order; }
            set
            {
                order = value;

                var s = Resources["LoadAnim"] as Storyboard;
                s.BeginTime = TimeSpan.FromMilliseconds(500 + 25 * value);
            }
        }

        public WidgetControl()
        {
            InitializeComponent();
        }

        public WidgetControl(WidgetProxy widgetProxy)
        {
            InitializeComponent();

            this.WidgetProxy = widgetProxy;
        }

        public void Load()
        {
            FocusManager.SetIsFocusScope(this, true);
            WidgetProxy.Load();
            Root.Children.Clear();
            try { Root.Children.Add(WidgetProxy.WidgetComponent.WidgetControl); }
            catch { }
            this.Width = E.MinTileWidth * WidgetProxy.WidgetComponent.ColumnSpan - E.TileSpacing * 2 * (WidgetProxy.WidgetComponent.ColumnSpan - 1);
            this.Height = E.MinTileHeight - E.TileSpacing * 2;
            this.Margin = new Thickness(E.TileSpacing);
            Grid.SetColumnSpan(this, WidgetProxy.WidgetComponent.ColumnSpan);

            if(WidgetProxy.TileCellType == TileCellType.Html)
            {
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonDown += WidgetControlMouseLeftButtonDown;
                WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonUp += WidgetControlMouseLeftButtonUp;
                WidgetProxy.WidgetComponent.WidgetControl.MouseMove += WidgetControlMouseMove;
            }

            try
            {
                var da = HelperMethods.GetDataArray(WidgetProxy.Name ?? WidgetProxy.Path);
                if(!string.IsNullOrWhiteSpace(da[0]))
                    WidgetProxy.WidgetComponent.WidgetControl.SetValue(
                        BackgroundProperty,
                        new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(da[0]))
                        );
            }
            catch { }

            var s = Resources["LoadAnim"] as Storyboard;
            s.Begin();

            if(WidgetProxy.WidgetComponent.WidgetControl.ContextMenu != null)
            {
                contextMenu = WidgetProxy.WidgetComponent.WidgetControl.ContextMenu;
            }
            else
            {
                contextMenu = new ContextMenu();
                this.ContextMenu = contextMenu;
            }

            MenuItem mi_cc = new MenuItem();
            mi_cc.Header = "Change Tile Color";
            mi_cc.Click += new RoutedEventHandler(MI_CC_Click);
            contextMenu.Items.Add(mi_cc);

            contextMenu.Items.Add(new Separator());

            HelperMethods.RunMethodAsyncThreadSafe(() =>
            {
                if(WidgetProxy.TileCellType == TileCellType.Generated)
                {
                    if(!string.IsNullOrEmpty(WidgetProxy.Path) && WidgetProxy.Path.StartsWith("http://"))
                    {
                        refreshItem = new MenuItem();
                        refreshItem.Header = Newgen.Resources.Resources.RefreshItem;
                        refreshItem.Click += RefreshItemClick;
                        contextMenu.Items.Add(refreshItem);
                    }
                }

                if(App.Settings.EnableOutOfBoxExperience)
                    Effect = App.Current.Resources["DropShadowEffect_WidgetNormal"] as Effect;
            });

            removeItem = new MenuItem();
            removeItem.Header = Newgen.Resources.Resources.RemoveItem;
            removeItem.Click += RemoveItemClick;
            contextMenu.Items.Add(removeItem);
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            WidgetProxy.WidgetComponent.Refresh();
        }

        private void RemoveItemClick(object sender, RoutedEventArgs e)
        {
            removeItem.Click -= RemoveItemClick;
            App.WidgetManager.UnloadWidget(WidgetProxy);
        }

        private void WidgetControlMouseMove(object sender, MouseEventArgs e)
        {
            this.RaiseEvent(e);
        }

        private void WidgetControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MousePressed = false;
            this.RaiseEvent(e);
        }

        private void WidgetControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePressed = true;
            this.RaiseEvent(e);
        }

        public void Unload()
        {
            if(refreshItem != null)
                refreshItem.Click -= RefreshItemClick;
            WidgetProxy.Unload();
            Root.Children.Clear();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //{
            if(!App.Settings.IsWidgetsLockEnabled)
            {
                MousePressed = true;
                Keyboard.Focus(this);
                FocusManager.SetFocusedElement(this, this);
                var a = Keyboard.FocusedElement;
            }
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if(App.Settings.EnableOutOfBoxExperience)
                Effect = App.Current.Resources["DropShadowEffect_WidgetNormal"] as Effect;
            else
                Effect = null;
            (Resources["MouseUpAnim"] as Storyboard).Begin();
            if(!MousePressed)
                return;
            MousePressed = false;
        }

        private void this_MouseEnter(object sender, MouseEventArgs e)
        {
            if(App.Settings.EnableOutOfBoxExperience)
                Effect = App.Current.Resources["DropShadowEffect_WidgetHover"] as Effect;
        }

        private void MI_CC_Click(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if(c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = System.Windows.Media.Color.FromRgb(c.Color.R, c.Color.G, c.Color.B);

                try { HelperMethods.SetDataArray(WidgetProxy.Name ?? WidgetProxy.Path, color.ToString(), 0); }
                catch { }

                WidgetProxy.WidgetComponent.WidgetControl.SetValue(BackgroundProperty, new SolidColorBrush(color));
            }
        }
    }
}
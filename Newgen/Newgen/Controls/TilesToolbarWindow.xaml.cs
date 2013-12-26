using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Controls;
using Newgen.Core;
using Newgen.Native;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for TilesToolbarWindow.xaml
    /// </summary>
    public partial class TilesToolbarWindow : Window
    {
        private bool isOpened;
        DateTime mouseclicktimestamp = DateTime.Now;
        private double mouseX, mouseY;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesToolbarWindow"/> class.
        /// </summary>
        public TilesToolbarWindow()
        {
            this.Left = 0;
            try
            {
                if (!App.Settings.ShowTaskbarAlways)
                {
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    this.Top = 0;
                }
                else
                {
                    this.Width = SystemParameters.WorkArea.Width;
                    this.Top = SystemParameters.WorkArea.Top;
                }
            }
            catch { }
            this.Opacity = 0;

            this.InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Top = -this.ActualHeight;

            isOpened = false;
        }

        /// <summary>
        /// Windows the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Topmost = true;
            if (!this.isOpened)
            {
                this.Open();
            }
        }

        /// <summary>
        /// Windows the preview key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!this.isOpened) this.Open();
                else this.CloseToolbar();
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the TouchSupportPin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TouchSupportPin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CloseToolbar();
        }

        /// <summary>
        /// Items the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int timediff = (int)DateTime.Now.Subtract(mouseclicktimestamp).TotalMilliseconds;
            mouseclicktimestamp = DateTime.Now;

            if (timediff < E.AnimationTimePrecision) return;

            if (mouseX != e.GetPosition(this).X || mouseY != e.GetPosition(this).Y) return;

            var name = ((TilesToolbarItem)sender).Title;

            if (App.WidgetManager.IsWidgetLoaded(name)) App.WidgetManager.UnloadWidget(name);
            else App.WidgetManager.LoadWidget(name);
        }

        /// <summary>
        /// Items the mouse left button down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseX = e.GetPosition(this).X;
            mouseY = e.GetPosition(this).Y;
        }

        /// <summary>
        /// Toolbars the close anim completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ToolbarCloseAnimCompleted(object sender, EventArgs e)
        {
            foreach (TilesToolbarItem item in this.WidgetsList.Children)
            {
                item.MouseLeftButtonDown -= ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }

            this.WidgetsList.Children.Clear();
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public void Open()
        {
            try
            {
                if (!App.Settings.ShowTaskbarAlways)
                {
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    this.Top = 0;
                }
                else
                {
                    this.Width = SystemParameters.WorkArea.Width;
                    this.Top = SystemParameters.WorkArea.Top;
                }
            }
            catch { }

            this.Opacity = 1;

            try
            {
                this.MainContent.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);
                this.TouchSupportPin.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);
            }
            catch { }

            Helper.Animate(this, TopProperty, 250, -this.ActualHeight, -1, 0.7, 0.3);

            this.isOpened = true;

            Helper.Delay(() =>
            {
                foreach (var w in App.WidgetManager.Widgets)
                {
                    if (w.TileCellType == TileCellType.Generated) continue;

                    var item = new TilesToolbarItem();
                    item.Title = w.Name;

                    if (w.WidgetComponent.IconPath == null) item.Icon = new BitmapImage(new Uri("/Resources/default_icon.png", UriKind.Relative));
                    else item.Icon = new BitmapImage((w.WidgetComponent.IconPath));

                    item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                    item.MouseLeftButtonUp += ItemMouseLeftButtonUp;

                    this.WidgetsList.Children.Add(item);
                }

                try
                {
                    for (int i = 0; i < this.WidgetsList.Children.Count; i++)
                    {
                        try
                        {
                            TilesToolbarItem item = null;
                            int order = 1;
                            if (i < this.WidgetsList.Children.Count / 2)
                            {
                                item = ((TilesToolbarItem)this.WidgetsList.Children[i]);
                                order = this.WidgetsList.Children.Count - i;
                            }
                            else
                            {
                                item = ((TilesToolbarItem)this.WidgetsList.Children[i]);
                                order = 1;
                            }

                            item.Translate.Y = -200.0;

                            DoubleAnimation doubleAnimation1 = new DoubleAnimation()
                            {
                                To = 0,
                                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                                BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds((double)(order * 50 + 10))),
                                AccelerationRatio = 0.3,
                                DecelerationRatio = 0.3,
                                FillBehavior = FillBehavior.Stop
                            };
                            doubleAnimation1.Completed += (a, b) => { item.Translate.Y = 0; };
                            item.Translate.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, doubleAnimation1);
                        }
                        catch { }
                    }
                }
                catch { }
            }, 250);

            WinAPI.RemoveFromDWM(this);
        }

        public void CloseToolbar()
        {
            this.isOpened = false;

            Helper.Animate(this, TopProperty, 250, -this.ActualHeight, 0.3, 0.7);
            Helper.Delay(() => { this.Close(); }, 250);
        }
    }
}
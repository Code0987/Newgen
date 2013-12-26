using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newgen.Base;
using Newgen.Windows;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for TilesGroupBar.xaml
    /// </summary>
    public partial class TilesGroupBar : UserControl
    {
        private MainWindow tsw;
        private TileScreenGroup groupinfo;
        private double colsize = App.Settings.MinTileWidth / 2.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesGroupBar"/> class.
        /// </summary>
        public TilesGroupBar(MainWindow tsw, TileScreenGroup groupinfo)
        {
            InitializeComponent();

            this.tsw = tsw;

            this.groupinfo = groupinfo;

            if (this.groupinfo.Column == 0 || this.groupinfo.Column == -1)
                this.groupinfo.Column = 6;

            this.Width = this.groupinfo.Column * colsize;
            this.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor)
            {
                Opacity = 0.5
            };
            this.TextBlock_Add.Opacity = 0.0;
            this.TextBlock_Delete.Opacity = 0.0;
        }

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextBox_Title.Text = this.groupinfo.Title;

            Helper.Animate(this.Background, SolidColorBrush.OpacityProperty, 250, 0.0, true);
        }

        /// <summary>
        /// Handles the MouseEnter event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Helper.Animate(this.Background, SolidColorBrush.OpacityProperty, 250, 0.5, true);
            Helper.Animate(this.TextBlock_Add, TextBlock.OpacityProperty, 250, 1.0, true);
            Helper.Animate(this.TextBlock_Delete, TextBlock.OpacityProperty, 250, 1.0, true);
        }

        /// <summary>
        /// Handles the MouseLeave event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Helper.Animate(this.Background, SolidColorBrush.OpacityProperty, 450, 0.0, true);
            Helper.Animate(this.TextBlock_Add, TextBlock.OpacityProperty, 350, 0.0, true);
            Helper.Animate(this.TextBlock_Delete, TextBlock.OpacityProperty, 350, 0.0, true);
        }

        /// <summary>
        /// Handles the TextChanged event of the TextBox_Title control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBox_Title_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.groupinfo != null)
                this.groupinfo.Title = this.TextBox_Title.Text;
        }

        /// <summary>
        /// Handles the MouseUp event of the TextBlock_Add control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseTextBlockEventArgs"/> instance containing the event data.</param>
        private void TextBlock_Add_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.tsw.AddNewGroup(this.groupinfo);
        }

        /// <summary>
        /// Handles the MouseUp event of the TextBlock_Delete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TextBlock_Delete_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.tsw.DeleteGroup(this.groupinfo);
        }

        /// <summary>
        /// Handles the DragDelta event of the Thumb_Right control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void Thumb_Right_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            bool isleftdrag = e.HorizontalChange < 0;

            if (Math.Abs(e.HorizontalChange) > colsize && this.Width >= App.Settings.MinTileWidth)
            {
                if (!isleftdrag)
                {
                    this.Width += colsize;
                    this.groupinfo.Column += 1;
                    this.tsw.PushWidgets(this.tsw.GetGroupStartColumn(this.groupinfo) - 1, 1);
                }
                else
                {
                    this.Width += -1 * colsize;
                    this.groupinfo.Column -= 1;
                    this.tsw.PushWidgets(this.tsw.GetGroupStartColumn(this.groupinfo) + 1, -1);
                }
            }
        }
    }
}
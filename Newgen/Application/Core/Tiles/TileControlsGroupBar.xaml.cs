using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Newgen
{
    public partial class TileControlsGroupBar : UserControl
    {
        private TilesControl tilesControl;
        private TileControlsGroupBarSettings tileControlsGroupBarSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileControlsGroupBar"/> class.
        /// </summary>
        public TileControlsGroupBar(TilesControl tilesControl, TileControlsGroupBarSettings tileControlsGroupBarSettings)
        {
            InitializeComponent();

            this.tilesControl = tilesControl;
            this.tileControlsGroupBarSettings = tileControlsGroupBarSettings;

            if (tileControlsGroupBarSettings.Column == 0 || tileControlsGroupBarSettings.Column == -1)
                tileControlsGroupBarSettings.Column = 6;

            Width = tileControlsGroupBarSettings.Column * Settings.Current.MinTileWidth;
            Background = new SolidColorBrush(Settings.Current.ToolbarBackgroundColor)
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
            this.TextBox_Title.Text = this.tileControlsGroupBarSettings.Title;

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
            if (this.tileControlsGroupBarSettings != null)
                this.tileControlsGroupBarSettings.Title = this.TextBox_Title.Text;
        }

        /// <summary>
        /// Handles the MouseUp event of the TextBlock_Add control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseTextBlockEventArgs"/> instance containing the event data.</param>
        private void TextBlock_Add_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.tilesControl.AddNewGroup(this.tileControlsGroupBarSettings);
        }

        /// <summary>
        /// Handles the MouseUp event of the TextBlock_Delete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TextBlock_Delete_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.tilesControl.DeleteGroup(this.tileControlsGroupBarSettings);
        }

        /// <summary>
        /// Handles the DragDelta event of the Thumb_Right control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.Primitives.DragDeltaEventArgs"/> instance containing the event data.</param>
        private void Thumb_Right_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            bool isleftdrag = e.HorizontalChange < 0;

            if (Math.Abs(e.HorizontalChange) > Settings.Current.MinTileWidth && this.Width >= Settings.Current.MinTileWidth)
            {
                if (!isleftdrag)
                {
                    this.Width += Settings.Current.MinTileWidth;
                    this.tileControlsGroupBarSettings.Column += 1;
                    this.tilesControl.PushAtColumnBy(this.tilesControl.GetGroupStartColumn(this.tileControlsGroupBarSettings) - 1, 1);
                }
                else
                {
                    this.Width += -1 * Settings.Current.MinTileWidth;
                    this.tileControlsGroupBarSettings.Column -= 1;
                    this.tilesControl.PushAtColumnBy(this.tilesControl.GetGroupStartColumn(this.tileControlsGroupBarSettings) + 1, -1);
                }
            }
        }
    }
}
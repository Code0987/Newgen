using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using libns.Native;
using libns.UI;
using Newgen.Controls;

namespace Newgen {

    /// <summary>
    /// Class ThumbnailsBar.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class ThumbnailsBar : ToolbarWindow {

        /// <summary>
        /// Initializes a new instance of the <see cref="StartBar" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public ThumbnailsBar()
            : base() {
            Location = ToolbarLocation.Left;
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:SourceInitialized" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnSourceInitialized(object sender, EventArgs e) {
            Width++;
            Width--;
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

        /// <summary>
        /// Initializes the thumbnails.
        /// </summary>
        /// <remarks>...</remarks>
        private void InitializeThumbnails() {
            this.ForEachHWND((current, text) => {
                var t = new TextBlock();
                t.HorizontalAlignment = HorizontalAlignment.Center;
                t.TextAlignment = TextAlignment.Center;
                t.TextWrapping = TextWrapping.Wrap;
                t.TextTrimming = TextTrimming.CharacterEllipsis;
                t.MaxHeight = 46;
                t.FontSize = 14;
                t.Margin = new Thickness(0, 10, 0, 0);
                t.Foreground = Brushes.White;
                t.Text = text;
                ThumbsList.Children.Add(t);

                var thumb = new DWMThumbnailControl(this);
                thumb.Width = 120;
                thumb.Height = 120;
                thumb.Source = current;
                ThumbsList.Children.Add(thumb);
            });

            this.Width++;
            this.Width--;
        }

        /// <summary>
        /// Thumbses the list mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void ThumbsListMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var c = e.GetPosition(ThumbsList);
            foreach (var thumb in ThumbsList.Children.OfType<DWMThumbnailControl>()) {
                var transform = thumb.TransformToVisual(ThumbsList);
                var p = transform.Transform(new System.Windows.Point(0, 0));
                if (c.Y > p.Y && c.Y < p.Y + thumb.Height && c.X > p.X && c.X < p.X + thumb.Width) {
                    if (WinAPI.IsIconic(thumb.Source))
                        WinAPI.ShowWindow(thumb.Source, WindowShowStyle.Restore);
                    else
                        WinAPI.ShowWindow(thumb.Source, WindowShowStyle.Show);
                    WinAPI.SetForegroundWindow(thumb.Source);
                    break;
                }
            }
        }

        /// <summary>
        /// Opens the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void OpenToolbar() {
            base.OpenToolbar();

            InitializeThumbnails();
            foreach (var thumb in ThumbsList.Children.OfType<DWMThumbnailControl>()) {
                thumb.Width++;
                thumb.Width--;
                thumb.Height++;
                thumb.Height--;
            }
            Width++;
            Width--;
            Height++;
            Height--;
        }

        /// <summary>
        /// Closes the toolbar.
        /// </summary>
        /// <remarks>...</remarks>
        public override void CloseToolbar() {
            base.CloseToolbar();

            ThumbsList.Children.Clear();
        }
    }
}
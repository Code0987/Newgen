using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns.Media.Animation;
using libns.Media.Imaging;
using libns.Threading;

namespace Newgen {

    public partial class BackgroundCanvas : UserControl {

        public BackgroundCanvas() {
            InitializeComponent();

            Background = Settings.Current.BackgroundColor.ToBrush();

            if (Settings.Current.UseBgImage)
                try {
                    Background = new ImageBrush(Api.BgImage.ToBitmapSource());
                }
                catch {
                    MessageBox.Show(Api.MSG_ER_FEATURE, "Error");
                }
        }

        private void AnimateImage() {
            var w = (int)SystemParameters.PrimaryScreenWidth;
            var h = (int)SystemParameters.PrimaryScreenHeight;

            var image = new System.Drawing.Bitmap(w, h);
            var graphics = System.Drawing.Graphics.FromImage(image);

            graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(w, h));
            graphics.Dispose();

            BitmapSource result;

            if (image == null) {
                result = null;
                return;
            }
            else {
                result = Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }

            AnimationImage.Source = result;
            AnimationImage.Width = w;
            AnimationImage.Height = h;
            AnimationImage2.Width = w;
            AnimationImage2.Height = h;

            AnimationExtensions.Animate(
                AnimationImage, Canvas.LeftProperty,
                750, 0, -SystemParameters.PrimaryScreenWidth,
                0.7, 0.3
                );

            ThreadingExtensions.LazyInvokeThreadSafe(new Action(() => {
                AnimationCanvas.Visibility = Visibility.Collapsed;

                {
                    if (Settings.Current.SlideShowImages.Count > 1) {
                        AnimationCanvas.Visibility = Visibility.Visible;

                        AnimationImage.Source = new BitmapImage(new Uri(
                                                          Settings.Current.SlideShowImages[(new Random()).Next(0, Settings.Current.SlideShowImages.Count - 1)], UriKind.Absolute)
                                                          );

                        this.Dispatcher.RunFor(() => {
                            try {
                                AnimationImage2.Source = new BitmapImage(new Uri(
                                                                  Settings.Current.SlideShowImages[(new Random()).Next(0, Settings.Current.SlideShowImages.Count - 1)], UriKind.Absolute)
                                                                  );

                                AnimationExtensions.Animate(
                                    AnimationImage, Canvas.LeftProperty,
                                    750, 0, -SystemParameters.PrimaryScreenWidth, null,
                                    0.7, 0.3, false, 1, null, FillBehavior.HoldEnd,
                                    (a, b) => {
                                        try {
                                            AnimationImage.Source = AnimationImage2.Source;
                                        }
                                        catch { }
                                    }
                                    , null);
                            }
                            catch { }
                        }, -1, Settings.Current.SlideShowTime * 750);
                    }
                }
            }), 1200);
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            AnimateImage();
        }
    }
}
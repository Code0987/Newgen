using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using libns.Media.Animation;
using libns.Threading;

namespace BackgroundPackage {

    public class ImageSlideShowController {
        private Image image2;
        private Image image1;
        private DispatcherTimer timer;

        public void Start(Settings settings, Canvas artBoard) {
            artBoard.Children.Clear();

            image2 = new Image() { Stretch = System.Windows.Media.Stretch.Fill };
            artBoard.Children.Add(image2);
            image2.SetValue(Canvas.LeftProperty, 0);

            image1 = new Image() { Stretch = System.Windows.Media.Stretch.Fill };
            artBoard.Children.Add(image1);
            image1.SetValue(Canvas.LeftProperty, 0);
            
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

            image1.Source = result;
            image1.Width = w;
            image1.Height = h;
            image2.Width = w;
            image2.Height = h;

            AnimationExtensions.Animate(
                image1, Canvas.LeftProperty,
                750, 0, -SystemParameters.PrimaryScreenWidth,
                0.7, 0.3
                );
            
            ThreadingExtensions.LazyInvokeThreadSafe(new Action(() => {
                artBoard.Visibility = Visibility.Collapsed;

                {
                    if (settings.SlideShowImages.Count > 1) {
                        artBoard.Visibility = Visibility.Visible;

                        image1.Source = new BitmapImage(new Uri(
                            settings.SlideShowImages[(new Random()).Next(0, settings.SlideShowImages.Count - 1)], UriKind.Absolute)
                            );

                        timer = Application.Current.Dispatcher.RunFor(() => {
                            try {
                                image2.Source = new BitmapImage(new Uri(
                                    settings.SlideShowImages[(new Random()).Next(0, settings.SlideShowImages.Count - 1)], UriKind.Absolute)
                                    );

                                AnimationExtensions.Animate(
                                    image1, Canvas.LeftProperty,
                                    750, 0, -SystemParameters.PrimaryScreenWidth, null,
                                    0.7, 0.3, false, 1, null, FillBehavior.HoldEnd,
                                    (a, b) => {
                                        try {
                                            image1.Source = image2.Source;
                                        }
                                        catch { }
                                    }
                                    , null);
                            }
                            catch { }
                        }, -1, settings.SlideShowTime * 750);
                    }
                }
            }), 1200);
        }

        public void Stop(Canvas artBoard) {
            if (timer != null) {
                timer.Stop();
                if (artBoard.Children.Contains(image1))
                    artBoard.Children.Remove(image1);
                image1 = null;
                if (artBoard.Children.Contains(image2))
                    artBoard.Children.Remove(image2);
                image2 = null;
            }
        }
    }
}
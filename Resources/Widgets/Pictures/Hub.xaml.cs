using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newgen.Base;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        public event EventHandler Close;

        private Random random = new Random();
        private List<Category> categories;
        private List<string> images;
        private string currentimg;

        public Hub()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Delay(new Action(() =>
            {
                var tuple = Widget.Settings.GetImages();
                categories = tuple.Item1;
                images = tuple.Item2;
                foreach(var category in categories)
                {
                    var control = new PicturesCategoryControl();
                    control.Initialize(category);
                    PicturessPanel.Children.Add(control);
                }
            }), 500);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close(this, EventArgs.Empty);
        }

        private void FWButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for(int im = 0; im < images.Count; im++)
                    if(currentimg == images[im])
                        OpenImg(new Uri(images[random.Next(im, images.Count)], UriKind.Absolute));
            }
            catch { }
        }

        private void BkButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                for(int im = 0; im < images.Count; im++)
                    if(currentimg == images[im])
                        OpenImg(new Uri(images[random.Next(0, im)], UriKind.Absolute));
            }
            catch { }
        }

        private void VpBackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImgV.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        internal void OpenImg(Uri source)
        {
            try
            {
                Helper.Animate(this.ImgV, OpacityProperty, 100, 0);

                currentimg = source.OriginalString;
                ImgV.Opacity = 0;
                ImgV.Visibility = Visibility.Visible;
                Vw_Img.Source = new System.Windows.Media.Imaging.BitmapImage(source);

                Helper.Animate(this.ImgV, OpacityProperty, 250, 1);
            }
            catch { }
        }
    }
}
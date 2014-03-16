using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using libns.Native;

namespace Newgen
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window
    {
        public WaitWindow()
        {
            InitializeComponent();

            var handle = new WindowInteropHelper(this).Handle;

            WinAPI.RemoveFromAeroPeek(handle);
            WinAPI.RemoveFromAltTab(handle);
            WinAPI.RemoveFromFlip3D(handle);

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Top = 0;
            this.Left = 0;
        }

        public void Start(bool showcircles)
        {
            if (showcircles)
            {
                this.Ellipse_Cir.Visibility = Visibility.Visible;
                this.Ellipse_Cir2.Visibility = Visibility.Visible;
                this.Ellipse_Cir3.Visibility = Visibility.Visible;
                this.Ellipse_Cir.BeginStoryboard((Storyboard)this.Ellipse_Cir.Resources["Start"]);
                this.Ellipse_Cir2.BeginStoryboard((Storyboard)this.Ellipse_Cir2.Resources["Start"]);
                this.Ellipse_Cir3.BeginStoryboard((Storyboard)this.Ellipse_Cir3.Resources["Start"]);
            }
            else
            {
                this.Ellipse_Cir.Visibility = Visibility.Collapsed;
                this.Ellipse_Cir2.Visibility = Visibility.Collapsed;
                this.Ellipse_Cir3.Visibility = Visibility.Collapsed;
            }
        }

        public void Stop()
        {
            this.BeginStoryboard((Storyboard)this.Resources["Stop"]);

            if (this.Ellipse_Cir.Visibility == Visibility.Visible) this.Ellipse_Cir.BeginStoryboard((Storyboard)this.Ellipse_Cir.Resources["Stop"]);
            if (this.Ellipse_Cir2.Visibility == Visibility.Visible) this.Ellipse_Cir2.BeginStoryboard((Storyboard)this.Ellipse_Cir2.Resources["Stop"]);
            if (this.Ellipse_Cir3.Visibility == Visibility.Visible) this.Ellipse_Cir3.BeginStoryboard((Storyboard)this.Ellipse_Cir3.Resources["Stop"]);
        }
    }
}
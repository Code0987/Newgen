using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Newgen.Base;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for DesktopWindow.xaml
    /// </summary>
    public partial class DesktopWindow : HubWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopWindow"/> class.
        /// </summary>
        public DesktopWindow()
        {
            this.InitializeComponent();

            this.Animation = AnimationType.Custom;
        }

        /// <summary>
        /// Handles the SourceInitialized event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                this.Left = 0;
                this.Top = 0;
                this.Width = SystemParameters.PrimaryScreenWidth;
                this.Height = SystemParameters.PrimaryScreenHeight;

                int w = (int)SystemParameters.PrimaryScreenWidth;
                int h = (int)SystemParameters.PrimaryScreenHeight;

                this.DesktopImage.Width = w;
                this.DesktopImage.Height = h;

                // Canvas.SetLeft(this.DesktopImage, -this.Width);
                Canvas.SetTop(this.DesktopImage, 0);

                var s = (Storyboard)Resources["Anim"];
                (s.Children[0] as DoubleAnimation).To = 0;
                // s.Begin();
            }
            catch { }

            this.Animation = AnimationType.Internal;

            Helper.Delay(() =>
            {
                var NewgenWindow = (Window)(Application.Current.MainWindow);

                NewgenWindow.Hide();
                this.Close();
            }, 150);
        }

        /// <summary>
        /// Storyboards the completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void StoryboardCompleted(object sender, EventArgs e)
        {
            var NewgenWindow = (Window)(Application.Current.MainWindow);

            NewgenWindow.Hide();

            this.Visibility = Visibility.Collapsed;

            this.Close();
        }
    }
}
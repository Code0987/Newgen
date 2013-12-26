using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newgen.Base;

namespace Newgen.Hubs
{
    /// <summary>
    /// Interaction logic for NewgenHelp.xaml
    /// </summary>
    public partial class NewgenHelp : HubWindow
    {
        Dictionary<string, Uri> help;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tour"/> class.
        /// </summary>
        public NewgenHelp() :
            base()
        {
            InitializeComponent();

            this.InitHelp();
        }

        /// <summary>
        /// Handles the Click event of the CancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the NextButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.TabControl.SelectedIndex += 1;
        }

        /// <summary>
        /// Handles the Click event of the PreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.TabControl.SelectedIndex <= 0)
                this.TabControl.SelectedIndex = 0;
            else
                this.TabControl.SelectedIndex -= 1;
        }

        private void InitHelp()
        {
            this.help = new Dictionary<string, Uri>();

            this.help.Add("Welcome",
                new Uri("/Newgen;component/Resources/Help/Slide1.png", UriKind.Relative));
            this.help.Add("About",
                new Uri("/Newgen;component/Resources/Help/Slide2.png", UriKind.Relative));
            this.help.Add("Index",
                new Uri("/Newgen;component/Resources/Help/Slide3.png", UriKind.Relative));
            this.help.Add("What's new ?",
                new Uri("/Newgen;component/Resources/Help/Slide4.png", UriKind.Relative));
            this.help.Add("Overview",
                new Uri("/Newgen;component/Resources/Help/Slide5.png", UriKind.Relative));
            this.help.Add("StartScreen",
                new Uri("/Newgen;component/Resources/Help/Slide6.png", UriKind.Relative));
            this.help.Add("StartScreen Groups",
                new Uri("/Newgen;component/Resources/Help/Slide7.png", UriKind.Relative));
            this.help.Add("Shortcuts",
                new Uri("/Newgen;component/Resources/Help/Slide8.png", UriKind.Relative));
            this.help.Add("StartBar",
                new Uri("/Newgen;component/Resources/Help/Slide9.png", UriKind.Relative));
            this.help.Add("UserTile, ThumbsBar, TilesBar",
                new Uri("/Newgen;component/Resources/Help/Slide10.png", UriKind.Relative));
            this.help.Add("Tiles and App Widget",
                new Uri("/Newgen;component/Resources/Help/Slide11.png", UriKind.Relative));
            this.help.Add("Setting, Part 1",
                new Uri("/Newgen;component/Resources/Help/Slide12.png", UriKind.Relative));
            this.help.Add("Setting, Part 2",
                new Uri("/Newgen;component/Resources/Help/Slide13.png", UriKind.Relative));
            this.help.Add("Setting, Part 3",
                new Uri("/Newgen;component/Resources/Help/Slide14.png", UriKind.Relative));
            this.help.Add("Setting, Part 4",
                new Uri("/Newgen;component/Resources/Help/Slide15.png", UriKind.Relative));
            this.help.Add("Activating Newgen",
                new Uri("/Newgen;component/Resources/Help/Slide16.png", UriKind.Relative));
            this.help.Add("How to's and what if's, Part 1",
                new Uri("/Newgen;component/Resources/Help/Slide17.png", UriKind.Relative));
            this.help.Add("How to's and what if's, Part 2",
                new Uri("/Newgen;component/Resources/Help/Slide18.png", UriKind.Relative));
            this.help.Add("How to's and what if's, Part 3",
                new Uri("/Newgen;component/Resources/Help/Slide19.png", UriKind.Relative));
            this.help.Add("How to's and what if's, Part 4",
                new Uri("/Newgen;component/Resources/Help/Slide20.png", UriKind.Relative));
            this.help.Add("How to's and what if's, Part 5",
                new Uri("/Newgen;component/Resources/Help/Slide21.png", UriKind.Relative));
            this.help.Add("Know more",
                new Uri("/Newgen;component/Resources/Help/Slide22.png", UriKind.Relative));

            foreach (var i in this.help)
            {
                this.TabControl.Items.Add(new TabItem()
                {
                    Header = i.Key,
                    Content = new Image()
                    {
                        Height = 650,
                        Width = 870,
                        Stretch = Stretch.Fill,
                        Source = new BitmapImage(i.Value)
                    }
                });
            }

            this.TabControl.SelectedIndex = 2;
        }
    }
}
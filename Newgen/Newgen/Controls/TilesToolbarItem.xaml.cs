using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarItem.xaml
    /// </summary>
    public partial class TilesToolbarItem : UserControl
    {
        private bool mousePressed;

        public TilesToolbarItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; ToolTip = value; }
        }

        public ImageSource Icon
        {
            get { return IconImage.Source; }
            set { IconImage.Source = value; }
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePressed = true;
            Background = new SolidColorBrush(Color.FromArgb(64,0,0,0));
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mousePressed = false;
            Background = Brushes.Transparent;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                mousePressed = false;
                Background = Brushes.Transparent;
            }
        }
    }
}
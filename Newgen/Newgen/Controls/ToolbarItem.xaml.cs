using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Newgen.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarItem.xaml
    /// </summary>
    public partial class ToolbarItem : UserControl
    {
        private bool mousePressed;

        public ToolbarItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
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

            if (App.Settings.EnableOutOfBoxExperience)
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 5.0,
                    ShadowDepth = 2.0,
                    Opacity = 0.5
                };
            else
                Effect = null;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (App.Settings.EnableOutOfBoxExperience)
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 20.0,
                    ShadowDepth = 1.0,
                    Opacity = 0.9,
                    Color = App.Settings.ToolbarBackgroundColor
                };
        }
    }
}
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Store
{
    /// <summary>
    /// Interaction logic for StoreItem.xaml
    /// </summary>
    public partial class StoreItem : UserControl
    {
        public string WidgetURL { get; set; }

        public string ID { get; set; }

        public string Title
        {
            get
            {
                return this.TitleTextBlock.Text;
            }
            set
            {
                this.TitleTextBlock.Text = value;
            }
        }

        public string Author
        {
            get
            {
                return this.AuthorTextBlock.Text;
            }
            set
            {
                this.AuthorTextBlock.Text = value;
            }
        }

        public string AuthorWeb { get; set; }

        public string Size { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public ImageSource Icon
        {
            get
            {
                return this.IconImage.Source;
            }
            set
            {
                this.IconImage.Source = value;
            }
        }

        public StoreItem()
        {
            InitializeComponent();
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (base.Resources["MouseDownAnim"] as Storyboard).Begin();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (base.Resources["MouseUpAnim"] as Storyboard).Begin();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
using System.Windows.Controls;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for PicturesCategoryControl.xaml
    /// </summary>
    public partial class PicturesCategoryControl : UserControl
    {
        public PicturesCategoryControl()
        {
            InitializeComponent();
        }

        public void Initialize(Category category)
        {
            Header.Text = category.Title.ToString();
            foreach (var file in category.Files)
            {
                var thumbnail = new ThumbnailControl();
                thumbnail.Initialize(file);
                ThumbnailsHost.Children.Add(thumbnail);
            }
        }
    }
}
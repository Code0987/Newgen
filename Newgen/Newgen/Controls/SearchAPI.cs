using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Newgen.Controls.Search
{
    /// <summary>
    /// ImageView displays image files using themselves as their icons.
    /// In order to write our own visual tree of a view, we should override its
    /// DefaultStyleKey and ItemContainerDefaultKey. DefaultStyleKey specifies
    /// the style key of ListView; ItemContainerDefaultKey specifies the style
    /// key of ListViewItem.
    /// </summary>
    public class ImageView : ViewBase
    {
        #region DefaultStyleKey

        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ImageView"); }
        }

        #endregion DefaultStyleKey

        #region ItemContainerDefaultStyleKey

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "ImageViewItem"); }
        }

        #endregion ItemContainerDefaultStyleKey
    }

    /// <summary>
    /// Represents a single item in the search results.
    /// This item will store the file's thumbnail, display name,
    /// and some properties (that will be displayed in the properties pane)
    /// </summary>
    public class SearchItem
    {
        public string Name { get; set; }

        public BitmapSource Thumbnail { get; set; }

        public string ParsingName { get; set; }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using libns.Media.Imaging;
using Newgen.Resources;

namespace Newgen {

    /// <summary>
    /// BackgroundCanvas. Provides background art board.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class BackgroundCanvas : Canvas {

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCanvas"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public BackgroundCanvas() {
            InitializeComponent();

            Background = Settings.Current.BackgroundColor.ToBrush();

            if (Settings.Current.UseBgImage)
                try {
                    Background = new ImageBrush(Settings.Current.StartScreenBackgroundImage.ToBitmapSource());
                }
                catch /* Eat */ {
                    MessageBox.Show(Api.MSG_ER_FEATURE, Definitions.Error);
                }

            try {
                Api.BackgroundCanvas = this;
            }
            catch /* Eat */ {
                MessageBox.Show(Api.MSG_ER_FEATURE, Definitions.Error);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor4.xaml
    /// </summary>
    public partial class SettingsEditor4 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor4"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor4() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                TilesSizeSlider.Value = (double)Settings.Current.MinTileHeight;
                TilesSizeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSizeScale_ValueChanged);

                TilesSpacingSlider.Value = Settings.Current.TileSpacing;
                TilesSpacingSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSpacingSlider_ValueChanged);

            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        void TilesSpacingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (TilesSpacingSlider.Value > 0)
                Settings.Current.TileSpacing = Convert.ToInt32(TilesSpacingSlider.Value);
        }
        
        void TilesSizeScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Settings.Current.MinTileWidth = TilesSizeSlider.Value;
            Settings.Current.MinTileHeight = TilesSizeSlider.Value;
        }

    }
}
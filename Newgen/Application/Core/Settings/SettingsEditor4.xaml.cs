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
using iFramework.Security.Licensing;

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
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
                EnableWidgetLock.IsChecked = Settings.Current.IsTilesLockEnabled;

                double tilesheight = (App.Screen).TilesControl.ActualHeight - (20);
                double rh = ((tilesheight - Settings.Current.TileSpacing * 2) / 3);

                TilesSizeScale.Maximum = (double)rh;
                TilesSizeScale.Minimum = 90.0;
                TilesSizeScale.Value = (double)Settings.Current.MinTileHeight;
                TilesSizeScale.ValueChanged += new RoutedPropertyChangedEventHandler<double>(TilesSizeScale_ValueChanged);
                ValTilesSpacing.Text = Settings.Current.TileSpacing.ToString();
                ValTilesSpacing.TextChanged += new TextChangedEventHandler(ValTilesSpacing_TextChanged);

            }
            catch /* Eat */ { /* Tasty ? */ }
        }

        private void Apply() {
            Settings.Current.IsTilesLockEnabled = (bool)EnableWidgetLock.IsChecked;

        }



        private void ValTilesSpacing_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                if (string.IsNullOrEmpty(ValTilesSpacing.Text) || string.IsNullOrWhiteSpace(ValTilesSpacing.Text)) {
                    return;
                }

                int anInteger;
                anInteger = Convert.ToInt32(ValTilesSpacing.Text);
                anInteger = int.Parse(ValTilesSpacing.Text);
                bool valid = anInteger > 0;
                if (valid) {
                    Settings.Current.TileSpacing = anInteger;
                }
            }
            catch {
                ValTilesSpacing.Text = Settings.Current.LockScreenTime.ToString();
            }
        }
        private void TilesSizeScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Settings.Current.MinTileWidth = TilesSizeScale.Value;
            Settings.Current.MinTileHeight = TilesSizeScale.Value;
        }

    }
}
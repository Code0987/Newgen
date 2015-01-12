using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using libns;
using libns.Threading;
using Newgen;

namespace BackgroundPackage {

    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class Tile : Border {

        /// <summary>
        /// The package
        /// </summary>
        private Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile" /> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

            InitializeComponent();

            SettingsEditor.DataContext = package.CustomizedSettings;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Load() {
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        }
    }
}
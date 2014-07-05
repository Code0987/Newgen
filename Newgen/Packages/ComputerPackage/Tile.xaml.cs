using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComputerPackage
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : Border {
        /// <summary>
        /// The package
        /// </summary>
        private Package package;
        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <remarks>...</remarks>
        public Tile(Package package) {
            this.package = package;

            InitializeComponent();
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

        /// <summary>
        /// Handles the <see cref="E:UserControlMouseLeftButtonUp" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnUserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualBasic.Devices;

namespace CPUMonitorPackage
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : Border
    {
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

            DataContext = new CPURealtimePerformanceDataContext();
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
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newgen;

namespace ClockPackage
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
        /// The timer
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile" /> class.
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
        public void Load()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OntimerTick;
            timer.Start();
            OntimerTick(null, EventArgs.Empty);
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Unload() {
            timer.Tick -= OntimerTick;
            timer.Stop();
        }

        /// <summary>
        /// Handles the <see cref="E:Tick" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OntimerTick(object sender, EventArgs e)
        {
            if (Api.Settings.TimeMode == TimeMode.H24)
            {
                Hours.Text = DateTime.Now.ToString("HH");
                Minutes.Text = DateTime.Now.ToString("mm");
                AmPm.Text = "";
            }
            else
            {
                Hours.Text = DateTime.Now.ToString(" h");
                Minutes.Text = DateTime.Now.ToString("mm");
                AmPm.Text = DateTime.Now.ToString("tt");
            }

            var power = SystemInformation.PowerStatus;
            if (power.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
                BatteryIcon.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                BatteryIcon.Visibility = System.Windows.Visibility.Visible;
                var iconNumber = (int)(power.BatteryLifePercent * 10) + 1;
                if (iconNumber >= 10)
                    BatteryIcon.Source = new BitmapImage(new Uri("Resources/batt10.png", UriKind.Relative));
                else
                    BatteryIcon.Source = new BitmapImage(new Uri(string.Format("Resources/batt{0}.png", iconNumber), UriKind.Relative));
            }

            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Date.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
        }
    }
}
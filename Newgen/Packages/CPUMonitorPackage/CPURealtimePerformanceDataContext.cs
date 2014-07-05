using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using OxyPlot;
using OxyPlot.Series;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace CPUMonitorPackage {
    /// <summary>
    /// Class CPURealtimePerformanceDataContext.
    /// </summary>
    /// <remarks>...</remarks>
    public class CPURealtimePerformanceDataContext : INotifyPropertyChanged {

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>...</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// The counter_cpu
        /// </summary>
        private PerformanceCounter counter_cpu;

        /// <summary>
        /// The maximum points in plot
        /// </summary>
        private List<PersistantPoint> cpuPoints, memoryPoints;

        /// <summary>
        /// The osinfo
        /// </summary>
        private ComputerInfo osinfo;

        /// <summary>
        /// Gets the plot model.
        /// </summary>
        /// <value>The plot model.</value>
        /// <remarks>...</remarks>
        public PlotModel PlotModel { get; private set; }

        /// <summary>
        /// Gets the total number of points.
        /// </summary>
        /// <value>The total number of points.</value>
        /// <remarks>...</remarks>
        public int TotalNumberOfPoints { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CPURealtimePerformanceDataContext"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public CPURealtimePerformanceDataContext() {
            timer = new Timer(OnTimerElapsed);

            try {
                counter_cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                osinfo = new ComputerInfo();
            }
            catch /* Eat */ { /* Tasty ?*/ }

            SetupModel();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CPURealtimePerformanceDataContext"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~CPURealtimePerformanceDataContext() {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();

            try {
                counter_cpu.Close();
            }
            catch /* Eat */ { /* Tasty ?*/ }

            counter_cpu = null;
            osinfo = null;
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <remarks>...</remarks>
        protected void RaisePropertyChanged(string property) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        /// Called when [timer elapsed].
        /// </summary>
        /// <param name="state">The state.</param>
        /// <remarks>...</remarks>
        private void OnTimerElapsed(object state) {
            lock (PlotModel.SyncRoot) {
                Update();
            }

            PlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// Setups the model.
        /// </summary>
        /// <remarks>...</remarks>
        private void SetupModel() {
            cpuPoints = new List<PersistantPoint>();
            memoryPoints = new List<PersistantPoint>();
            for (var i = 0; i < 50; i++) {
                cpuPoints.Add(new PersistantPoint(i, 0));
                memoryPoints.Add(new PersistantPoint(i, 0));
            }

            timer.Change(Timeout.Infinite, Timeout.Infinite);

            PlotModel = new PlotModel() {
                Background = OxyColors.Transparent,
                IsLegendVisible = true,
                PlotMargins = new OxyThickness(-10),
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBackground = OxyColors.Transparent,
                LegendSymbolPlacement = LegendSymbolPlacement.Right,
                LegendItemAlignment = OxyPlot.HorizontalAlignment.Center
            };

            PlotModel.Series.Add(new AreaSeries { Color = OxyColors.MediumVioletRed, StrokeThickness = 1, Title = "Memory" });
            PlotModel.Series.Add(new AreaSeries { Color = OxyColors.Blue, StrokeThickness = 1, Title = "CPU" });

            RaisePropertyChanged("PlotModel");

            timer.Change(1000, 3000);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <remarks>...</remarks>
        private void Update() {
            var n = 0;

            // For Memory usage
            {
                var i = 0;
                for (; i < memoryPoints.Count - 1; i++)
                    memoryPoints[i].Y = memoryPoints[i + 1].Y;
                memoryPoints[i].Y = 100.0 * osinfo.AvailablePhysicalMemory / (double)osinfo.TotalPhysicalMemory;

                var s = (AreaSeries)PlotModel.Series[0];
                s.Points.Clear();
                s.Points.Add(new DataPoint(0, 0));
                memoryPoints.ForEach(f => s.Points.Add(new DataPoint(f.X, f.Y)));
                s.Points.Add(new DataPoint(cpuPoints.Count, 0));

                n += s.Points.Count;
            }

            // For CPU usage
            {
                var i = 0;
                for (; i < cpuPoints.Count - 1; i++)
                    cpuPoints[i].Y = cpuPoints[i + 1].Y;
                cpuPoints[i].Y = counter_cpu.NextValue();

                var s = (AreaSeries)PlotModel.Series[1];
                s.Points.Clear();
                s.Points.Add(new DataPoint(0, 0));
                cpuPoints.ForEach(f => s.Points.Add(new DataPoint(f.X, f.Y)));
                s.Points.Add(new DataPoint(cpuPoints.Count, 0));

                n += s.Points.Count;
            }

            if (TotalNumberOfPoints != n) {
                TotalNumberOfPoints = n;
                RaisePropertyChanged("TotalNumberOfPoints");
            }
        }

        /// <summary>
        /// Class PersistantPoint.
        /// </summary>
        /// <remarks>...</remarks>
        [DebuggerDisplay("({X}, {Y})")]
        public class PersistantPoint {
            /// <summary>
            /// Gets or sets the x.
            /// </summary>
            /// <value>The x.</value>
            /// <remarks>...</remarks>
            public double X { get; set; }
            /// <summary>
            /// Gets or sets the y.
            /// </summary>
            /// <value>The y.</value>
            /// <remarks>...</remarks>
            public double Y { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersistantPoint"/> class.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            /// <remarks>...</remarks>
            public PersistantPoint(double x, double y) {
                X = x;
                Y = y;
            }
        }
    }
}
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

namespace CPUMonitor
{
    /// <summary>
    /// Interaction logic for CPUMonitorWidget.xaml
    /// </summary>
    public partial class CPUMonitorWidget : UserControl
    {
        private PerformanceCounter counter_cpu;
        private ComputerInfo osinfo;
        private DispatcherTimer timer;

        public CPUMonitorWidget()
        {
            InitializeComponent();

            this.counter_cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            this.osinfo = new ComputerInfo();

            this.timer = new DispatcherTimer(TimeSpan.FromSeconds(3), DispatcherPriority.Background, Update, this.Dispatcher);
        }

        private void Update(object sender, EventArgs e)
        {
            this.TextBlock_CPU.Text = string.Format("CPU Used : {0:F2}%", this.counter_cpu.NextValue());
            this.TextBlock_Memory.Text = string.Format(
                "Memory : {0:F2}MB free of {1:F2}MB",
                (this.osinfo.AvailablePhysicalMemory >> 10) / 1024.0,
                (this.osinfo.TotalPhysicalMemory >> 10) / 1024.0);
            this.TextBlock_Network.Text = string.Format(
                "Network : {0:F2}MB sent / {1:F2}MB received",
                (NetworkInterface.GetAllNetworkInterfaces().Max(a => a.GetIPv4Statistics().BytesSent) >> 10) / 1024.0,
                (NetworkInterface.GetAllNetworkInterfaces().Max(a => a.GetIPv4Statistics().BytesReceived) >> 10) / 1024.0);
        }

        public void OnRemoved()
        {
            this.counter_cpu.Close();

            this.counter_cpu = null;
            this.osinfo = null;
        }
    }
}
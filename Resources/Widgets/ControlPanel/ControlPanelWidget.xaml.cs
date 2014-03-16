using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlPanel
{
    /// <summary>
    /// Interaction logic for ControlPanelWidget.xaml
    /// </summary>
    public partial class ControlPanelWidget : UserControl
    {
        public ControlPanelWidget()
        {
            InitializeComponent();
        }

        private void NativeItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("control.exe");
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("control.exe");
            //(new Hub()).ShowDialog();
        }
    }
}
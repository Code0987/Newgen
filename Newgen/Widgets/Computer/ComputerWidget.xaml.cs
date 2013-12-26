using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace Computer
{
    /// <summary>
    /// Interaction logic for ComputerWidget.xaml
    /// </summary>
    public partial class ComputerWidget : UserControl
    {
        public ComputerWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
        }
    }
}
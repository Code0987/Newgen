using System;
using System.Windows.Controls;
using System.Windows.Input;
using Newgen.Base;

namespace Video
{
    /// <summary>
    /// Interaction logic for VideoWidget.xaml
    /// </summary>
    public partial class VideoWidget : UserControl
    {
        private HubWindow hub;
        private Hub hubContent;

        public VideoWidget()
        {
            InitializeComponent();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*var file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Libraries\\Videos.library-ms";
            WinAPI.ShellExecute(IntPtr.Zero, "open", file, null, null, 1);*/
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            //hub.Topmost = true;
            hub.AllowsTransparency = true;
            hubContent = new Hub();
            hub.Content = hubContent;
            hubContent.Close += HubContentClose;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hub.ShowDialog();
        }

        private void HubContentClose(object sender, EventArgs e)
        {
            hubContent.Close -= HubContentClose;
            hub.Close();
        }

        private void MenuItem_AddLookupFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            Widget.Settings.LookupDirectories.Add(dlg.SelectedPath);
        }

        private void MenuItem_ClearLookupFolders_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Widget.Settings.LookupDirectories.Clear();
        }
    }
}
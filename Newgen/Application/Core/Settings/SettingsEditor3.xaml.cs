using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns.Native;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor3.xaml
    /// </summary>
    public partial class SettingsEditor3 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor3"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor3() {
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
                UpdateTaskBarPEXL();
            }
            catch /* Eat */ { /* Tasty ? */ }
        }


        private bool isLoadingTaskBarDatadone = false;

        private void UpdateTaskBarPEXL() {
            try {
                return;
                //TODO:StartSystem.tbtimer.Stop();

                ListBox_ItemsToExclude.Items.Clear();

                WinAPI.ForEachVisibleWindow(
    ((HwndSource)HwndSource.FromVisual(this)).Handle,
    (current, text) => {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var fip = new FileInfo(WinAPI.GetProcessPath(current));
        ListBox_ItemsToExclude.Items.Add(new TaskBarProcessExclusionData() {
            ProcessName = fip.Name,
            Icon = InternalHelper.GetThumbnail(WinAPI.GetProcessPath(current)) as BitmapSource
        });
    });

                var addeditems = ListBox_ItemsToExclude.Items.OfType<TaskBarProcessExclusionData>().ToList();

                foreach (var item in Settings.Current.TaskBarProcessExclusionList) {
                    try {
                        var existcount = 0;
                        foreach (var item2 in addeditems) {
                            if (item2.ProcessName == item) { existcount++; ListBox_ItemsToExclude.SelectedItems.Add(item2); }
                        }
                        if (existcount <= 0) {
                            var data = new TaskBarProcessExclusionData() {
                                ProcessName = item
                            };
                            ListBox_ItemsToExclude.Items.Add(data);
                            ListBox_ItemsToExclude.SelectedItems.Add(data);
                        }
                    }
                    catch { }
                }

                isLoadingTaskBarDatadone = true;
            }
            catch { }
        }

        private void ListBox_ItemsToExclude_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (isLoadingTaskBarDatadone) {

                //SavePEXL();
            }
        }

        private void SavePEXL() {
            try {
                Settings.Current.TaskBarProcessExclusionList.Clear();

                foreach (var item in ListBox_ItemsToExclude.SelectedItems.OfType<TaskBarProcessExclusionData>()) {
                    Settings.Current.TaskBarProcessExclusionList.Add(item.ProcessName);
                }
            }
            catch { }

            //try {
            //    foreach (string item in Settings.Current.TaskBarProcessExclusionList) {
            //        foreach (Window wnd in App.Current.Windows) {
            //            if (wnd is StartSystem) {
            //                List<StartBarItem> items = ((StartSystem)wnd).Icons.Children.OfType<StartBarItem>().ToList();

            //                foreach (StartBarItem item2 in items) {
            //                    if (WinAPI.GetProcessPath(item2.Handles[0]).Contains(item))
            //                        ((StartSystem)wnd).RemoveIcon(item2);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch { }
        }

        private void Button_AddPEXL_Click(object sender, RoutedEventArgs e) {

            //try {
            //    var dialog = new OpenFileDialog();
            //    dialog.Filter = "Executable Files|*.exe";
            //    if (!(bool)dialog.ShowDialog())
            //        return;

            // FileInfo fip = new FileInfo(dialog.FileName);

            // TaskBarProcessExclusionData data = new TaskBarProcessExclusionData() { ProcessName =
            // fip.Name, Icon = IconExtractor.GetIcon(dialog.FileName) };

            // ListBox_ItemsToExclude.Items.Add(data);

            //    ListBox_ItemsToExclude.SelectedItems.Add(data);
            //}
            //catch (Exception) {
            //    Api.ShowErrorMessage("Cannot process your request.");
            //}
        }

    }
}
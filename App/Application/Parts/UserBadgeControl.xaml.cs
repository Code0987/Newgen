using Newgen.Resources;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using libns.Native;
using libns.Threading;
using Microsoft.Win32;
using libns.Media.Imaging;
using libns.Media.Animation;

namespace Newgen {

    /// <summary>
    /// Class UserBadgeControl.
    /// </summary>
    /// <remarks>...</remarks>
    public partial class UserBadgeControl : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBadgeControl" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public UserBadgeControl() {
            InitializeComponent();
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        /// <remarks>...</remarks>
        public void Reload() {
            this.InvokeAsyncThreadSafe(() => {
                if (!Settings.Current.IsUserTileEnabled)
                    this.Visibility = Visibility.Collapsed;
                else {
                    this.Visibility = Visibility.Visible;
                    this.Opacity = 0;
                    AnimationExtensions.Animate(this, OpacityProperty, 300, 1);
                    var userimagefile = System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp";
                    if (File.Exists(userimagefile))
                        File.Copy(userimagefile, Settings.Current.StartScreenUserImage, true);
                    UsernameText.Text = Environment.UserName;
                    UserImage.Source = (Settings.Current.StartScreenUserImage).ToBitmapSource() ?? UserImage.Source;
                }
            });
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_CUT control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void MenuItem_CUT_Click(object sender, RoutedEventArgs e) {
                var dialog = new OpenFileDialog();
                dialog.Filter = Api.ImageFilter;
                if (!(bool)dialog.ShowDialog())
                    return;

                try {
                    File.Copy(dialog.FileName, Settings.Current.StartScreenUserImage, true);
                }
                catch /* Eat */ {
                    MessageBox.Show(Definitions.ProblemWithUserAccountImage, Definitions.Error);
                }
        }
        /// <summary>
        /// Handles the <see cref="E:LockTextMouseLeftButtonDown" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLockTextMouseLeftButtonDown(object sender, RoutedEventArgs e) {
            AnimationExtensions.Animate(LockText, OpacityProperty, 250, 0.3);
            WinAPI.LockWorkStation();
            AnimationExtensions.Animate(LockText, OpacityProperty, 500, 1);
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_Lock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void MenuItem_Lock_Click(object sender, RoutedEventArgs e) {
            try { WinAPI.LockWorkStation(); }
            catch { }
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_LogOff control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void MenuItem_LogOff_Click(object sender, RoutedEventArgs e) {
            try { WinAPI.ExitWindowsEx(0, 0); }
            catch { }
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_Restart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void MenuItem_Restart_Click(object sender, RoutedEventArgs e) {
            try { WinAPI.ExitWindowsEx(0x00000002, 0); }
            catch { }
        }

        /// <summary>
        /// Handles the Click event of the MenuItem_Shutdown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void MenuItem_Shutdown_Click(object sender, RoutedEventArgs e) {
            try { WinAPI.ExitWindowsEx(0x00000001, 0); }
            catch { }
        }
    }
}
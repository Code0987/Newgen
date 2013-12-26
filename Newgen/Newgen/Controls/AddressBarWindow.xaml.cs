using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newgen.Native;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for AddressBoxWindow.xaml
    /// </summary>
    public partial class AddressBarWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressBarWindow"/> class.
        /// </summary>
        public AddressBarWindow()
        {
            InitializeComponent();
            WinAPI.RemoveFromDWM(this);
        }

        /// <summary>
        /// Windows the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Top = -60;
            this.Left = 0;
            this.Background = new SolidColorBrush(App.Settings.ToolbarBackgroundColor);

            this.AddressBox.Focus();
            this.AddressBox.CaretIndex = AddressBox.Text.Length;
            this.OpenAnim();
        }

        /// <summary>
        /// Windows the key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.AddressBox.Text = string.Empty;
                this.CloseAnim();
            }
        }

        /// <summary>
        /// Addresses the box key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void AddressBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) this.CloseAnim();
        }

        /// <summary>
        /// Addresses the bar close anim completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void AddressBarCloseAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OpenAnim()
        {
            ((Storyboard)Resources["OpenAnim"]).Begin();
        }

        private void CloseAnim()
        {
            ((Storyboard)Resources["CloseAnim"]).Begin();
        }
    }
}
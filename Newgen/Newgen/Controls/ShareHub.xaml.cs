using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Newgen.Base;
using Newgen.Controls;
using Newgen.Core;

namespace Newgen.Windows
{
    /// <summary>
    /// Interaction logic for ShareHub.xaml
    /// </summary>
    public partial class ShareHub : HubWindow
    {
        private SocialProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareHub"/> class.
        /// </summary>
        public ShareHub()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the SourceInitialized event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.provider = new SocialProvider();
            this.provider.SignedIn += SocialProviderSignedIn;
            ThreadStart threadStarter = delegate { this.provider.SignIn(); };
            Thread thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Windows the closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ShareItem item in this.SharePanel.Children)
            {
                item.MouseLeftButtonUp -= ItemMouseLeftButtonUp;
            }
        }

        /// <summary>
        /// Socials the provider signed in.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SocialProviderSignedIn(object sender, EventArgs e)
        {
            provider.SignedIn -= SocialProviderSignedIn;
            ThreadStart threadStarter = delegate
            {
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    var friends = provider.GetFriends();
                    foreach (var friend in friends)
                    {
                        var item = new ShareItem();
                        item.Friend = friend;
                        var loadedFriend = App.WidgetManager.Widgets.Find(x => x.Path == friend.Id);
                        if (loadedFriend != null)
                        {
                            item.IsChecked = true;
                        }
                        item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                        this.SharePanel.Children.Add(item);
                    }
                });
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Items the mouse left button up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (ShareItem)sender;
            if (App.WidgetManager.IsWidgetLoaded(item.Friend.Name)) return;
            var widget = App.WidgetManager.CreateGeneratedWidget(item.Friend.Id, item.Friend.Name);
            App.WidgetManager.LoadWidget(widget);
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
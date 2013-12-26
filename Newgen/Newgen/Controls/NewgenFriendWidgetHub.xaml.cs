using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Newgen.Base;
using Newgen.Controls;

namespace Newgen.Core
{
    /// <summary>
    /// Interaction logic for NewgenFriendWidgetHub.xaml
    /// </summary>
    public partial class NewgenFriendWidgetHub : HubWindow
    {
        private string id;
        private SocialProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewgenFriendWidgetHub"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public NewgenFriendWidgetHub(string id)
        {
            this.id = id;

            InitializeComponent();
        }

        /// <summary>
        /// Handles the SourceInitialized event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            provider = new SocialProvider();
            ThreadStart threadStarter =
                delegate
                {
                    var info = provider.GetFriendInfoById(id);
                    this.Dispatcher.Invoke((Action)delegate
                                                        {
                                                            if (info != null)
                                                            {
                                                                Username.Text = info.Name;
                                                                Gender.Text = info.Gender;
                                                                Birthday.Text = info.Birthday;
                                                                Hometown.Text = info.Hometown;
                                                                Relationship.Text = info.Relationship;
                                                            }
                                                        });

                    this.Dispatcher.Invoke((Action)delegate
                    {
                        var entries = provider.GetFriendStream(id);
                        foreach (var entry in entries)
                        {
                            var item = new WallItem();
                            item.WallEntry = entry;
                            item.Order = FeedsPanel.Children.Count;
                            FeedsPanel.Children.Add(item);
                        }
                    });
                };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (File.Exists(E.CacheRoot + id + ".png"))
            {
                UserPic.Source = new BitmapImage(new Uri(E.CacheRoot + id + ".png"));
            }
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
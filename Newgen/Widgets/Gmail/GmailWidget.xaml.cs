using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Linq;
using Newgen.Base;

namespace Gmail
{
    /// <summary>
    /// Interaction logic for GmailWidget.xaml
    /// </summary>
    public partial class GmailWidget : UserControl
    {
        private Options optionsWindow;
        private WebClient webClient;
        private DispatcherTimer timer;
        private DispatcherTimer tileAnimTimer;

        public GmailWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(6);
            tileAnimTimer.Tick += TileAnimTimerTick;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(Widget.Settings.RefreshInterval);
            timer.Tick += TimerTick;

            webClient = new WebClient();
            if (string.IsNullOrEmpty(Widget.Settings.Username) || string.IsNullOrEmpty(Widget.Settings.Password))
            {
                Tip.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            var cred = new NetworkCredential();
            cred.UserName = Helper.Decrypt(Widget.Settings.Username);
            cred.Password = Helper.Decrypt(Widget.Settings.Password);
            webClient.Credentials = cred;
            webClient.Encoding = Encoding.UTF8;
            GetMail();

            timer.Start();
        }

        private void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            GetMail();
        }

        private int count;

        private void GetMail()
        {
            ThreadStart threadStarter = () =>
                                            {
                                                string content;
                                                try
                                                {
                                                    content = webClient.DownloadString("https://mail.google.com/mail/feed/atom");
                                                }
                                                catch
                                                {
                                                    return;
                                                }
                                                content = content.Replace("<feed version=\"0.3\" xmlns=\"http://purl.org/atom/ns#\">", "<feed>");
                                                var doc = XElement.Parse(content);
                                                this.Dispatcher.Invoke((Action)delegate
                                                                                    {
                                                                                        count = 0;
                                                                                        UnreadCount.Text = count.ToString();
                                                                                        foreach (var entry in doc.Descendants("entry"))
                                                                                        {
                                                                                            count++;
                                                                                            UnreadCount.Text = count.ToString();
                                                                                            Refresh(UnreadCount);
                                                                                            Thread.Sleep(500);
                                                                                        }

                                                                                        if (count > 0)
                                                                                        {
                                                                                            var firstEntry = doc.Descendants("entry").First();
                                                                                            From.Text =
                                                                                                firstEntry.Element("author").Element("name").Value;
                                                                                            Header.Text = firstEntry.Element("title").Value;
                                                                                            Body.Text = firstEntry.Element("summary").Value;

                                                                                            tileAnimTimer.Start();
                                                                                        }
                                                                                        else
                                                                                            tileAnimTimer.Stop();
                                                                                    });
                                            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static readonly Action EmptyDelegate = delegate() { };

        public static void Refresh(UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public void Unload()
        {
            timer.Tick -= TimerTick;
            timer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsWindow.ShowDialog();
        }

        private void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;
            if (string.IsNullOrEmpty(Widget.Settings.Username) || string.IsNullOrEmpty(Widget.Settings.Password))
            {
                Tip.Visibility = System.Windows.Visibility.Visible;
                From.Text = "";
                Header.Text = "";
                Body.Text = "";
                UnreadCount.Text = "0";
                return;
            }
            else
                Tip.Visibility = System.Windows.Visibility.Collapsed;
            var cred = new NetworkCredential();
            cred.UserName = Helper.Decrypt(Widget.Settings.Username);
            cred.Password = Helper.Decrypt(Widget.Settings.Password);
            webClient.Credentials = cred;
            webClient.Encoding = Encoding.UTF8;
            GetMail();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            GetMail();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (count > 0)
            {
                Newgen.Base.MessagingHelper.SendMessageToNewgen("URL", "https://mail.google.com/mail/#inbox");

                tileAnimTimer.Stop();
            }
        }
    }
}
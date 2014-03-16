using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Newgen.Base;
using OpenPop.Pop3;

namespace Hotmail
{
    /// <summary>
    /// Interaction logic for HotmailWidget.xaml
    /// </summary>
    public partial class HotmailWidget : UserControl
    {
        private Options optionsWindow;
        private WebClient webClient;
        private DispatcherTimer timer;
        private DispatcherTimer tileAnimTimer;

        public HotmailWidget()
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
            cred.UserName = Widget.Settings.Username;
            cred.Password = Widget.Settings.Password;
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

        private void GetMail()
        {
            // Connexion
            var pop3 = new Pop3Client();

            ThreadStart threadHotmail = () =>
                                            {
                                                try
                                                {
                                                    pop3.Connect(Widget.Settings.PopServer, 995, true);
                                                    pop3.Authenticate(Widget.Settings.Username, Widget.Settings.Password);

                                                    this.Dispatcher.Invoke((Action)delegate
                                                                                       {
                                                                                           // Récupération du nombre de messages
                                                                                           int NbTotalMsg = 0;
                                                                                           try
                                                                                           {
                                                                                               NbTotalMsg = pop3.GetMessageCount();
                                                                                           }
                                                                                           catch (Exception)
                                                                                           {
                                                                                           }
                                                                                           MailCount.Text = NbTotalMsg.ToString();

                                                                                           if (Widget.Settings.LastMsgCount <= 0 || Widget.Settings.LastMsgCount > NbTotalMsg)
                                                                                               Widget.Settings.LastMsgCount = NbTotalMsg;

                                                                                           UnreadCount.Visibility = Visibility.Visible;
                                                                                           UnreadCount.Text = getNewMsgNumber(NbTotalMsg, Widget.Settings.LastMsgCount).ToString();

                                                                                           HotmailLogo.Visibility = Visibility.Visible;
                                                                                       });
                                                }
                                                catch { }
                                            };
            var thread = new Thread(threadHotmail);
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

            Tip.Visibility = System.Windows.Visibility.Collapsed;
            var cred = new NetworkCredential();
            cred.UserName = Widget.Settings.Username;
            cred.Password = Widget.Settings.Password;
            webClient.Credentials = cred;
            webClient.Encoding = Encoding.UTF8;
            GetMail();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            GetMail();
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        internal static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (count > 0)
            //{
            //    WinAPI.ShellExecute(IntPtr.Zero, "open", "https://mail.google.com/mail/#inbox", null, null, 1);
            //    tileAnimTimer.Stop();
            //}

            if (Str2Int(UnreadCount.Text) > 0)
            {
                ShellExecute(IntPtr.Zero, "open", Widget.Settings.MailHttp, null, null, 1);
                tileAnimTimer.Stop();
                // Reset unread messages number and refresh last saved total one
                Widget.Settings.LastMsgCount = Str2Int(MailCount.Text);
                UnreadCount.Text = "0";
            }
        }

        private int getMsgNumber(String temp)
        {
            string txt_arg = "";
            ArrayList retourMsg = new ArrayList();
            foreach (char c in temp.ToCharArray())
            {
                if (c.CompareTo(' ') == 0)
                {
                    retourMsg.Add(txt_arg);
                    txt_arg = "";
                }
                else
                    txt_arg += c;
            }

            try
            {
                return Str2Int((String)retourMsg[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur de parsing: " + ex.ToString());
                return -1;
            }
        }

        private int getNewMsgNumber(int total, int old)
        {
            int retour = total - old;

            if (retour > 0)
            {
                return retour;
            }
            else
            {
                return 0;
            }
        }

        private int Str2Int(String temp)
        {
            int ret = int.Parse(temp);
            return ret;
        }
    }
}
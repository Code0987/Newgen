using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using libns;
using libns.Applied;
using libns.Language;
using libns.Native;
using libns.Threading;
using Newgen.Packages;
using Newgen.Packages.Notifications;

namespace Newgen {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class App : ExtendedApplication {

        /// <summary>
        /// The PTR hook
        /// </summary>
        private IntPtr ptrHook;

        /// <summary>
        /// The object keyboard process
        /// </summary>
        private WinAPI.HookProc objKeyboardProcess;
        
#if !DEBUG

        /// <summary>
        /// The tracker
        /// </summary>
        internal static ExtendedTracker tracker;

#endif

        /// <summary>
        /// Gets the app icon.
        /// </summary>
        /// <value>The app icon.</value>
        /// <remarks>...</remarks>
        public override System.Drawing.Icon AppIcon { get { return new System.Drawing.Icon(Application.GetResourceStream(new Uri("/Resources/Newgen_Icon.ico", UriKind.Relative)).Stream); } }

        /// <summary>
        /// Gets the app logo.
        /// </summary>
        /// <value>The app logo.</value>
        /// <remarks>...</remarks>
        public override System.Windows.Media.Imaging.BitmapSource AppLogo { get { return new BitmapImage(new Uri("/Resources/Newgen_Icon.ico", UriKind.Relative)); } }

        /// <summary>
        /// Gets the screen.
        /// </summary>
        /// <value>The start screen.</value>
        /// <remarks>...</remarks>
        public static Screen Screen { get { return Application.Current.MainWindow as Screen; } }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <remarks>...</remarks>
        [STAThread]
        public static void Main(string[] args) {
            var app = (new App());
            app.AddSingleInstanceHelper(f => (f as App).InitializeComponent());
            app.DisposeSafely();
        }

        /// <summary>
        /// Signals the external command line args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>The Boolean.</returns>
        /// <remarks>...</remarks>
        public override bool SignalExternalCommandLineArgs(IList<string> args) {
            var parser = CommandLineArgumentsParser.Parse(args.ToArray());

            if (Settings.IsProMode) {
            }

            return base.SignalExternalCommandLineArgs(args);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        public App() {
            Api.Init();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="App" /> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~App() {
            WinAPI.UnhookWindowsHookEx(this.ptrHook);
            objKeyboardProcess = null;
        }

        /// <summary>
        /// Handles the Startup event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void Application_Startup(object sender, StartupEventArgs e) {

            // Helper registration
            this.RegisterApplication();

            // Notifications
            this.AddNotificationManager(new SysTrayNotificationManager(this.CreateWinFormsNotifiyIcon(notifier => {
                notifier.ContextMenu = new System.Windows.Forms.ContextMenu();
                notifier.ContextMenu.MenuItems.Add("Resume UI", (a, b) => {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                        foreach (var window in App.Current.Windows) {
                            ((Window)window).Activate();
                            ((Window)window).Show();
                        }
                    }));
                });
                notifier.ContextMenu.MenuItems.Add("Suspend UI", (a, b) => {
                    App.Current.Dispatcher.BeginInvoke(new Action(() => {
                        foreach (var window in App.Current.Windows) {
                            ((Window)window).Hide();
                        }
                    }));
                });
                notifier.ContextMenu.MenuItems.Add("Restart", (a, b) => {
                    App.Current.Dispatcher.BeginInvoke(new Action(Restart));
                });
                notifier.ContextMenu.MenuItems.Add("Close", (a, b) => {
                    App.Current.Dispatcher.BeginInvoke(new Action(Close));
                });
                // OBSOLETE: notifier.ShowBalloonTip(1000, "Newgen", "Loading ...", System.Windows.Forms.ToolTipIcon.Info);
            })));

            // Hot key
            try {
                var objCurrentModule = Process.GetCurrentProcess().MainModule;
                objKeyboardProcess = new WinAPI.HookProc(CaptureKey);
                ptrHook = WinAPI.SetWindowsHookEx(13, objKeyboardProcess, WinAPI.GetModuleHandle(objCurrentModule.ModuleName), 0);
            }
            catch { }

            // IPC messages
            Api.Messenger.MessageReceived += new Action<IntPtr, EMessage>(OnMessageReceived);

            // Hub helpers
            Api.HubOpening += new Action(() => Screen.ZOrderHelper(false));
            Api.HubClosing += new Action(() => {
                if (Screen != null)
                    Screen.ZOrderHelper(true);
                ThreadingExtensions.LazyInvoke(WinAPI.FlushMemory, 250);
            });

            // Load view
            StartupUri = new Uri("Core/Screen.xaml", UriKind.Relative);

#if !DEBUG

            // Tracker
            tracker = new ExtendedTracker(
                NS.Web.WebShared.GoogleAnalytics_NSApps_net_Id,
                NS.Web.WebShared.GoogleAnalytics_NSApps_net_Domain
                );

            // Run tracker under non-dispatcher context.
            this.Dispatcher.RunFor(async () => {
                var uri = new Uri(Newgen.Resources.Definitions.Link_App);

                await tracker.TrackEventAsync(Newgen.Resources.Definitions.Text_App, "Packages", "Loaded", Settings.Current.LoadedWidgets.Count);
                await tracker.TrackEventAsync(Newgen.Resources.Definitions.Text_App, "Packages", "Installed", PackageManager.Current.Packages.Count);
                await tracker.TrackEventAsync(Newgen.Resources.Definitions.Text_App, "License", Settings.IsProMode ? "Full" : "Free", 1);
            }, -1, TimeSpan.FromMinutes(45).TotalMilliseconds);

#endif
        }

        /// <summary>
        /// Handles the Exit event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExitEventArgs" /> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void Application_Exit(object sender, ExitEventArgs e) {
            PackageServer.Current.Stop();

            Api.Messenger.MessageReceived -= new Action<IntPtr, EMessage>(OnMessageReceived);

            PackageManager.Current.UnloadAll();

            try {
                var taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                var hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                WinAPI.ShowWindow(taskbar, WindowShowStyle.Show);
                WinAPI.ShowWindow(hwndOrb, WindowShowStyle.Show);
            }
            catch /* Eat */ { }

            Thread.Sleep(1500); // 1.5s enough ?
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs" />
        /// instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;
        }

        /// <summary>
        /// Called when [message received].
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        private void OnMessageReceived(IntPtr hWnd, EMessage message) {
            switch (message.Key) {

            // Url
            case EMessage.UrlKey:
            case EMessage.InternetKey:
                if (PackageManager.Current.IsEnabled(EMessage.InternetKey))
                    PackageManager.Current.Get(EMessage.InternetKey).OnMessageReceived(message);
                else
                    WinAPI.ShellExecute(IntPtr.Zero, "open", message.Value, string.Empty, string.Empty, 0);

                break;

            // Notification
            case EMessage.NotificationKey:
                App.Current.ShowNotification(
                    new Notification(message.Value.Substring(0, System.Math.Min(10, message.Value.Length)) + "...", message.Value),
                    NotificationProviderType.Custom | NotificationProviderType.Native
                    );

                break;

            // Url
            case EMessage.AllKey:
                foreach (var package in PackageManager.Current.Packages)
                    package.OnMessageReceived(message);

                break;

            // Must be a package !
            default: {
                    var package = PackageManager.Current.Get(message.Key);
                    if (package != null)
                        package.OnMessageReceived(message);
                }
                break;
            }
        }

        /// <summary>
        /// The current key state
        /// </summary>
        private int currentKeyState = 1; //1=Down, 2=Up

        /// <summary>
        /// Captures the key.
        /// </summary>
        /// <param name="nCode">The n code.</param>
        /// <param name="wp">The wp.</param>
        /// <param name="lp">The lp.</param>
        /// <returns>IntPtr.</returns>
        /// <remarks>...</remarks>
        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp) {
            if (currentKeyState == 2) { currentKeyState = 0; }
            if (currentKeyState == 1) { currentKeyState = 2; }
            if (currentKeyState == 0) { currentKeyState = 1; }

            if (nCode >= 0 && Settings.Current.EnableHotkeys) {
                var objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                try {
                    if (currentKeyState == 2 && (
                        objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.LWin))) {
                        foreach (Window window in Application.Current.Windows) {
                            if (window.GetType() == typeof(Screen)) {
                                Application.Current.Dispatcher.Invoke(new Action(() => {
                                    if (window.IsVisible) {
                                        window.Hide();
                                    }
                                    else {
                                        window.Show();
                                        window.Activate();
                                    }
                                }));
                            }
                        }

                        return (IntPtr)1;
                    }
                    if (currentKeyState == 2 && (
                        objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.RWin))) {
                        if (Screen.StartBar.IsOpened) {
                            Screen.StartBar.CloseToolbar();
                        }
                        else {
                            Screen.StartBar.OpenToolbar();
                        }

                        return (IntPtr)1;
                    }
                }
                catch { return WinAPI.CallNextHookEx(ptrHook, nCode, wp, lp); }
            }
            return WinAPI.CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void Close() {
            try {
                Settings.Current.Save();
                Application.Current.MainWindow.Close();
            }
            finally { Application.Current.Shutdown(0); }
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void Restart() {
            var psi = new ProcessStartInfo {
                Arguments = "/C TIMEOUT /T 5 /NOBREAK && \"" + Assembly.GetEntryAssembly().Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(psi);
            Close();
        }
    }
}
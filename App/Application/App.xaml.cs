﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using libns;
using libns.Language;
using libns.Logging;
using libns.Native;
using libns.Threading;
using Newgen.HtmlApp;
using PackageManager;
using Application = System.Windows.Application;

namespace Newgen {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>...</remarks>
    public partial class App : ExtendedApplication {

        /// <summary>
        /// The pm
        /// </summary>
        public static PackageManagerPackage PM;

        /// <summary>
        /// The PSS
        /// </summary>
        public static PackageSettingsStorage PSS;

        /// <summary>
        /// The PTR hook
        /// </summary>
        private IntPtr ptrHook;

        /// <summary>
        /// The object keyboard process
        /// </summary>
        private WinAPI.HookProc objKeyboardProcess;

        /// <summary>
        /// Gets the app icon.
        /// </summary>
        /// <value>The app icon.</value>
        /// <remarks>...</remarks>
        public override Icon AppIcon { get { return new Icon(GetResourceStream(new Uri("/Newgen.Core;component/Resources/Newgen_Icon.ico", UriKind.Relative)).Stream); } }

        /// <summary>
        /// Gets the app logo.
        /// </summary>
        /// <value>The app logo.</value>
        /// <remarks>...</remarks>
        public override BitmapSource AppLogo { get { return new BitmapImage(new Uri("/Newgen.Core;component/Resources/Newgen_Icon.ico", UriKind.Relative)); } }

        private static Screen screen;

        /// <summary>
        /// Gets the screen.
        /// </summary>
        /// <value>The start screen.</value>
        /// <remarks>...</remarks>
        public static Screen Screen {
            get {
                if (screen == null) {
                    screen = new Screen();
                    Application.Current.MainWindow = screen;
                }

                if (Application.Current.MainWindow as Screen == null)
                    Api.Logger.LogWarning("Somehow Application.Current.MainWindow is not screen.");

                return screen;
            }
        }

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
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public App() {
            Api.OnPreInitialization(this);

            Api.Logger.LogInformation("STARTING app.");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="App"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        ~App() {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            WinAPI.UnhookWindowsHookEx(ptrHook);
            objKeyboardProcess = null;
        }

        /// <summary>
        /// Handles the Startup event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="StartupEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void Application_Startup(object sender, StartupEventArgs e) {
            // Helper registration
            this.RegisterApplication();

            // Notifications
            this.AddNotificationManager(new SysTrayNotificationManager(this.CreateWinFormsNotifiyIcon(notifier => {
                notifier.ContextMenu = new ContextMenu();
                notifier.ContextMenu.MenuItems.Add("Resume UI", (a, b) => Current.Dispatcher.BeginInvoke(new Action(() => {
                    foreach (var window in Current.Windows) {
                        ((Window)window).Activate();
                        ((Window)window).Show();
                    }
                })));
                notifier.ContextMenu.MenuItems.Add("Suspend UI", (a, b) => Current.Dispatcher.BeginInvoke(new Action(() => {
                    foreach (var window in Current.Windows) {
                        ((Window)window).Hide();
                    }
                })));
                notifier.ContextMenu.MenuItems.Add("Restart", (a, b) => Current.Dispatcher.BeginInvoke(new Action(Restart)));
                notifier.ContextMenu.MenuItems.Add("Close", (a, b) => Current.Dispatcher.BeginInvoke(new Action(Close)));
                // OBSOLETE: notifier.ShowBalloonTip(1000, "Newgen", "Loading ...", System.Windows.Forms.ToolTipIcon.Info);
            })));

            // Hot key
            try {
                var objCurrentModule = Process.GetCurrentProcess().MainModule;
                objKeyboardProcess = new WinAPI.HookProc(CaptureKey);
                ptrHook = WinAPI.SetWindowsHookEx(13, objKeyboardProcess, WinAPI.GetModuleHandle(objCurrentModule.ModuleName), 0);

                Api.Logger.LogInformation("Hooking into system key events done without error.");
            }
            catch { Api.Logger.LogWarning("Hooking into system key events done with error."); }

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
            Screen.Show();

#if !DEBUG

            // Run tracker under non-dispatcher context.
            InternalHelper.SendAnalytics().ConfigureInstances(-1);

#endif
            try {
                // PM.
                PSS = new PackageSettingsStorage();
                PM = PackageManagerPackage.Create(PSS);

                PM.Settings.RemoveAll(f => f is RuntimeSettings);
                PM.Settings.Add(new NewgenPackageManagerRuntimeSettings(PM));

                PM.Logger.Listeners.Add(new LoggerLogListener(PackageManagerExtensions.Logger));
                PackageManagerExtensions.Logger.Listeners.Add(new LoggerLogListener(Api.Logger));

                PM.Packages.CollectionChanged += (o, a) => {
                    switch (a.Action) {
                    // On package loaded.
                    case NotifyCollectionChangedAction.Add:
                        foreach (var package in a.NewItems.OfType<NewgenPackage>()) {
                            Dispatcher.BeginInvoke(new Action(() => {
                                package.Start();
                            }));
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        break;

                    // On package un-loaded.
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var package in a.NewItems.OfType<NewgenPackage>()) {
                            Dispatcher.BeginInvoke(new Action(() => {
                                package.Stop();
                            }));
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        break;

                    default:
                        break;
                    }
                };

                // Find all packages
                if (!PM.GetSettings().ScanLocations.Contains(Api.PackagesRoot))
                    PM.GetSettings().ScanLocations.Add(Api.PackagesRoot);
                ThreadingExtensions.LazyInvoke(async () => await PM.LoadAll(), 3500);
            }
            catch (Exception ex) {
                Api.Logger.LogError("PM cannot be started.", ex);
            }

            Api.Logger.LogInformation("STARTED app.");
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Api.Logger.LogError(e.ExceptionObject);
        }

        /// <summary>
        /// Handles the Exit event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExitEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private async void Application_Exit(object sender, ExitEventArgs e) {
            Api.OnPreFinalization();

            // Detach from IPC.
            Api.Messenger.MessageReceived -= new Action<IntPtr, EMessage>(OnMessageReceived);

            // Save all packages.
            await PM.SaveAll().ConfigureAwait(false);

            // Fix taskbar before leaving.
            try {
                WinAPI.ShowWindow(WinAPI.FindWindow("Shell_TrayWnd", ""), WindowShowStyle.Show);
                WinAPI.ShowWindow(WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null), WindowShowStyle.Show);
            }
            catch (Exception ex) { Api.Logger.LogWarning(ex); }

            Thread.Sleep(1500); // HACK: 1.5s enough ?
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/>
        /// instance containing the event data.
        /// </param>
        /// <remarks>...</remarks>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;

            Api.Logger.LogError(e.Exception);
        }

        /// <summary>
        /// Called when [message received].
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        private void OnMessageReceived(IntPtr hWnd, EMessage message) {
            Api.Logger.LogInformation("IPC message received.", message.Key, message.Value);

            switch (message.Key) {
            // Url
            case EMessage.UrlKey:
            case EMessage.InternetKey:
                var internetPackage = PM.Packages.OfType<NewgenPackage>().Where(f => f.GetId().Equals(EMessage.InternetKey)).FirstOrDefault();

                if (internetPackage.GetSettings().IsEnabled)
                    internetPackage.OnMessageReceived(message);
                else
                    WinAPI.ShellExecute(IntPtr.Zero, "open", message.Value, string.Empty, string.Empty, 0);

                break;

            // Notification
            case EMessage.NotificationKey:
                Current.ShowNotification(
                    new Notification(message.Value.Substring(0, Math.Min(10, message.Value.Length)) + "...", message.Value),
                    NotificationProviderType.Custom | NotificationProviderType.Native
                    );

                break;

            // All
            case EMessage.AllKey:
                foreach (var package in PM.Packages.OfType<NewgenPackage>())
                    package.OnMessageReceived(message);

                break;

            // Must be a package !
            default: {
                    var package = PM.Packages.OfType<NewgenPackage>().Where(f => f.GetId().Equals(message.Key)).FirstOrDefault();
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
            catch (Exception ex) {
                Api.Logger.LogWarning("CLOSING app.", ex);
            }
            finally {
                Application.Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void Restart() {
            var psi = new ProcessStartInfo {
                Arguments = "/C TIMEOUT /T 25 /NOBREAK && \"" + Assembly.GetEntryAssembly().Location + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(psi);
            Close();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Shell;
using Newgen.Base;
using Newgen.Base.Communication;
using Newgen.Core;
using Newgen.Native;
using Newgen.Windows;

namespace Newgen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp, IDisposable
    {
        #region Internals

        internal static WindowManager WindowManager;
        internal static WidgetManager WidgetManager;
        internal static Settings Settings;

        internal static Windows.MainWindow StartScreen { get { return App.Current.MainWindow as Windows.MainWindow; } }

        private IntPtr ptrHook;
        private WinAPI.LowLevelKeyboardProc objKeyboardProcess;

        internal static Server LocalServer;
        internal static bool IsProMode = false;

        #endregion Internals

        #region App

        #region ISingleInstanceApp Members

        /// <summary>
        /// Mains this instance.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if(SingleInstance<App>.InitializeAsFirstInstance(HelperMethods.AppId.ToString()))
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();

                SingleInstance<App>.Cleanup();
            }
        }

        /// <summary>
        /// Signals the external command line args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            CommandLineArgumentsParser command = new CommandLineArgumentsParser(args.ToArray<string>());

            if(!HelperMethods.CheckIESettingsEnabled())
                HelperMethods.UpdateIESettings();

            if(App.IsProMode)
                if(command["nwp"] != null)
                    if(HelperMethods.ShowQAMessage(E.MSG_QA_INSTALLWIDGET).HasFlag(MessageBoxResult.Yes))
                        if(File.Exists(command["nwp"]))
                        {
                            var fi = new FileInfo(command["nwp"]);
                            WidgetManager.InstallWidgetFromPackage(fi.FullName ?? command["nwp"], fi.Name ?? "Widget-" + DateTime.Now.Ticks.ToString());
                        }

            return true;
        }

        #endregion ISingleInstanceApp Members

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">More than one instance of the <see cref="T:System.Windows.Application"/> class is created per <see cref="T:System.AppDomain"/>.</exception>
        public App()
        {
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(ApplicationDispatcherUnhandledException);

            E.Init();
        }

        /// <summary>
        /// Applications the startup.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.StartupEventArgs"/> instance containing the event data.</param>
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            SignalExternalCommandLineArgs(e.Args.ToList<string>());

            Settings = new Settings();

            try
            {
                iFramework.Security.Licensing.LicenseManager.Initialize(
                    HelperMethods.AppId,
                    "Newgen.iFr-License",
                    iFramework.Security.Licensing.LicenseFileType.Isolated
                    );
                var lic = iFramework.Security.Licensing.LicenseManager.Current.LoadLicense();
                if(lic != null)
                    IsProMode = (lic.Status == iFramework.Security.Licensing.LicenseStatus.Licensed);
            }
            catch { }

            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.Config) ?? new Settings();
            Settings.Update();

            try
            {
                LocalServer = new Base.Communication.Server();
                Base.Communication.Server.LoadDefaultCommands(ref LocalServer);
            }
            catch { HelperMethods.ShowErrorMessage(E.MSG_ER_SRVER); }

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Settings.Language);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Settings.Language);

            E.Language = Settings.Language;
            E.TileSpacing = Settings.TileSpacing;
            E.MinTileWidth = Settings.MinTileWidth;
            E.MinTileHeight = Settings.MinTileHeight;
            E.BackgroundColor = App.Settings.BackgroundColor;

            WindowManager = new WindowManager();
            WidgetManager = new WidgetManager();

            HelperMethods.RunMethodAsync(App.WidgetManager.FindWidgets);

            try
            {
                var objCurrentModule = Process.GetCurrentProcess().MainModule;
                objKeyboardProcess = new WinAPI.LowLevelKeyboardProc(CaptureKey);
                ptrHook = WinAPI.SetWindowsHookEx(13, objKeyboardProcess, WinAPI.GetModuleHandle(objCurrentModule.ModuleName), 0);
            }
            catch { }

            StartupUri = new Uri("Controls/MainWindow.xaml", UriKind.Relative);

            MessagingHelper.MessageReceived += new MessagingHelper.MessageHandler(MessagingHelper_MessageReceived);

            E.HubOpening += new Action(() => App.StartScreen.ZOrderHelper(false));
            E.HubClosing += new Action(() =>
            {
                if(App.StartScreen != null)
                App.StartScreen.ZOrderHelper(true);
                Helper.Delay(() => WinAPI.FlushMemory(), 250);
            });

            // HACK: Critical ! May bring down whole app.
            Action sud = () => { try { HelperMethods.SendUsageData(); } catch { } };
            Helper.Delay(sud, 1d * 60d * 1000d);
            Helper.RunFor(sud, -1, 55d * 60d * 1000d);
        }

        /// <summary>
        /// Messagings the helper_ message received.
        /// </summary>
        /// <param name="e">The <see cref="Newgen.Base.MessageEventArgs"/> instance containing the event data.</param>
        private void MessagingHelper_MessageReceived(MessageEventArgs e)
        {
            string command = e.Data.Message;
            List<string> parameters = null;

            if(e.Data.Message.Contains(":"))
            {
                string[] commandandparam = e.Data.Message.Split(new char[] { ':' }, 2);
                command = commandandparam[0];
                parameters = commandandparam[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if(e.Data.MessageKey == MessagingHelper.NewgenWidgetKey)
            {
                var widget = WidgetManager.GetWidgetByName(command);
                if(widget != null && parameters != null && parameters.Count > 0) { widget.WidgetComponent.HandleMessage(parameters[0]); }
            }

            else if(e.Data.MessageKey == MessagingHelper.NewgenKey)
            {
                switch(command)
                {
                    case "TEST":
                        if(parameters == null) { return; }
                        else
                        {
                            string p = "";
                            foreach(string a in parameters) { p += a; }
                            HelperMethods.ShowInfoMessage("Test successful !\n\n->" + p);
                        }
                        break;

                    case "POPMSG":
                        if(parameters == null) { return; }
                        else
                        {
                            string p = "";
                            foreach(string a in parameters) { p += a; }
                            HelperMethods.ShowInfoMessage(p);
                        }
                        break;

                    case "POPERR":
                        if(parameters == null) { return; }
                        else
                        {
                            string p = "";
                            foreach(string a in parameters) { p += a; }
                            HelperMethods.ShowErrorMessage(p);
                        }
                        break;

                    case "URL":
                        if(parameters == null) { return; }
                        else
                        {
                            if(parameters[0] != null)
                            {
                                if(App.WidgetManager.IsWidgetLoaded("Internet")) { MessagingHelper.SendMessageToWidget("Internet", (parameters[0])); }
                                else { Newgen.Native.WinAPI.ShellExecute(IntPtr.Zero, "open", (parameters[0]), string.Empty, string.Empty, 0); }
                            }
                        }
                        break;

                    case "Update":
                        if(parameters == null) { return; }
                        else { if(parameters[0] == "UserInfo") { App.StartScreen.LoadUserTileInfo(); } }
                        break;

                    case "InstallWidget":
                        if(parameters == null || parameters.Count < 2) { return; }
                        if(App.IsProMode)
                            WidgetManager.InstallWidgetFromPackage(parameters[0], parameters[1]);
                        break;

                    case "RemoveWidget":
                        if(parameters == null || parameters.Count < 1) { return; }
                        if(App.IsProMode)
                            WidgetManager.RemoveWidget(parameters[0]);
                        break;
                }
            }
        }

        /// <summary>
        /// Applications the exit.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.ExitEventArgs"/> instance containing the event data.</param>
        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            try
            {
                LocalServer.IsAlive = false;
                Thread.Sleep(100);

                MessagingHelper.MessageReceived -= new MessagingHelper.MessageHandler(MessagingHelper_MessageReceived);

                IntPtr taskbar = WinAPI.FindWindow("Shell_TrayWnd", "");
                IntPtr hwndOrb = WinAPI.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
                WinAPI.ShowWindow(taskbar, WinAPI.WindowShowStyle.Show);
                WinAPI.ShowWindow(hwndOrb, WinAPI.WindowShowStyle.Show);
                HelperMethods.SetAutoStart(App.Settings.Autostart);
                foreach(var widget in WidgetManager.Widgets)
                    if(widget.IsLoaded)
                        widget.Unload();

                e.ApplicationExitCode = 0;
            }
            catch { }
        }

        #endregion App

        #region Win32 Hooks

        private int KeyState = 1; //1=Down, 2=Up

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if(KeyState == 2) { KeyState = 0; }
            if(KeyState == 1) { KeyState = 2; }
            if(KeyState == 0) { KeyState = 1; }

            if(nCode >= 0 && App.Settings.EnableHotkeys)
            {
                WinAPI.KBDLLHOOKSTRUCT objKeyInfo = (WinAPI.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(WinAPI.KBDLLHOOKSTRUCT));

                try
                {
                    if(KeyState == 2 && (
                        objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.LWin)))
                    {
                        foreach(Window window in Application.Current.Windows)
                        {
                            if(window.GetType() == typeof(Windows.MainWindow))
                            {
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if(window.IsVisible)
                                    {
                                        window.Hide();
                                    }
                                    else
                                    {
                                        window.Show();
                                        window.Activate();
                                    }
                                }));
                            }
                        }

                        return (IntPtr)1;
                    }
                    if(KeyState == 2 && (
                        objKeyInfo.vkCode == KeyInterop.VirtualKeyFromKey(Key.RWin)))
                    {
                        if(App.StartScreen.StartBar.IsOpened)
                        {
                            App.StartScreen.StartBar.CloseToolbar();
                        }
                        else
                        {
                            App.StartScreen.StartBar.OpenToolbar();
                        }

                        return (IntPtr)1;
                    }
                }
                catch { return WinAPI.CallNextHookEx(ptrHook, nCode, wp, lp); }
            }
            return WinAPI.CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        #endregion Win32 Hooks

        #region Error Handling

        /// <summary>
        /// Applications the dispatcher unhandled exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        #endregion Error Handling

        #region Internal Methods

        /// <summary>
        /// Shows the options.
        /// </summary>
        internal static void ShowOptions(bool nlic = false, bool isupdatemode = false)
        {
            var window = new Windows.Settings(nlic, isupdatemode);
            E.CallEvent("HubOpening");
            window.ShowDialog();
            E.CallEvent("HubClosing");
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                List<TileScreenWidgetInfo> lw = new List<TileScreenWidgetInfo>();
                foreach(var w in App.StartScreen.runningWidgets)
                {
                    TileScreenWidgetInfo loadedWidget = new TileScreenWidgetInfo();
                    loadedWidget.Column = Grid.GetColumn(w);
                    loadedWidget.Row = Grid.GetRow(w);
                    loadedWidget.Name = w.WidgetProxy.Name;
                    loadedWidget.Data = w.WidgetProxy.ObjectData;
                    if(w.WidgetProxy.Path != null &&
                        (w.WidgetProxy.Path.Contains(@"\") || w.WidgetProxy.Path.Contains(@"/"))
                        )
                        loadedWidget.Path = w.WidgetProxy.Path.Replace(E.WidgetsRoot, "");
                    else
                        loadedWidget.Id = w.WidgetProxy.Path.Replace(E.WidgetsRoot, "");
                    lw.Add(loadedWidget);
                }
                App.Settings.LoadedWidgets.Clear();
                App.Settings.LoadedWidgets.AddRange(lw);
                App.Settings.Save(E.Config);
            }
            catch { }
        }

        /// <summary>
        /// Restarts this instance.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void Restart()
        {
            SingleInstance<App>.Cleanup();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = "/C ping 192.0.2.2 -n 1 -w 2000 && \"" + Application.ResourceAssembly.Location + "\"";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.FileName = "cmd.exe";
            Process.Start(psi);
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        internal static void Close()
        {
            try
            {
                App.SaveSettings();
                Application.Current.MainWindow.Close();
            }
            finally { Application.Current.Shutdown(0); }
        }

        #endregion Internal Methods

        #region IDisposable Members

        public void Dispose()
        {
            WinAPI.UnhookWindowsHookEx(this.ptrHook);
            objKeyboardProcess = null;
        }

        #endregion
    }
}
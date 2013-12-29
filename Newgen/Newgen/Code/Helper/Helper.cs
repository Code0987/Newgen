using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Ionic.Zip;
using Microsoft.Win32;
using Newgen.Base;
using NS.Web;

namespace Newgen.Core
{
    /// <summary>
    /// Helper.
    /// </summary>
    public static partial class HelperMethods
    {
        /// <summary>
        /// App id.
        /// </summary>
        public static Guid AppId = new Guid("c3373d77-29c6-4670-8afb-43f0830bc3cf");

        public static readonly DateTime StartupTime = DateTime.Now;

        /// <summary>
        /// Unpacks the widget.
        /// </summary>
        /// <param name="packagefile">The packagefile.</param>
        public static void UnpackWidget(string packagefile)
        {
            Task t = new Task(new Action(() =>
            {
                FileInfo pf = new FileInfo(packagefile);
                if(!pf.Exists) { return; }

                try
                {
                    using(ZipFile zip = ZipFile.Read(packagefile))
                    {
                        foreach(ZipEntry e in zip)
                        {
                            e.Extract(E.WidgetsRoot, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch
                {
                }
            }));

            t.Start();
        }

        /// <summary>
        /// Runs the method async thread safe.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static DispatcherOperation RunMethodAsyncThreadSafe(Action method)
        {
            return Application.Current.Dispatcher.BeginInvoke(method, DispatcherPriority.Background, null);
        }

        /// <summary>
        /// Runs the method async.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static Task RunMethodAsync(Action method)
        {
            return Task.Factory.StartNew(method);
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "// Newgen / : Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// Shows the info message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "// Newgen / : Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows the QA message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static MessageBoxResult ShowQAMessage(string message)
        {
            return MessageBox.Show(message, "// Newgen / : ?", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        /// <summary>
        /// Sends the usage data.
        /// </summary>
        /// <remarks>...</remarks>
        internal static void SendUsageData()
        {
            if(!App.Settings.ProvideUsageData || (DateTime.Now-App.Settings.ProvideUsageDataLastSentOn).Hours < 1d)
                return;

            App.Settings.ProvideUsageDataLastSentOn = DateTime.Now;

            var uri = new Uri(Resources.Resources.Link_App);

            var r = AnalyticsHelper.CreateRequest(Resources.Resources.Text_App, uri.Host, uri.PathAndQuery, "UA-30426206-1");

            r.UpdateRequest(new AnalyticsHelper.Event(r.PageTitle, "Usage", "Startup", 1)).GetResponseAsync();
            r.UpdateRequest(new AnalyticsHelper.Event(r.PageTitle, "Usage", "1h-KA", (DateTime.Now-StartupTime).Hours)).GetResponseAsync();
            r.UpdateRequest(new AnalyticsHelper.Event(r.PageTitle, "Usage", "Widgets", App.Settings.LoadedWidgets.Count)).GetResponseAsync();

            r.UpdateRequest(new AnalyticsHelper.Event(r.PageTitle, "License", App.IsProMode ? "Full" : "Free", 1)).GetResponseAsync();
        }

        /// <summary>
        /// Updates the IE settings.
        /// </summary>
        public static void UpdateIESettings()
        {
            try
            {
                RegistryKey feature = Registry.LocalMachine.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl");

                RegistryKey FEATURE_BROWSER_EMULATION = null;
                FEATURE_BROWSER_EMULATION = feature.OpenSubKey("FEATURE_BROWSER_EMULATION", true);
                FEATURE_BROWSER_EMULATION.SetValue("Newgen.exe", 9000, RegistryValueKind.DWord);
                FEATURE_BROWSER_EMULATION.Close();

                RegistryKey FEATURE_GPU_RENDERING = null;
                FEATURE_GPU_RENDERING = feature.OpenSubKey("FEATURE_GPU_RENDERING", true);
                FEATURE_GPU_RENDERING.SetValue("Newgen.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_GPU_RENDERING.Close();

                RegistryKey FEATURE_DISABLE_NAVIGATION_SOUNDS = null;
                FEATURE_DISABLE_NAVIGATION_SOUNDS = feature.OpenSubKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", true);
                FEATURE_DISABLE_NAVIGATION_SOUNDS.SetValue("Newgen.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_DISABLE_NAVIGATION_SOUNDS.Close();

                RegistryKey FEATURE_TABBED_BROWSING = null;
                FEATURE_TABBED_BROWSING = feature.OpenSubKey("FEATURE_TABBED_BROWSING", true);
                FEATURE_TABBED_BROWSING.SetValue("Newgen.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_TABBED_BROWSING.Close();

                RegistryKey FEATURE_ADDON_MANAGEMENT = null;
                FEATURE_ADDON_MANAGEMENT = feature.OpenSubKey("FEATURE_ADDON_MANAGEMENT", true);
                FEATURE_ADDON_MANAGEMENT.SetValue("Newgen.exe", 00000001, RegistryValueKind.DWord);
                FEATURE_ADDON_MANAGEMENT.Close();
            }
            catch { }
        }

        /// <summary>
        /// Checks the IE settings enabled.
        /// </summary>
        /// <returns></returns>
        public static bool CheckIESettingsEnabled()
        {
            try
            {
                using(var FEATURE_BROWSER_EMULATION = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadSubTree).OpenSubKey("Microsoft").OpenSubKey("Internet Explorer").OpenSubKey("MAIN").OpenSubKey("FeatureControl").OpenSubKey("FEATURE_BROWSER_EMULATION"))
                {
                    var v = FEATURE_BROWSER_EMULATION.GetValue("Newgen.exe");
                    FEATURE_BROWSER_EMULATION.Close();
                    if(v == null)
                        return false;
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Sets the auto start.
        /// </summary>
        /// <param name="autostart">if set to <c>true</c> [autostart].</param>
        public static void SetAutoStart(bool autostart)
        {
            try
            {
                RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if(autostart)
                {
                    regSUK.SetValue("Newgen", Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    regSUK.DeleteValue("Newgen", false);
                }
            }
            catch { }
        }

        public static bool GetAutoStart()
        {
            try
            {
                RegistryKey regSUK = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                bool result = false;
                if(regSUK.GetValue("Newgen", null) != null)
                    result = true;
                return result;
            }
            catch { }
            return false;
        }

        public static string ConvertToSafeFileName(string url)
        {
            return Path.GetFileName(Uri.UnescapeDataString(url).Replace("/", "\\").Replace("?", "-").Replace(":", "-"));
        }

        /// <summary>
        /// Creates the bitmap source from bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            if(bitmap == null)
                throw new ArgumentNullException("bitmap");

            if(Application.Current.Dispatcher == null)
                return null; // Is it possible?

            try
            {
                using(MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream.
                    // I'm assuming it is PNG
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if(InvokeRequired)
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            DispatcherPriority.Normal,
                            memoryStream
                            );

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch(Exception)
            {
                return null;
            }
        }

        private static bool InvokeRequired
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        /// <summary>
        /// Creates the bitmap source from bitmap.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }

        /// <summary>
        /// Gets the data array.
        /// </summary>
        /// <param name="pathorname">The pathorname.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        internal static string[] GetDataArray(string pathorname, char delimiter = ';')
        {
            WidgetProxy lwlast = null;

            foreach(var lw in App.StartScreen.runningWidgets)
            {
                if(lw.WidgetProxy.Name != null) if(pathorname.Contains(lw.WidgetProxy.Name))
                        lwlast = lw.WidgetProxy;
                if(lw.WidgetProxy.Path != null) if(pathorname.Contains(lw.WidgetProxy.Path))
                        lwlast = lw.WidgetProxy;
            }

            if(lwlast == null)
                return new string[1];
            else
            {
                try { return lwlast.ObjectData.Split(delimiter); }
                catch { return new string[] { lwlast.ObjectData }; }
            }
        }

        /// <summary>
        /// Sets the data array.
        /// </summary>
        /// <param name="pathorname">The pathorname.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        internal static void SetDataArray(string pathorname, string value, int index)
        {
            foreach(var lw in App.StartScreen.runningWidgets)
            {
                if(lw.WidgetProxy.Name != null) if(pathorname.Contains(lw.WidgetProxy.Name))
                    {
                        lw.WidgetProxy.ObjectData = UpdateArraryAndGetAsString(lw.WidgetProxy.ObjectData, ';', value, index);
                    }
                if(lw.WidgetProxy.Path != null) if(pathorname.Contains(lw.WidgetProxy.Path))
                    {
                        lw.WidgetProxy.ObjectData = UpdateArraryAndGetAsString(lw.WidgetProxy.ObjectData, ';', value, index);
                    }
            }
        }

        /// <summary>
        /// Updates the arrary and get as string.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="newvalue">The newvalue.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private static string UpdateArraryAndGetAsString(string original, char delimiter, string newvalue, int index)
        {
            string[] newvaluearray = ((string)(original ?? "")).Split(delimiter);

            if(index >= newvaluearray.Length)
            {
                for(int i = 0; i <= index; i++)
                    original += ';';
                newvaluearray = original.Split(delimiter);
            }

            newvaluearray[index] = newvalue;

            string newstringdata = "";
            for(int i = 0; i < newvaluearray.Length; i++)
                newstringdata += newvaluearray[i] + ';';
            newstringdata = newstringdata.Trim().Remove(newstringdata.Length - 1);

            return newstringdata;
        }
    }

    /// <summary>
    /// Args
    /// </summary>
    public class CommandLineArgumentsParser
    {
        // Variables
        private StringDictionary Parameters;

        // Constructor
        public CommandLineArgumentsParser(string[] Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string Parameter = null;
            string[] Parts;

            foreach(string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);
                switch(Parts.Length)
                {
                    // Found a value (for the last parameter found (space separator))
                    case 1:
                        if(Parameter != null)
                        {
                            if(!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] = Remover.Replace(Parts[0], "$1");
                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }

                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:

                        // The last parameter is still waiting. With no value, set it to true.
                        if(Parameter != null)
                        {
                            if(!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:

                        // The last parameter is still waiting. With no value, set it to true.
                        if(Parameter != null)
                        {
                            if(!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')
                        if(!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }
                        Parameter = null;
                        break;
                }
            }

            // In case a parameter is still waiting
            if(Parameter != null)
            {
                if(!Parameters.ContainsKey(Parameter))
                    Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists
        public string this[string Param]
        {
            get
            {
                return (Parameters[Param]);
            }
        }
    }
}
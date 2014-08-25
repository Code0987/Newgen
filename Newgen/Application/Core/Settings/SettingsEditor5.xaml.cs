using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using System.Xml.Linq;
using iFramework.Security.Licensing;
using Newgen.Resources;
using libns.Threading;

namespace Newgen {

    /// <summary>
    /// Interaction logic for SettingsEditor5.xaml
    /// </summary>
    public partial class SettingsEditor5 : UserControl {

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEditor5"/> class.
        /// </summary>
        /// <remarks>...</remarks>
        public SettingsEditor5() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the <see cref="E:Loaded" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>...</remarks>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            try {
            }
            catch /* Eat */ { /* Tasty ? */ }
        }




        private const string UpdatesBase = "http://data.nsapps.net/cache/c3373d77-29c6-4670-8afb-43f0830bc3cf/12/updates/";

        private string UpdateFile = "Newgen.exe";
        private string UpdateVersion = "0.0.0.0";
        private string UpdateReleaseDate = "0-0-0";
        private string UpdateNotes = "-";

        internal static bool IsUpdateAvailable() {
            var xml = (XElement)null;
            var uv = "0.0.0.0";
            try {
                xml = XElement.Load(UpdatesBase + "meta.xml");
                foreach (XElement element in xml.Elements())
                    if (element.Name == "Version") { uv = element.Value; continue; }
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var uversion = Version.Parse(uv);
                if (version < uversion)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e) {
            try {
                if (UpdateButton.Content.ToString() == Definitions.CFU) {
                    ProgressBar.IsIndeterminate = true;
                    XElement xml = null;
                    try {
                        xml = XElement.Load(UpdatesBase + "meta.xml");

                        foreach (XElement element in xml.Elements()) {
                            if (element.Name == "Version") { UpdateVersion = element.Value; continue; }
                            if (element.Name == "File") { UpdateFile = element.Value; continue; }
                            if (element.Name == "ReleaseDate") { UpdateReleaseDate = element.Value; continue; }
                            if (element.Name == "Notes") { UpdateNotes = element.Value; continue; }
                        }

                        var version = Assembly.GetExecutingAssembly().GetName().Version;
                        var uversion = Version.Parse(UpdateVersion);

                        if (version < uversion) {
                            VersionTextBlock.Text = "Version : " + UpdateVersion;
                            RDTextBlock.Text = "Release Date : " + UpdateReleaseDate;
                            RnTextBlock.Text = "Release Notes : \n\n" + UpdateNotes;
                            UpdatesInfo.Visibility = Visibility.Visible;
                            ProgressBar.IsIndeterminate = true;

                            int ContentLength;
                            string size = "0.00 kb";

                            this.InvokeAsync(() => {
                                System.Net.WebRequest req = System.Net.HttpWebRequest.Create(UpdatesBase + UpdateFile);
                                req.Method = "HEAD";
                                System.Net.WebResponse resp = req.GetResponse();
                                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength)) {
                                    if (ContentLength >= 1048576)
                                        size = "Size : " + ((ContentLength / 1048576).ToString("0.00")) + " mb";
                                    else
                                        size = "Size : " + ((ContentLength / 1024).ToString("0.00")) + " kb";
                                }
                            }).Wait();

                            SizeTextBlock.Text = size;
                            ProgressBar.IsIndeterminate = false;
                            UpdateButton.Content = Definitions.ULV;
                        }
                        else {
                            UpdateButton.Content = Definitions.UNA;
                        }
                    }
                    catch { Api.ShowErrorMessage(Api.MSG_NE); }

                    return;
                }

                if (UpdateButton.Content.ToString() == Definitions.ULV) {
                    try {
                        UpdateButton.IsEnabled = false;
                        ProgressBar.IsIndeterminate = true;

                        WebClient client = new WebClient();
                        string url = UpdatesBase + UpdateFile;
                        string path = Path.GetTempPath();
                        string tempdown = Path.Combine(path, Path.GetFileName(url));

                        client.DownloadFileCompleted += (a, b) => {
                            try {
                                UpdateButton.Content = Definitions.UNA;
                                UpdateButton.IsEnabled = false;
                                if (Api.ShowQAMessage("Do you want copy of downloaded update file, in case the update installation failed ?\n\n(Note: File will be copied to your desktop.)").HasFlag(MessageBoxResult.Yes)) {
                                    FileInfo fi = new FileInfo(tempdown);
                                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.DoNotVerify);
                                    this.InvokeAsync(() => File.Copy(tempdown, desktop + "\\" + fi.Name)).Wait();
                                }
                                Process.Start(tempdown);
                                System.Threading.Thread.Sleep(500);
                                App.Current.Shutdown(98);
                            }
                            catch { File.Delete(tempdown); }
                        };

                        client.DownloadProgressChanged += (a, b) => {
                            ProgressBar.IsIndeterminate = false;
                            ProgressBar.Value = b.ProgressPercentage;
                        };
                        client.DownloadFileAsync(new Uri(url), tempdown);
                    }
                    catch { Api.ShowErrorMessage(Api.MSG_NE); }

                    ProgressBar.IsIndeterminate = false;
                }
                return;
            }
            catch { }
        }
    }
}
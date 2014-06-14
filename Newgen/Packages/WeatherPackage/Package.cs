using System;
using System.Windows;
using Newgen.Packages;
using libns;
using NS.Web;

namespace WeatherPackage {
    /// <summary>
    /// Class Package.
    /// </summary>
    /// <remarks>...</remarks>
    public class Package : Newgen.Packages.Package {
        internal const string PackageId = "Weather";

        internal const string SettingsCustomizerKeyForCurrentWeather = "WeatherRecord";
        internal const string SettingsCustomizerKeyForWeatherOptions = "WeatherOptions";

        internal WeatherData CurrentWeather;
        internal WeatherSettings WeatherOptions;

        private PackageMetadata metadata;
        private Tile tile;
        /// <summary>
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>...</remarks>
        public override int ColumnSpan {
            get {
                return 3;
            }
        }

        /// <summary>
        /// Path to widget icon. Return null if there is no icon.
        /// </summary>
        public override Uri IconPath {
            get { return new Uri("pack://application:,,,/" + PackageId + ";component/Resources/icon.png", UriKind.RelativeOrAbsolute); }
        }


        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>...</remarks>
        public override int RowSpan {
            get {
                return 2;
            }
        }

        public override FrameworkElement Tile {
            get { return tile; }
        }

        public Package(string location)
            : base(location) {
            Metadata = WebShared.CreateMetadataFor(
                typeof(Package),
                PackageId,
                "Weather package for Newgen. Provides latest local/global weather information."
                );
        }

        public override void Load() {
            base.Load();

            WeatherOptions = Settings.Customize<WeatherSettings>(s => {
            }, Package.SettingsCustomizerKeyForWeatherOptions);
            CurrentWeather = Settings.Customize<WeatherData>(s => {
                s.Location = new LocationData() {
                    Code = WeatherOptions.LocationCode
                };
            }, Package.SettingsCustomizerKeyForCurrentWeather);


            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile = new Tile(this);
                tile.Load();
            }));
        }

        public override void Unload() {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                tile.Unload();
            }));

            Settings.Customize(CurrentWeather, Package.SettingsCustomizerKeyForCurrentWeather);
            Settings.Customize(WeatherOptions, Package.SettingsCustomizerKeyForWeatherOptions);

            base.Unload();
        }
    }
}
using System;
using System.IO;
using System.Windows;
using libns;

namespace Newgen.Packages {

    /// <summary>
    /// Abstract for package definition
    /// </summary>
    public abstract class Package {
        private PackageMetadata metadata;

        private PackageSettings settings;

        /// <summary>
        /// Gets the column span.
        /// </summary>
        public virtual int ColumnSpan { get { return 1; } }

        /// <summary>
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>...</remarks>
        public virtual int RowSpan { get { return 1; } }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        public virtual Uri IconPath { get { return new Uri("pack://application:,,,/Newgen;component/Resources/NWP_Icon.ico", UriKind.RelativeOrAbsolute); } }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        /// <remarks>Do not override, unless you know what what to do !</remarks>
        public virtual PackageMetadata Metadata {
            get {
                if (metadata == null) {
                    var metadataFile = Path.Combine(Settings.Location, PackageMetadata.CacheFilename);
                    if (File.Exists(metadataFile))
                        metadata = metadataFile.LoadJavascriptFromFile<PackageMetadata>();
                }
                return metadata;
            }
            set {
                metadata = value;
                metadata.SaveJavascriptToFile(Path.Combine(Settings.Location, PackageMetadata.CacheFilename));
            }
        }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        /// <remarks>Do not override, unless you know what what to do !</remarks>
        public virtual PackageSettings Settings {
            get {
                if (settings == null)
                    settings = settings ?? new PackageSettings();
                return settings;
            }
            set {
                settings = value;
            }
        }

        /// <summary>
        /// Gets the package control.
        /// </summary>
        public abstract FrameworkElement Tile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        public Package(string location) {
            Settings.Location = location;
        }

        /// <summary>
        /// Called when [message received].
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        public virtual void OnMessageReceived(EMessage message) {
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public virtual void Load() {
            var location = settings.Location;
            try {
                if (string.IsNullOrWhiteSpace(location))
                    throw new Exception();
                var oldObjectData = settings.ObjectData;
                settings = Path.Combine(location, PackageSettings.CacheFilename).LoadJavascriptFromFile<PackageSettings>();
                settings.ObjectData = oldObjectData.MergeWith(settings.ObjectData);
            }
            catch /* Eat */ {

                // Tasty ?
            }
            settings.Location = location;
        }

        /// <summary>
        /// Unloads this instance.
        /// </summary>
        public virtual void Unload() {
            try {
                if (!Directory.Exists(settings.Location))
                    Directory.CreateDirectory(settings.Location);

                settings.SaveJavascriptToFile(Path.Combine(settings.Location, PackageSettings.CacheFilename));
            }
            catch /* Eat */ {

                // Tasty ?
            }
        }
    }
}
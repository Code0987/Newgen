using System;
using System.IO;
using System.Windows;
using libns;

namespace Newgen.Packages {

    /// <summary>
    /// Abstract for package definition
    /// </summary>
    /// <remarks>Use this class to mark a assembly as package.</remarks>
    public abstract class Package {

        /// <summary>
        /// The is settings file loaded
        /// </summary>
        private bool isSettingsFileLoaded;

        /// <summary>
        /// The metadata
        /// </summary>
        private PackageMetadata metadata;

        /// <summary>
        /// The settings
        /// </summary>
        private PackageSettings settings;

        /// <summary>
        /// Gets the column span.
        /// </summary>
        /// <value>The column span.</value>
        /// <remarks>It defines the vertical span of tile.</remarks>
        public virtual int ColumnSpan { get { return 1; } }

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
        /// Gets the row span.
        /// </summary>
        /// <value>The row span.</value>
        /// <remarks>It defines the horizontal span of tile.</remarks>
        public virtual int RowSpan { get { return 1; } }

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
        /// Gets the package tile control.
        /// </summary>
        /// <value>The tile.</value>
        /// <remarks>Return a valid XAML control for display.</remarks>
        public abstract FrameworkElement Tile { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Package" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <remarks>...</remarks>
        public Package(string location) {
            Settings.Location = location;

            isSettingsFileLoaded = false;
        }

        /// <summary>
        /// Called whenever the package is loaded into user context.
        /// </summary>
        /// <remarks>Do all loading steps here ! (e.g. loading settings, preparing UI)</remarks>
        public virtual void Load() {
            LoadSettings();
        }

        /// <summary>
        /// Called whenever the package is loaded into user context before <see cref="Load">Load</see>.
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        /// <remarks>...</remarks>
        public void LoadSettings(bool force = false) {
            if (isSettingsFileLoaded && !force)
                return;

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

            isSettingsFileLoaded = true;
        }

        /// <summary>
        /// Called whenever a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>...</remarks>
        public virtual void OnMessageReceived(EMessage message) {
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <remarks>
        /// This functions is automatically called on un-load. Call it only when needed
        /// exceptionally !
        /// </remarks>
        public void SaveSettings() {
            try {
                if (!Directory.Exists(settings.Location))
                    Directory.CreateDirectory(settings.Location);

                settings.SaveJavascriptToFile(Path.Combine(settings.Location, PackageSettings.CacheFilename));
            }
            catch /* Eat */ {
                // Tasty ?
            }
        }

        /// <summary>
        /// Called whenever the package is un-loaded from user context.
        /// </summary>
        /// <remarks>Do all finalization steps here ! (e.g. saving settings)</remarks>
        public virtual void Unload() {
            SaveSettings();
        }
    }
}